using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using TestHelper;

namespace CodingStandardCodeAnalyzers.Test {
    [TestClass]
    public class FieldAccessCodeAnalyzerTest : CodeFixVerifier {
        #region Code samples
        public static readonly string Wrong1 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            public string Wrong1;
        }
    }";

        public static readonly string Wrong2 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            internal string Wrong2;
        }
    }";

        public static readonly string Wrong3 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
            //test

            string Wrong3;

        }
    }";

        public static readonly string Wrong4 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           internal internal string Wrong4;
        }
    }";

        public static readonly string Wrong5 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           internal static string Wrong5;
        }
    }";

        public static readonly string Wrong6 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public static string Wrong6;
        }
    }";

        public static readonly string Wrong7 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           public virtual string Wrong7;
        }
    }";

        public static readonly string Wrong8 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           internal virtual string Wrong8;
        }
    }";

        public static readonly string Wrong9 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
           static string Wrong9;
        }
    }";

        public static readonly string Correct1 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
               public static readonly string Correct1;
        }
    }";

        public static readonly string Correct2 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
                private static readonly string Correct2;
        }
    }";

        public static readonly string Correct3 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
              private readonly string Correct3;
        }
    }";

        public static readonly string Correct4 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
               private string Correct4;
        }
    }";

        public static readonly string Correct5 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
                private static readonly string Correct5;
        }
    }";

        public static readonly string Correct7 = @"
    using System;

    namespace ConsoleApplication1
    {
        class TypeName
        {   
               internal static readonly string Correct7;
        }
    }";
        #endregion

        [TestMethod]
        public void NoDiagnosticsExpectedForEmptyLine() {
            VerifyCSharpDiagnostic("");
        }

        [TestMethod]
        public void PublicFieldDeclarationIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(8, 13);
            VerifyCSharpDiagnostic(Wrong1, expected);
        }

        [TestMethod]
        public void InternalFieldDeclarationIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(8, 13);
            VerifyCSharpDiagnostic(Wrong2, expected);
        }

        [TestMethod]
        public void FieldWithoutAccessModifierDeclarationIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(10, 13);
            VerifyCSharpDiagnostic(Wrong3, expected);
        }

        [TestMethod]
        public void FieldWithoutDoubleAccessModifierDeclarationIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(8, 12);
            VerifyCSharpDiagnostic(Wrong4, expected);
        }

        [TestMethod]
        public void InternalStaticFieldDeclarationIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(8, 12);
            VerifyCSharpDiagnostic(Wrong5, expected);
        }

        [TestMethod]
        public void PublicStaticFieldDeclarationIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(8, 12);
            VerifyCSharpDiagnostic(Wrong6, expected);
        }

        [TestMethod]
        public void PublicVirtualFieldDeclarationIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(8, 12);
            VerifyCSharpDiagnostic(Wrong7, expected);
        }

        [TestMethod]
        public void InternalVirtualFieldDeclarationIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(8, 12);
            VerifyCSharpDiagnostic(Wrong8, expected);
        }

        [TestMethod]
        public void StaticFieldWithoutAccessModifierDeclarationIsError() {
            DiagnosticResult expected = CreateDiagnosticResult(8, 12);
            VerifyCSharpDiagnostic(Wrong9, expected);
        }

        [TestMethod]
        public void PublicStaticReadonlyFieldDeclarationIsCorrect() {
            VerifyCSharpDiagnostic(Correct1);
        }

        [TestMethod]
        public void PrivateStaticReadonlyFieldDeclarationIsCorrect() {
            VerifyCSharpDiagnostic(Correct2);
        }

        [TestMethod]
        public void PrivateReadonlyFieldDeclarationIsCorrect() {
            VerifyCSharpDiagnostic(Correct3);
        }

        [TestMethod]
        public void PrivateFieldDeclarationIsCorrect() {
            VerifyCSharpDiagnostic(Correct4);
        }

        [TestMethod]
        public void InternalStaticReadonlyFieldDeclarationIsCorrect() {
            VerifyCSharpDiagnostic(Correct5);
        }

        private static DiagnosticResult CreateDiagnosticResult(int line, int column) {
            return new DiagnosticResult {
                Id = "FieldAccessCodeAnalyzer",
                Message = "Field must be private.",
                Severity = DiagnosticSeverity.Error,
                Locations = new[] { new DiagnosticResultLocation("Test0.cs", line, column) }
            };
        }

        protected override DiagnosticAnalyzer GetCSharpDiagnosticAnalyzer() {
            return new FieldAccessCodeAnalyzer();
        }
    }
}