using System;

namespace DataView
{
    /// <summary>
    /// 
    /// </summary>
    class FeatureVector
    {
        private static int NumberOfFeatures = -1;

        /// <summary>
        /// Point to which the feature vector belongs
        /// </summary>
        private Point3D point;

        /// <summary>
        /// Array of features
        /// </summary>
        private double[] features;

        /// <summary>
        /// 
        /// </summary>
        public FeatureVector()
        {
            new FeatureVector(new Point3D(0, 0, 0), 0, 0, 0, 0, 0);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="x1"></param>
        /// <param name="x2"></param>
        /// <param name="x3"></param>
        /// <param name="x4"></param>
        /// <param name="x5"></param>
        public FeatureVector(Point3D p, double x1, double x2, double x3, double x4, double x5)
        {
            this.Point = p;
            this.features = new double[] { x1, x2, x3, x4, x5 };
            CheckVectorLength();
        }

        public FeatureVector(Point3D p, double[] features)
        {
            this.Point = p;
            this.features = features;
            CheckVectorLength();
        }

        private void CheckVectorLength()
        {
            //Sets the number of features if it has not been set yet
            if (NumberOfFeatures < 0)
                NumberOfFeatures = this.features.Length;

            else if (NumberOfFeatures != this.features.Length)
                throw new Exception("Number of features needs to be the same");
        }

        /// <summary>
        /// Calculates thee magnitude of a given vector
        /// </summary>
        /// <returns>Returns calculated magnitude</returns>
        public double Magnitude()
        {
            double sum = 0;

            for (int i = 0; i < features.Length; i++)
                sum += this.features[i] * this.features[i];

            return Math.Sqrt(sum);
        }

        /// <summary>
        /// Calculates the difference
        /// </summary>
        /// <param name="fv"></param>
        /// <returns>Distance between the two vectors</returns>
        public double DistTo2(FeatureVector fv)
        {
            double sum = 0;
            for (int i = 0; i < GetNumberOfFeatures; i++)
            {
                double d = this.features[i] - fv.features[i];
                sum += d * d;
            }

            return sum;
        }

        internal Point3D Point { get => point; set => point = value; }
        public int GetNumberOfFeatures { get => features.Length; }
        public double[] Features { get => features; }

        public override string ToString()
        {
            string returnS = "";
            for (int i = 0; i < GetNumberOfFeatures; i++)
            {
                returnS += Math.Round(features[i], 2);
                if (i != GetNumberOfFeatures - 1)
                    returnS += ", ";
            }

            return returnS;
        }
    }
}
