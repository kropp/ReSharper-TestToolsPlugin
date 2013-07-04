using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestExplorer.Manager;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.ReSharper.UnitTestFramework.Strategy;
using MethodRunner;

namespace CreateTestPlugin
{
  [UnitTestProvider]
  public class MethodRunnerProvider : IUnitTestProvider
  {
    private readonly IUnitTestRunStrategy myStrategy = new OutOfProcessUnitTestRunStrategy(new RemoteTaskRunnerInfo(Id, typeof(RunMethodRunner)));

    public void ExploreExternal(UnitTestElementConsumer consumer)
    {
    }

    public void ExploreSolution(ISolution solution, UnitTestElementConsumer consumer)
    {
    }

    public bool IsElementOfKind(IDeclaredElement declaredElement, UnitTestElementKind elementKind)
    {
      return false;
    }

    public bool IsElementOfKind(IUnitTestElement element, UnitTestElementKind elementKind)
    {
      return false;
    }

    public bool IsSupported(IHostProvider hostProvider)
    {
      return hostProvider is ProcessHostProvider || hostProvider is DebugHostProvider;
    }

    public int CompareUnitTestElements(IUnitTestElement x, IUnitTestElement y)
    {
      return 0;
    }

    public string ID { get { return Id; } }
    public const string Id = "MethodRunner";

    public string Name { get { return Id; } }

    public IUnitTestRunStrategy Strategy
    {
      get
      {
        return myStrategy;
      }
    }

    public MethodRunnerElement CreateElement(IProject project, IClrTypeName className, string methodName, bool isClassStatic, bool isMethodStatic)
    {
      return new MethodRunnerElement(project, className, methodName, isClassStatic, isMethodStatic, this);
    }
  }
}