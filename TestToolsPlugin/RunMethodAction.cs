using System;
using System.Linq;
using JetBrains.Application.Progress;
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Feature.Services.Bulbs;
using JetBrains.ReSharper.Feature.Services.CSharp.Bulbs;
using JetBrains.ReSharper.Intentions.Extensibility;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace CreateTestPlugin
{
  [ContextAction(Name = "RunMethod", Description = "Run method as a single test", Group = "C#")]
  public class RunMethodAction : ContextActionBase
  {
    private readonly ICSharpContextActionDataProvider myProvider;
    private IMethodDeclaration myMethodDeclaration;

    public RunMethodAction(ICSharpContextActionDataProvider provider)
    {
      myProvider = provider;
    }

    protected override Action<ITextControl> ExecutePsiTransaction(ISolution solution, IProgressIndicator progress)
    {
      return null;
    }

    public override string Text
    {
      get { return "Run method as test"; }
    }

    public override bool IsAvailable(IUserDataHolder cache)
    {
      myMethodDeclaration = myProvider.GetSelectedElement<IMethodDeclaration>(true, true);
      if (myMethodDeclaration == null)
        return false;

      // only on non-generic methods
      var method = myMethodDeclaration.DeclaredElement;
      if (method == null || method.TypeParameters.Any())
        return false;

      if (!method.IsStatic)
      {
        // only on non-generic types
        var klass = method.GetContainingType() as IClass;
        if (klass == null || klass.HasTypeParameters())
          return false;

        // with default constructor
        if (!klass.IsStatic ||
          (!klass.Constructors.IsEmpty() && !klass.Constructors.Any(c => c.Parameters.IsEmpty())))
          return false;
      }

      return !method.Parameters.Any();
    }
  }
}