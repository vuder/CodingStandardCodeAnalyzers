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

        public static readonly string DiagnosticId = Rule.Id;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            context.RegisterSyntaxNodeAction(AnalyzeDateTimeUsage, SyntaxKind.IdentifierName);
        }

        private void AnalyzeDateTimeUsage(SyntaxNodeAnalysisContext context) {
            if (context.IsGeneratedOrNonUserCode()) { return; }
            var node = context.Node as IdentifierNameSyntax;

            var methodsToSearch = new[] {
                new SearchMethodInfo("System", "DateTime", "Now"),
                new SearchMethodInfo("System", "DateTime", "Today")
            };
            var symbol = node.CheckNodeIsMemberOfType(context, methodsToSearch);
            if (symbol == null) { return; }
            Diagnostic diagnostic = Diagnostic.Create(Rule, node.GetLocation(), $"{symbol.ContainingType.Name}.{symbol.Name}");
            context.ReportDiagnostic(diagnostic);
        }
    }
}