using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataView
{
    public class RegistrationLauncher : IRegistrationLauncher
    {
        private Transform3D inverseTransformation;
        //private Transform3D expectedTransformation;

        /* Registration blocks */
        private ISampler sampler;
        private IFeatureComputer featureComputer;
        private IMatcher matcher;
        private ITransformer transformer;

        private int NUMBER_OF_POINTS_MICRO;
        private int NUMBER_OF_POINTS_MACRO;
        private double THRESHOLD;

        public RegistrationLauncher()
        {
            this.NUMBER_OF_POINTS_MICRO = 10_000;
            this.NUMBER_OF_POINTS_MACRO = 10_000;
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
            this.sampler = new Sampler();
            this.featureComputer = new FeatureComputerISOSurfaceCurvature();
            this.matcher = new Matcher();
            this.transformer = new Transformer3D();
        }

        public Transform3D RunRegistration(FilePathDescriptor microDataPath, FilePathDescriptor macroDataPath)
        {
            return RunRegistration(new VolumetricData(microDataPath), new VolumetricData(macroDataPath));
        }

        public Transform3D RunRegistration(AData microData, AData macroData)
        {
            //Sets Locale to US
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            //----------------------------------------PARAMETERS----------------------------------------------


            //((VolumetricData)(iDataMicro)).CenterObjectAroundOrigin();
            //this.inverseTransformation = new Transform3D(Matrix<double>.Build.DenseIdentity(3), Vector<double>.Build.Dense(3));

            microData.CenterObject();

            //----------------------------------------FEATURE VECTOR CALCULATION------------------------------------------------

            Debug.Log("Computing micro feature vectors.");
            List<FeatureVector> featureVectorsMicro = CalculateFeatureVectors(sampler, featureComputer, microData, NUMBER_OF_POINTS_MICRO);
            Debug.Log("Computing macro feature vectors.");
            List<FeatureVector> featureVectorsMacro = CalculateFeatureVectors(sampler, featureComputer, macroData, NUMBER_OF_POINTS_MACRO);

            //----------------------------------------SETUP TRANSFORMATION METRICS------------------------------------------------
            ITransformationDistance transformationDistance = new TransformationDistanceSix(microData);
            Transform3D.SetTransformationDistance(transformationDistance);

            //----------------------------------------MATCHES-------------------------------------------------
            Debug.Log("Matching.");
            Match[] matches = matcher.Match(featureVectorsMicro.ToArray(), featureVectorsMacro.ToArray(), THRESHOLD);

            //PrintRealDistances(matches);

            //------------------------------------GET TRANSFORMATION -----------------------------------------

            Debug.Log("Computing transformations.\n");

            List<Transform3D> transformations = new List<Transform3D>();

            for (int i = 0; i < matches.Length; i++)
            {
                //Calculate transformation and if the transformation doesnt exist, it will skip it and print out the error message
                try
                {
                    Transform3D transformation = transformer.GetTransformation(matches[i], microData, macroData);
                    transformations.Add(transformation);
                    Debug.Log("Candidate for transformation: " + transformation);
                }
                catch { continue; }
            }

            DensityStructure densityStructure = new DensityStructure(transformations.ToArray());

            Transform3D tr = densityStructure.FindBestTransformation(0.5, 50);
            return tr;
        }

        public Transform3D RevertCenteringTransformation(Transform3D tr)
        {
            Transform3D finalTransformation = tr.ChainWithTransformation(inverseTransformation);
            //return finalTransformation;
            return tr;
        }

        private List<FeatureVector> CalculateFeatureVectors(ISampler sampler, IFeatureComputer featureComputer, AData data, int NUMBER_OF_POINTS)
        {
            Debug.Log("Sampling.");
            Point3D[] pointsMicro = sampler.Sample(data, NUMBER_OF_POINTS);


            List<FeatureVector> featureVectors = new List<FeatureVector>();

            Debug.Log("Computing micro feature vectors.");
            for (int i = 0; i < pointsMicro.Length; i++)
            {
                try { featureVectors.Add(featureComputer.ComputeFeatureVector(data, pointsMicro[i])); }
                catch { continue; }
            }

            return featureVectors;
        }

        //private void PrintRealDistances(Match[] matches)
        //{
        //    if (expectedTransformation == null)
        //    {
        //        Debug.Log("Expected transformation not specified.");
        //        return;
        //    }

        //    for (int i = 0; i<matches.Length; i++)
        //        Debug.Log(matches[i]
        //            .microFV
        //            .Point
        //            .ApplyRotationTranslation(expectedTransformation)
        //            .Distance(matches[i].macroFV.Point)
        //        );
        //}

        //private Match[] CreateFakeMatches()
        //{
        //    int NUMBER_OF_POINTS = Math.Min(NUMBER_OF_POINTS_MACRO, NUMBER_OF_POINTS_MICRO);

        //    Match[] matches = new Match[NUMBER_OF_POINTS];

        //    Point3D[] microPoints = this.sampler.Sample(microData, NUMBER_OF_POINTS);
        //    Point3D[] macroPoints = this.sampler.Sample(microData, NUMBER_OF_POINTS);

        //    for (int i = 0; i<NUMBER_OF_POINTS; i++)
        //        matches[i] = new Match(
        //            new FeatureVector(microPoints[i], new double[] { }),
        //            new FeatureVector(macroPoints[i], new double[] { }),
        //            1
        //        );


        //    return matches;
        //}



    }
}