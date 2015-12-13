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

            //Get all nodes where a method that return current time is invoked
            var getTimeNodes = GetNodesUsedToGetCurrentTime(context.SemanticModel, methodDeclaration);

            //if a method has less than 2 get current time expression - the time measurement not applicable
            //if a method has more than 2 get current time expression - the analyzer need to handle this, but this is a bit more complicated. Room for improvement....
            if (getTimeNodes.Length != 2) { return; }

            SyntaxNode firstGetTimeNode = getTimeNodes[0];
            SyntaxNode secondGetTimeNode = getTimeNodes[1];

            //both expressions have to call one method to get current time e.g. both should call DateTimeOffset.UtcNow()
            if (firstGetTimeNode.ToString() != secondGetTimeNode.ToString()) { return; }

            //first expression must be assignment of variable
            SyntaxNode equalsClauseNode = firstGetTimeNode.Parent.Kind() == SyntaxKind.SimpleMemberAccessExpression ? firstGetTimeNode.Parent.Parent : firstGetTimeNode.Parent;
            var variableDeclarator = (equalsClauseNode as EqualsValueClauseSyntax)?.Parent as VariableDeclaratorSyntax;
            if (variableDeclarator == null) { return; }
            SyntaxToken variable = variableDeclarator.Identifier;

            //second expression have to be subtract expression and use:
            //  1) firstGetTimeNode as left expression 
            //  2) variableDeclarator from the first expression as right expression.
            SyntaxNode binaryExpressionNode = secondGetTimeNode.Parent.Kind() == SyntaxKind.SimpleMemberAccessExpression ? secondGetTimeNode.Parent.Parent : secondGetTimeNode.Parent;
            var expressionSyntax = binaryExpressionNode as BinaryExpressionSyntax;
            if (expressionSyntax == null || expressionSyntax.Kind() != SyntaxKind.SubtractExpression) { return; }
            if (!AreSameSemantically(expressionSyntax.Left, firstGetTimeNode, context)) { return; }

            if (expressionSyntax.Right.ToString() != variable.ValueText) { return; }

            Diagnostic diagnostic = Diagnostic.Create(Rule, expressionSyntax.GetLocation(), context.SemanticModel.GetSymbolInfo(firstGetTimeNode).Symbol.ToString());
            context.ReportDiagnostic(diagnostic);
        }

        internal static SyntaxNode[] GetNodesUsedToGetCurrentTime(SemanticModel semanticModel, MethodDeclarationSyntax methodDeclaration) {
            return methodDeclaration.Body.DescendantNodes()
                .Where(statement => statement.Kind() == SyntaxKind.IdentifierName)
                .Where(statement => IsTimeGetterStatement(statement, semanticModel)).ToArray();
        }

        private static bool IsTimeGetterStatement(SyntaxNode node, SemanticModel semanticModel) {
            var methodsToSearch = new[] {
                new SearchMethodInfo("System", "DateTime", "Now"),
                new SearchMethodInfo("System", "DateTime", "UtcNow"),
                new SearchMethodInfo("System", "DateTimeOffset", "Now"),
                new SearchMethodInfo("System", "DateTimeOffset", "UtcNow")
            };
            return node.CheckNodeIsMemberOfType(semanticModel, methodsToSearch) != null;
        }

        private bool AreSameSemantically(SyntaxNode node1, SyntaxNode node2, SyntaxNodeAnalysisContext context) {
            ISymbol symbol1 = context.SemanticModel.GetSymbolInfo(node1).Symbol;
            ISymbol symbol2 = context.SemanticModel.GetSymbolInfo(node2).Symbol;
            return symbol1 != null && symbol2 != null && symbol1.ToString() == symbol2.ToString();
        }
    }
}