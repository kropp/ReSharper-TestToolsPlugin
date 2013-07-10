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
using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Plugins.TestTools.MethodRunner;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Strategy;
using JetBrains.Util;

namespace JetBrains.ReSharper.Plugins.TestTools
{
  public class MethodRunnerElement : IUnitTestElement
  {
    private readonly ProjectModelElementEnvoy myProjectEnvoy;
    private readonly IClrTypeName myClassName;
    private readonly string myMethodName;
    private readonly bool myIsClassStatic;
    private readonly bool myIsMethodStatic;

    private readonly MethodRunnerProvider myProvider;

    internal MethodRunnerElement(IProject project, IClrTypeName className, string methodName, bool isClassStatic, bool isMethodStatic, MethodRunnerProvider provider)
    {
      myProjectEnvoy = ProjectModelElementEnvoy.Create(project);
      myClassName = className;
      myMethodName = methodName;
      myIsClassStatic = isClassStatic;
      myIsMethodStatic = isMethodStatic;
      myProvider = provider;
    }

    public IProject GetProject()
    {
      return myProjectEnvoy.GetValidProjectElement() as IProject;
    }

    public string GetPresentation(IUnitTestElement parent = null)
    {
      return myMethodName + "()";
    }

    public UnitTestNamespace GetNamespace()
    {
      return new UnitTestNamespace(myClassName.GetNamespaceName());
    }

    public UnitTestElementDisposition GetDisposition()
    {
      return UnitTestElementDisposition.InvalidDisposition;
    }

    public IDeclaredElement GetDeclaredElement()
    {
      return null;
    }

    public IEnumerable<IProjectFile> GetProjectFiles()
    {
      return null;
    }

    public IUnitTestRunStrategy GetRunStrategy(IHostProvider hostProvider)
    {
      return myProvider.Strategy;
    }

    public IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch launch)
    {
      return new List<UnitTestTask> { new UnitTestTask(this, new RunMethodTask(MethodRunnerProvider.Id, ((IProject)myProjectEnvoy.GetValidProjectElement()).GetOutputFilePath().FullPath, myClassName.FullName, myMethodName, myIsClassStatic, myIsMethodStatic)) };
    }

    public string Kind { get { return "Method"; } }
    public IEnumerable<UnitTestElementCategory> Categories { get { return UnitTestElementCategory.Uncategorized; } }
    public string ExplicitReason { get { return null; } }
    public string Id { get { return MethodRunnerProvider.Id; } }

    public IUnitTestProvider Provider { get { return myProvider; } }

    public IUnitTestElement Parent { get { return null; } set {} }

    public ICollection<IUnitTestElement> Children { get { return EmptyList<IUnitTestElement>.InstanceList; } }

    public string ShortName { get { return myMethodName; } }
    
    public bool Explicit { get { return true; } }

    public UnitTestElementState State
    {
      get { return UnitTestElementState.Fake; }
      set { }
    }

    public bool Equals(IUnitTestElement other)
    {
      return ReferenceEquals(this, other);
    }
  }
}