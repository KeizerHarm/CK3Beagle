using System;
using System.Collections.Generic;

namespace CK3Analyser.Analysis.Comparing.Building
{
    public class MyersAlgorithm
    {
        //private IList<T> FindMiddleSnake<T>(IList<T> first, IList<T> second)
        //{
        //    var m = first.Count;
        //    var n = second.Count;
        //    var delta = n - m;

        //    for (var d = 0; d < Math.Ceiling((m + n) / 2.0); d++)
        //    {
        //        for (var k = -d; k < d; k += 2)
        //        {

        //        }
        //    }
        //}


        private int GetLengthOfLCS<T>(IList<T> first, IList<T> second)
        {
            var m = first.Count;
            var n = second.Count;
            var max = Math.Max(m, n);

            var v = new int[max * 2];
            v[max + 1] = 0;

            for (var d = 0; d < max; d++)
            {
                for (var k = -d; k < d; k += 2)
                {
                    var x = 0;
                    if (k == -d || (k != d && v[k - 1 + max] < v[k + 1 + max]))
                    {
                        x = v[k + 1 + max];
                    }
                    else
                    {
                        x = v[k - 1 + max] + 1;
                    }
                    var y = x - k;
                    while (x < n && y < m && EqualityComparer<T>.Default.Equals(first[x + 1], second[y + 1]))
                    {
                        x++;
                        y++;
                    }
                    v[k] = x;
                    if (x >= n && y >= m)
                    {
                        return d;
                    }
                }
            }

            return -1;
        }
    }

    public interface ILongestCommonSubsequenceCalculator
    {
        
    }
}
