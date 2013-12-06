using System.Collections.Generic;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Strategy;
using JetBrains.Util;
using MethodRunner;

namespace CreateTestPlugin
{
  public class MethodRunnerElement : IUnitTestElement
  {
    private static readonly MethodRunnerElement Instance = new MethodRunnerElement();

    private MethodRunnerElement()
    {
    }

    public static MethodRunnerElement GetInstance(MethodRunnerProvider provider, string className, string methodName, bool isClassStatic, bool isMethodStatic)
    {
      Instance.myClassName = className;
      Instance.myMethodName = methodName;
      Instance.myIsClassStatic = isClassStatic;
      Instance.myIsMethodStatic = isMethodStatic;
      Instance.myProvider = provider;

      return Instance;
    }

    private string myClassName;
    private string myMethodName;
    private bool myIsClassStatic;
    private bool myIsMethodStatic;
    private IUnitTestProvider myProvider;

    public IProject GetProject()
    {
      throw new System.NotImplementedException();
    }

    public string GetPresentation(IUnitTestElement parent = null)
    {
      throw new System.NotImplementedException();
    }

    public UnitTestNamespace GetNamespace()
    {
      throw new System.NotImplementedException();
    }

    public UnitTestElementDisposition GetDisposition()
    {
      throw new System.NotImplementedException();
    }

    public IDeclaredElement GetDeclaredElement()
    {
      throw new System.NotImplementedException();
    }

    public IEnumerable<IProjectFile> GetProjectFiles()
    {
      throw new System.NotImplementedException();
    }

    public IUnitTestRunStrategy GetRunStrategy(IHostProvider hostProvider)
    {
      throw new System.NotImplementedException();
    }

    public IList<UnitTestTask> GetTaskSequence(ICollection<IUnitTestElement> explicitElements, IUnitTestLaunch launch)
    {
      return new List<UnitTestTask> { new UnitTestTask(this, new RunMethodTask(MethodRunnerProvider.Id, myClassName, myMethodName, myIsClassStatic, myIsMethodStatic)) };
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
      return ReferenceEquals(other, this);
    }
  }
}