using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodingStandardCodeAnalyzers {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class TimeMeasurementCodeAnalyzer : DiagnosticAnalyzer {
        private static readonly LocalizableString Title = "Use Stopwatch to measure elapsed time.";
        private static readonly LocalizableString MessageFormat = "Use Stopwatch to measure elapsed time instead of '{0}()'.";
        private static readonly LocalizableString Description = "";
        private static readonly string Category = AnalyzerDiagnosticCategories.CodingPractices;
        private static readonly string HelpLink = "";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(AnalyzerDiagnosticIds.TimeMeasurementCodeAnalyzer.ToDiagnosticsId(),
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description,
            helpLinkUri: HelpLink);

        public static string DiagnosticId => Rule.Id;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            context.RegisterSyntaxNodeAction(AnalyzeTimeMeasurement, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeTimeMeasurement(SyntaxNodeAnalysisContext context) {
            if (context.IsGeneratedOrNonUserCode()) { return; }
            var methodDeclaration = context.Node as MethodDeclarationSyntax;
            if (methodDeclaration == null || context.SemanticModel == null) { return; }
            IEnumerable<SyntaxNode> descendantNodes = methodDeclaration.Body.DescendantNodes();

            //Get all nodes where a methid that return current time is invoked
            var getTimeNodes = descendantNodes
                .Where(statement => /*statement.Kind() == SyntaxKind.SimpleMemberAccessExpression ||*/ statement.Kind() == SyntaxKind.IdentifierName)
                //   .Cast<MemberAccessExpressionSyntax>()
                .Where(statement => IsTimeGetterStatement(statement, context)).ToArray();

            //if a method has less than 2 get current time expression - the time measurement not applicable
            //if a method has more than 2 get current time expression - the analizer need to handle this, but this is a bit more complicated. Room for improvement....
            if (getTimeNodes.Length != 2) { return; }

            var firstGetTimeNode = getTimeNodes[0];
            var secondGetTimeNode = getTimeNodes[1];

            //both expressions have to call one method to get current time e.g. both should call DateTimeOffset.UtcNow()
            if (firstGetTimeNode.ToString() != secondGetTimeNode.ToString()) { return; }

            //fisrt expresssion must be assignment of variable
            var equalsClauseNode = firstGetTimeNode.Parent.Kind() == SyntaxKind.SimpleMemberAccessExpression ? firstGetTimeNode.Parent.Parent : firstGetTimeNode.Parent;
            var variableDeclarator = (equalsClauseNode as EqualsValueClauseSyntax)?.Parent as VariableDeclaratorSyntax;
            if (variableDeclarator == null) { return; }
            SyntaxToken variable = variableDeclarator.Identifier;

            //second expression have to be subtract expression and use:
            //  1)firstGetTimeNode as left expression 
            //  2) variableDeclarator from the first exression as right expression.
            var binaryExpressionNode = secondGetTimeNode.Parent.Kind() == SyntaxKind.SimpleMemberAccessExpression ? secondGetTimeNode.Parent.Parent : secondGetTimeNode.Parent;
            var expressionSyntax = binaryExpressionNode as BinaryExpressionSyntax;
            if (expressionSyntax == null || expressionSyntax.Kind() != SyntaxKind.SubtractExpression) { return; }
            if (!AreSameSemantically(expressionSyntax.Left, firstGetTimeNode, context)) { return; }

            ExpressionSyntax right = expressionSyntax.Right;
            if (right.ToString() != variable.ValueText) { return; }

            Diagnostic diagnostic = Diagnostic.Create(Rule, expressionSyntax.GetLocation(), context.SemanticModel.GetSymbolInfo(firstGetTimeNode).Symbol.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        private bool IsTimeGetterStatement(SyntaxNode node, SyntaxNodeAnalysisContext context) {
            SymbolInfo diagnostics = context.SemanticModel.GetSymbolInfo(node);
            ISymbol symbol = diagnostics.Symbol;
            if (symbol?.ContainingType?.ContainingNamespace == null) { return false; }
            if (symbol.ContainingType.ContainingNamespace.Name != "System") {
                return false;
            }
            string className = symbol.ContainingType.Name;
            string methodName = symbol.Name;
            if (className == "DateTime" || className == "DateTimeOffset") {
                if (methodName == "Now" || methodName == "UtcNow") {
                    return true;
                }
            }
            return false;
        }

        private bool AreSameSemantically(SyntaxNode node1, SyntaxNode node2, SyntaxNodeAnalysisContext context) {
            ISymbol symbol1 = context.SemanticModel.GetSymbolInfo(node1).Symbol;
            ISymbol symbol2 = context.SemanticModel.GetSymbolInfo(node2).Symbol;
            return symbol1 != null && symbol2 != null && symbol1.ToString() == symbol2.ToString();
        }
    }
}