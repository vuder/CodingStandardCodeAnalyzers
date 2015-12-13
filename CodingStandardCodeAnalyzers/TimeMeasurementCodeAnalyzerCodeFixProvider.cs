using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodingStandardCodeAnalyzers {
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(TimeMeasurementCodeAnalyzer))]
    [Shared]
    public class TimeMeasurementCodeAnalyzerCodeFixProvider : CodeFixProvider {
        private const string Title = "Use Stopwatch to measure elapsed time";

        public string DiagnosticId => TimeMeasurementCodeAnalyzer.DiagnosticId;

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            await context.RegisterCodeFixAsync<IdentifierNameSyntax>(Title, InsertStopwatchToMeasureTime);
        }

        private async Task<Document> InsertStopwatchToMeasureTime(Document document, IdentifierNameSyntax node, CancellationToken cancellationToken) {
            SemanticModel semanticModel = await document.GetSemanticModelAsync(cancellationToken);
            var nodes = new List<Tuple<SyntaxNode, SyntaxNode>>();

            SyntaxNode nodeLine = GetRootNodeOfLine(node);
            SyntaxNode newNode = CommentLine(nodeLine);
            newNode = newNode.WithTrailingTrivia(newNode.GetTrailingTrivia()
                .AddRange(nodeLine.GetLeadingTrivia())
                .Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "timer.Stop();"))
                .Add(SyntaxFactory.EndOfLine(Environment.NewLine))
                .AddRange(nodeLine.GetLeadingTrivia())
                .Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "TimeSpan elapsed = timer.Elapsed;"))
                .Add(SyntaxFactory.EndOfLine(Environment.NewLine)));

            nodes.Add(new Tuple<SyntaxNode, SyntaxNode>(nodeLine, newNode));

            var methodDeclaration = node.FirstAncestorOfType(typeof(MethodDeclarationSyntax)) as MethodDeclarationSyntax;
            if (semanticModel == null) { return document; }
            SyntaxNode firstGetTime = TimeMeasurementCodeAnalyzer.GetNodesUsedToGetCurrentTime(semanticModel, methodDeclaration).First();
            nodeLine = GetRootNodeOfLine(firstGetTime);
            newNode = CommentLine(nodeLine);
            newNode = newNode.WithTrailingTrivia(newNode.GetTrailingTrivia()
                .AddRange(nodeLine.GetLeadingTrivia())
                .Add(SyntaxFactory.SyntaxTrivia(SyntaxKind.SingleLineCommentTrivia, "var timer = Stopwatch.StartNew();"))
                .Add(SyntaxFactory.EndOfLine(Environment.NewLine)));

            nodes.Add(new Tuple<SyntaxNode, SyntaxNode>(nodeLine, newNode));

            document = await this.ReplaceNodesInDocumentAsync(document, cancellationToken, nodes.ToArray());
            return await this.CheckNamespaceUsageAsync(document, cancellationToken, "System.Diagnostics");
        }

        private static SyntaxNode CommentLine(SyntaxNode nodeLine) {
            return nodeLine.WithLeadingTrivia(nodeLine.GetLeadingTrivia().Add(SyntaxFactory.Comment("//")));
        }

        private static SyntaxNode GetRootNodeOfLine(SyntaxNode node) {
            int nodeLocationLine = node.GetLocation().GetLineSpan().StartLinePosition.Line;
            var ancestors = node.Ancestors();
            var nodeLocationCharacter = ancestors.Where(ancestor => GetSpanLocation(ancestor).Start.Line == nodeLocationLine)
                .Min(ancestor => GetSpanLocation(ancestor).Start.Character);
            return ancestors.Last(ancestor => GetSpanLocation(ancestor).Start == new LinePosition(nodeLocationLine, nodeLocationCharacter));
        }

        private static LinePositionSpan GetSpanLocation(SyntaxNode ancestor) {
            return ancestor.GetLocation().GetLineSpan().Span;
        }
    }
}