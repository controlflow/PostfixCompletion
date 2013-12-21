﻿using JetBrains.Annotations;
using JetBrains.ReSharper.ControlFlow.PostfixCompletion.LookupItems;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Hotspots;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.LiveTemplates;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Macros;
using JetBrains.ReSharper.Feature.Services.LiveTemplates.Macros.Implementations;
using JetBrains.ReSharper.Feature.Services.Lookup;
using JetBrains.ReSharper.LiveTemplates;
using JetBrains.ReSharper.Psi.CSharp;
using JetBrains.ReSharper.Psi.CSharp.Tree;
using JetBrains.ReSharper.Psi.Tree;
using JetBrains.TextControl;
using JetBrains.Util;

namespace JetBrains.ReSharper.ControlFlow.PostfixCompletion.Templates
{
  [PostfixTemplate(
    templateName: "cast",
    description: "Surrounds expression with cast",
    example: "(SomeType) expr")]
  public class CastExpressionTemplate : IPostfixTemplate
  {
    [NotNull] private readonly LiveTemplatesManager myTemplatesManager;

    public CastExpressionTemplate([NotNull] LiveTemplatesManager templatesManager)
    {
      myTemplatesManager = templatesManager;
    }

    public ILookupItem CreateItem(PostfixTemplateContext context)
    {
      if (!context.IsForceMode) return null;

      PrefixExpressionContext bestExpression = null;
      foreach (var expression in context.Expressions.Reverse())
      {
        if (CommonUtils.IsNiceExpression(expression.Expression))
        {
          bestExpression = expression;
          break;
        }
      }

      return new CastItem(bestExpression ?? context.OuterExpression, myTemplatesManager);
    }

    private sealed class CastItem : ExpressionPostfixLookupItem<ICastExpression>
    {
      [NotNull] private readonly LiveTemplatesManager myTemplatesManager;

      public CastItem(
        [NotNull] PrefixExpressionContext context,
        [NotNull] LiveTemplatesManager templatesManager) : base("cast", context)
      {
        myTemplatesManager = templatesManager;
      }

      protected override ICastExpression CreateExpression(
        CSharpElementFactory factory, ICSharpExpression expression)
      {
        return (ICastExpression) factory.CreateExpression("(T) $0", expression);
      }

      protected override void AfterComplete(ITextControl textControl, ICastExpression expression)
      {
        var typeExpression = new MacroCallExpressionNew(new GuessExpectedTypeMacroDef());
        var hotspotInfo = new HotspotInfo(
          new TemplateField("T", typeExpression, 0),
          expression.TargetType.GetDocumentRange().GetHotspotRange());

        var endSelectionRange = expression.GetDocumentRange().EndOffsetRange().TextRange;
        var session = myTemplatesManager.CreateHotspotSessionAtopExistingText(
          expression.GetSolution(), endSelectionRange, textControl,
          LiveTemplatesManager.EscapeAction.LeaveTextAndCaret, new[] {hotspotInfo});

        session.Execute();
      }
    }
  }
}