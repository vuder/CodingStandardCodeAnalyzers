using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CodeSamples
{
    class TimeMeasurementSamples
    {
        public void Wrong1()
        {
            var start = DateTimeOffset.Now;
            //do something
            System.Threading.Thread.Sleep(1000);
            var elapsed = DateTimeOffset.Now - start;
        }
    }
}
