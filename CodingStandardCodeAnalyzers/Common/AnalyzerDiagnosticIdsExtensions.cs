namespace CodingStandardCodeAnalyzers {
    public static class AnalyzerDiagnosticIdsExtensions {
        public static string ToDiagnosticsId(this AnalyzerDiagnosticIds analyzerDiagnosticIds) => $"{analyzerDiagnosticIds}";
    }
}