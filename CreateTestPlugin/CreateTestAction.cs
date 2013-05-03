using System;
using JetBrains.Application;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.ExtensionsAPI.Tree;
using JetBrains.ReSharper.Psi.Impl.Search;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace CreateTestPlugin
{
  /// <summary>
  /// This is an example context action. The test project demonstrates tests for
  /// availability and execution of this action.
  /// </summary>
  [ContextAction(Name = "CreateTest", Description = "Creates a test", Group = "C#")]
  public class CreateTestAction : ContextActionBase
  {
    private readonly ICSharpContextActionDataProvider myProvider;
    private IClassDeclaration myClassDeclaration;

    public CreateTestAction(ICSharpContextActionDataProvider provider)
    {
      myProvider = provider;
    }

    public override bool IsAvailable(IUserDataHolder cache)
    {
      myClassDeclaration = myProvider.GetSelectedElement<IClassDeclaration>(true, true);
      return myClassDeclaration != null;
    }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var factory = CSharpElementFactory.GetInstance(myProvider.PsiModule);

      var testClassName = myClassDeclaration.DeclaredName + "Test";
      
      var file = factory.CreateFile("public class " + testClassName + " {}");
      var testClass = file.TypeDeclarations.First() as IClassDeclaration;

      if (testClass == null)
        return null;

      var nunitFixtureType = TypeFactory.CreateTypeByCLRName("NUnit.Framework.TestFixtureAttribute", myProvider.PsiModule, myClassDeclaration.GetProject().GetResolveContext());
      var attribute = factory.CreateAttribute(nunitFixtureType.GetTypeElement());
      testClass.AddAttributeBefore(attribute, null);

      var nunitTestType = TypeFactory.CreateTypeByCLRName("NUnit.Framework.TestAttribute", myProvider.PsiModule, myClassDeclaration.GetProject().GetResolveContext()).GetTypeElement();
      foreach (var methodDeclaration in myClassDeclaration.MethodDeclarations)
      {
        var testMethod = factory.CreateTypeMemberDeclaration("public void Test" + methodDeclaration.DeclaredName + "(){}") as IClassMemberDeclaration;
        if (testMethod == null)
          continue;

        testMethod.AddAttributeBefore(factory.CreateAttribute(nunitTestType), null);
        testClass.AddClassMemberDeclaration(testMethod);
      }

      using (WriteLockCookie.Create())
        ModificationUtil.AddChildAfter(myClassDeclaration, testClass);

      return null;
    }

    public override string Text
    {
      get { return "Create a test"; }
    }
  }
}
