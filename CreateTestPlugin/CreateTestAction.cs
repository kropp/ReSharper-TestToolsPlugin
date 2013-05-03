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
using JetBrains.ReSharper.Psi.Modules;
using JetBrains.ReSharper.Psi.Services;
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
        var testMethod = factory.CreateTypeMemberDeclaration("public void Test" + methodDeclaration.DeclaredName + "(){}") as IMethodDeclaration;
        if (testMethod == null)
          continue;

        var declaredMethod = methodDeclaration.DeclaredElement;
        if (declaredMethod == null)
          continue;

        GenerateTestMethodBody(factory, methodDeclaration.GetPsiModule(), testMethod, declaredMethod);

        testMethod.AddAttributeBefore(factory.CreateAttribute(nunitTestType), null);
        testClass.AddClassMemberDeclaration(testMethod);
      }

      // finally add newly created test class to file
      using (WriteLockCookie.Create())
        ModificationUtil.AddChildAfter(myClassDeclaration, testClass);

      return null;
    }

    /// <summary>
    /// Generates Arrange/Act/Assert stub for test method based on original method declaration
    /// </summary>
    /// <param name="factory"></param>
    /// <param name="testMethod"></param>
    /// <param name="originalMethod"></param>
    /// <param name="psiModule"></param>
    private void GenerateTestMethodBody(CSharpElementFactory factory, IPsiModule psiModule, IMethodDeclaration testMethod, IMethod originalMethod)
    {
      ICSharpStatement anchorStatement = null;
      
      // Arrange
      foreach (var parameterDeclaration in originalMethod.Parameters)
      {
        var type = parameterDeclaration.Type;
        var stmt = factory.CreateStatement("$0 " + parameterDeclaration.ShortName + " = $1;", type, DefaultValueUtil.GetDefaultValue(type, testMethod.Language, psiModule));
        anchorStatement = testMethod.Body.AddStatementAfter(stmt, anchorStatement);
      }

      // Act
      var invocation = !originalMethod.ReturnType.IsVoid()
        ? factory.CreateStatement("$0 result = " + originalMethod.ShortName + "();", originalMethod.ReturnType)
        : factory.CreateStatement(originalMethod.ShortName + "();");
      anchorStatement = testMethod.Body.AddStatementAfter(invocation, anchorStatement);

      // Assert
      if (!originalMethod.ReturnType.IsVoid())
      {
        var stmt = factory.CreateStatement("$0 expected = $1;", originalMethod.ReturnType, DefaultValueUtil.GetDefaultValue(originalMethod.ReturnType, testMethod.Language, psiModule));
        anchorStatement = testMethod.Body.AddStatementAfter(stmt, anchorStatement);

        stmt = factory.CreateStatement("Assert.AreEqual(expected, result);");
        anchorStatement = testMethod.Body.AddStatementAfter(stmt, anchorStatement);
      }
    }

    public override string Text
    {
      get { return "Create a test"; }
    }
  }
}
