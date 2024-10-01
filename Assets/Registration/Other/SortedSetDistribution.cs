using System;
using System.Collections.Generic;
using System.Drawing;

namespace DataView
{
    /// <summary>
    /// This class will calculate the percentile of a given point
    /// Used in computation of feature vector
    /// </summary>
    public class SortedSetDistribution
    {

        SortedSet<int> sortedValues = new SortedSet<int>();

        public void AddValue(int value)
        {
            sortedValues.Add(value);
        }

        public double GetDistributionPercentage(int value)
        {
            //
            SortedSet<int> smallerSet = sortedValues.GetViewBetween(int.MinValue, value - 1);
            SortedSet<int> biggerSet = sortedValues.GetViewBetween(value + 1, int.MaxValue);


            Console.WriteLine("This is the biggest from smaller: " + smallerSet.Max);
            Console.WriteLine("This is the smallerst from bigger: " + biggerSet.Min);

            if(sortedValues.Contains(value))
                return smallerSet.Max;

            int countLessThanOrEqualTo = sortedValues.GetViewBetween(int.MinValue, value).Count;
            double percentile = (double)countLessThanOrEqualTo / sortedValues.Count * 100;

            return percentile;
        }
    }
}

