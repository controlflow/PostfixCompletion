﻿using System;
using JetBrains.Annotations;
using JetBrains.DocumentModel;
using JetBrains.ReSharper.Feature.Services.CodeCompletion.Infrastructure;
using JetBrains.ReSharper.Psi;
using JetBrains.ReSharper.Psi.CSharp.Parsing;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.CSharp.Util;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.ReSharper.Psi.Web.CodeBehindSupport;
using JetBrains.Util;

namespace JetBrains.ReSharper.PostfixTemplates
{
  public static class CommonUtils
  {
    public static PredefinedType GetPredefinedType([NotNull] this ITreeNode node)
    {
      return node.GetPsiModule().GetPredefinedType(node.GetResolveContext());
    }

    public static void CommitAllDocuments([NotNull] this IPsiServices services)
    {
      services.Files.CommitAllDocuments();
    }

    public static void DoTransaction(
      [NotNull] this IPsiServices services, [NotNull] string commandName,
      [NotNull, InstantHandle] Action action)
    {
      services.Transactions.Execute(commandName, action);
    }

    public static T DoTransaction<T>(
      [NotNull] this IPsiServices services, [NotNull] string commandName,
      [NotNull, InstantHandle] Func<T> func)
    {
      var value = default(T);
      services.Transactions.Execute(commandName, () => value = func());
      return value;
    }

    public static bool IsForeachEnumeratorPatternType([NotNull] this ITypeElement typeElement)
    {
      return CSharpDeclaredElementUtil.IsForeachEnumeratorPatternType(typeElement);
    }

    public static DocumentRange ToDocumentRange([CanBeNull] this ReparsedCodeCompletionContext context,
                                                [NotNull] ITreeNode treeNode)
    {
      var documentRange = treeNode.GetDocumentRange();
      if (context == null) return documentRange;

      var reparsedTreeRange = treeNode.GetTreeTextRange();

      var document = documentRange.Document;
      if (document != null)
      {
        var originalDocRange = context.ToDocumentRange(reparsedTreeRange);
        return new DocumentRange(document, originalDocRange);
      }
      else
      {
        var originalGeneratedTreeRange = context.ToOriginalTreeRange(reparsedTreeRange);
        var sandBox = treeNode.GetContainingNode<ISandBox>().NotNull();
        var contextNode = sandBox.ContextNode.NotNull();

        var containingFile = contextNode.GetContainingFile().NotNull();

        var translator = containingFile.GetRangeTranslator();

        var originalTreeRange = translator.GeneratedToOriginal(originalGeneratedTreeRange);
        var originalDocRange = translator.OriginalFile.GetDocumentRange(originalTreeRange);

        return originalDocRange;
      }
    }

    public static bool CanTypeBecameExpression([CanBeNull] ICSharpExpression expression)
    {
      var referenceExpression = expression as IReferenceExpression;
      if (referenceExpression != null)
      {
        var parent = referenceExpression.Parent;

        if (IsRelationalExpressionWithTypeOperand(parent)) return false;

        var expressionStatement = parent as IExpressionStatement;
        if (expressionStatement != null)
        {
          var previous = expressionStatement.GetPreviousStatementInBlock();
          if (previous == null) return true;

          // two children: relational and error element
          return !(
            IsRelationalExpressionWithTypeOperand(previous.FirstChild) &&
            previous.FirstChild != null &&
            previous.FirstChild.NextSibling == previous.LastChild &&
            previous.LastChild is IErrorElement);
        }

        return CanTypeBecameExpression(parent as IReferenceExpression);
      }

      var predefinedType = expression as IPredefinedTypeExpression;
      if (predefinedType != null)
      {
        var parent = predefinedType.Parent;

        return CanTypeBecameExpression(parent as IReferenceExpression);
      }

      return true;
    }

    [ContractAnnotation("null => false")]
    public static bool IsRelationalExpressionWithTypeOperand([CanBeNull] ITreeNode node)
    {
      var relationalExpression = node as IRelationalExpression;
      if (relationalExpression == null) return false;

      var operatorSign = relationalExpression.OperatorSign;
      if (operatorSign != null && operatorSign.GetTokenType() == CSharpTokenType.LT)
      {
        var left = relationalExpression.LeftOperand as IReferenceExpression;
        if (left != null && left.Reference.Resolve().DeclaredElement is ITypeElement)
          return true;

        var right = relationalExpression.LeftOperand as IReferenceExpression;
        if (right != null && right.Reference.Resolve().DeclaredElement is ITypeElement)
          return true;
      }

      return false;
    }

    public static bool IsNiceExpression([NotNull] ICSharpExpression expression)
    {
      if (expression is IAssignmentExpression) return false;
      if (expression is IPrefixOperatorExpression) return false;
      if (expression is IPostfixOperatorExpression) return false;

      if (expression is IInvocationExpression)
      {
        var expressionType = expression.GetExpressionType();
        if (expressionType.ToIType().IsVoid()) return false;
      }

      return true;
    }
  }
}