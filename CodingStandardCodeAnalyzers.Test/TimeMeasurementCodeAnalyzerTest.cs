using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CodingStandardCodeAnalyzers.Test {
    [TestClass]
    public class TimeMeasurementCodeAnalyzerTest : CodeFixVerifier {
        #region Code samples
        public static readonly string Wrong1 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            var start = DateTime.Now;
            //do something
            System.Threading.Thread.Sleep(1000);  
            var elapsed = DateTime.Now - start;
        }
    }
}";

        public static readonly string Wrong2 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            var start = DateTime.Now;
            //do something
            System.Threading.Thread.Sleep(1000);  
            Debug.WriteLine(DateTime.Now - start);
        }
    }
}";

        public static readonly string Wrong3 = @"
using System;
using static System.DateTime;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            var start = Now;
            //do something
            System.Threading.Thread.Sleep(1000);  
            Debug.WriteLine(Now - start);
        }
    }
}";

        public static readonly string Wrong4 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            var start = DateTime.UtcNow;
            //do something
            System.Threading.Thread.Sleep(1000);  
            var elapsed = DateTime.UtcNow - start;
        }
    }
}";

        public static readonly string Wrong5 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            var start = DateTimeOffset.UtcNow;
            //do something
            System.Threading.Thread.Sleep(1000);  
            var elapsed = DateTimeOffset.UtcNow - start;
        }
    }
}";

        public static readonly string Wrong6 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            var start = DateTimeOffset.Now;
            //do something
            System.Threading.Thread.Sleep(1000);  
            var elapsed = DateTimeOffset.Now - start;
        }
    }
}";

        public static readonly string Correct1 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTime.Now);  
        }
    }
}";

        public static readonly string Correct2 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            var start = DateTimeOffset.Now;
            var start1 = DateTimeOffset.Now
            //do something
            System.Threading.Thread.Sleep(1000);  
            var elapsed = DateTimeOffset.Now - start;
        }
    }
}";

        public static readonly string Correct3 = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            var start = DateTimeOffset.Now;
            //do something
            System.Threading.Thread.Sleep(1000);  
            var elapsed = DateTimeOffset.UtcNow - start;
        }
    }
}";

        #endregion

        [TestMethod]
        public void NoDiagnosticsExpectedForEmptyLine() {
            VerifyCSharpDiagnostic("");
        }

        [TestMethod]
        public void UseOfOneDateTimeNowForTimeMeasurementIsWrong1() {
            DiagnosticResult expected = CreateDiagnosticResult(9, 27, "DateTime.Now");
            VerifyCSharpDiagnostic(Wrong1, expected);
        }

        [TestMethod]
        public void UseOfOneDateTimeNowForTimeMeasurementIsWrong2() {
            DiagnosticResult expected = CreateDiagnosticResult(9, 29, "DateTime.Now");
            VerifyCSharpDiagnostic(Wrong2, expected);
        }

        [TestMethod]
        public void UseOfOneDateTimeNowWithStaticUsingForTimeMeasurementIsWrong() {
            DiagnosticResult expected = CreateDiagnosticResult(10, 29, "DateTime.Now");
            VerifyCSharpDiagnostic(Wrong3, expected);
        }

        [TestMethod]
        public void UseOfOneDateTimeUtcNowForTimeMeasurementIsWrong() {
            DiagnosticResult expected = CreateDiagnosticResult(9, 27, "DateTime.UtcNow");
            VerifyCSharpDiagnostic(Wrong4, expected);
        }

        [TestMethod]
        public void UseOfOneDateTimeOffsetUtcNowForTimeMeasurementIsWrong() {
            DiagnosticResult expected = CreateDiagnosticResult(9, 27, "DateTimeOffset.UtcNow");
            VerifyCSharpDiagnostic(Wrong5, expected);
        }

        [TestMethod]
        public void UseOfOneDateTimeOffsetNowForTimeMeasurementIsWrong() {
            DiagnosticResult expected = CreateDiagnosticResult(9, 27, "DateTimeOffset.Now");
            VerifyCSharpDiagnostic(Wrong6, expected);
        }

        [TestMethod]
        public void UseOfOneDateTimeNowIsCorrect() {
            VerifyCSharpDiagnostic(Correct1);
        }

        [TestMethod]
        public void UseOfGetCurrentTimeMoreThanTwoTimesIsCorrect() {
            VerifyCSharpDiagnostic(Correct2);
        }

        [TestMethod]
        public void UseOfGetCurrentTimeOfDifferentMethodsIsCorrect() {
            VerifyCSharpDiagnostic(Correct3);
        }
        
        private static DiagnosticResult CreateDiagnosticResult(int line, int column, string method) {
            return new DiagnosticResult {
                Id = "TimeMeasurementCodeAnalyzer",
                Message = $"Use Stopwatch to measure elapsed time instead of 'System.{method}()'.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new TimeMeasurementCodeAnalyzer();
        }
    }
}