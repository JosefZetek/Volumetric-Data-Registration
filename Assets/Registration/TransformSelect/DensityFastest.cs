
using System;
using System.Collections.Generic;
using System.Linq;

namespace DataView
{
    public class DensityFastest
    {
        public Transform3D Find(Transform3D[] transforms)
        {

            Candidate[] candidates = new Candidate[transforms.Length];

            for (int i = 0; i < candidates.Length; i++)
                candidates[i] = new Candidate(transforms[i]);

            //Mozna ten index by se mohl vybirat podle toho, jestli to pole neobsahuje nejako vzdalenost, co by byla nekonecno, tak potom bych vymyazal ten argument ty funkce a zjistovala by si to sama
            List<TransformationComparisonWithIndex> transformationComparisonsArray = getTransformationComparisonList(candidates, 0);

            int part = 2;

            double startRadius = TransformationComparisonWithIndex.getValueAtNthPart(part);

            if (double.IsInfinity(startRadius))
            {
                part *= 2;
                startRadius = TransformationComparisonWithIndex.getValueAtNthPart(part);
            }


            return findClosest(transformationComparisonsArray, candidates, startRadius);
        }

        /// <summary>
        /// This method calculates distance from a single transformation to all transformations
        /// </summary>
        /// <param name="candidates">List of candidate transformations</param>
        /// <returns>Returns list with transformation comparisons</returns>
        private List<TransformationComparisonWithIndex> getTransformationComparisonList(Candidate[] candidates, int selectedNode)
        {
            double translationDistance;
            double rotationDistance;


            Console.WriteLine("Calculating distances from " + selectedNode);
            List<TransformationComparisonWithIndex> transformationsSimilarity = new List<TransformationComparisonWithIndex>();

            for (int i = 0; i < candidates.Length; i++)
            {
                //The list for a particular transformation will include the reference transformation as well
                if (i == selectedNode)
                {
                    transformationsSimilarity.Add(new TransformationComparisonWithIndex(i, 0, 0));
                    continue;
                }

                try
                {
                    translationDistance = candidates[selectedNode].TranslationVectorDistance(candidates[i]);
                    rotationDistance = candidates[selectedNode].FrobeniusRotationMatrixDistance(candidates[i]);

                    transformationsSimilarity.Add(new TransformationComparisonWithIndex(i, translationDistance, rotationDistance));
                }
                catch (Exception e)
                {
                    //Transition matrix for compared matrices doesn't exist, so the transformation can be skipped
                    Console.WriteLine(e.Message);
                    continue;
                }
            }
            return transformationsSimilarity;
        }


        private Transform3D findClosest(List<TransformationComparisonWithIndex> transformationComparisons, Candidate[] candidates, double radius)
        {

            if (candidates.Length == 1)
                return candidates[0].toTransform3D();

            //Intialization of transformation score array
            TransformationScore[] transformationScore = new TransformationScore[transformationComparisons.Count];

            for (int i = 0; i < transformationScore.Length; i++)
                transformationScore[i] = new TransformationScore(transformationComparisons[i].getTransformationIndex());


            for (int i = 0; i < transformationComparisons.Count; i++)
            {
                for(int j = 0; j<transformationComparisons.Count; j++)
                {
                    double rotationDistance = transformationComparisons[i].getRotationDistance() - transformationComparisons[j].getRotationDistance();
                    double translationDistance = transformationComparisons[i].getTranslationDistance() - transformationComparisons[j].getTranslationDistance();

                    double distance = Math.Sqrt(Math.Pow(rotationDistance, 2) + Math.Pow(translationDistance, 2)); 
                    if(distance<= radius)
                    {
                        //Test if the transformations are close one to each other
                        translationDistance = candidates[i].TranslationVectorDistance(candidates[j]);
                        rotationDistance = candidates[i].FrobeniusRotationMatrixDistance(candidates[j]);
                        distance = Math.Sqrt(Math.Pow(rotationDistance, 2) + Math.Pow(translationDistance, 2));

                        if(distance<= radius)
                        {
                            transformationScore[i].incrementTransformationScore();
                            transformationScore[j].incrementTransformationScore();
                        }
                    }
                }
            }

            Array.Sort(transformationScore);

            for (int i = 0; i < transformationScore.Length; i++)
            {
                Console.WriteLine("Transformation at index: " + transformationScore[i].getTransformationIndex() + " has a score of: " + transformationScore[i].getTransformatinScore());
                Console.WriteLine("Transformation: " + candidates[transformationScore[i].getTransformationIndex()].toTransform3D());

            }


            Candidate[] tempCandidates = new Candidate[transformationScore.Length/2];

            for (int i = 0; i < tempCandidates.Length; i++)
                tempCandidates[i] = candidates[transformationScore[i].getTransformationIndex()];

            candidates = tempCandidates;

            return findClosest(transformationComparisons, candidates, radius / 2);
        }
    }

    /// <summary>
    /// Represents the similarity of transformations
    /// </summary>
    public class TransformationComparisonWithIndex : IComparable<TransformationComparisonWithIndex>
    {
        private int transformationIndex;
        private double translationDistance;
        private double rotationDistance;
        private double distanceFromOrigin;

        private static SortedSet<double> distances = new SortedSet<double>();


        public TransformationComparisonWithIndex(int transformationIndex, double translationDistance, double rotationDistance)
        {
            this.transformationIndex = transformationIndex;
            this.translationDistance = translationDistance;
            this.rotationDistance = rotationDistance;

            this.distanceFromOrigin = Math.Sqrt(Math.Pow(translationDistance, 2) + Math.Pow(rotationDistance, 2));

            distances.Add(this.distanceFromOrigin);
        }

        /// <summary>
        /// Returns the median distance that is a real number (not infinity)
        /// </summary>
        /// <returns>Returns the median distance</returns>
        public static double getValueAtNthPart(int part)
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
        /// Getter for transformation index at Candidates
        /// array passed to the initializer
        /// </summary>
        /// <returns>Returns the index of a transformation</returns>
        public int getTransformationIndex()
        {
            return transformationIndex;
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
        public int CompareTo(TransformationComparisonWithIndex other)
        {
            return Math.Sign(other.getDistanceFromOrigin() - this.getDistanceFromOrigin());
        }
    }

    /// <summary>
    /// This class should represent how many transformations for each of them are within radius
    /// </summary>
    public class TransformationScore : IComparable<TransformationScore>
    {
        int transformationIndex;
        int score = 0;

        public TransformationScore(int transformationIndex)
        {
            this.transformationIndex = transformationIndex;
        }

        public int getTransformationIndex()
        {
            return transformationIndex;
        }

        public int getTransformatinScore()
        {
            return score;
        }

        public void incrementTransformationScore()
        {
            if (score == int.MaxValue)
                return;

            score++;
            
        }

        public int CompareTo(TransformationScore other)
        {
            return other.getTransformatinScore() - this.getTransformatinScore();
        }
    }
}
