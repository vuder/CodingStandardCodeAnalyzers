using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CodingStandardCodeAnalyzers.Test {

    [TestClass]
    public class DateTimeClassUsageCodeAnalyzerCodeFixProviderTest : CodeFixVerifier {
        #region Code samples
        private static readonly string Fix1ForDateTimeOffset = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTimeOffset.Now);  
        }
    }
}";

        private static readonly string Fix2ForDateTimeOffset = @"
using System;
using static System.DateTime;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTimeOffset.Now);  
        }
    }
}";

        private static readonly string Fix3ForDateTimeOffset = @"
using System;
namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTimeOffset.Now.Date);  
        }
    }
}";

        private static readonly string Fix4ForDateTimeOffset = @"using System;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(DateTimeOffset.Now.Date);  
        }
    }
}";

        private static readonly string Fix1ForNodaTime = @"
using System;
using NodaTime;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(SystemClock.Instance.Now);  
        }
    }
}";

        private static readonly string Fix2ForNodaTime = @"
using System;
using static System.DateTime;
using NodaTime;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(SystemClock.Instance.Now);  
        }
    }
}";

        private static readonly string Fix3ForNodaTime = @"
using System;
using NodaTime;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(SystemClock.Instance.Now.InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()).Date);  
        }
    }
}";

        private static readonly string Fix4ForNodaTime = @"using NodaTime;

namespace DateTimeClassAnalyzerTest {
    class Program {
        static void Main(string[] args) {
            Console.WriteLine(SystemClock.Instance.Now.InZone(DateTimeZoneProviders.Tzdb.GetSystemDefault()).Date);  
        }
    }
}";
        #endregion

        [TestMethod]
        public void UseOfDateTimeNowReplaceWithDateTimeOffsetNowFixCheck() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong1, Fix1ForDateTimeOffset, 0);
        }

        [TestMethod]
        public void UseOfDateTimeNowAsStaticUsingReplaceWithDateTimeOffsetNowFixCheck() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong2, Fix2ForDateTimeOffset, 0);
        }

        [TestMethod]
        public void UseOfDateTimeTodayReplaceWithDateTimeOffsetNowFixCheck() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong3, Fix3ForDateTimeOffset, 0);
        }

        [TestMethod]
        public void UseOfDateTimeWithNamespaceReplaceWithDateTimeOffsetNowFixCheck() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong4, Fix4ForDateTimeOffset, 0);
        }

        [TestMethod]
        public void UseOfDateTimeNowReplaceWithNodaTimeFixCheck() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong1, Fix1ForNodaTime, 1);
        }

        [TestMethod]
        public void UseOfDateTimeNowAsStaticUsingReplaceWithNodaTimeFixCheck() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong2, Fix2ForNodaTime, 1);
        }

        [TestMethod]
        public void UseOfDateTimeTodayReplaceWithNodaTimeFixCheck() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong3, Fix3ForNodaTime, 1);
        }

        [TestMethod]
        public void UseOfDateTimeWithNamespaceReplaceWithNodaTimeFixCheck() {
            VerifyCSharpFix(DateTimeClassUsageCodeAnalyzerTest.Wrong4, Fix4ForNodaTime, 1);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
            return new DateTimeClassUsageCodeAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new DateTimeClassUsageCodeAnalyzer();
        }
    }
}