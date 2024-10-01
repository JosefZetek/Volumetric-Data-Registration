using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using MathNet.Numerics.Distributions;


namespace DataView
{

    //TODO: This class is ineffective, optimizations needed

    class TestDensity
    {
        //Original radius describing what values are summed together
        //The algorithm used bellow gradually divides transformations into 2 groups one of which is considered relevant. This division continues until there is either only one value or all transformations have 0 distance to the selected one
        //Radius at the end of the algorithm should be equal to the min distance between transformation
        private double radius;

        public Transform3D Find(Transform3D[] transforms)
        {
            Candidate[] candidates = new Candidate[transforms.Length];

            for (int i = 0; i < candidates.Length; i++)
                candidates[i] = new Candidate(transforms[i]);

            //radius by se měl rovnat 2 x počet dělení (log2(transforms.length) x původní hodnota (taková, že je minimální vzdáleností mezi jednotlivými transformacemi)
            radius = Math.Floor(Math.Log(transforms.Length, 2)) * 2 * findSmallestDeviation(candidates);

            Random rnd = new Random();
            return findClosest(candidates, radius, rnd.Next(candidates.Length));

        }

        private Transform3D findClosest(Candidate[] candidates, double radius, int selectedNode)
        {

            printTransformations(candidates);
            TransformationDistances transformationDistances = fillArrayWithDistances(selectedNode, candidates); //the selected node as the initial can be random
            double[] distances = normalizeDistances(transformationDistances);

            ScoreElement<int>[] scores = fillArrayWithScores(distances, radius);

            Array.Sort(scores);

            //If the first score (when sorted should have the highest value) is equal to 0, then all must be 0 and all are approximately the same
            if (scores[0].getScore() == 0)
                return candidates[0].toTransform3D();

            Candidate[] tempCandidates = new Candidate[scores.Length / 2];

            for (int i = 0; i < scores.Length / 2; i++)
                tempCandidates[i] = candidates[scores[i].getCandidateIndex()];

            candidates = tempCandidates;


            if (candidates.Length == 1)
                return candidates[0].toTransform3D();

            return findClosest(candidates, radius / 2, 0);
        }

        private double findSmallestDeviation(Candidate[] candidates)
        {
            TransformationDistances transformationDistances = fillArrayWithDistances(0, candidates); //the selected node as the initial can be random

            double[] distances = normalizeDistances(transformationDistances);

            Array.Sort(distances);

            double minDeviation = double.MaxValue;
            for (int i = 0; i < distances.Length - 1; i++)
            {
                double currentDeviation = distances[i + 1] - distances[i];
                minDeviation = Math.Min(currentDeviation, minDeviation);
            }

            return minDeviation + double.Epsilon;

        }

        /// <summary>
        /// This evaluates the score of transformations by counting how many transformations are closer than radius/2 after normalization
        /// </summary>
        /// <param name="distances">Array with distances of transformations</param>
        /// <param name="radius">Currently used radius</param>
        /// <returns>Returns scores of transformations</returns>
        private ScoreElement<int>[] fillArrayWithScores(double[] distances, double radius)
        {
            ScoreElement<int>[] scores = new ScoreElement<int>[distances.Length];

            for (int i = 0; i < scores.Length; i++)
            {
                //Goes over the whole array (highly ineffective)
                for (int j = 0; j < scores.Length; j++)
                {
                    double upperBound = (distances[i] + radius / 2);
                    double lowerBound = (distances[i] - radius / 2);

                    if ((distances[j] <= upperBound) && (distances[j] >= lowerBound))
                    {

                        if (scores[i] == null)
                            scores[i] = new ScoreElement<int>(1, i); //One is the current score

                        else
                            scores[i].incrementScore();
                    }
                }
            }

            return scores;
        }

        /// <summary>
        /// Prints distances in a line
        /// </summary>
        /// <param name="distances">Array of distances</param>
        private void printDistances(double[] distances)
        {
            for (int i = 0; i < distances.Length; i++)
                Console.Write(distances[i] + " ");

            Console.WriteLine();
        }

        private void printTransformations(Candidate[] candidates)
        {
            for(int i = 0; i<candidates.Length; i++)
            {
                Transform3D a = candidates[i].getOriginalTransform();
                Console.WriteLine(a.RotationMatrix);
                Console.WriteLine(a.TranslationVector);
                Console.WriteLine("-------------------------");
            }
        }

