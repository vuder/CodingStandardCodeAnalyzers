using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CodingStandardCodeAnalyzers.Test {
    [TestClass]
    public class IfStatementCodeAnalyzerTest : CodeFixVerifier  {
        #region Code samples
        public static readonly string Wrong1 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public string Wrong1(){
                if(DateTime.Now == DateTimeOffset.Now) return \""wrong\"";
            }
        }
    }";

        #endregion

        [TestMethod]
        public void NoDiagnosticsExpectedForEmptyLine() {
            VerifyCSharpDiagnostic("");
        }

        [TestMethod]
        public void IfStatementWithoutBracesIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(6, 17);
            VerifyCSharpDiagnostic(Wrong1, expected);
        }

        private static DiagnosticResult CreateDiagnosticResult(int line, int column) {
            return new DiagnosticResult {
                Id = "IfStatementCodeAnalyzer",
                Message = "Use braces in 'if' statement.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new IfStatementCodeAnalyzer();
        }
    }
}