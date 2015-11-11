using System;
using static System.DateTime;

namespace CodeSamples {
    public class DateTimeAnalyzerSamples2 {
        public DateTime Wrong1() {
            return Now;
        }

        public DateTime Wrong2() {
            var now = Today;
            return now;
        }
        public DateTime Wrong3() {
            var today = Now.Date;
            return today;
        }

        /****************** CORRECT ************************************/

        public DateTime Correct21() {
            return UtcNow;
        }
        public DateTimeOffset Correct2() {
            return DateTimeOffset.Now;
        }
    }
}