        /// <summary>
        /// Fills array with distances of transformations with respect to the transformation at index selectedNode
        /// </summary>
        /// <param name="selectedNode">Index of selected transformation that is used as a reference</param>
        /// <param name="candidates">Array of candidate transformations</param>
        /// <returns>Returns instance of TransformationDistances that contains the array of distances with respect to the transformation at selectedNode index.</returns>
        private TransformationDistances fillArrayWithDistances(int selectedNode, Candidate[] candidates)
        {

            double[] rotationDistances = new double[candidates.Length];
            double maxRotationDistance = double.MinValue;
            double minRotationDistance = double.MaxValue;

            double[] translationDistances = new double[candidates.Length];
            double maxTranslationDistance = double.MinValue;
            double minTranslationDistance = double.MaxValue;

            for (int i = 0; i < candidates.Length; i++)
            {
                if (i == selectedNode)
                {
                    rotationDistances[i] = 0;
                    maxRotationDistance = Math.Max(0, maxRotationDistance);
                    minRotationDistance = Math.Min(0, minRotationDistance);

                    maxTranslationDistance = Math.Max(0, maxTranslationDistance);
                    minTranslationDistance = Math.Min(0, minTranslationDistance);
                    continue;
                }

                try
                {
                    translationDistances[i] = candidates[selectedNode].TranslationVectorDistance(candidates[i]);
                    maxTranslationDistance = Math.Max(translationDistances[i], maxTranslationDistance);
                    minTranslationDistance = Math.Min(translationDistances[i], minTranslationDistance);

                    rotationDistances[i] = candidates[selectedNode].FrobeniusRotationMatrixDistance(candidates[i]);
                    maxRotationDistance = Math.Max(rotationDistances[i], maxRotationDistance);
                    minRotationDistance = Math.Min(rotationDistances[i], minRotationDistance);
                    
                }
                catch (Exception e)
                {
                    //Transition matrix for compared matrices doesn't exist
                    Console.WriteLine(e.Message);
                    rotationDistances[i] = double.PositiveInfinity;
                }
            }

            //Convert the distances to ulong array
            //Translation distance will be converted to value between 0-2^7-1 and will be located at the bottom 7 bits
            //Rotation distance will be converted to number between 0-2^57-1

            uint[] totalDistances = new uint[candidates.Length];

            uint minTotalDistance = uint.MaxValue;
            uint maxTotalDistance = uint.MinValue;

            for (int i = 0; i < candidates.Length; i++)
            {
                double currentRotationDistance = rotationDistances[i];
                double currentTranslationDistance = translationDistances[i];

                //If the rotation distance is infinity
                if (currentRotationDistance > maxRotationDistance)
                {
                    totalDistances[i] = uint.MaxValue;
                    maxTotalDistance = uint.MaxValue;
                    continue;
                }

                double rotationDenominator = maxRotationDistance - minRotationDistance;
                double translationDenominator = maxTranslationDistance - minTranslationDistance;

                double distanceRotation;
                double distanceTranslation;

                if (Math.Abs(rotationDenominator) < double.Epsilon)
                    distanceRotation = 0;
                else
                    distanceRotation = (currentRotationDistance - minRotationDistance) / rotationDenominator;

                if (Math.Abs(translationDenominator) < double.Epsilon)
                    distanceTranslation = 0;
                else
                    distanceTranslation = (currentTranslationDistance - minTranslationDistance) / translationDenominator;

                //2^57-1 is max distanceRotation

                //2^7-1 is max distanceTranslation

                //Test
                distanceRotation = 0.51;
                distanceTranslation = 0.5;
                
                totalDistances[i] = ((uint)(distanceRotation*(Math.Pow(2, 25) - 1)) << 7);
                totalDistances[i] += (uint)(distanceTranslation * (Math.Pow(2, 7) - 1));

                minTotalDistance = Math.Min(minTotalDistance, totalDistances[i]);
                maxTotalDistance = Math.Max(maxTotalDistance, totalDistances[i]);
                
                //totalDistances[i] += (ulong)(distanceTranslation * (Math.Pow(2, 7) - 1));
                //Console.WriteLine(totalDistances[i]);
            }
             

            return new TransformationDistances(totalDistances, minTotalDistance, maxTotalDistance);
        }


        private void PrintBinary(uint number)
        {
            string binary = Convert.ToString((int)number, 2); // Convert to binary as a string
            Console.WriteLine($"Binary representation of {number} is: {binary}");
        }


        /// <summary>
        /// Normalizes the distances so that they can be interpreted as percentages from the range
        /// </summary>
        /// <param name="transformationDistances">Wrapper object with distances and minimum and maximum distances</param>
        /// <returns>Returns normalized array of distances</returns>
        private double[] normalizeDistances(TransformationDistances transformationDistances)
        {

            uint minDistance = transformationDistances.getMinDistance();
            uint maxDistance = transformationDistances.getMaxDistance();

            uint[] currentValues = transformationDistances.getDistances();

            double[] distances = new double[currentValues.Length];


            for(int i = 0; i<distances.Length; i++)
            {
                double denominator = maxDistance - minDistance;

                //maxDistance == minDistance
                if (denominator == 0)
                    distances[i] = 0;
                else
                    distances[i] = (currentValues[i] - minDistance) / (double)denominator;
            }

            return distances;
        }
    }

    /// <summary>
    /// Class that represents the distances array
    /// </summary>
    public class TransformationDistances
    {
        private uint[] distances;
        private uint maxDistance;
        private uint minDistance;

        public TransformationDistances(uint[] distances, uint minDistance, uint maxDistance)
        {
            this.distances = distances;
            this.maxDistance = maxDistance;
            this.minDistance = minDistance;
        }

        /// <summary>
        /// Returns an array with distances between currently selected
        /// transformation and the transformation at a corresponding index
        /// </summary>
        /// <returns>Returns an array of distances</returns>
        public uint[] getDistances()
        {
            return distances;
        }

        /// <summary>
        /// Returns maximum distance from the array
        /// </summary>
        /// <returns></returns>
        public uint getMaxDistance()
        {
            return maxDistance;
        }

        /// <summary>
        /// Returns minimum distance from the array (should always be the case of distance between selected transformation and itself, thus it would be 0)
        /// </summary>
        /// <returns></returns>
        public uint getMinDistance()
        {
            return minDistance;
        }
    }
}
