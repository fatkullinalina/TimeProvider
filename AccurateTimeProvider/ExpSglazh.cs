using System;
using System.Collections.Generic;
using System.Text;

namespace AccurateTimeProviderLib
{
    public static class ExpSglazh
    {
        public static (double a, double b) CountCoef(long time, Tuple<double, double> firstParams)
        {

            var alpha = 1;
            var beta = 1;

            var b = alpha * time + (1 - alpha) * (firstParams.Item2 - firstParams.Item1);
            var a = beta * (b - firstParams.Item2) + (1 - beta) * firstParams.Item1;

            return (a, b);
        }
    }
}
