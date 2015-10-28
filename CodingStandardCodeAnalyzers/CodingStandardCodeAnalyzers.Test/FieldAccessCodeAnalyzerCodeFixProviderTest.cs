using Microsoft.CodeAnalysis.CodeFixes;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CodingStandardCodeAnalyzers.Test {
    [TestClass]
    public class FieldAccessCodeAnalyzerCodeFixProviderTest : CodeFixVerifier {
        #region Code samples
        public static readonly string Fix1 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            private string Wrong1;
        }
    }";

        public static readonly string Fix2 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            private string Wrong2;
        }
    }";

        public static readonly string Fix3 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            //test

            private string Wrong3;

        }
    }";

        public static readonly string Fix4 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           private string Wrong4;
        }
    }";

        public static readonly string Fix5 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           private static string Wrong5;
        }
    }";

        public static readonly string Fix6 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           private static string Wrong6;
        }
    }";

        public static readonly string Fix7 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           private virtual string Wrong7;
        }
    }";

        public static readonly string Fix8 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           private virtual string Wrong8;
        }
    }";

        public static readonly string Fix9 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           private static string Wrong9;
        }
    }";
        #endregion

        [TestMethod]
        public void PublicFieldDeclarationErrorFixCheck() {
            VerifyCSharpFix(FieldAccessCodeAnalyzerTest.Wrong1, Fix1);
        }

        [TestMethod]
        public void InternalFieldDeclarationErrorFixCheck() {
            VerifyCSharpFix(FieldAccessCodeAnalyzerTest.Wrong2, Fix2);
        }

        [TestMethod]
        public void FieldWithoutAccessModifierDeclarationErrorFixCheck() {
            VerifyCSharpFix(FieldAccessCodeAnalyzerTest.Wrong3, Fix3);
        }

        [TestMethod]
        public void FieldWithoutDoubleAccessModifierDeclarationErrorFixCheck() {
            VerifyCSharpFix(FieldAccessCodeAnalyzerTest.Wrong4, Fix4);
        }

        [TestMethod]
        public void InternalStaticFieldDeclarationErrorFixCheck() {
            VerifyCSharpFix(FieldAccessCodeAnalyzerTest.Wrong5, Fix5);
        }

        [TestMethod]
        public void PublicStaticFieldDeclarationErrorFixCheck() {
            VerifyCSharpFix(FieldAccessCodeAnalyzerTest.Wrong6, Fix6);
        }

        [TestMethod]
        public void PublicVirtualFieldDeclarationErrorFixCheck() {
            VerifyCSharpFix(FieldAccessCodeAnalyzerTest.Wrong7, Fix7);
        }

        [TestMethod]
        public void InternalVirtualFieldDeclarationErrorFixCheck() {
            VerifyCSharpFix(FieldAccessCodeAnalyzerTest.Wrong8, Fix8);
        }

        [TestMethod]
        public void StaticFieldWithoutAccessModifierDeclarationErrorFixCheck() {
            VerifyCSharpFix(FieldAccessCodeAnalyzerTest.Wrong9, Fix9);
        }

        protected override CodeFixProvider GetCSharpCodeFixProvider() {
            return new FieldAccessCodeAnalyzerCodeFixProvider();
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FieldAccessCodeAnalyzer();
        }
    }
}