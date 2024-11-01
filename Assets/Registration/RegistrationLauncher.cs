using System;
using System.Collections.Generic;
using UnityEngine;

namespace DataView
{
    public class RegistrationLauncher
    {
        private AData microData;
        private AData macroData;

        //private Transform3D inverseTransformation;
        private Transform3D expectedTransformation;

        /* Registration blocks */
        private ISampler s;
        private IFeatureComputer fc;
        private IMatcher matcher;
        private ITransformer transformer;

        const int NUMBER_OF_POINTS_MICRO = 10_000;
        const int NUMBER_OF_POINTS_MACRO = 10_000;

        public RegistrationLauncher(FilePathDescriptor microPathDescriptor, FilePathDescriptor macroPathDescriptor, Transform3D expectedTransformation)
        {
            this.expectedTransformation = expectedTransformation;

            this.microData = new VolumetricData(microPathDescriptor);
            this.macroData = new VolumetricData(macroPathDescriptor);

            this.s = new SamplerFake(microData, macroData, 10_000, 0.1, expectedTransformation);
            InitializeInterfaces();
        }

        public RegistrationLauncher(AData microData, AData macroData, Transform3D expectedTransformation)
        {
            this.expectedTransformation = expectedTransformation;

            this.microData = microData;
            this.macroData = macroData;

            this.s = new SamplerFake(microData, macroData, Math.Max(NUMBER_OF_POINTS_MACRO, NUMBER_OF_POINTS_MICRO), 0.1, expectedTransformation);
            InitializeInterfaces();
        }

        public RegistrationLauncher(FilePathDescriptor microPathDescriptor, FilePathDescriptor macroPathDescriptor)
        {
            this.expectedTransformation = null;
            this.microData = new VolumetricData(microPathDescriptor);
            this.macroData = new VolumetricData(macroPathDescriptor);

            this.s = new Sampler();
            InitializeInterfaces();
        }

        public RegistrationLauncher(AData microData, AData macroData)
        {
            this.expectedTransformation = null;
            this.microData = microData;
            this.macroData = macroData;

            this.s = new Sampler();
            InitializeInterfaces();
        }

        private void InitializeInterfaces()
        {
            this.fc = new FeatureComputerISOSurfaceCurvature();
            this.matcher = new Matcher();
            this.transformer = new Transformer3D();
        }

        private Transform3D FakeRegistration()
        {


            //----------------------------------------SETUP TRANSFORMATION METRICS------------------------------------------------
            ITransformationDistance transformationDistance = new TransformationDistance(this.microData);
            Transform3D.SetTransformationDistance(transformationDistance);

            //----------------------------------------MATCHES-------------------------------------------------
            Debug.Log("Matching.");
            Match[] matches = CreateFakeMatches();

            PrintRealDistances(matches);

            //------------------------------------GET TRANSFORMATION -----------------------------------------

            Debug.Log("Computing transformations.\n");

            List<Transform3D> transformations = new List<Transform3D>();

            for (int i = 0; i < matches.Length; i++)
            {
                //Calculate transformation and if the transformation doesnt exist, it will skip it and print out the error message
                try
                {
                    Transform3D transformation = transformer.GetTransformation(matches[i], this.microData, this.macroData);
                    transformations.Add(transformation);
                    Debug.Log("Candidate for transformation: " + transformation);
                }
                catch { continue; }
            }

            DensityStructure densityStructure = new DensityStructure(transformations.ToArray());

            Transform3D tr = densityStructure.FindBestTransformation(0.5, 50);
            return tr;
        }

        private Transform3D RealRegistration()
        {
            //Sets Locale to US
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            //----------------------------------------PARAMETERS----------------------------------------------



            const double THRESHOLD = 10; //percentage - top 10% best matches should be kept

            //((VolumetricData)(iDataMicro)).CenterObjectAroundOrigin();
            //this.inverseTransformation = new Transform3D(Matrix<double>.Build.DenseIdentity(3), Vector<double>.Build.Dense(3));

            //----------------------------------------FEATURE VECTOR CALCULATION------------------------------------------------

            Debug.Log("Computing micro feature vectors.");
            List<FeatureVector> featureVectorsMicro = CalculateFeatureVectors(s, fc, this.microData, NUMBER_OF_POINTS_MICRO);
            Debug.Log("Computing macro feature vectors.");
            List<FeatureVector> featureVectorsMacro = CalculateFeatureVectors(s, fc, this.macroData, NUMBER_OF_POINTS_MACRO);

            //----------------------------------------SETUP TRANSFORMATION METRICS------------------------------------------------
            ITransformationDistance transformationDistance = new TransformationDistance(this.microData);
            Transform3D.SetTransformationDistance(transformationDistance);

            //----------------------------------------MATCHES-------------------------------------------------
            Debug.Log("Matching.");
            Match[] matches = matcher.Match(featureVectorsMicro.ToArray(), featureVectorsMacro.ToArray(), THRESHOLD);

            PrintRealDistances(matches);

            //------------------------------------GET TRANSFORMATION -----------------------------------------

            Debug.Log("Computing transformations.\n");

            List<Transform3D> transformations = new List<Transform3D>();

            for (int i = 0; i < matches.Length; i++)
            {
                //Calculate transformation and if the transformation doesnt exist, it will skip it and print out the error message
                try
                {
                    Transform3D transformation = transformer.GetTransformation(matches[i], this.microData, this.macroData);
                    transformations.Add(transformation);
                    Debug.Log("Candidate for transformation: " + transformation);
                }
                catch { continue; }
            }

            DensityStructure densityStructure = new DensityStructure(transformations.ToArray());

            Transform3D tr = densityStructure.FindBestTransformation(0.5, 50);
            return tr;
        }

        public Transform3D RunRegistration()
        {
            if (expectedTransformation != null)
                return FakeRegistration();

            return RealRegistration();
        }

        public Transform3D RevertCenteringTransformation(Transform3D tr)
        {
            //Transform3D finalTransformation = tr.ChainWithTransformation(inverseTransformation);
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

        private void PrintRealDistances(Match[] matches)
        {
            if (expectedTransformation == null)
            {
                Debug.Log("Expected transformation not specified.");
                return;
            }

            for (int i = 0; i<matches.Length; i++)
                Debug.Log(matches[i]
                    .microFV
                    .Point
                    .ApplyRotationTranslation(expectedTransformation)
                    .Distance(matches[i].macroFV.Point)
                );
        }

        private Match[] CreateFakeMatches()
        {
            int NUMBER_OF_POINTS = Math.Min(NUMBER_OF_POINTS_MACRO, NUMBER_OF_POINTS_MICRO);

            Match[] matches = new Match[NUMBER_OF_POINTS];

            Point3D[] microPoints = this.s.Sample(microData, NUMBER_OF_POINTS);
            Point3D[] macroPoints = this.s.Sample(microData, NUMBER_OF_POINTS);

            for (int i = 0; i<NUMBER_OF_POINTS; i++)
                matches[i] = new Match(
                    new FeatureVector(microPoints[i], new double[] { }),
                    new FeatureVector(macroPoints[i], new double[] { }),
                    1
                );


            return matches;
        }



    }
}