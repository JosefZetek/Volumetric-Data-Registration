using System;
using System.Collections.Generic;
using System.Linq;

namespace DataView
{
    class SamplerFeatureVector : ISampler
    {
        private int count;

        public SamplerFeatureVector(int count)
        {
            this.count = count;
        }

        public Point3D[] Sample(IData d, int count)
        {
            Random rnd = new Random();
            int quality = 5; //how many times this method discards undersirable points (higher quality -> more proccesor time)
            int[] measures = d.Measures;
            int border = 5;
            
            List<Point3D> points = GenerateRandomPoints(d, count, border, rnd);
            List<PointWithFeatures> pointsWithFeatures = ComputeFeaturesForPoints(points, d);
            List<PointRated> ratedPoints = RatePoints(pointsWithFeatures);

            double averageRating;

            for(int i = 0; i < quality; i++)
            {
                averageRating = ComputeAverageRating(ratedPoints);
                RemovePointsUnderAverage(ratedPoints, averageRating); 
                AddPointsToList(ratedPoints, d, count, border, measures, rnd); //this should increase the average (statistically)
            }
            
            Point3D[] resultPoints = new Point3D[count]; 
            for(int i = 0; i < count; i++)//list of RatedPoint into array of Point3D
            {
                resultPoints[i] = new Point3D(ratedPoints[i].X, ratedPoints[i].Y, ratedPoints[i].Z);
            }

            return resultPoints;
        }

        //Using config file
        public Point3D[] Sample(IData d)
        {
            return Sample(d, count);
        }

        /// <summary>
        /// Adds random RatedPoints to list until ratedPoints.Count() == count
        /// </summary>
        /// <param name="ratedPoints"></param>
        /// <param name="d"></param>
        /// <param name="count"></param>
        /// <param name="border"></param>
        /// <param name="measures"></param>
        /// <param name="rnd"></param>
        private void AddPointsToList(List<PointRated> ratedPoints, IData d, int count, int border, int[] measures, Random rnd)
        {
            FeatureComputer featureComputer = new FeatureComputer();
            while(ratedPoints.Count() < count)
            {
                Point3D point = GenerateRandomPoint(border, measures, rnd);
                PointWithFeatures pointWithFeatures = new PointWithFeatures(point, featureComputer.ComputeFeatureVector(d, point).Features);
                PointRated ratedPoint = new PointRated(pointWithFeatures, RatePoint(pointWithFeatures));

                ratedPoints.Add(ratedPoint);
            }
        }

        /// <summary>
        /// Removes all points from list where RatedPoint[i].rating < averageRating 
        /// </summary>
        /// <param name="ratedPoints"></param>
        /// <param name="averageRating"></param>
        private void RemovePointsUnderAverage(List<PointRated> ratedPoints, double averageRating)
        {
            for(int i = 0; i < ratedPoints.Count(); i++) 
            {
                if(ratedPoints[i].rating < averageRating)
                {
                    ratedPoints.RemoveAt(i);
                }
            }
        }


        /// <summary>
        /// Computes the average of RatedPoint.rating in an array of RatedPoints
        /// </summary>
        /// <param name="ratedPoints"></param>
        /// <returns></returns>
        private double ComputeAverageRating(PointRated[] ratedPoints)
        {
            double ratingSum = 0;
            foreach(PointRated point in ratedPoints)
            {
                ratingSum += point.rating;
            }

            return ratingSum / ratedPoints.Count();
        }

        /// <summary>
        /// Computes the average of RatedPoint.rating in a list of RatedPoints
        /// </summary>
        /// <param name="ratedPoints"></param>
        /// <returns></returns>
        private double ComputeAverageRating(List<PointRated> ratedPoints)
        {
            double ratingSum = 0;
            foreach (PointRated point in ratedPoints)
            {
                ratingSum += point.rating;
            }

            return ratingSum / ratedPoints.Count();
        }

        /// <summary>
        /// Computes the rating for all points in input array based on RateVector()
        /// 
        /// Array version
        /// </summary>
        /// <param name="pointsWithFeatures"></param>
        /// <returns></returns>
        private PointRated[] RatePoints(PointWithFeatures[] pointsWithFeatures)
        {
            PointRated[] ratedPoints = new PointRated[pointsWithFeatures.Count()];
            double rating;
            for (int i = 0; i < ratedPoints.Count(); i++) 
            {
                rating = RateVector(pointsWithFeatures[i].featureVector);
                ratedPoints[i] = new PointRated(pointsWithFeatures[i], rating);
            }
            return ratedPoints;
        }

