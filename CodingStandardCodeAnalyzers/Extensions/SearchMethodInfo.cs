namespace CodingStandardCodeAnalyzers {
    public struct SearchMethodInfo {
        public string Namespace { get; private set; }
        public string ClassName { get; private set; }
        public string MemberName { get; private set; }

        public SearchMethodInfo(string @namespace, string className, string memberName) {
            Namespace = @namespace;
            ClassName = className;
            MemberName = memberName;
        }

        public override string ToString() {
            return $"{Namespace}.{ClassName}.{MemberName}";
        }
    }
}