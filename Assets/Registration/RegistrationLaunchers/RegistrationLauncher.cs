using MathNet.Numerics.LinearAlgebra.Factorization;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace DataView
{
    public class RegistrationLauncher : IRegistrationLauncher
    {
        /* Registration blocks */
        private ISampler sampler;
        private AFeatureComputer featureComputer;
        private IMatcher matcher;
        private ATransformer transformer;

        public static Transform3D expectedTransformation = null;

        public RegistrationLauncher()
        {
            InitializeRegistrationModules();
        }

        private void InitializeRegistrationModules()
        {
            //RegistrationLauncher.expectedTransformation = TransformationIO.FetchTransformation("/Users/pepazetek/Desktop/Tests/TEST2/microData5.txt");
            //this.featureComputer = new CompoundFeatureComputer(new AFeatureComputer[] { new FeatureComputerISOCurvature()});
            //this.featureComputer = new FakeFeatureComputer(expectedTransformation);

            UniformSphereSampler uniformSphereSampler = new UniformSphereSampler();

            this.sampler = new SamplerPercentile(0.1);
            this.featureComputer = new CompoundFeatureComputer(new AFeatureComputer[] {
                new FeatureComputerISOCurvature(),
                new FeatureComputerPCALength(uniformSphereSampler),
                new FeatureComputerGradient()
                //new FeatureComputerQuantiles()
        });
            this.matcher = new Matcher();
            this.transformer = new UniformRotationComputerPCA();
        }

        public Transform3D RunRegistration(FilePathDescriptor microDataPath, FilePathDescriptor macroDataPath)
        {
            return RunRegistration(new VolumetricData(microDataPath), new VolumetricData(macroDataPath));
        }

        public void Krivost(AData data)
        {
            List<Point2D> graphPoints = new List<Point2D>();

            Point3D[] points = sampler.Sample(data, Constants.NUMBER_OF_POINTS_MACRO);
            for (int i = 0; i<points.Length; i++)
            {
                double distance = points[i].Distance(new Point3D(2.5, 2.5, 2.5));

                graphPoints.Add(new Point2D(1.0 / distance, -featureComputer.ComputeFeatureVector(data, points[i]).Features[1]));
            }

            CSVWriter.WriteResult("Realna krivost X vypocitana krivost 1 (prevraceny znak)", "Realna krivost", "Vypocitana krivost", graphPoints);
        }

        public void ZK(AData microData, AData macroData)
        {
            Point3D[] microPoints = sampler.Sample(microData, Constants.NUMBER_OF_POINTS_MICRO);
            Point3D[] macroPoints = sampler.Sample(macroData, Constants.NUMBER_OF_POINTS_MACRO);

            GenerateFVGraph(microData, macroData, microPoints, macroPoints);
        }

        public Transform3D RunRegistration(AData microData, AData macroData)
        {
            //Sets Locale to US
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            //----------------------------------------PARAMETERS---------------------------------------------- 

            //----------------------------------------FEATURE VECTOR CALCULATION------------------------------------------------

            

            // var fc = (FakeFeatureComputer)featureComputer;

            //fc.SetTransformedSampling(true);
            Point3D[] pointsMicro = sampler.Sample(microData, Constants.NUMBER_OF_POINTS_MICRO);

            Console.WriteLine("Computing micro feature vectors.");
            List<FeatureVector> featureVectorsMicro = CalculateFeatureVectors(microData, featureComputer, pointsMicro);

            //fc.SetTransformedSampling(false);
            Point3D[] pointsMacro = sampler.Sample(macroData, Constants.NUMBER_OF_POINTS_MACRO);

            Console.WriteLine("Computing macro feature vectors.");
            List<FeatureVector> featureVectorsMacro = CalculateFeatureVectors(macroData, featureComputer, pointsMacro);

            Console.WriteLine($"Number of feature vectors micro: {featureVectorsMicro.Count}");
            Console.WriteLine($"Number of feature vectors macro: {featureVectorsMacro.Count}");

            FeatureNormalizer featureNormalizer = new FeatureNormalizer(featureVectorsMacro, featureVectorsMacro);
            featureVectorsMicro = featureNormalizer.NormalizeList(featureVectorsMicro);
            featureVectorsMacro = featureNormalizer.NormalizeList(featureVectorsMacro);

            //----------------------------------------SETUP TRANSFORMATION METRICS------------------------------------------------
            ITransformationDistance transformationDistance = new TransformationDistanceSeven(microData);
            Transform3D.SetTransformationDistance(transformationDistance);

            //----------------------------------------MATCHES-------------------------------------------------
            Console.WriteLine("Matching.");

            Match[] matches = matcher.Match(featureVectorsMicro.ToArray(), featureVectorsMacro.ToArray(), Constants.THRESHOLD);
            //Match[] matches = SampleGreedy(this.sampler, this.featureComputer, microData, featureVectorsMacro.ToArray());
            //CompareAlternativeMatches(matches, matchesAlternative);

            PrintRealDistances(matches);

            //------------------------------------GET TRANSFORMATION -----------------------------------------

            Console.WriteLine("Computing transformations.\n");

            List<Transform3D> transformations = CalculateTransformations(microData, macroData, matches);

            Console.WriteLine($"Number of transformations: {transformations.Count}");

            DensityStructure densityStructure = new DensityStructure(transformations, 0.1);

            Transform3D tr = densityStructure.TransformationsDensityFilter();

            if (expectedTransformation != null)
                Console.WriteLine($"Transformation distance: {expectedTransformation.SqrtDistanceTo(tr)}");

            return tr;
        }

        private Match[] SampleGreedy(ISampler sampler, AFeatureComputer fc, AData microData, FeatureVector[] fMacro)
        {
            KDTree tree = new KDTree(fMacro);

            PriorityQueue<Match> priorityQueue = new PriorityQueue<Match>();
            int notEnhancedCounter = 0;
            int stopIteration = 100;
            bool stopped = false;
            while (!stopped)
            {
                Point3D[] sampledPoints = sampler.Sample(microData, Constants.NUMBER_OF_POINTS_MICRO);

                foreach (Point3D sampledPoint in sampledPoints)
                {
                    FeatureVector microFC = fc.ComputeFeatureVector(microData, sampledPoint);

                    int index = tree.FindNearest(microFC);

                    Match currentMatch = new Match(microFC, fMacro[index]);

                    priorityQueue.Enqueue(currentMatch);

                    if (priorityQueue.Count < fMacro.Length)
                        continue;

                    if (priorityQueue.Dequeue() == currentMatch)
                        notEnhancedCounter++;
                    else
                        notEnhancedCounter = 0;

                    if (notEnhancedCounter >= stopIteration)
                    {
                        stopped = true;
                        break;
                    }
                }
            }

            return priorityQueue.GetData();
        }


        private List<FeatureVector> CalculateFeatureVectors(AData data, AFeatureComputer featureComputer, Point3D[] sampledPoints)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            object lockObj = new object();

            List<FeatureVector> featureVectors = new List<FeatureVector>();
            Thread[] threads = new Thread[Environment.ProcessorCount];

            int chunkSize = (sampledPoints.Length / Environment.ProcessorCount) + 1;

            for (int t = 0; t < Environment.ProcessorCount; t++)
            {
                int start = t * chunkSize;
                int end = Math.Min(start + chunkSize, sampledPoints.Length);

                threads[t] = new Thread(() =>
                {
                    for (int i = start; i < end; i++)
                    {
                        FeatureVector fv = featureComputer.ComputeFeatureVector(data, sampledPoints[i]);
                        lock (lockObj)
                        {
                            featureVectors.Add(fv);
                        }
                    }
                });

                threads[t].Start();
            }

            for (int t = 0; t < threads.Length; t++)
                threads[t].Join();

            stopwatch.Stop();
            Debug.Log($"Feature computation time: {stopwatch.ElapsedMilliseconds}");

            return featureVectors;
        }

        private void GenerateFVGraph(AData microData, AData macroData, Point3D[] microPoints, Point3D[] macroPoints)
        {

            List<Point2D> distances = new List<Point2D>();
            double fvDistance;
            FeatureVector microFV, macroFV;

            SpheresMockData spheresMockData = new SpheresMockData();
            //spheresMockData.PrintSpherePositions();

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
                    int sphereIndex2 = spheresMockData.GetSphereIndex(macroPoints[j].X, macroPoints[j].Y, macroPoints[j].Z);


                    int indexDistance = (sphereIndex1 == 8 || sphereIndex2 == 8) ? 10 : Math.Abs(sphereIndex1 - sphereIndex2);

                    //if (fvDistance > 1E4 && indexDistance == 0)
                    //{
                    //    Debug.Log($"Sphere index 1 = {sphereIndex1}");
                    //    Debug.Log($"Sphere index 2 = {sphereIndex2}");
                    //    Debug.Log($"");
                    //    Debug.Log($"Micro FV: {microFV}");
                    //    Debug.Log($"Macro FV: {macroFV}");
                    //    Debug.Log($"");
                    //}


                    distances.Add(new Point2D(indexDistance, fvDistance));
                }
            }

            CSVWriter.WriteResult("index X FV vzdalenosti.csv", "Index distance", "FV Distance", distances);
        }

        private List<Transform3D> CalculateTransformations(AData microData, AData macroData, Match[] matches)
        {
            System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            List<Transform3D> transformations = new List<Transform3D>();

            Thread[] threads = new Thread[Environment.ProcessorCount];

            object lockObj = new object();

            int chunkSize = (matches.Length / Environment.ProcessorCount) + 1;

            for (int t = 0; t < Environment.ProcessorCount; t++)
            {
                int start = t * chunkSize;
                int end = Math.Min(start + chunkSize, matches.Length);

                threads[t] = new Thread(() =>
                {
                    for (int i = start; i < end; i++)
                    {
                        Transform3D[] currentTransformations = transformer.GetTransformations(matches[i], microData, macroData);
                        lock (lockObj)
                        {
                            foreach (Transform3D currentTransformation in currentTransformations)
                                transformations.Add(currentTransformation);
                        }
                    }
                });

                threads[t].Start();
            }

            for (int t = 0; t < threads.Length; t++)
                threads[t].Join();

            stopwatch.Stop();
            Console.WriteLine($"Transformations computation time: {stopwatch.ElapsedMilliseconds}");

            return transformations;
        }

        private void PrintRealDistances(Match[] matches)
        {
            //Point3D center = new Point3D(2.5, 2.5, 2.5);
            //for (int i = 0; i < matches.Length; i++)
            //{
            //    Debug.Log(Math.Abs(matches[i].microFV.Point.Distance(center) - matches[i].macroFV.Point.Distance(center)));

            //}

            if (expectedTransformation == null)
            {
                Console.WriteLine("Expected transformation not specified.");
                return;
            }


            for (int i = 0; i<matches.Length; i++)
                Console.WriteLine(matches[i]
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