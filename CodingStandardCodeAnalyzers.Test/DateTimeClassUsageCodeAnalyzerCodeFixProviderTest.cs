using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CodingStandardCodeAnalyzers.Test {

    [TestClass]
    public class DateTimeClassUsageCodeAnalyzerCodeFixProviderTest : CodeFixVerifier {
        #region Code samples
        public static readonly string Fix1ForDateTimeOffset = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTimeOffset.Now);  
        }
    }
}";


        public static readonly string Fix2ForDateTimeOffset = @"
using System;
using static System.DateTime;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTimeOffset.Now);  
        }
    }
}";

        public static readonly string Fix3ForDateTimeOffset = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTimeOffset.Today);  
        }
    }
}";

        public static readonly string Fix4ForDateTimeOffset = @"using System;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTimeOffset.Today);  
        }
    }
}";

        #endregion
        
        [TestMethod]
        public void UseOfDateTimeNowErrorReplaceWithDateTimeOffsetNowFixCheck() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong1, Fix1ForDateTimeOffset, 0);
        }

        [TestMethod]
        public void UseOfDateTimeNowAsStaticUsingIsError() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong2, Fix2ForDateTimeOffset, 0);
        }

        [TestMethod]
        public void UseOfDateTimeTodayIsError() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong3, Fix3ForDateTimeOffset, 0);
        }

        [TestMethod]
        public void UseOfDateTimeWithNamespaceIsError() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong4, Fix4ForDateTimeOffset, 0);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
            return new DateTimeClassUsageCodeAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new DateTimeClassUsageCodeAnalyzer();
        }
    }
}