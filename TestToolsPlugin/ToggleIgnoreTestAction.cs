using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using DataConstants = JetBrains.ProjectModel.DataContext.DataConstants;

namespace CreateTestPlugin
{
  [ActionHandler("TestToolsPlugin.ToggleIgnoreTest")]
  public class ToggleIgnoreTestAction : IActionHandler
  {
    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      // action is in context menu, so it is updated only once when menu is shown
      var solution = context.GetData(DataConstants.SOLUTION);
      var elements = context.GetData(UnitTestDataConstants.UNIT_TEST_ELEMENTS);
      if (solution == null || elements == null || elements.ExplicitElements.IsEmpty())
      {
        presentation.Visible = false;
        return false;
      }

      var element = elements.ExplicitElements.First();
      var result = solution.GetComponent<IUnitTestResultManager>().GetResult(element);

      // show correct action depending on test status
      switch (result.Status)
      {
        case UnitTestStatus.Success:
        case UnitTestStatus.Ignored:
          if (element.Explicit)
          {
            presentation.Text = "Unignore test";
            presentation.Visible = true;
          }
          else
            presentation.Visible = false;
          break;
        case UnitTestStatus.Failed:
        case UnitTestStatus.Aborted:
          if (!element.Explicit)
          {
            presentation.Text = "Ignore test";
            presentation.Visible = true;
          }
          else
            presentation.Visible = false;
          break;
        default:
          presentation.Visible = false;
          return false;
      }

      return true;
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
      var solution = context.GetData(DataConstants.SOLUTION);
      if (solution == null)
        return;

      var elements = context.GetData(UnitTestDataConstants.UNIT_TEST_ELEMENTS);
      if (elements == null || elements.ExplicitElements.IsEmpty())
        return;

      solution.GetComponent<IPsiServices>().Transactions.Execute("Toggle Ignore Test", () =>
      {
        foreach (var element in elements.ExplicitElements)
          element.ToggleIgnore(solution);
      });
    }
  }
}