using System;
using System.Collections.Generic;
using System.Linq;
public static class Extend
{
    public static double StandardDeviation(this IEnumerable<double> values)
    {
        if (values.Count() > 0)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }
        else
        {
            return 0;
        }
    }
    public static double StandardDeviation(this IEnumerable<int> values)
    {
        if (values.Count() > 0)
        {
            double avg = values.Average();
            return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
        }
        else
        {
            return 0;
        }
    }
}