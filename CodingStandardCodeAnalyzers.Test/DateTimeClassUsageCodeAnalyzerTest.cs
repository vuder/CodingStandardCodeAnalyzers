using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CodingStandardCodeAnalyzers.Test {
    [TestClass]
    public class DateTimeClassUsageCodeAnalyzerTest : CodeFixVerifier {
        #region Code samples
        public static readonly string Wrong1 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTime.Now );  
        }
    }
}";

        public static readonly string Wrong2 = @"
using System;
using static System.DateTime;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(Now);  
        }
    }
}";

        public static readonly string Wrong3 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTime.Today );  
        }
    }
}";

        public static readonly string Correct1 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTimeOffset.Now);  
        }
    }
}";

        public static readonly string Correct2 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(UtcNow);  
        }
    }
}";
        #endregion

        [TestMethod]
        public void NoDiagnosticsExpectedForEmptyLine() {
            VerifyCSharpDiagnostic("");
        }

        [TestMethod]
        public void UseOfDateTimeNowIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(6, 40, "Now");
            VerifyCSharpDiagnostic(Wrong1, expected);
        }

        [TestMethod]
        public void UseOfDateTimeNowAsStaticUsingIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(7, 31, "Now");
            VerifyCSharpDiagnostic(Wrong2, expected);
        }

        [TestMethod]
        public void UseOfDateTimeTodayIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(6, 40, "Today");
            VerifyCSharpDiagnostic(Wrong3, expected);
        }

        [TestMethod]
        public void UseOfNowPropertyInOtherClassIsCorrect() {
            VerifyCSharpDiagnostic(Correct1);
        }

        [TestMethod]
        public void UseOfOtherPropertyInDateTimeClassIsCorrect() {
            VerifyCSharpDiagnostic(Correct2);
        }
        
        private static DiagnosticResult CreateDiagnosticResult(int line, int column, string methodName) {
            return new DiagnosticResult {
                Id = "DateTimeClassUsageCodeAnalyzer",
                Message = $"Use of 'DateTime.{methodName}' is not recommended. Consider other options to achive what you need.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new DateTimeClassUsageCodeAnalyzer();
        }
    }
}
