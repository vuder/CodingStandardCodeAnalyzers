// ReSharper disable UnassignedReadonlyField
#pragma warning disable 169
#pragma warning disable 649

namespace CodeSamples {
    public class FieldAccessCodeAnalyzerTest {
        internal string Wrong1;

        public string Wrong2;

        string Wrong3;

       // internal internal string Wrong4;

        internal static string Wrong5;

        public static string Wrong6;

       // public virtual string Wrong7;

        /****************** CORRECT ************************************/

        public static readonly string Correct1;

        private static readonly string Correct2;

        private readonly string Correct3;

        private string Correct4;

        internal static readonly string Correc5;
    }
}
