using System;
using System.Text;
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

namespace JetBrains.ReSharper.Plugins.TestTools
{
  [ContextAction(Name = "CreateTests", Description = "Creates tests for given class", Group = "C#")]
  public class CreateTestsAction : ContextActionBase
  {
    private readonly ICSharpContextActionDataProvider myProvider;
    private IClassDeclaration myClassDeclaration;

    public CreateTestsAction(ICSharpContextActionDataProvider provider)
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

      if (myClassDeclaration.MethodDeclarations.Any(m => !m.IsStatic))
      {
        var fieldName = "my" + myClassDeclaration.DeclaredName + "Instance";

        var setUpMethod = factory.CreateTypeMemberDeclaration("public void SetUp(){}") as IMethodDeclaration;
        if (setUpMethod != null)
        {
          var nunitSetUpType = TypeFactory.CreateTypeByCLRName("NUnit.Framework.SetUpAttribute", myProvider.PsiModule, myClassDeclaration.GetProject().GetResolveContext());
          attribute = factory.CreateAttribute(nunitSetUpType.GetTypeElement());
          setUpMethod.AddAttributeBefore(attribute, null);

          setUpMethod.Body.AddStatementAfter(factory.CreateStatement(fieldName + " = new $0();", myClassDeclaration.DeclaredElement), null);

          testClass.AddClassMemberDeclarationBefore(setUpMethod, null);
        }

        var field = factory.CreateTypeMemberDeclaration("private $0 " + fieldName + ";", myClassDeclaration.DeclaredElement) as IFieldDeclaration;
        if (field != null)
        {
          testClass.AddClassMemberDeclarationBefore(field, null);
        }
      }

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
        testClass = ModificationUtil.AddChildAfter(myClassDeclaration, testClass);

      return control => control.Caret.MoveTo(control.Document.GetCoordsByOffset(testClass.NameIdentifier.GetDocumentStartOffset().TextRange.StartOffset), CaretVisualPlacement.DontScrollIfVisible);
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

      var paramsList = new StringBuilder();

      // Arrange
      foreach (var parameterDeclaration in originalMethod.Parameters)
      {
        var type = parameterDeclaration.Type;
        var stmt = factory.CreateStatement("$0 " + parameterDeclaration.ShortName + " = $1;", type, DefaultValueUtil.GetDefaultValue(type, testMethod.Language, psiModule));
        anchorStatement = testMethod.Body.AddStatementAfter(stmt, anchorStatement);

        if (paramsList.Length == 0) // First parameter
          ModificationUtil.AddChildBefore(anchorStatement, factory.CreateComment("// Arrange" + Environment.NewLine));
        else
          paramsList.Append(", ");
        paramsList.Append(parameterDeclaration.ShortName);
      }

      if (!originalMethod.ReturnType.IsVoid())
      {
        var stmt = factory.CreateStatement("$0 expected = $1;", originalMethod.ReturnType,
          DefaultValueUtil.GetDefaultValue(originalMethod.ReturnType, testMethod.Language, psiModule));
        anchorStatement = testMethod.Body.AddStatementAfter(stmt, anchorStatement);

        if (originalMethod.Parameters.IsEmpty())
          ModificationUtil.AddChildBefore(anchorStatement, factory.CreateComment("// Arrange"));
      }

      
      // Act
      var methodInvocation = (originalMethod.IsStatic ? "$1" : "my" + originalMethod.GetContainingType().ShortName + "Instance") + "." + originalMethod.ShortName;
      var invocationStatement = !originalMethod.ReturnType.IsVoid()
        ? factory.CreateStatement("$0 result = " + methodInvocation + "(" + paramsList + ");", originalMethod.ReturnType, originalMethod.GetContainingType())
        : factory.CreateStatement(methodInvocation + "(" + paramsList + ");", null, originalMethod.GetContainingType());
      anchorStatement = testMethod.Body.AddStatementAfter(invocationStatement, anchorStatement);

      AddCommentWithNewLineBefore(factory, anchorStatement, "// Act");


      // Assert
      if (!originalMethod.ReturnType.IsVoid())
      {
        var stmt = factory.CreateStatement("Assert.AreEqual(expected, result);");
        anchorStatement = testMethod.Body.AddStatementAfter(stmt, anchorStatement);

        AddCommentWithNewLineBefore(factory, anchorStatement, "// Assert");
      }
    }

    private static void AddCommentWithNewLineBefore(CSharpElementFactory factory, ITreeNode anchorStatement, string text)
    {
      foreach (var whitespaceNode in factory.CreateWhitespaces(Environment.NewLine))
        ModificationUtil.AddChildBefore(anchorStatement, whitespaceNode);
      ModificationUtil.AddChildBefore(anchorStatement, factory.CreateComment(text));
    }

    public override string Text
    {
      get { return "Create tests"; }
    }
  }
}
