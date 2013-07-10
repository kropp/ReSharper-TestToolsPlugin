using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.Application.Interop.NativeHook;
using JetBrains.DataFlow;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Features.Common.UI;
using JetBrains.ReSharper.Features.Shared.UnitTesting;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.ReSharper.UnitTestFramework.Resources;
using JetBrains.Text;
using JetBrains.UI.Application;
using JetBrains.UI.GotoByName;
using JetBrains.UI.PopupMenu;
using JetBrains.UI.PopupMenu.Impl;
using JetBrains.UI.PopupWindowManager;
using JetBrains.UI.RichText;
using JetBrains.UI.Tooltips;
using JetBrains.Util;
using DataConstants = JetBrains.ProjectModel.DataContext.DataConstants;

namespace JetBrains.ReSharper.Plugins.TestTools
{
  [ActionHandler("TestToolsPlugin.TagTest")]
  public class TagTestAction : IActionHandler
  {
    public bool Update(IDataContext context, ActionPresentation presentation, DelegateUpdate nextUpdate)
    {
      // action is in context menu, so it is updated only once when menu is shown
      var elements = context.GetData(UnitTestDataConstants.UNIT_TEST_ELEMENTS);
      return elements != null && elements.ExplicitElements.Any();
    }

    public void Execute(IDataContext context, DelegateExecute nextExecute)
    {
      var solution = context.GetData(DataConstants.SOLUTION);
      if (solution == null)
        return;

      var elements = context.GetData(UnitTestDataConstants.UNIT_TEST_ELEMENTS);
      if (elements == null || elements.ExplicitElements.IsEmpty())
        return;

      // ask user to enter category name, provide nice completition of existing categories

      var categoriesProvider = solution.GetComponent<IUnitTestingCategoriesProvider>();

      var ltd = Lifetimes.Define(solution.GetLifetime());

      var completionPicker = new CompletionPickerPopupDialog(solution.GetComponent<IUIApplication>(), solution.GetComponent<ITooltipManager>(), solution.GetComponent<IMainWindow>(), solution.GetComponent<IWindowsHookManager>(), solution.GetComponent<MainWindowPopupWindowContext>(), solution.GetComponent<PopupWindowManager>(), solution)
      {
        LabelText = new RichTextBlock("Enter category name"),
        Text = "Tag tests"
      };
      var settings = new CompletionPickerSettings();
      var completion = new GotoByNameModel(ltd.Lifetime);
      completion.FilterText.Change.Advise(ltd.Lifetime, args =>
      {
        completion.Items.Clear();
        if (!args.HasNew)
          return;
        completion.Items.AddRange(SuggestCategories(categoriesProvider, args.New).Select(str => new JetPopupMenuItem(str, new SimpleMenuItem(str, UnitTestingThemedIcons.Category.Id, null))));
      });
      settings.CompletionModel.Value = completion;
      completionPicker.Settings.Value = settings;

      if (completionPicker.ShowDialog() == DialogResult.OK)
      {
        // finally mark explicitly selected tests with chosen category
        var psiServices = solution.GetComponent<IPsiServices>();
        psiServices.Transactions.Execute("Tag test", () =>
        {
          foreach (var element in elements.ExplicitElements)
            element.MarkWithCategory(completionPicker.TypeChooserText.Value);
        });
      }

      ltd.Terminate();
    }

    private static IEnumerable<string> SuggestCategories(IUnitTestingCategoriesProvider categoriesProvider, string filter)
    {
      // using camel humps matcher that is used everywhere in ReSharper
      var matcher = new IdentifierMatcher(filter);
      return categoriesProvider.Categories.Where(matcher.Matches);
    }
  }
}