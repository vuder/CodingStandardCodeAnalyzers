﻿using System.Collections.Immutable;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace CodingStandardCodeAnalyzers {
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class IfStatementCodeAnalyzer : DiagnosticAnalyzer {
        private static readonly LocalizableString Title = "Always use braces if IF-THEN and IF-ELSE statements.";
        private static readonly LocalizableString MessageFormat = "Use braces in '{0}' statement.";
        private static readonly string Category = AnalyzerDiagnosticCategories.CodingPractices;

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(AnalyzerDiagnosticIds.IfStatementCodeAnalyzer.ToDiagnosticsId(),
            Title,
            MessageFormat,
            Category,
            DiagnosticSeverity.Error,
            isEnabledByDefault: true);

        public static readonly string DiagnosticId = Rule.Id;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public override void Initialize(AnalysisContext context) {
            context.RegisterSyntaxNodeAction(AnalyzeIfStatementDeclaration, SyntaxKind.IfStatement, SyntaxKind.ElseClause);
        }

        private void AnalyzeIfStatementDeclaration(SyntaxNodeAnalysisContext context) {
            if (context.IsGeneratedOrNonUserCode()) { return; }
            var ifStatement = context.Node as IfStatementSyntax;
            if (ifStatement == null) { return; }
            CSharpSyntaxNode errorLocation = null;
            StatementSyntax thenClause = ifStatement.Statement;
            if (thenClause != null && !(thenClause is BlockSyntax)) {
                errorLocation = thenClause;
            }
            ElseClauseSyntax elseClause = ifStatement.Else;
            if (elseClause != null) {
                if (elseClause.Statement is BlockSyntax == false && elseClause.Statement is IfStatementSyntax == false) {
                    errorLocation = errorLocation == null ? elseClause : (CSharpSyntaxNode)ifStatement;
                }
            }
            if (errorLocation == null) { return; }
            Diagnostic diagnostic = Diagnostic.Create(Rule, errorLocation.GetLocation(), errorLocation.ToString());
            context.ReportDiagnostic(diagnostic);
        }
    }
}
