// Copyright 2013 Victor Kropp, JetBrains s.r.o.
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using System.Reflection;
using JetBrains.Application.Progress;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ProjectModel.Resources;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Elements;
using JetBrains.TextControl;
using JetBrains.Util;

namespace ReSharper.Plugins.TestTools.MethodRunner
{
  [ContextAction(Name = "RunMethod", Description = "Run method as a single test", Group = "C#")]
  public class RunMethodAction : ContextActionBase
  {
    private const string SessionID = "TestToolsPlugin::RunMethodSession";
    private readonly ICSharpContextActionDataProvider myProvider;
    private IMethodDeclaration myMethodDeclaration;
    private IClassDeclaration myClassDeclaration;

    public RunMethodAction(ICSharpContextActionDataProvider provider)
    {
      myProvider = provider;
    }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var unitTestSessionManager = solution.GetComponent<IUnitTestSessionManager>();

      var element = solution.GetComponent<MethodRunnerProvider>().CreateElement(myClassDeclaration.GetSourceFile().GetProject(), new ClrTypeName(myClassDeclaration.CLRName), myMethodDeclaration.DeclaredName, myClassDeclaration.IsStatic, myMethodDeclaration.IsStatic);

      var unitTestElementManager = solution.GetComponent<IUnitTestElementManager>();
      unitTestElementManager.AddElement(element);

      var sessionView = unitTestSessionManager.GetSession(SessionID) ?? unitTestSessionManager.CreateSession(id: SessionID);
      sessionView.Title.Value = "Run Method " + myMethodDeclaration.DeclaredName;
      sessionView.Session.AddElement(element);

      var launchLifetime = Lifetimes.Define(solution.GetLifetime(), "MethodRunner");
      launchLifetime.Lifetime.AddAction(() =>
      {
        unitTestElementManager.RemoveElements(new[] {element});
        unitTestSessionManager.CloseSession(sessionView);
      });

      var unitTestResultManager = solution.GetComponent<IUnitTestResultManager>();

      EventHandler<UnitTestResultEventArgs> onUnitTestResultUpdated = (sender, args) =>
      {
        if (args.Element == element && args.Result.RunStatus == UnitTestRunStatus.Completed)
        {
          var exceptions = string.Empty;
          var resultData = unitTestResultManager.GetResultData(args.Element);
          if (resultData.Exceptions.Any())
            exceptions = resultData.Exceptions.First().StackTrace;
          MessageBox.ShowInfo(args.Result.Message + " " + resultData.Output + " " + exceptions);
          
          // cleanup
          launchLifetime.Terminate();
        }
      };

      unitTestResultManager.UnitTestResultUpdated += onUnitTestResultUpdated;
      launchLifetime.Lifetime.AddAction(() => unitTestResultManager.UnitTestResultUpdated -= onUnitTestResultUpdated);

      sessionView.Run(new UnitTestElements(new[] {element}), solution.GetComponent<ProcessHostProvider>());

      Assembly.LoadFile(typeof(RunMethodTask).Assembly.Location);

      return null;
    }

    public override string Text
    {
      get { return "Run method as test"; }
    }

    public override bool IsAvailable(IUserDataHolder cache)
    {
      myMethodDeclaration = myProvider.GetSelectedElement<IMethodDeclaration>(true, true);
      if (myMethodDeclaration == null)
        return false;

      // only on non-generic methods
      var method = myMethodDeclaration.DeclaredElement;
      if (method == null || method.TypeParameters.Any())
        return false;

      if (!method.IsStatic)
      {
        // only on non-generic types
        myClassDeclaration = myMethodDeclaration.GetContainingTypeDeclaration() as IClassDeclaration;
        if (myClassDeclaration == null || myClassDeclaration.TypeParameters.Any())
          return false;

        // with default constructor
        if (!myClassDeclaration.IsStatic &&
          (!myClassDeclaration.ConstructorDeclarations.IsEmpty() && !myClassDeclaration.ConstructorDeclarations.Any(c => c.ParameterDeclarations.IsEmpty())))
          return false;
      }

      return !method.Parameters.Any();
    }
  }
}