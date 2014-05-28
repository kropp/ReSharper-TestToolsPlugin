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
using JetBrains.ProjectModel;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.TaskRunnerFramework;
using JetBrains.ReSharper.UnitTestFramework;
using JetBrains.Util;

namespace ReSharper.Plugins.TestTools.Util
{
  public static class UnitTestElementEx
  {
    public static void MarkWithCategory(this IUnitTestElement element, string category)
    {
      var attributesOwner = element.GetDeclaredElement() as IAttributesOwner;
      if (attributesOwner == null)
        return;
      
      var declaration = attributesOwner.GetDeclarations().FirstOrDefault() as IAttributesOwnerDeclaration;
      if (declaration == null)
        return;

      var resolveContext = declaration.GetProject().GetResolveContext();
      var psiModule = attributesOwner.Module;

      // create attribute instance
      var nunitCategoryType = TypeFactory.CreateTypeByCLRName("NUnit.Framework.CategoryAttribute", psiModule, resolveContext).GetTypeElement();
      var attribute = CSharpElementFactory.GetInstance(declaration).CreateAttribute(nunitCategoryType, new[] {new AttributeValue(new ConstantValue(category, psiModule, resolveContext))}, new Pair<string, AttributeValue>[0]);

      // and add it to existing declaration
      declaration.AddAttributeBefore(attribute, null);
    }

    public static void ToggleIgnore(this IUnitTestElement element, ISolution solution)
    {
      var resultManager = solution.GetComponent<IUnitTestResultManager>();
      var result = resultManager.GetResult(element);
      if (element.Explicit)
      {
        if (result.Status == UnitTestStatus.Ignored || result.Status == UnitTestStatus.Success)
          element.UnIgnore();
      }
      else
      {
        if (result.Status == UnitTestStatus.Failed || result.Status == UnitTestStatus.Aborted)
        {
          element.Ignore();
          resultManager.TestFinishing(element, null, string.Empty, TaskResult.Skipped);
        }
      }
    }

    public static void Ignore(this IUnitTestElement element)
    {
      var attributesOwner = element.GetDeclaredElement() as IAttributesOwner;
      if (attributesOwner == null)
        return;

      var declaration = attributesOwner.GetDeclarations().FirstOrDefault() as IAttributesOwnerDeclaration;
      if (declaration == null)
        return;

      var resolveContext = declaration.GetProject().GetResolveContext();
      var psiModule = attributesOwner.Module;

      // create attribute instance
      var nunitIgnoreType = TypeFactory.CreateTypeByCLRName("NUnit.Framework.IgnoreAttribute", psiModule, resolveContext).GetTypeElement();
      var attribute = CSharpElementFactory.GetInstance(declaration).CreateAttribute(nunitIgnoreType);

      // and add it to existing declaration
      declaration.AddAttributeBefore(attribute, null);
    }

    public static void UnIgnore(this IUnitTestElement element)
    {
      var attributesOwner = element.GetDeclaredElement() as IAttributesOwner;
      if (attributesOwner == null)
        return;

      var psiModule = attributesOwner.Module;
      var resolveContext = attributesOwner.ResolveContext;

      var nunitIgnoreType = TypeFactory.CreateTypeByCLRName("NUnit.Framework.IgnoreAttribute", psiModule, resolveContext).GetTypeElement();
      var nunitExplicitType = TypeFactory.CreateTypeByCLRName("NUnit.Framework.ExplicitAttribute", psiModule, resolveContext).GetTypeElement();

      // need to check all declarations and remove all ignore/explicit attributes from all parts
      foreach (var declaration in attributesOwner.GetDeclarations())
      {
        var attributesOwnerDeclaration = declaration as IAttributesOwnerDeclaration;
        if (attributesOwnerDeclaration != null)
          foreach (var a in attributesOwnerDeclaration.Attributes)
          {
            if (a.TypeReference == null)
              continue;
            var type = a.TypeReference.Resolve().DeclaredElement as ITypeElement;
            if (Equals(type, nunitIgnoreType) || Equals(type, nunitExplicitType))
              attributesOwnerDeclaration.RemoveAttribute(a);
          }
      }
    }
  }
}