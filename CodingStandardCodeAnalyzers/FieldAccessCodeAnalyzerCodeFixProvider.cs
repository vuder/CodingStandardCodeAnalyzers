using System.Collections.Generic;
using System.Collections.Immutable;
using System.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.CodeActions;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Formatting;
using Microsoft.CodeAnalysis.Text;

namespace CodingStandardCodeAnalyzers {
    [ExportCodeFixProvider(LanguageNames.CSharp, Name = nameof(FieldAccessCodeAnalyzerCodeFixProvider))]
    [Shared]
    public class FieldAccessCodeAnalyzerCodeFixProvider : CodeFixProvider {
        private const string CodeFixTitle = "Make private";

        public sealed override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(FieldAccessCodeAnalyzer.DiagnosticId);

        public sealed override FixAllProvider GetFixAllProvider() {
            return WellKnownFixAllProviders.BatchFixer;
        }

        public sealed override async Task RegisterCodeFixesAsync(CodeFixContext context) {
            SyntaxNode root = await context.Document.GetSyntaxRootAsync(context.CancellationToken).ConfigureAwait(false);

            Diagnostic diagnostic = context.Diagnostics.First();
            TextSpan diagnosticSpan = diagnostic.Location.SourceSpan;

            FieldDeclarationSyntax declaration = root.FindToken(diagnosticSpan.Start).Parent.AncestorsAndSelf().OfType<FieldDeclarationSyntax>().First();
            context.RegisterCodeFix(
                 CodeAction.Create(
                     title: CodeFixTitle,
                     createChangedDocument: cancellationToken => MakePrivateAsync(context.Document, declaration, cancellationToken),
                     equivalenceKey: CodeFixTitle),
                 diagnostic);
        }

        private async Task<Document> MakePrivateAsync(Document document, FieldDeclarationSyntax fieldDeclaration, CancellationToken cancellationToken) {
            SyntaxToken privateSyntaxToken = CreatePrivateSyntaxToken(fieldDeclaration);
            IEnumerable<SyntaxToken> newFieldModifiers = new[] { privateSyntaxToken }.Union(
                fieldDeclaration.Modifiers.Where(modifier =>
                        modifier.Kind() != SyntaxKind.InternalKeyword && modifier.Kind() != SyntaxKind.PublicKeyword)
                        .Select(modifier => modifier.WithLeadingTrivia()));

            FieldDeclarationSyntax newfieldDeclaration = fieldDeclaration.Update(fieldDeclaration.AttributeLists,
                        new SyntaxTokenList().AddRange(newFieldModifiers),
                        fieldDeclaration.Declaration.WithoutTrivia(),
                        fieldDeclaration.SemicolonToken)
                        .WithLeadingTrivia(fieldDeclaration.GetLeadingTrivia())
                        .WithTrailingTrivia(fieldDeclaration.GetTrailingTrivia());

            return await ReplacePropertyInDocumentAsync(document, fieldDeclaration, newfieldDeclaration, cancellationToken);
        }

        private static SyntaxToken CreatePrivateSyntaxToken(FieldDeclarationSyntax fieldDeclaration) {
            SyntaxTriviaList leadingTrivia = fieldDeclaration.Declaration.GetLeadingTrivia();
            SyntaxTriviaList trailingTrivia = SyntaxTriviaList.Create(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "));
            SyntaxToken privateSyntaxToken = SyntaxFactory.Token(leadingTrivia, SyntaxKind.PrivateKeyword, trailingTrivia);
            return privateSyntaxToken;
        }

        private static async Task<Document> ReplacePropertyInDocumentAsync(Document document, FieldDeclarationSyntax fieldDeclaration, FieldDeclarationSyntax newFieldDeclaration, CancellationToken cancellationToken) {
            SyntaxNode root = await document.GetSyntaxRootAsync(cancellationToken);
            SyntaxNode newRoot = root.ReplaceNode(fieldDeclaration, newFieldDeclaration);
            SyntaxNode formattedRoot = Formatter.Format(newRoot, Formatter.Annotation, document.Project.Solution.Workspace);
            return document.WithSyntaxRoot(formattedRoot);
        }
    }
}