using System;

public class DateTimeAnalyzerSamples {
    public DateTime Wrong1() {
        return DateTime.Now;
    }

    public DateTime Wrong2() {
        var now = DateTime.Today;
        return now;
    }
    public DateTime Wrong3() {
        var today = DateTime.Now.Date;
        return today;
    }

    /****************** CORRECT ************************************/

    public DateTime Correct1() {
        return DateTime.UtcNow;
    }
    public DateTimeOffset Correct2() {
        return DateTimeOffset.Now;
    }
    public DateTime Correct3() {
        return DateTime.Today;
    }
}

