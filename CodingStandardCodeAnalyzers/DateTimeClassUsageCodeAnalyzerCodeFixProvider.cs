using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace CodingStandardCodeAnalyzers {
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(DateTimeClassUsageCodeAnalyzer))]
    [Shared]
    public class DateTimeClassUsageCodeAnalyzerCodeFixProvider : CodeFixProvider {
        private const string CodeFixTitleForReplaceWithDateTimeOffsetMember = "Replace with equivalent DateTimeOffset member";
        private const string CodeFixTitleForReplaceWithNodeTime = "Replace with equivalent NodaTime member";
        private const string CodeFixTitleForUseStopwatch = "Use Stopwatch to measure elapsed time";

        public string DiagnosticId => DateTimeClassUsageCodeAnalyzer.DiagnosticId;

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            (await context.RegisterCodeFixAsync<IdentifierNameSyntax>(CodeFixTitleForReplaceWithDateTimeOffsetMember, ReplaceWrongDateTimeUsageWithDateTimeOffset))
                .RegisterAdditionalCodeFixes(CodeFixTitleForReplaceWithNodeTime, ReplaceWrongDateTimeUsageWithNodaTimeEquivalent)
                .RegisterAdditionalCodeFixes(CodeFixTitleForUseStopwatch, ReplaceWrongDateTimeUsageWithStopwatch);
        }

        private async Task<Document> ReplaceWrongDateTimeUsageWithDateTimeOffset(Document document, IdentifierNameSyntax node, CancellationToken cancellationToken) {
            return await ApplyReplacement(document, node, cancellationToken, "DateTimeOffset." + node.ToString(), "System");
        }

        private async Task<Document> ApplyReplacement(Document document, IdentifierNameSyntax node, CancellationToken cancellationToken, string replaceWithName, string namespaceUsed) {
            IdentifierNameSyntax newNode = SyntaxFactory.IdentifierName(replaceWithName)
                .WithLeadingTrivia(node.GetLeadingTrivia())
                .WithTrailingTrivia(node.GetTrailingTrivia());
            var oldNode = node.Parent.Kind() == SyntaxKind.SimpleMemberAccessExpression ? node.Parent : node;

            document = await this.ReplaceNodeInDocumentAsync(document, cancellationToken, oldNode, newNode);
            return await this.CheckNamespaceUsageAsync(document, cancellationToken, namespaceUsed);
        }

        private async Task<Document> ReplaceWrongDateTimeUsageWithNodaTimeEquivalent(Document document, IdentifierNameSyntax node, CancellationToken cancellationToken) {
            string replacement = node.ToString() == "Today" ? "SystemClock.Instance.Now.InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()).Date"
                                                            : "SystemClock.Instance.Now";
            return await ApplyReplacement(document, node, cancellationToken, replacement, "NodaTime");
        }

        private async Task<Document> ReplaceWrongDateTimeUsageWithStopwatch(Document document, IdentifierNameSyntax node, CancellationToken cancellationToken) {
            var nodes = new List<Tuple<SyntaxNode, SyntaxNode>>();
            return await this.ReplaceNodesInDocumentAsync(document, cancellationToken, nodes.ToArray());
        }
    }
}