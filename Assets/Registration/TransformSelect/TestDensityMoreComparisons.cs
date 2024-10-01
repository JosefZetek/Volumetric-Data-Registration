using System;
using System.Collections.Generic;

namespace DataView
{
    //TODO: This class is ineffective, optimizations needed

    class TestDensityMoreComparisons
    {
        public Transform3D Find(Transform3D[] transforms)
        {
            Candidate[] candidates = new Candidate[transforms.Length];

            for (int i = 0; i < candidates.Length; i++)
                candidates[i] = new Candidate(transforms[i]);

            return findClosest(candidates);
        }

        private Transform3D findClosest(Candidate[] candidates)
        {
            if (candidates.Length == 1)
                return candidates[0].toTransform3D();

            //This list represents close candidates
            List<SimilarTransformation>[] arrayOfSimilarCandidatesList = new List<SimilarTransformation>[candidates.Length];

            double translationDistance;
            double rotationDistance;

            for (int selectedNode = 0; selectedNode < candidates.Length; selectedNode++)
            {
                //Intialize list and assign it to all candidates
                List<SimilarTransformation> similarTransformationsList = new List<SimilarTransformation>();
                arrayOfSimilarCandidatesList[selectedNode] = similarTransformationsList;


                for (int i = 0; i < candidates.Length; i++)
                {
                    //The list for a particular transformation will include the reference transformation as well
                    if (i == selectedNode)
                    {
                        arrayOfSimilarCandidatesList[selectedNode].Add(new SimilarTransformation(i, 0, 0, candidates[i].toTransform3D()));
                        continue;
                    }

                    try
                    {
                        translationDistance = candidates[selectedNode].TranslationVectorDistance(candidates[i]);
                        rotationDistance = candidates[selectedNode].FrobeniusRotationMatrixDistance(candidates[i]);

                        arrayOfSimilarCandidatesList[selectedNode].Add(new SimilarTransformation(i, translationDistance, rotationDistance, candidates[i].toTransform3D()));
                    }
                    catch (Exception e)
                    {
                        //Transition matrix for compared matrices doesn't exist, so the transformation can be skipped
                        Console.WriteLine(e.Message);
                        continue;
                    }
                }
            }

            foreach (List<SimilarTransformation> listOfSimilarTransformations in arrayOfSimilarCandidatesList)
            {
                foreach (SimilarTransformation similarTransformation in listOfSimilarTransformations)
                {
                    similarTransformation.normalizeDistances();
                    similarTransformation.calculateTotalDistance();
                }
            }



            ScoreElement<double>[] scores = new ScoreElement<double>[candidates.Length];

            for (int i = 0; i < arrayOfSimilarCandidatesList.Length; i++)
            {
                double currentScore = 0;

                //Sum off all scores
                foreach (SimilarTransformation similarTransformation in arrayOfSimilarCandidatesList[i])
                    currentScore += similarTransformation.getTransformationScore();

                scores[i] = new ScoreElement<double>(currentScore, i);
            }

            Array.Sort(scores);

            Candidate[] filteredCandidates = new Candidate[candidates.Length / 2];

            HashSet<int> usedIndexes = new HashSet<int>();

            //PRINT POINT CLUSTERS BEFORE REDUCTION

            /*
            for (int i = 0; i < scores.Length; i++)
            {
                int index = scores[i].getCandidateIndex();
                int referenceTransformationIndex = arrayOfSimilarCandidatesList[index][index].getIndex();
                Console.WriteLine("Point cluster related to transformation: " +  candidates[referenceTransformationIndex].toTransform3D() + "with score: " + scores[i].getScore());

                arrayOfSimilarCandidatesList[index].Sort();
                for (int j = 0; j < arrayOfSimilarCandidatesList[index].Count; j++)
                {
                    int currentTransformation = arrayOfSimilarCandidatesList[index][j].getIndex();

                    double transformationScore = arrayOfSimilarCandidatesList[index][currentTransformation].getTransformationScore();

                    bool isUsed = true;


                    if (!(usedIndexes.Count < (filteredCandidates.Length)))
                        isUsed = false;


                    Console.WriteLine("Transformation with score: " + transformationScore + "; is used = " + isUsed);
                    Console.WriteLine(candidates[currentTransformation].toTransform3D());
                    Console.WriteLine("----------------------");
                }

                Console.WriteLine();
                Console.WriteLine();
            }
            */

            usedIndexes = new HashSet<int>();



            //Filter candidates
            for (int i = 0; i < scores.Length && usedIndexes.Count < (filteredCandidates.Length); i++)
            {
                int currentCandidateIndex = scores[i].getCandidateIndex();
                arrayOfSimilarCandidatesList[currentCandidateIndex].Sort();
                for (int j = 0; i < arrayOfSimilarCandidatesList[currentCandidateIndex].Count && usedIndexes.Count < (filteredCandidates.Length); j++)
                {
                    int index = arrayOfSimilarCandidatesList[currentCandidateIndex][j].getIndex();

                    //Candidate already used
                    if (usedIndexes.Contains(index))
                        continue;

                    usedIndexes.Add(index);
                    filteredCandidates[usedIndexes.Count - 1] = candidates[index];
                }
            }

            candidates = filteredCandidates;
            SimilarTransformation.resetMaxValues();

            return findClosest(candidates);
        }
    }

