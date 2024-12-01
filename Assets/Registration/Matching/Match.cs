namespace DataView
{
    /// <summary>
    /// 
    /// </summary>
    public class Match
    {
        private FeatureVector microFeatureVector;
        private FeatureVector macroFeatureVector;
        private double similarity;

        /// <summary>
        /// Invalid match
        /// </summary>
        /// <param name="f1"></param>
        public Match(FeatureVector f1)
        {
            this.microFeatureVector = f1;
            this.macroFeatureVector = new FeatureVector();
            this.Similarity = 0;
        }

        /// <summary>
        /// Creation of valid match
        /// </summary>
        /// <param name="microFeatureVector">Micro feature vector</param>
        /// <param name="macroFeatureVector">Macro feature vector</param>
        /// <param name="similarity">Similarity of feature vectors</param>
        public Match(FeatureVector microFeatureVector, FeatureVector macroFeatureVector, double similarity)
        {
            this.microFeatureVector = microFeatureVector;
            this.macroFeatureVector = macroFeatureVector;
            this.Similarity = similarity;
        }

        public FeatureVector microFV { get => microFeatureVector; }
        public FeatureVector macroFV { get => macroFeatureVector; }
        public double Similarity { get => similarity; set => similarity = value; }

        public override string ToString()
        {
            return $"Match contains:\nMicro feature vector:\n{microFeatureVector}\nMacro feature vector:\n{macroFeatureVector}";
        }
    }
}
