using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CodingStandardCodeAnalyzers.Test {
    [TestClass]
    public class IfStatementCodeAnalyzerCodeFixProviderTest : CodeFixVerifier {
        #region Code samples
        public static readonly string Fix1 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong1(){
                if (Environment.MachineName == String.Empty)
            {
                return 1;
            }

            return 0;
            }
        }
    }";

        public static readonly string Fix2 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong2(){
                if (Environment.MachineName == String.Empty)
            {
                return 1;
            }

            return 0;
            }
        }
    }";

        public static readonly string Fix3 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong3(){
                if (Environment.MachineName == String.Empty) {
                    return 1;
                } else
            {
                return 2;
            }
        }
        }
    }";

        public static readonly string Fix4 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong4(){
                if (Environment.MachineName == String.Empty) {
                    return 1;
                } else
            {
                return 2;
            }
        }
        }
    }";

        public static readonly string Fix5 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong5(){
                if (Environment.MachineName == String.Empty)
            {
                return 1;
            }
            else
            {
                return 2;
            }
        }
        }
    }";

        public static readonly string Fix6 = @"
    using System;
    namespace ConsoleApplication1 {
        class TypeName {   
            public int Wrong6(){
                if (Environment.MachineName == String.Empty) {
                    return 1;
                }
                else if (Environment.MachineName.Lenght > 2)
            {
                return 2;
            }
        }
        }
    }";
        #endregion

        [TestMethod]
        public void IfStatementWithoutBracesInOneLineFixIsCorrect() {
            VerifyCSharpFix(IfStatementCodeAnalyzerTest.Wrong1, Fix1);
        }

        [TestMethod]
        public void IfStatementWithoutBracesInTwoLineFixIsCorrect() {
            VerifyCSharpFix(IfStatementCodeAnalyzerTest.Wrong2, Fix2);
        }

        [TestMethod]
        public void ElseStatementWithoutBracesInOneLineFixIsCirrect() {
            VerifyCSharpFix(IfStatementCodeAnalyzerTest.Wrong3, Fix3);
        }

        [TestMethod]
        public void ElseStatementWithoutBracesInTwoLineFixIsCorrect() {
            VerifyCSharpFix(IfStatementCodeAnalyzerTest.Wrong4, Fix4);
        }

        [TestMethod]
        public void IfAndElseStatementWithoutBracesFixIsCorrect() {
            VerifyCSharpFix(IfStatementCodeAnalyzerTest.Wrong5, Fix5);
        }

        [TestMethod]
        public void IfAndElseStatementWithoutBracesInElseIfStatementFixIsCorrect() {
            VerifyCSharpFix(IfStatementCodeAnalyzerTest.Wrong6, Fix6);
        }
        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new IfStatementCodeAnalyzer();
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
            return new IfStatementCodeAnalyzerCodeFixProvider();
        }
    }
}