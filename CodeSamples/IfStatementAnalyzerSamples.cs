using System;
// ReSharper disable RedundantIfElseBlock

namespace CodeSamples {
    public class IfStatementAnalyzerSamples {
        public int Wrong1() {
            if (Environment.ProcessorCount == 1)
                return 1;
            return 0;
        }

        public int Wrong2() {
            if (Environment.ProcessorCount == 1)
                return 2;
            return 0;
        }

        public int Wrong3() {
            if (Environment.ProcessorCount == 1) {
                return 3;
            } else
                return 4;
        }

        public int Wrong4() {
            if (Environment.ProcessorCount == 1)
                return 3;
            else if (Environment.ProcessorCount == 2)
                return 4;
            else if (Environment.ProcessorCount == 3)
                return 4;
            return 0;
        }

        public int Wrong5() {
            if (Environment.ProcessorCount == 1) {
                return 3;
            } else
                return 4;
        }

        public int Wrong6() {
            if (Environment.ProcessorCount == 1)
                return 3;
            else
                return 4;
        }

        /****************** CORRECT ************************************/

        public int Correct1() {
            if (Environment.ProcessorCount == 2) {
                return 5;
            } else if (Environment.ProcessorCount == 1) {
                return 3;
            } else
            if (Environment.ProcessorCount == 3) {
                return 6;
            } else {
                return 1;
            }

        }
    }
}
