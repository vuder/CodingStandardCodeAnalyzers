﻿using System.Collections.Immutable;
using System.Linq;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodingStandardCodeAnalyzers {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IfStatementCodeAnalyzer : DiagnosticAnalyzer {
        private static readonly LocalizableString Title = "Always use braces if IF and ELSE statements.";
        private static readonly LocalizableString MessageFormat = "Use braces in '{0}' statement.";
        private static readonly string Category = "Coding pratices";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor("IfStatementCodeAnalyzer",
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly string DiagnosticId = "IfStatementCodeAnalyzer";
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatementDeclaration, SyntaxKind.IfStatement);
        }

        private void AnalyzeIfStatementDeclaration(SyntaxNodeAnalysisContext context) {
            var ifStatement = context.Node as IfStatementSyntax;
            if (ifStatement == null) { return; }
            if (ifStatement.ChildNodes().Count(node => node.Kind() == SyntaxKind.Block) == 0) {
                Diagnostic diagnostic = Diagnostic.Create(Rule, ifStatement.GetLocation(), "if");
                context.ReportDiagnostic(diagnostic);
            }
            SyntaxNode elseClause = ifStatement.ChildNodes().FirstOrDefault(node => node.Kind() == SyntaxKind.ElseClause);
            if (elseClause?.ChildNodes().Count(node => node.Kind() == SyntaxKind.Block) == 0) {
                Diagnostic diagnostic = Diagnostic.Create(Rule, ifStatement.GetLocation(), "else");
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}