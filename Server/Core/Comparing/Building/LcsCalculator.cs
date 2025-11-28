using CK3Analyser.Core.Domain.Entities;
using System;
using System.Collections.Generic;

namespace CK3Analyser.Core.Comparing.Building
{
    public static class LcsCalculator
    {
        public static IEnumerable<T> ComputeLcs<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, IEqualityComparer<T> cmp)
        {
            var result = new List<T>();
            BuildLcsHirschberg(a, 0, a.Length, b, 0, b.Length, cmp, result);
            return result;
        }

        public static int ComputeLcsLength<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, IEqualityComparer<T> cmp)
        {
            // Only the forward DP is required for the full LCS length.
            // This runs in O(|a| * |b|) time but only O(min(|a|,|b|)) space.
            return LcsLength(a, b, cmp)[b.Length];
        }

        private static void BuildLcsHirschberg<T>(ReadOnlySpan<T> a, int aStart, int aEnd,
                                                  ReadOnlySpan<T> b, int bStart, int bEnd,
                                                  IEqualityComparer<T> cmp, List<T> output)
        {
            int aLen = aEnd - aStart;
            int bLen = bEnd - bStart;

            if (aLen == 0 || bLen == 0)
                return;

            if (aLen == 1)
            {
                // If single element in a, check if it occurs in b
                T val = a[aStart];
                for (int j = bStart; j < bEnd; j++)
                {
                    if (cmp.Equals(val, b[j]))
                    {
                        output.Add(val);
                        return;
                    }
                }
                return;
            }

            int aMid = aStart + aLen / 2;

            // Compute LCS length arrays for left half and reversed right half
            var lcsLeft = LcsLength(a.Slice(aStart, aMid - aStart), b.Slice(bStart, bLen), cmp);
            var lcsRight = LcsLengthReverse(a.Slice(aMid, aEnd - aMid), b.Slice(bStart, bLen), cmp);

            // Find split index k in b that maximizes lcsLeft[k] + lcsRight[k]
            int splitB = bStart;
            int best = -1;
            for (int k = 0; k <= bLen; k++)
            {
                int leftVal = lcsLeft[k];
                int rightVal = lcsRight[bLen - k];
                if (leftVal + rightVal > best)
                {
                    best = leftVal + rightVal;
                    splitB = bStart + k;
                }
            }

            // Recurse on two halves
            BuildLcsHirschberg(a, aStart, aMid, b, bStart, splitB, cmp, output);
            BuildLcsHirschberg(a, aMid, aEnd, b, splitB, bEnd, cmp, output);
        }

        // LCS length DP that returns array of size (bLen+1) with the LCS lengths for each prefix of b
        private static int[] LcsLength<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, IEqualityComparer<T> cmp)
        {
            int n = a.Length;
            int m = b.Length;
            var prev = new int[m + 1];
            var curr = new int[m + 1];

            for (int i = 1; i <= n; i++)
            {
                // clear curr[0] = 0;
                curr[0] = 0;
                T ai = a[i - 1];
                for (int j = 1; j <= m; j++)
                {
                    if (cmp.Equals(ai, b[j - 1]))
                        curr[j] = prev[j - 1] + 1;
                    else
                        curr[j] = Math.Max(prev[j], curr[j - 1]);
                }
                // swap prev and curr
                var tmp = prev;
                prev = curr;
                curr = tmp;
            }

            // prev now holds the last computed row
            return prev;
        }

        // Compute LCS length for reversed a and reversed b without allocating reversed spans
        // Equivalent to running LcsLength on reversed sequences but implemented by iterating backwards
        private static int[] LcsLengthReverse<T>(ReadOnlySpan<T> a, ReadOnlySpan<T> b, IEqualityComparer<T> cmp)
        {
            int n = a.Length;
            int m = b.Length;
            var prev = new int[m + 1];
            var curr = new int[m + 1];

            for (int i = 1; i <= n; i++)
            {
                curr[0] = 0;
                T ai = a[n - i]; // reversed index
                for (int j = 1; j <= m; j++)
                {
                    if (cmp.Equals(ai, b[m - j])) // reversed b index
                        curr[j] = prev[j - 1] + 1;
                    else
                        curr[j] = Math.Max(prev[j], curr[j - 1]);
                }
                var tmp = prev;
                prev = curr;
                curr = tmp;
            }

            return prev;
        }
    }
}