        /// <summary>
        /// Computes the rating for all points in input list based on RateVector()
        /// 
        /// List version
        /// </summary>
        /// <param name="pointsWithFeatures"></param>
        /// <returns></returns>
        private List<PointRated> RatePoints(List<PointWithFeatures> pointsWithFeatures)
        {

            List<PointRated> ratedPoints = new List<PointRated>();
            double rating;
            for (int i = 0; i < ratedPoints.Count(); i++)
            {
                rating = RateVector(pointsWithFeatures[i].featureVector);
                ratedPoints.Add(new PointRated(pointsWithFeatures[i], rating));
            }
            return ratedPoints;
        }

        /// <summary>
        /// Returns a positive number
        /// Bigger number -> more interesting vector -> values are more varied
        /// return 0 -> every value of vector is the same
        /// 
        /// Doesnt take into account how the vector values are arranged 
        /// RateVector({-2,1,3}) == RateVector({1,3,-2}) 
        /// </summary>
        /// <param name="vector"></param>
        /// <returns></returns>
        private double RateVector(double[] vector)
        {
            double sum = 0;
            foreach(double a in vector)
            {
                sum += a;
            }
            double average = sum / vector.Count(); //average value of vector

            double distance = 0;
            double distanceSum = 0;
            for(int i = 0; i < vector.Count(); i++)
            {
                distance = Math.Abs(vector[i] - average); //distance of current value from the average
                distanceSum += distance;
            }
            double averageDistance = distanceSum / vector.Count(); 

            return averageDistance;
        }

        private double RatePoint(PointWithFeatures point)
        {
            return RateVector(point.featureVector);
        }

        /// <summary>
        /// Computes the feature vector for all points in array
        /// 
        /// Array version
        /// </summary>
        private PointWithFeatures[] ComputeFeaturesForPoints(Point3D[] points, IData d)
        {
            PointWithFeatures[] pointsWithFeatures = new PointWithFeatures[points.Count()];
            FeatureComputer featureComputer = new FeatureComputer();
            for (int i = 0; i < points.Count(); i++)
            {
                Point3D point = points[i];
                double[] featureVector = featureComputer.ComputeFeatureVector(d, point).Features;
                pointsWithFeatures[i] = new PointWithFeatures(point, featureVector);
            }
            return pointsWithFeatures;
        }

        /// <summary>
        /// Computes feature vector for all points in list
        /// 
        /// List version
        /// </summary>
        /// <param name="points"></param>
        /// <param name="d"></param>
        /// <returns></returns>
        private List<PointWithFeatures> ComputeFeaturesForPoints(List<Point3D> points, IData d)
        {
            List<PointWithFeatures> pointsWithFeatures = new List<PointWithFeatures>();
            FeatureComputer featureComputer = new FeatureComputer();
            for (int i = 0; i < points.Count(); i++)
            {
                Point3D point = points[i];
                double[] featureVector = featureComputer.ComputeFeatureVector(d, point).Features;
                pointsWithFeatures.Add(new PointWithFeatures(point, featureVector));
            }
            return pointsWithFeatures;
        }


        /// <summary>
        /// Generates array of count random points in the borders of the volumetric data
        /// </summary>
        /// <param name="d"></param>
        /// <param name="count"></param>
        /// <param name="border"></param>
        /// <returns></returns>
        private Point3D[] GenerateRandomPoints(IData d, int count, int border)
        {
            Point3D[] points = new Point3D[count];
            int[] measures = d.Measures;
            Random r = new Random();

            for (int i = 0; i < count; i++)
            {
                int x = r.Next(border, measures[0] - border);
                int y = r.Next(border, measures[1] - border);
                int z = r.Next(border, measures[2] - border);

                points[i] = new Point3D(x, y, z);
            }
            return points;
        }

        /// <summary>
        /// Generates 1 random point inside border, using Random rnd
        /// </summary>
        /// <param name="d"></param>
        /// <param name="border"></param>
        /// <param name="measures"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private Point3D GenerateRandomPoint(int border, int[] measures, Random rnd)
        {

            int x = rnd.Next(border, measures[0] - border);
            int y = rnd.Next(border, measures[1] - border);
            int z = rnd.Next(border, measures[2] - border);

            return new Point3D(x, y, z);
        }

        /// <summary>
        /// Generates list of count random points using GenerateRandomPoint()
        /// using a predefined random to allow correct usage of seeds
        /// </summary>
        /// <param name="d"></param>
        /// <param name="count"></param>
        /// <param name="border"></param>
        /// <returns></returns>
        private List<Point3D> GenerateRandomPoints(IData d, int count, int border, Random rnd)
        {
            List<Point3D> points = new List<Point3D>();
            int[] measures = d.Measures;

            for (int i = 0; i < count; i++)
            {
                points.Add(GenerateRandomPoint(border, measures, rnd));
            }
            return points;
        }
    }
}
