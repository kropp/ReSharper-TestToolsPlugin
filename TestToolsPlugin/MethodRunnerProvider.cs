using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestFramework;

namespace CreateTestPlugin
{
  [UnitTestProvider]
  public class MethodRunnerProvider : IUnitTestProvider
  {
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
      return hostProvider is ProcessHostProvider;
    }

    public int CompareUnitTestElements(IUnitTestElement x, IUnitTestElement y)
    {
      return 0;
    }

    public string ID { get { return Id; } }
    public const string Id = "MethodRunner";

    public string Name { get { return Id; } }
  }
}