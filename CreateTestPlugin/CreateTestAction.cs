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
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace CreateTestPlugin
{
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
      // this context action is available on every class at the moment
      // TODO: add some constraints, like: there is already a test for this class
      return myClassDeclaration != null;
    }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      var factory = CSharpElementFactory.GetInstance(myProvider.PsiModule);

      // create a test fixture class with name of original class plus Test

      var testClassName = myClassDeclaration.DeclaredName + "Test";
      
      var file = factory.CreateFile("public class " + testClassName + " {}");
      var testClass = file.TypeDeclarations.First() as IClassDeclaration;

      if (testClass == null)
        return null;

      // resolve TestFixture attribute from NUnit (assembly/NuGet reference should be added beforehand)
      // this way all necessary usings will be added automatically

      var nunitFixtureType = TypeFactory.CreateTypeByCLRName("NUnit.Framework.TestFixtureAttribute", myProvider.PsiModule, myClassDeclaration.GetProject().GetResolveContext());
      var attribute = factory.CreateAttribute(nunitFixtureType.GetTypeElement());
      testClass.AddAttributeBefore(attribute, null);

      var nunitTestType = TypeFactory.CreateTypeByCLRName("NUnit.Framework.TestAttribute", myProvider.PsiModule, myClassDeclaration.GetProject().GetResolveContext()).GetTypeElement();

      // create test method for each method defined in original class
      foreach (var methodDeclaration in myClassDeclaration.MethodDeclarations)
      {
        var testMethod = factory.CreateTypeMemberDeclaration("public void Test" + methodDeclaration.DeclaredName + "(){}") as IClassMemberDeclaration;
        if (testMethod == null)
          continue;

        testMethod.AddAttributeBefore(factory.CreateAttribute(nunitTestType), null);
        testClass.AddClassMemberDeclaration(testMethod);
      }

      // finally add newly created test class to file
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
