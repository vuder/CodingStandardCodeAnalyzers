using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CodingStandardCodeAnalyzers.Test {
    [TestClass]
    public class IfStatementCodeAnalyzerTest : CodeFixVerifier {
        #region Code samples
        public static readonly string Wrong1 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong1(){
                if (Environment.MachineName == String.Empty) return 1;
                return 0;
            }
        }
    }";

        public static readonly string Wrong2 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong2(){
                if (Environment.MachineName == String.Empty) 
                    return 1;
                return 0;
            }
        }
    }";

        public static readonly string Wrong3 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong3(){
                if (Environment.MachineName == String.Empty) {
                    return 1;
                } else return 2;
            }
        }
    }";

        public static readonly string Wrong4 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong4(){
                if (Environment.MachineName == String.Empty) {
                    return 1;
                } else 
                    return 2;
            }
        }
    }";

        public static readonly string Wrong5 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong4(){
                if (Environment.MachineName == String.Empty) 
                    return 1;
                else 
                    return 2;
            }
        }
    }";

        public static readonly string Wrong6 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong4(){
                if (Environment.MachineName == String.Empty) {
                    return 1;
                }
                else if (Environment.MachineName.Lenght > 2)
                    return 2;
            }
        }
    }";

        public static readonly string Correct1 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Correct1(){
                if (Environment.MachineName == String.Empty) { return 1; }
                return 0;
            }
        }
    }";

        public static readonly string Correct2 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Correct2(){
                if (Environment.MachineName == String.Empty) {
                    return 1;
                } else {
                    return 2;
                }
            }
        }
    }";

        public static readonly string Correct3 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Correct2(){
                if (Environment.MachineName == String.Empty) {
                    return 1;
                } else if (Environment.MachineName.Lenght > 2) {
                    return 2;
                } else {
                    return 3;
                }
            }
        }
    }";
        #endregion

        [TestMethod]
        public void NoDiagnosticsExpectedForEmptyLine() {
            VerifyCSharpDiagnostic("");
        }

        [TestMethod]
        public void IfStatementWithoutBracesInOneLineIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(6, 62, "return 1;");
            VerifyCSharpDiagnostic(Wrong1, expected);
        }


        [TestMethod]
        public void IfStatementWithoutBracesInTwoLineIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(7, 21, "return 1;");
            VerifyCSharpDiagnostic(Wrong2, expected);
        }

        [TestMethod]
        public void ElseStatementWithoutBracesInOneLineIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(8, 19, "else return 2;");
            VerifyCSharpDiagnostic(Wrong3, expected);
        }

        [TestMethod]
        public void ElseStatementWithoutBracesInTwoLineIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(8, 19, "else \r\n                    return 2;");
            VerifyCSharpDiagnostic(Wrong4, expected);
        }

        [TestMethod]
        public void IfAndElseStatementWithoutBracesIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(6, 17, "if (Environment.MachineName == String.Empty) \r\n                    return 1;\r\n                else \r\n                    return 2;");
            VerifyCSharpDiagnostic(Wrong5, expected);
        }

        [TestMethod]
        public void IfAndElseStatementWithoutBracesInElseIfIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(10, 21, "return 2;");
            VerifyCSharpDiagnostic(Wrong6, expected);
        }

        [TestMethod]
        public void IfStatementBracesInTwoLineIsCorrect() {
            VerifyCSharpDiagnostic(Correct1);
        }

        [TestMethod]
        public void IfAndElseStatementBracesInTwoLineIsCorrect() {
            VerifyCSharpDiagnostic(Correct2);
        }

        [TestMethod]
        public void IfAndElseStatementBracesInElseIfIsCorrect() {
            VerifyCSharpDiagnostic(Correct3);
        }

        private static DiagnosticResult CreateDiagnosticResult(int line, int column, string clause) {
            return new DiagnosticResult {
                Id = "IfStatementCodeAnalyzer",
                Message = $"Use braces in '{clause}' statement.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new IfStatementCodeAnalyzer();
        }
    }
}