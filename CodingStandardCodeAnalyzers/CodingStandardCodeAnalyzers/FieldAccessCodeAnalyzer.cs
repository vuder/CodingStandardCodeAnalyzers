using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodingStandardCodeAnalyzers {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class FieldAccessCodeAnalyzer : DiagnosticAnalyzer {
        private static readonly string Title = "Access modifier for the field is wrong.";
        private static readonly string MessageFormat = "Field must be private.";
        private static readonly string Description = "All fields must be private, the only exception is static readonly fields.";
        private static readonly string Category = "Access";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor("FieldAccessCodeAnalyzer", Title, MessageFormat, Category, DiagnosticSeverity.Error, isEnabledByDefault: true, description: Description);

        public static readonly string DiagnosticId = "FieldAccessCodeAnalyzer";
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            context.RegisterSyntaxNodeAction(AnalyzeFieldDeclaration, SyntaxKind.FieldDeclaration);
        }

        private static void AnalyzeFieldDeclaration(SyntaxNodeAnalysisContext context) {
            var fieldDeclaration = (FieldDeclarationSyntax)context.Node;
            if (IsStaticReadonlyField(fieldDeclaration)) { return; }

            SyntaxToken[] accessTokens = GetAccessTokenFor(fieldDeclaration, SyntaxKind.PrivateKeyword);
            if (accessTokens.Length != 1) {
                string fieldName = fieldDeclaration.DescendantTokens().FirstOrDefault(token => token.IsKind(SyntaxKind.IdentifierToken)).Value as string;
                Diagnostic diagnostic = Diagnostic.Create(Rule, fieldDeclaration.GetLocation(), fieldName);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private static bool IsStaticReadonlyField(FieldDeclarationSyntax fieldDeclaration) {
            return IsStatic(fieldDeclaration) && IsReadonly(fieldDeclaration);
        }

        private static SyntaxToken[] GetAccessTokenFor(FieldDeclarationSyntax fieldDeclaration, SyntaxKind syntaxKind) {
            return fieldDeclaration.ChildTokens().Where(token => token.Kind() == syntaxKind).ToArray();
        }

        private static bool IsReadonly(FieldDeclarationSyntax fieldDeclaration) {
            return fieldDeclaration.ChildTokens().Any(token => token.Kind() == SyntaxKind.ReadOnlyKeyword);
        }

        private static bool IsStatic(FieldDeclarationSyntax fieldDeclaration) {
            return fieldDeclaration.ChildTokens().Any(token => token.Kind() == SyntaxKind.StaticKeyword);
        }
    }
}
