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
using System.Linq;
using JetBrains.ActionManagement;
using JetBrains.Application.DataContext;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.UnitTestExplorer;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;
using DataConstants = JetBrains.ProjectModel.DataContext.DataConstants;

namespace JetBrains.ReSharper.Plugins.TestTools
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