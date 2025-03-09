using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataView
{
    public class RegistrationLauncher : IRegistrationLauncher
    {
        /* Registration blocks */
        private ISampler sampler;
        private IFeatureComputer featureComputer;
        private IMatcher matcher;
        private ITransformer transformer;

        private int NUMBER_OF_POINTS_MICRO;
        private int NUMBER_OF_POINTS_MACRO;
        private double THRESHOLD;

        public static Transform3D expectedTransformation = null;

        public RegistrationLauncher()
        {
            this.NUMBER_OF_POINTS_MICRO = 1_000;
            this.NUMBER_OF_POINTS_MACRO = 1_000;
            this.THRESHOLD = 10;

            InitializeRegistrationModules();
        }

        /// <summary>
        /// Constructor for creation of registration launcher
        /// </summary>
        /// <param name="NUMBER_OF_POINTS_MICRO">Number of points to sample in Micro object - at least 1</param>
        /// <param name="NUMBER_OF_POINTS_MACRO">Number of points to sample in Macro object - at least 1</param>
        /// <param name="THRESHOLD">How many % of top matches should be kept - number between 1 - 100</param>
        public RegistrationLauncher(int NUMBER_OF_POINTS_MICRO, int NUMBER_OF_POINTS_MACRO, double THRESHOLD)
        {
            const int MIN_NUMBER_OF_SAMPLES = 1;
            const double MIN_THRESHOLD = 1, MAX_THRESHOLD = 100;

            this.NUMBER_OF_POINTS_MICRO = Math.Max(NUMBER_OF_POINTS_MICRO, MIN_NUMBER_OF_SAMPLES);
            this.NUMBER_OF_POINTS_MACRO = Math.Max(NUMBER_OF_POINTS_MACRO, MIN_NUMBER_OF_SAMPLES);
            this.THRESHOLD = Math.Min(Math.Max(THRESHOLD, MIN_THRESHOLD), MAX_THRESHOLD);

            InitializeRegistrationModules();
        }

        private void InitializeRegistrationModules()
        {
            this.sampler = new SamplerVariance();
            this.featureComputer = new FeatureComputerISOSurfaceCurvature();
            this.matcher = new Matcher();
            this.transformer = new Transformer3D();
        }

        public Transform3D RunRegistration(FilePathDescriptor microDataPath, FilePathDescriptor macroDataPath)
        {
            return RunRegistration(new VolumetricData(microDataPath), new VolumetricData(macroDataPath));
        }

        public void Krivost(AData data)
        {
            List<Point2D> graphPoints = new List<Point2D>();

            Point3D[] points = sampler.Sample(data, NUMBER_OF_POINTS_MACRO);
            for (int i = 0; i<points.Length; i++)
            {
                double distance = points[i].Distance(new Point3D(2.5, 2.5, 2.5));

                graphPoints.Add(new Point2D(1.0 / distance, -featureComputer.ComputeFeatureVector(data, points[i]).Features[1]));
            }

            CSVWriter.WriteResult("Realna krivost X vypocitana krivost 1 (prevraceny znak)", "Realna krivost", "Vypocitana krivost", graphPoints);
        }

        public void ZK(AData microData, AData macroData)
        {
            Point3D[] microPoints = sampler.Sample(microData, NUMBER_OF_POINTS_MICRO);
            Point3D[] macroPoints = sampler.Sample(macroData, NUMBER_OF_POINTS_MACRO);

            GenerateFVGraph(microData, macroData, microPoints, macroPoints);
        }

        public Transform3D RunRegistration(AData microData, AData macroData)
        {
            //Sets Locale to US
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            //----------------------------------------PARAMETERS---------------------------------------------- 

            //----------------------------------------FEATURE VECTOR CALCULATION------------------------------------------------

            

            Debug.Log("Computing micro feature vectors.");
            List<FeatureVector> featureVectorsMicro = CalculateFeatureVectors(sampler, featureComputer, microData, NUMBER_OF_POINTS_MICRO);
            Debug.Log("Computing macro feature vectors.");
            List<FeatureVector> featureVectorsMacro = CalculateFeatureVectors(sampler, featureComputer, macroData, NUMBER_OF_POINTS_MACRO);

            Debug.Log($"Number of feature vectors micro: {featureVectorsMicro.Count}");
            Debug.Log($"Number of feature vectors macro: {featureVectorsMacro.Count}");

            /*
            System.Random random = new System.Random();
            FeatureVector[] featureVectorsMicro = FeatureVectorsMicroData(microData, random, NUMBER_OF_POINTS_MICRO);
            FeatureVector[] featureVectorsMacro = FeatureVectorsMacroData(featureVectorsMicro);
            */

            //CheckValidity(featureVectorsMicro, featureVectorsMacro);

            //----------------------------------------SETUP TRANSFORMATION METRICS------------------------------------------------
            ITransformationDistance transformationDistance = new TransformationDistanceSeven(microData);
            Transform3D.SetTransformationDistance(transformationDistance);

            //----------------------------------------MATCHES-------------------------------------------------
            Debug.Log("Matching.");

            //Match[] matches = matcher.Match(featureVectorsMicro.ToArray(), featureVectorsMacro.ToArray(), THRESHOLD);

            //FakeMatcher matcher = new FakeMatcher(expectedTransformation);
            Match[] matches = matcher.Match(featureVectorsMicro.ToArray(), featureVectorsMacro.ToArray(), THRESHOLD);

            //CompareAlternativeMatches(matches, matchesAlternative);

            //Match[] matches = CreateFakeMatches(microData, Math.Min(NUMBER_OF_POINTS_MACRO, NUMBER_OF_POINTS_MICRO));

            //Match testMatch = new Match(
            //    featureVectorsMicro[0],
            //    featureVectorsMacro[0],
            //    1
            //);

            //Debug.Log($"Test match: {testMatch}");

            PrintRealDistances(matches);

            //------------------------------------GET TRANSFORMATION -----------------------------------------

            Debug.Log("Computing transformations.\n");

            List<Transform3D> transformations = new List<Transform3D>();
            CalculateTransformations(microData, macroData, matches, ref transformations);

            Debug.Log($"Number of transformations: {transformations.Count}");

            DensityStructure densityStructure = new DensityStructure(transformations, 0.1);

            Transform3D tr = densityStructure.TransformationsDensityFilter();
            return tr;
        }


        private List<FeatureVector> CalculateFeatureVectors(ISampler sampler, IFeatureComputer featureComputer, AData data, int NUMBER_OF_POINTS)
        {
            Debug.Log("Sampling.");
            Point3D[] pointsMicro = sampler.Sample(data, NUMBER_OF_POINTS);

            Debug.Log($"Sampled points: {pointsMicro.Length}");
            List<FeatureVector> featureVectors = new List<FeatureVector>();

            Debug.Log("Computing micro feature vectors.");
            for (int i = 0; i < pointsMicro.Length; i++)
            {
                try { featureVectors.Add(featureComputer.ComputeFeatureVector(data, pointsMicro[i])); }
                catch { continue; }
            }

            return featureVectors;
        }

        private void GenerateFVGraph(AData microData, AData macroData, Point3D[] microPoints, Point3D[] macroPoints)
        {

            List<Point2D> distances = new List<Point2D>();
            double realDistance, fvDistance;
            FeatureVector microFV, macroFV;

            SpheresMockData spheresMockData = new SpheresMockData();
            spheresMockData.PrintSpherePositions();

            for(int i = 0; i<Math.Min(microPoints.Length, 100); i++)
            {
                for(int j = 0; j<Math.Min(macroPoints.Length, 100); j++)
                {
                    
                    microFV = featureComputer.ComputeFeatureVector(microData, microPoints[i]);
                    macroFV = featureComputer.ComputeFeatureVector(macroData, macroPoints[j]);

                    //realDistance = microPoints[i].ApplyRotationTranslation(expectedTransformation).Distance(macroPoints[j]);
                    //realDistance = Math.Abs(microPoints[i].Distance(new Point3D(2.5, 2.5, 2.5)) - macroPoints[i].Distance(new Point3D(2.5, 2.5, 2.5)));
                    fvDistance = microFV.DistTo2(macroFV);

                    int sphereIndex1 = spheresMockData.GetSphereIndex(microPoints[i].X, microPoints[i].Y, microPoints[i].Z);
                    int sphereIndex2 = spheresMockData.GetSphereIndex(macroPoints[i].X, macroPoints[i].Y, macroPoints[i].Z);


                    int indexDistance = (sphereIndex1 == 8 || sphereIndex2 == 8) ? 10 : Math.Abs(sphereIndex1 - sphereIndex2);

                    if (fvDistance > 1E4 && indexDistance == 0)
                    {
                        Debug.Log($"Sphere index 1 = {sphereIndex1}");
                        Debug.Log($"Sphere index 2 = {sphereIndex2}");
                        Debug.Log($"");
                        Debug.Log($"Micro FV: {microFV}");
                        Debug.Log($"Macro FV: {macroFV}");
                        Debug.Log($"");
                    }


                    distances.Add(new Point2D(indexDistance, fvDistance));
                }
            }

            CSVWriter.WriteResult("FV - zavislost indexu koule na vzdalenosti FV.csv", "Real distance", "FV Distance", distances);
        }

        private List<Transform3D> CalculateTransformations(AData microData, AData macroData, Match[] matches)
        {
            List<Transform3D> transformations = new List<Transform3D>();

            for (int i = 0; i < matches.Length; i++)
            {
                //Calculate transformation and if the transformation doesnt exist, it will skip it and print out the error message
                try
                {
                    Transform3D transformation = transformer.GetTransformation(matches[i], microData, macroData);
                    transformations.Add(transformation);
                    //Debug.Log("Candidate for transformation: " + transformation);
                    Debug.Log($"Candidate's distance from correct transformation: {transformation.DistanceTo(expectedTransformation)}");
                }
                catch { continue; }
            }

            return transformations;
        }


        /// <summary>
        /// Adds transformations to a list passed by reference
        /// </summary>
        /// <param name="microData"></param>
        /// <param name="macroData"></param>
        /// <param name="matches"></param>
        /// <param name="transformations"></param>
        private void CalculateTransformations(AData microData, AData macroData, Match[] matches, ref List<Transform3D> transformations)
        {
            for (int i = 0; i < matches.Length; i++)
            {
                try
                {
                    transformer.AppendTransformation(matches[i], microData, macroData, ref transformations);
                }
                catch { continue; }
            }
        }

        private void PrintRealDistances(Match[] matches)
        {
            if (expectedTransformation == null)
            {
                Debug.Log("Expected transformation not specified.");
                return;
            }

            for (int i = 0; i < matches.Length; i++)
                Debug.Log(matches[i]
                    .microFV
                    .Point
                    .ApplyRotationTranslation(expectedTransformation)
                    .Distance(matches[i].macroFV.Point)
                );
        }

        private Point3D RandomOffsetPoint(System.Random random, double maxValue)
        {
            return new Point3D(
                random.NextDouble() * maxValue,
                random.NextDouble() * maxValue,
                random.NextDouble() * maxValue
            );
        }

        private Match[] CreateFakeMatches(AData microData, int count)
        {
            Match[] matches = new Match[count];

            Point3D[] microPoints = this.sampler.Sample(microData, count);
            System.Random random = new System.Random();


            for (int i = 0; i < count; i++)
            {
                matches[i] = new Match(
                    new FeatureVector(microPoints[i], new double[1]),
                    new FeatureVector(microPoints[i].ApplyRotationTranslation(expectedTransformation), new double[1]),
                    1
                );
            }
            return matches;
        }

        private FeatureVector[] FeatureVectorsMicroData(AData microData, System.Random random, int count)
        {
            FeatureVector[] featureVectorsMicro = new FeatureVector[count];
            Point3D currentPoint;
            Point3D transformedPoint;

            for (int i = 0; i < count; i++)
            {
                currentPoint = GenerateRandomPoint(random, microData.MaxValueX, microData.MaxValueY, microData.MaxValueZ);
                transformedPoint = currentPoint.ApplyRotationTranslation(expectedTransformation);
                featureVectorsMicro[i] = (new FeatureVector(currentPoint, new double[3] { transformedPoint.X, transformedPoint.Y, transformedPoint.Z }));
            }

            return featureVectorsMicro;
        }

        private FeatureVector[] FeatureVectorsMacroData(FeatureVector[] microFeatureVectors)
        {
            FeatureVector[] featureVectorsMacro = new FeatureVector[microFeatureVectors.Length] ;
            Point3D currentPoint;

            for (int i = 0; i< microFeatureVectors.Length; i++)
            {
                currentPoint = microFeatureVectors[i].Point.ApplyRotationTranslation(expectedTransformation);
                featureVectorsMacro[i] = new FeatureVector(currentPoint, new double[3] {currentPoint.X, currentPoint.Y, currentPoint.Z});
            }

            return featureVectorsMacro;
        }

        private void CheckValidity(List<FeatureVector> microFV, List<FeatureVector> macroFV)
        {
            Debug.Log("Checking validity");
            for(int i = 0; i<microFV.Count; i++)
            {
                double distanceFromMacro = macroFV[i].Point.Translate(-expectedTransformation.TranslationVector).Rotate(expectedTransformation.RotationMatrix.Transpose()).Distance(microFV[i].Point);
                double distanceFromMicro = microFV[i].Point.Rotate(expectedTransformation.RotationMatrix).Translate(expectedTransformation.TranslationVector).Distance(macroFV[i].Point);
                Debug.Log($"Distance of FV Points {distanceFromMacro}, {distanceFromMicro}");

            }
            Debug.Log("Validity checked");
        }

        private Point3D GenerateRandomPoint(System.Random random, double maxX, double maxY, double maxZ)
        {
            return new Point3D(
                    random.NextDouble() * maxX,
                    random.NextDouble() * maxY,
                    random.NextDouble() * maxZ
            );
        }

        private void ShuffleFVList(List<FeatureVector> featureVectors)
        {
            System.Random random = new System.Random();
            FeatureVector temp;
            int randomIndex;
            for(int i = 0; i < featureVectors.Count; i++)
            {
                temp = featureVectors[i];
                randomIndex = random.Next(featureVectors.Count);

                featureVectors[i] = featureVectors[randomIndex];
                featureVectors[randomIndex] = temp;
            }
        }


    }
}