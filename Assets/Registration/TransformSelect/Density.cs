using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;


namespace DataView
{
    public class Density
    {
        // configuration
        public static double radius = Double.NaN; // this has to be set for particular object roughly as average distance to centroid
        public static double spread = 25;

        DistanceTree dt;

        public Transform3D Find(Transform3D[] transforms)
        {
            
            Candidate[] candidate = new Candidate[transforms.Length];

            for (int i = 0; i < candidate.Length; i++)
                candidate[i] = new Candidate(transforms[i]);

            int maxDensityIndex = -1;

            dt = new DistanceTree(candidate);

            double maxDensity = 0;
            maxDensityIndex = -1;

            double t = -radius * Math.Log(0.01) / spread;

            object[] teInput = new object[candidate.Length / 100+1];

            for (int i = 0; i < teInput.Length; i++)
                teInput[i] = i * 100;

            ThreadedExecution<object, double[]> te = new ThreadedExecution<object, double[]>(teInput, new ThreadedExecution<object, double[]>.ExecBody(this.density), -1, new object[] { candidate, t});

            double[][] density = te.Execute();

            for (int i = 0; i < teInput.Length; i++)
            {
                for (int j = 0; j < 100; j++)
                    if (density[i][j] > maxDensity)
                    {
                        maxDensity = density[i][j];
                        maxDensityIndex = i * 100 + j;
                    }
            }
            return candidate[maxDensityIndex].toTransform3D();
        }

        private double[] density(object input, int threadId, object[] parameters)
        {
            double[] result = new double[100];

            Candidate[] candidate = (Candidate[])parameters[0];
            double t = (double)parameters[1];

            for (int i = 0; i < 100; i++)
            {
                int idx = (int)input + i;

                if (idx == candidate.Length)
                    break;

                var nbs = dt.findAllCloserThan(candidate[idx], t);
                double density = 0;
                foreach (int n in nbs)
                    density += Math.Exp(-spread * candidate[idx].DistanceTo(candidate[n]) / (radius* radius));
                result[i] = density;
            }

            return result;
        }        
    }
}
