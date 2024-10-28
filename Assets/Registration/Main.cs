using System;
using System.Collections.Generic;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

namespace DataView
{
    public class RegistrationLauncher
    {
        private AData iDataMicro;
        private AData macroData;

        private Transform3D inverseTransformation;

        public Transform3D RunRegistration(FilePathDescriptor microPathDescriptor, FilePathDescriptor macroPathDescriptor)
        {
            Debug.Log("Reading micro data.");
            VolumetricData microData = new VolumetricData(microPathDescriptor);
            Debug.Log("Reading macro data.");
            VolumetricData macroData = new VolumetricData(macroPathDescriptor);
            return RunRegistration(microData, macroData);
        }

        public Transform3D RunRegistration(VolumetricData microData,  VolumetricData macroData)
        {
            //Sets Locale to US
            //Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

            //----------------------------------------PARAMETERS----------------------------------------------

            const int NUMBER_OF_POINTS_MICRO = 10_000;
            const int NUMBER_OF_POINTS_MACRO = 10_000;

            const double THRESHOLD = 10; //percentage - top 10% best matches should be kept

            Debug.Log("Reading micro data.");
            this.iDataMicro = microData;
            //((VolumetricData)(iDataMicro)).CenterObjectAroundOrigin();
            this.inverseTransformation = new Transform3D(Matrix<double>.Build.DenseIdentity(3), Vector<double>.Build.Dense(3));

            Debug.Log("Reading macro data.");
            this.macroData = macroData;

            ISampler s = new SamplerFake(iDataMicro, this.macroData, 1, 0.1);
            IFeatureComputer fc = new FeatureComputerISOSurfaceCurvature();

            IMatcher matcher = new Matcher();
            ITransformer transformer = new Transformer3D();

            //----------------------------------------FEATURE VECTOR CALCULATION------------------------------------------------

            Debug.Log("Computing micro feature vectors.");
            List<FeatureVector> featureVectorsMicro = CalculateFeatureVectors(s, fc, iDataMicro, NUMBER_OF_POINTS_MICRO);
            Debug.Log("Computing macro feature vectors.");
            List<FeatureVector> featureVectorsMacro = CalculateFeatureVectors(s, fc, this.macroData, NUMBER_OF_POINTS_MACRO);

            //----------------------------------------SETUP TRANSFORMATION METRICS------------------------------------------------
            //What object is the result transformation going to be applied on (in this case, micro)
            ITransformationDistance transformationDistance = new TransformationDistance(iDataMicro);
            Transform3D.SetTransformationDistance(transformationDistance);

            //----------------------------------------MATCHES-------------------------------------------------
            Debug.Log("Matching.");
            Match[] matches = matcher.Match(featureVectorsMicro.ToArray(), featureVectorsMacro.ToArray(), THRESHOLD);

            //------------------------------------GET TRANSFORMATION -----------------------------------------

            Debug.Log("Computing transformations.\n");

            List<Transform3D> transformations = new List<Transform3D>();

            for (int i = 0; i < matches.Length; i++)
            {
                //Calculate transformation and if the transformation doesnt exist, it will skip it and print out the error message
                try
                {
                    Transform3D transformation = transformer.GetTransformation(matches[i], iDataMicro, this.macroData);
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
            return finalTransformation;
        }

        private static List<FeatureVector> CalculateFeatureVectors(ISampler sampler, IFeatureComputer featureComputer, AData data, int NUMBER_OF_POINTS)
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

    }
}