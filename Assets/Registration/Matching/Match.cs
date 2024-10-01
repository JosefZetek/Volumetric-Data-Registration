using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataView
{
    /// <summary>
    /// 
    /// </summary>
    class Match
    {
        private FeatureVector f1;
        private FeatureVector f2;
        private double similarity;

        public Match(FeatureVector f1)
        {
            this.F1 = f1;
            this.F2 = new FeatureVector();
            this.Similarity = 0;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="f1"></param>
        /// <param name="f2"></param>
        /// <param name="similarity"></param>
        public Match(FeatureVector f1, FeatureVector f2, double similarity)
        {
            this.F1 = f1;
            this.F2 = f2;
            this.Similarity = similarity;
        }

        public double Similarity { get => similarity; set => similarity = value; }
        internal FeatureVector F1 { get => f1; set => f1 = value; }
        internal FeatureVector F2 { get => f2; set => f2 = value; }

        public override string ToString()
        {
            return "f1: " + F1.ToString() + " f2: " + F2.ToString() + " s: " + Similarity;
        }
    }
}
