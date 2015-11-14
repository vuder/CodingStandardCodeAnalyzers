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
            await context.RegisterCodeFixAsync<FieldDeclarationSyntax>(CodeFixTitle, MakePrivateAsync);
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

            return await this.ReplaceNodeInDocumentAsync(document, cancellationToken, fieldDeclaration, newfieldDeclaration);
        }

        private static SyntaxToken CreatePrivateSyntaxToken(FieldDeclarationSyntax fieldDeclaration) {
            SyntaxTriviaList leadingTrivia = fieldDeclaration.Declaration.GetLeadingTrivia();
            SyntaxTriviaList trailingTrivia = SyntaxTriviaList.Create(SyntaxFactory.SyntaxTrivia(SyntaxKind.WhitespaceTrivia, " "));
            SyntaxToken privateSyntaxToken = SyntaxFactory.Token(leadingTrivia, SyntaxKind.PrivateKeyword, trailingTrivia);
            return privateSyntaxToken;
        }
    }
}