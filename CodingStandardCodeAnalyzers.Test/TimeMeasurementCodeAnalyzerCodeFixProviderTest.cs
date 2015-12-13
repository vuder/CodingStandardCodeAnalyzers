using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CodingStandardCodeAnalyzers.Test {
    [TestClass]
    public class TimeMeasurementCodeAnalyzerCodeFixProviderTest : CodeFixVerifier {
        #region Code samples
        public static readonly string Fix1 = @"
using System;
using System.Diagnostics;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            //var start = DateTime.Now;
            var timer = Stopwatch.StartNew();
            //do something
            System.Threading.Thread.Sleep(1000);  
            //var elapsed = DateTime.Now - start;
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
        }
    }
}";

        public static readonly string Fix2 = @"
using System;
using System.Diagnostics;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            //var start = DateTime.Now;
            var timer = Stopwatch.StartNew();
            //do something
            System.Threading.Thread.Sleep(1000);  
            //Debug.WriteLine(DateTime.Now - start);
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
        }
    }
}";

        public static readonly string Fix3 = @"
using System;
using static System.DateTime;
using System.Diagnostics;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            //var start = Now;
            var timer = Stopwatch.StartNew();
            //do something
            System.Threading.Thread.Sleep(1000);  
            //Debug.WriteLine(Now - start);
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
        }
    }
}";

        public static readonly string Fix4 = @"
using System;
using System.Diagnostics;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            //var start = DateTime.UtcNow;
            var timer = Stopwatch.StartNew();
            //do something
            System.Threading.Thread.Sleep(1000);  
            //var elapsed = DateTime.UtcNow - start;
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
        }
    }
}";

        public static readonly string Fix5 = @"
using System;
using System.Diagnostics;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            //var start = DateTimeOffset.UtcNow;
            var timer = Stopwatch.StartNew();
            //do something
            System.Threading.Thread.Sleep(1000);  
            //var elapsed = DateTimeOffset.UtcNow - start;
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
        }
    }
}";

        public static readonly string Fix6 = @"
using System;
using System.Diagnostics;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            //var start = DateTimeOffset.Now;
            var timer = Stopwatch.StartNew();
            //do something
            System.Threading.Thread.Sleep(1000);  
            //var elapsed = DateTimeOffset.Now - start;
            timer.Stop();
            TimeSpan elapsed = timer.Elapsed;
        }
    }
}";
        #endregion
        [TestMethod]
        public void UseOfOneDateTimeNowForTimeMeasurementFixCheck1() {
            VerifyCSharpFix(TimeMeasurementCodeAnalyzerTest.Wrong1, Fix1, 0);
        }

        [TestMethod]
        public void UseOfOneDateTimeNowForTimeMeasurementFixCheck2() {
            VerifyCSharpFix(TimeMeasurementCodeAnalyzerTest.Wrong2, Fix2, 0);
        }

        [TestMethod]
        public void UseOfOneDateTimeNowWithStaticUsingForTimeMeasurementFixCheck() {
            VerifyCSharpFix(TimeMeasurementCodeAnalyzerTest.Wrong3, Fix3, 0);
        }

        [TestMethod]
        public void UseOfOneDateTimeUtcNowForTimeMeasurementFixCheck() {
            VerifyCSharpFix(TimeMeasurementCodeAnalyzerTest.Wrong4, Fix4, 0);
        }

        [TestMethod]
        public void UseOfOneDateTimeOffsetUtcNowForTimeMeasurementFixCheck() {
            VerifyCSharpFix(TimeMeasurementCodeAnalyzerTest.Wrong5, Fix5, 0);
        }

        [TestMethod]
        public void UseOfOneDateTimeOffsetNowForTimeMeasurementFixCheck() {
            VerifyCSharpFix(TimeMeasurementCodeAnalyzerTest.Wrong6, Fix6, 0);
        }   

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
            return new TimeMeasurementCodeAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new TimeMeasurementCodeAnalyzer();
        }
    }
}