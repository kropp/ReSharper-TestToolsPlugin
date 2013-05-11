using System.Windows.Forms;
using JetBrains.ActionManagement;
using JetBrains.Application;
using JetBrains.Application.DataContext;
using JetBrains.Application.Interop.NativeHook;
using JetBrains.CommonControls.Validation;
using JetBrains.IDE.TreeBrowser;
using JetBrains.Interop.WinApi;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.UI.Application;
using JetBrains.UI.CommonDialogs;
using JetBrains.Util;
using DataConstants = JetBrains.ProjectModel.DataContext.DataConstants;
using MessageBox = JetBrains.Util.MessageBox;

namespace CreateTestPlugin
{
  [ActionHandler("TestToolsPlugin.TagTest")]
  public class TagTestAction : IActionHandler
  {
    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      // for performance reasons we don't do lots of checks here, because Update is called very often
      return context.GetData(TreeModelBrowser.TREE_MODEL_DESCRIPTOR) is IUnitTestSessionView;
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
      var solution = context.GetData(DataConstants.SOLUTION);
      if (solution == null)
        return;

      var elements = context.GetData(UnitTestDataConstants.UNIT_TEST_ELEMENTS);
      if (elements == null || elements.ExplicitElements.IsEmpty())
        return;

      var prompt = new PromptWinForm(solution.GetComponent<IWin32Window>(), "Tag test", "Enter category name",
        string.Empty, s => null, solution.GetComponent<IWindowsHookManager>(), solution.GetComponent<FormValidators>(),
        solution.GetComponent<IUIApplication>());
      if (prompt.ShowDialog())
      {
        var psiServices = solution.GetComponent<IPsiServices>();
        psiServices.Transactions.Execute("Tag test", () =>
        {
          foreach (var element in elements.ExplicitElements)
            element.MarkWithCategory(prompt.Value);
        });
      }
    }
  }
}