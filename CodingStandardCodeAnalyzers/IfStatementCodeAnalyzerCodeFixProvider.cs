using System;
using System.Composition;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace CodingStandardCodeAnalyzers {
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(IfStatementCodeAnalyzerCodeFixProvider)), Shared]
    public class IfStatementCodeAnalyzerCodeFixProvider : CodeFixProvider {
        private const string CodeFixTitle = "Enclose clause in { }";

        public string DiagnosticId => IfStatementCodeAnalyzer.DiagnosticId;

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            Diagnostic diagnostic = context.Diagnostics.First();
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            var statement = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<IfStatementSyntax>().First();
            context.RegisterCodeFix(
                 CodeAction.Create(
                     title: CodeFixTitle,
                     createChangedDocument: cancellationToken => MakeBlockAsync(context.Document, statement, cancellationToken),
                     equivalenceKey: CodeFixTitle),
                 diagnostic);
        }

        private async Task<Document> MakeBlockAsync(Document document, IfStatementSyntax ifStatement, CancellationToken cancellationToken) {
            var nodes = new List<Tuple<SyntaxNode, SyntaxNode>>();
            StatementSyntax thenClause = ifStatement.Statement;
            if (thenClause != null && !(thenClause is BlockSyntax)) {
                nodes.Add(new Tuple<SyntaxNode, SyntaxNode>(thenClause, SyntaxFactory.Block(thenClause)));
            }

            StatementSyntax elseClause = ifStatement.Else?.Statement;
            if (elseClause != null) {
                if (elseClause is BlockSyntax == false && elseClause is IfStatementSyntax == false) {
                    nodes.Add(new Tuple<SyntaxNode, SyntaxNode>(elseClause, SyntaxFactory.Block(elseClause)));
                }
            }
            return await this.ReplaceNodesInDocumentAsync(document, cancellationToken, nodes.ToArray());
        }
    }
}