    /// <summary>
    /// Represents the similarity of transformations
    /// </summary>
    class SimilarTransformation : IComparable<SimilarTransformation>
    {
        private static uint maxTotalDistance = uint.MaxValue;

        private static double maxTranslationDistance = double.MinValue;
        private static double maxRotationDistance = double.MinValue;

        private int indexInCandidatesArray;

        private uint totalDistance;

        private double totalDistanceNormalized;

        private double translationDistance;
        private double rotationDistance;

        private Transform3D similarTransformation;

        public SimilarTransformation(int indexInCandidatesArray, double translationDistance, double rotationDistance, Transform3D similarTransformation)
        {
            this.indexInCandidatesArray = indexInCandidatesArray;
            this.translationDistance = translationDistance;
            this.rotationDistance = rotationDistance;
            this.similarTransformation = similarTransformation;

            maxRotationDistance = Math.Max(maxRotationDistance, rotationDistance);
            maxTranslationDistance = Math.Max(maxTranslationDistance, translationDistance);
        }


        public int getIndex()
        {
            return indexInCandidatesArray;
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
        /// 
        /// </summary>
        /// <returns></returns>
        public Transform3D getSimilarTransformation()
        {
            return similarTransformation;
        }

        /// <summary>
        /// Normalizes given value
        /// </summary>
        /// <param name="value">Value to be normalized</param>
        /// <param name="maxValue">Maximum value that is equal to 1</param>
        /// <returns></returns>
        private double normalizeDistance(double value, double maxValue)
        {
            if (Math.Abs(maxValue) < double.Epsilon)
                return 0;
            return value /= maxValue;
        }

        /// <summary>
        /// Normalizes the value based on the static maxValue attribute
        /// </summary>
        public void normalizeDistances()
        {
            rotationDistance = normalizeDistance(rotationDistance, maxRotationDistance);
            translationDistance = normalizeDistance(translationDistance, maxTranslationDistance);
        }

        /// <summary>
        /// This method inverts passed normalized distances, see example bellow
        /// 1 => 0
        /// 0 => 1
        /// 0.4 => 0.6
        /// </summary>
        /// <param name="normalizedDistance">Normalized distance between 0-1</param>
        /// <returns>Returns the inverse of a normalized distance</returns>
        private double invertNormalizedDistance(double normalizedDistance)
        {
            return (1 - normalizedDistance);
        }


        //TODO: Method calculateTotalDistance() can adjust its accuracy based on the max rotations and translations distances

        /// <summary>
        /// It is required to first normalize distances
        /// This method calculates a value representing transformations distance with relatively high accuracy
        /// </summary>
        public void calculateTotalDistance()
        {
            const int BITS_TOTAL = 32;
            const int BITS_ROTATION = 25;
            const int BITS_TRANSLATION = BITS_TOTAL - BITS_ROTATION;


            totalDistance = ((uint)(rotationDistance * (Math.Pow(2, BITS_ROTATION) - 1)) << BITS_TRANSLATION);
            totalDistance += (uint)(translationDistance * (Math.Pow(2, BITS_TRANSLATION) - 1));

            maxTotalDistance = Math.Max(totalDistance, maxTotalDistance);
        }

        /// <summary>
        /// Normalizes total distance
        /// </summary>
        public void normalizeTotalDistance()
        {
            totalDistanceNormalized = totalDistance / (double)maxTotalDistance;
        }

        /// <summary>
        /// Returns the distance 
        /// </summary>
        /// <returns></returns>
        public double getTransformationScore()
        {
            totalDistanceNormalized = normalizeDistance(totalDistance, maxTotalDistance);
            return invertNormalizedDistance(totalDistanceNormalized);
        }

        /// <summary>
        /// Resets max values for rotation and translation distances
        /// </summary>
        public static void resetMaxValues()
        {
            maxRotationDistance = double.MinValue;
            maxTranslationDistance = double.MinValue;
            maxTotalDistance = uint.MinValue;
        }

        public int CompareTo(SimilarTransformation other)
        {
            return Math.Sign(other.getTransformationScore() - this.getTransformationScore());
        }
    }

