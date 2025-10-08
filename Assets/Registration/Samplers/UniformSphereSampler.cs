using System;
using System.Collections.Generic;

namespace DataView
{

    public class UniformSphereSampler
    {
        private double RADIUS;
        private double SPACING;

        private List<Point3D> distributedPoints;

        public UniformSphereSampler()
        {
            this.RADIUS = Constants.RADIUS;
            this.SPACING = Constants.SPACING;

            this.distributedPoints = InitializeDistributedPoints();
        }

        public UniformSphereSampler(double radius, double spacing)
        {
            this.RADIUS = radius;
            this.SPACING = spacing;
            this.distributedPoints = InitializeDistributedPoints();
        }

        #region Public Methods - Distributed Points Calculation

        /// <summary>
        /// Method returns uniformly distributed points around the center point
        /// </summary>
        /// <param name="data">Data to check the bounds of an object</param>
        /// <param name="centerPoint">Center point</param>
        /// <returns>Returns list of points satisfying the condition</returns>
        public List<Point3D> GetDistributedPoints(AData data, Point3D centerPoint)
        {
            List<Point3D> resultPoints = new List<Point3D>();

            foreach (Point3D currentPoint in this.distributedPoints)
            {
                if (data.PointWithinBounds(currentPoint.X + centerPoint.X, currentPoint.Y + centerPoint.Y, currentPoint.Z + centerPoint.Z))
                    resultPoints.Add(new Point3D(
                        currentPoint.X + centerPoint.X,
                        currentPoint.Y + centerPoint.Y,
                        currentPoint.Z + centerPoint.Z
                    ));
            }

            return resultPoints;
        }

        #endregion

        #region Private Methods - Precalculation of Points


        /// <summary>
        /// Method initializes uniformly distributed points
        /// closer than RADIUS from the origin with specified spacing
        /// </summary>
        /// <returns>Returns list of points</returns>
        private List<Point3D> InitializeDistributedPoints()
        {
            List<Point3D> points = new List<Point3D>();

            for (double x = -this.RADIUS; x <= this.RADIUS; x += this.SPACING)
            {
                for (double y = -this.RADIUS; y <= this.RADIUS; y += this.SPACING)
                {
                    SphereBounds zBounds = GetSphereBounds(x, y);
                    if (zBounds == null)
                        continue; // No point in z is in the bounds

                    for (double z = zBounds.MinCoordinate; z <= zBounds.MaxCoordinate; z += this.SPACING)
                    {
                        Point3D point = new Point3D(x, y, z);
                        points.Add(point);
                    }
                }
            }

            return points;
        }

        private SphereBounds GetSphereBounds(double x, double y)
        {
            double rSquared = Math.Pow(this.RADIUS, 2);
            double zSquared = rSquared - Math.Pow(x, 2) - Math.Pow(y, 2);

            if (zSquared < 0)
                return null; // No point in z is in the bounds

            double minZ = -Math.Sqrt(zSquared);
            double maxZ = -minZ;
            return new SphereBounds(minZ, maxZ);
        }

        /// <summary>
        /// This is a messenger class for passing min and max bounds
        /// </summary>
        private class SphereBounds
        {
            private double minCoordinate;
            private double maxCoordinate;

            public SphereBounds(double minCoordinate, double maxCoordinate)
            {
                this.minCoordinate = minCoordinate;
                this.maxCoordinate = maxCoordinate;
            }

            //GETTERS
            public double MinCoordinate => minCoordinate;
            public double MaxCoordinate => maxCoordinate;
        }

        #endregion
    }
}
