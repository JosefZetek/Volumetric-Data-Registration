
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataView
{
    /// <summary>
    /// This class calculates rotation and translation distances for transformations
    /// It uses median as a radius
    /// Calculates how many transformations are closer than the range (median in the start)
    /// Then radius gets divided by 2 in each iteration
    /// If the radius is smaller than 1, return transformation with maximum number of points within the bounds
    /// If the biggest number of points within the radius is smaller than {threshold} percentage of total number of transformations => returns transformation with biggest score from previous iteration
    /// </summary>

    class TestDensityAccurate
    {
        public Transform3D Find(Transform3D[] transforms)
        {

            Candidate[] candidates = new Candidate[transforms.Length];

            for (int i = 0; i < candidates.Length; i++)
                candidates[i] = new Candidate(transforms[i]);

            List<TransformationComparison>[] transformationComparisonsArray = getTransformationComparisonList(candidates);

            int part = 2;

            double startRadius = TransformationComparison.getMedianDistance(part);

            if (double.IsInfinity(startRadius))
            {
                part *= 2;
                startRadius = TransformationComparison.getMedianDistance(part);
            }


            return findClosest(transformationComparisonsArray, candidates, startRadius, 0);
        }

        /// <summary>
        /// This method calculates distance between all transformations
        /// </summary>
        /// <param name="candidates">List of candidate transformations</param>
        /// <returns>Returns list with transformation comparisons</returns>
        private List<TransformationComparison>[] getTransformationComparisonList(Candidate[] candidates)
        {
            List<TransformationComparison>[] arrayOfTransformationComparisonList = new List<TransformationComparison>[candidates.Length];

            double translationDistance;
            double rotationDistance;

            for (int selectedNode = 0; selectedNode < candidates.Length; selectedNode++)
            {

                Console.WriteLine("Calculating distances from " + selectedNode);
                //Intialize list and assign it to all candidates
                List<TransformationComparison> similarTransformationsList = new List<TransformationComparison>();
                arrayOfTransformationComparisonList[selectedNode] = similarTransformationsList;


                for (int i = 0; i < candidates.Length; i++)
                {
                    //The list for a particular transformation will include the reference transformation as well
                    if (i == selectedNode)
                    {
                        arrayOfTransformationComparisonList[selectedNode].Add(new TransformationComparison(0, 0));
                        continue;
                    }

                    try
                    {
                        translationDistance = candidates[selectedNode].TranslationVectorDistance(candidates[i]);
                        rotationDistance = candidates[selectedNode].FrobeniusRotationMatrixDistance(candidates[i]);

                        arrayOfTransformationComparisonList[selectedNode].Add(new TransformationComparison(translationDistance, rotationDistance));
                    }
                    catch (Exception e)
                    {
                        //Transition matrix for compared matrices doesn't exist, so the transformation can be skipped
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
            }

            return arrayOfTransformationComparisonList;
        }


        private Transform3D findClosest(List<TransformationComparison>[] transformationComparisons, Candidate[] candidates, double radius, int previouslySelected)
        {
            //Smallest cluster will be considered with minimally 5% transformations in its surrounding
            const double threshold = 5;

            int[] values = new int[candidates.Length];
            int maxValue = int.MinValue;
            int maxIndex = -1;


            for(int i = 0; i<candidates.Length; i++)
            {

                int inRange = 0;

                foreach (TransformationComparison transformationComparison in transformationComparisons[i])
                {
                    if (transformationComparison.getDistanceFromOrigin() <= radius)
                        inRange++;
                }

                values[i] = inRange;

                if(maxValue < inRange)
                {
                    maxValue = inRange;
                    maxIndex = i;
                }

            }

            /*
            for(int i = 0; i<values.Length; i++)
                Console.WriteLine("Value: " + values[i] + ", index: " + i);
            */

            if (maxValue < ((transformationComparisons.Length * threshold) / 100))
                return candidates[previouslySelected].toTransform3D();
            if (radius < 1)
                return candidates[maxIndex].toTransform3D();

            return findClosest(transformationComparisons, candidates, radius/2, maxIndex);
        }
    }

    /// <summary>
    /// Represents the similarity of transformations
    /// </summary>
    public class TransformationComparison: IComparable<TransformationComparison>
    {
        private double translationDistance;
        private double rotationDistance;
        private double distanceFromOrigin;

        private static SortedSet<double> distances = new SortedSet<double>();
        

        public TransformationComparison(double translationDistance, double rotationDistance)
        {
            this.translationDistance = translationDistance;
            this.rotationDistance = rotationDistance;

            this.distanceFromOrigin = Math.Sqrt(Math.Pow(translationDistance, 2) + Math.Pow(rotationDistance, 2));

            distances.Add(this.distanceFromOrigin);
        }

        /// <summary>
        /// Returns the median distance that is a real number (not infinity)
        /// </summary>
        /// <returns>Returns the median distance</returns>
        public static double getMedianDistance(int part)
        {
            
            return distances.ElementAt(distances.Count / part);
        }

        /// <summary>
        /// Getter for translation distance
        /// </summary>
        /// <returns></returns>
        public double getTranslationDistance()
        {
            return translationDistance;
        }

        /// <summary>
        /// Getter for rotation distance
        /// </summary>
        /// <returns></returns>
        public double getRotationDistance()
        {
            return rotationDistance;
        }

        /// <summary>
        /// Returns distance from origin
        /// </summary>
        /// <returns>Returns a result of sqrt(translationDistance^2 + rotationDistance^2).</returns>
        public double getDistanceFromOrigin()
        {
            return distanceFromOrigin;
        }

        /// <summary>
        /// Compares instances based on their distances from origin
        /// </summary>
        /// <param name="other">Other instance</param>
        /// <returns>
        /// Returns positive number if other instance is bigger than the one that's been used to call this method
        /// Returns negative number if other instance is smaller than the one that's been used to call this method
        /// Returns 0 if their distances are same
        /// </returns>
        public int CompareTo(TransformationComparison other)
        {
            return Math.Sign(other.getDistanceFromOrigin() - this.getDistanceFromOrigin());
        }
    }
}
