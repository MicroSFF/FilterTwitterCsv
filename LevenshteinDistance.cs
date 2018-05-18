using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FilterTwitterCsv
{
    public static class LevenshteinDistance
    {
        /// <summary>
        /// Computes the Levenshtein distance between two strings
        /// From https://stackoverflow.com/questions/6944056/c-sharp-compare-string-similarity#6944095
        /// Licenced under CC BY-SA https://creativecommons.org/licenses/by-sa/4.0/
        /// </summary>
        /// <param name="lhs">fist strign</param>
        /// <param name="rhs">second string</param>
        /// <returns>Number of changes needed to go from lhs to rhs</returns>
        public static int Compute(string lhs, string rhs)
        {
            if (string.IsNullOrEmpty(lhs))
            {
                if (string.IsNullOrEmpty(rhs))
                    return 0;
                return rhs.Length;
            }

            if (string.IsNullOrEmpty(rhs))
            {
                return lhs.Length;
            }

            int n = lhs.Length;
            int m = rhs.Length;
            int[,] d = new int[n + 1, m + 1];

            // initialize the top and right of the table to 0, 1, 2, ...
            for (int i = 0; i <= n; d[i, 0] = i++) ;
            for (int j = 1; j <= m; d[0, j] = j++) ;

            for (int i = 1; i <= n; i++)
            {
                for (int j = 1; j <= m; j++)
                {
                    int cost = (rhs[j - 1] == lhs[i - 1]) ? 0 : 1;
                    int min1 = d[i - 1, j] + 1;
                    int min2 = d[i, j - 1] + 1;
                    int min3 = d[i - 1, j - 1] + cost;
                    d[i, j] = Math.Min(Math.Min(min1, min2), min3);
                }
            }
            return d[n, m];
        }
    }
}
