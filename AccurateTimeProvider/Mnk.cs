using System;
using System.Collections.Generic;
using System.Text;

namespace AccurateTimeProviderLib
{
    public static class Mnk
    {
        public static (double a, double b) CountCoef(Queue<(long x, long y)> points)
        {
            var sumy = 0L;
            var sumx = 0L;
            var pr = 0L;
            var x2 = 0L;
            foreach (var pt in points)
            {
                sumx += pt.x;
                sumy += pt.y;
                pr += pt.y * pt.x;
                x2 += pt.x * pt.x;
            }
            var a = ((double)(points.Count * pr - (sumx * sumy)) / (points.Count * x2 - (sumx * sumx)));
            var b = ((((double)sumy) / points.Count) - ((double)(a * sumx) / points.Count));
            return (a, b);
        }
    }
}
