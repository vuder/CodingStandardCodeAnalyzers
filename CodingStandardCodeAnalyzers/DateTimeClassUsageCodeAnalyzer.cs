using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodingStandardCodeAnalyzers {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DateTimeClassUsageCodeAnalyzer : DiagnosticAnalyzer {
        private static readonly LocalizableString Title = "Use of some methods of DateTime is not recommended.";
        private static readonly LocalizableString MessageFormat = "Use of '{0}' is not recommended. Consider other options to achieve what you need.";
        private static readonly LocalizableString Description = "";
        private static readonly string Category = AnalyzerDiagnosticCategories.CodingPractices;
        private static readonly string HelpLink = @"http://tinyurl.com/CaseAgainsDateTimeNow";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(AnalyzerDiagnosticIds.DateTimeClassUsageCodeAnalyzer.ToDiagnosticsId(),
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true,
            description: Description,
            helpLinkUri: HelpLink);

        public static readonly string DiagnosticId = "DateTimeClassUsageCodeAnalyzer";

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            context.RegisterSyntaxNodeAction(AnalyzeDateTimeUsage, SyntaxKind.IdentifierName);
        }

        private void AnalyzeDateTimeUsage(SyntaxNodeAnalysisContext context) {
            var expression = context.Node as IdentifierNameSyntax;
            if (expression == null || context.SemanticModel == null) { return; }
            SymbolInfo diagnostics = context.SemanticModel.GetSymbolInfo(expression);
            ISymbol symbol = diagnostics.Symbol;
            if (symbol?.ContainingType?.ContainingNamespace == null) { return; }
            if (symbol.ContainingType.ContainingNamespace.Name == "System" && symbol.ContainingType.Name == "DateTime") {
                if (symbol.Name == "Now" || symbol.Name == "Today") {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, expression.GetLocation(), $"{symbol.ContainingType.Name}.{symbol.Name}");
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}