    /// <summary>
    /// Wrapper object for score and candidate index
    /// </summary>
    public class ScoreElement<T> : IComparable<ScoreElement<T>> where T : struct, IComparable<T>
    {
        private T score;
        private int candidateIndex;

        /// <summary>
        /// Wrapper for score and candidate index
        /// </summary>
        /// <param name="score">Score of a candidate transformation at candidateIndex</param>
        /// <param name="candidateIndex">Index of the particular candidate in candidate array</param>
        public ScoreElement(T score, int candidateIndex)
        {
            this.score = score;
            this.candidateIndex = candidateIndex;
        }

        /// <summary>
        /// Getter for score
        /// </summary>
        /// <returns>Returns the score of a candidate at candidateIndex</returns>
        public T getScore()
        {
            return score;
        }

        /// <summary>
        /// Getter for candidate index
        /// </summary>
        /// <returns>Returns the index of a candidate.</returns>
        public int getCandidateIndex()
        {
            return candidateIndex;
        }

        public void incrementScore()
        {
            if (!(typeof(T) == typeof(int)))
                throw new Exception("This method is supposed to be used only in combination with ScoreElement<int>");

            object scoreObject = score;
            int scoreNumber = (int)scoreObject;
            scoreNumber++;
            scoreObject = scoreNumber;

            score = (T)scoreObject;
        }

        /// <summary>
        /// Descending order used
        /// </summary>
        /// <param name="other">Other instance of ScoreElement</param>
        /// <returns>
        /// Returns positive number if this instance has lower score than other.
        /// If this instance has higher score, the return value is negative.
        /// If equal, it returns 0
        /// </returns>
        public int CompareTo(ScoreElement<T> other)
        {
            object otherObject = other.getScore();
            object thisObject = getScore();

            if (typeof(T) == typeof(int))
            {
                int otherScore = (int)otherObject;
                int thisScore = (int)thisObject;
                return otherScore - thisScore;
            }
            else if (typeof(T) == typeof(double))
            {
                double otherScore = (double)otherObject;
                double thisScore = (double)thisObject;
                return Math.Sign(otherScore - thisScore);
            }
            else
                throw new Exception("The generic parameter T is supposed to be double or int");

        }
    }
}