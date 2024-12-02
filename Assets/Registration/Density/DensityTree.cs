using System.Collections.Generic;

namespace DataView
{
    public class DensityTree
    {
        private const int PIVOT_INDEX = 0;
        private const int LEAF_NODE_TRANSFORMATIONS = 1;

        private List<Transform3D> nodesTransformations;

        private double threshold;

        private DensityTree closeNode = null;
        private DensityTree farNode = null;


        public DensityTree(List<Transform3D> transformations)
        {
            this.nodesTransformations = new List<Transform3D>();
            ConstructTree(transformations);
        }

        private void ConstructTree(List<Transform3D> transformations)
        {
            // If this is the leaf node
            if (transformations.Count <= LEAF_NODE_TRANSFORMATIONS)
            {
                this.nodesTransformations = transformations;
                return;
            }

            UnityEngine.Debug.Log("Transformations: ");

            for(int i = 0; i<transformations.Count; i++)
            {
                UnityEngine.Debug.Log($"Transforamation: {transformations[i]}");
            }


            nodesTransformations.Add(transformations[PIVOT_INDEX]);
            transformations.RemoveAt(PIVOT_INDEX);

            UnityEngine.Debug.Log($"Reference Transformation {transformations[PIVOT_INDEX]}");


            List<Transform3D> farTransformations = new List<Transform3D>();
            List<Transform3D> closeTransformations = new List<Transform3D>();

            DivideIntoLists(transformations, ref closeTransformations, ref farTransformations);

            if (closeTransformations.Count > 0)
            {
                UnityEngine.Debug.Log($"Close ones");
                this.closeNode = new DensityTree(closeTransformations);
            }


            if (farTransformations.Count > 0)
            {
                UnityEngine.Debug.Log($"Far ones");
                this.farNode = new DensityTree(farTransformations);
            }
        }

        private void DivideIntoLists(List<Transform3D> transformations, ref List<Transform3D> closeNodes, ref List<Transform3D> farNodes)
        {
            Transform3D referenceTransformation = this.nodesTransformations[0];

            List<double> transformationDistances = new List<double>();

            for (int i = 0; i < transformations.Count; i++)
                transformationDistances.Add(referenceTransformation.DistanceTo(transformations[i]));

            QuickSelectClass testClass = new QuickSelectClass();
            this.threshold = testClass.QuickSelect(transformationDistances, transformationDistances.Count / 2);

            for (int i = 0; i < transformationDistances.Count; i++)
            {
                if (transformationDistances[i] > threshold)
                    farNodes.Add(transformations[i]);
                else
                    closeNodes.Add(transformations[i]);
            }
        }

        public void ProximityQuery(Transform3D queryPoint, double radius, List<Transform3D> result)
        {
            double distance = queryPoint.DistanceTo(this.nodesTransformations[0]);

            // Check if the current node is within the radius
            if (distance < radius)
            {
                result.AddRange(this.nodesTransformations);
            }

            // Recursively search the subtrees based on distance and threshold
            if (this.closeNode != null && (distance - radius) < this.threshold)
            {
                this.closeNode.ProximityQuery(queryPoint, radius, result);
            }

            if (this.farNode != null && (distance + radius) > this.threshold)
            {
                this.farNode.ProximityQuery(queryPoint, radius, result);
            }
        }
    }
}

