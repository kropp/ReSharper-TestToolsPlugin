using System;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.TextControl;
using JetBrains.Util;

namespace CreateTestPlugin
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

      var sessionView = unitTestSessionManager.GetSession(SessionID) ?? unitTestSessionManager.CreateSession(id: SessionID);
      sessionView.Title.Value = "Run Method " + myMethodDeclaration.DeclaredName;
      sessionView.Session.RemoveElements(sessionView.Session.Elements);
      sessionView.Session.AddElement(element);

      sessionView.Run(new UnitTestElements(new[] {element}), solution.GetComponent<ProcessHostProvider>());

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