using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataView
{
    /// <summary>
    /// K-Dimensional Tree for FeautureVector proximity queries
    /// </summary>
    public class KDTree
    {
        /// <summary>
        /// Node in K-Dimensional Tree
        /// </summary>
        class Node
        {
            public Node left, right;
            public int point;
            public int axis;
        }

        Node root;
        FeatureVector[] fVectors;
        double smDist, smDistSq;
        int nearest;
        FeatureVector searchFeatureVector;
        Random rnd = new Random(); // seed

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fVectors"></param>
        public KDTree(FeatureVector[] fVectors)
        {
            this.fVectors = fVectors;
            List<int> list = new List<int>();
            for (int i = 0; i < fVectors.Length; i++)
                list.Add(i);
            root = new Node();
            AddChildren(root, list, 0);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="p"></param>
        /// <param name="maxDist"></param>
        /// <returns></returns>
        public int FindNearest(FeatureVector featureVector)
        {
            smDist = double.MaxValue;
            smDistSq = double.MaxValue;
            nearest = -1;
            searchFeatureVector = featureVector;
            SearchSubtree(root);

            return nearest;
        }

        /// <summary>
        /// Search subtree for nearest Node to the given
        /// </summary>
        /// <param name="n"></param>
        private void SearchSubtree(Node n)
        {
            // node point
            FeatureVector np = fVectors[n.point];

            if (searchFeatureVector.Features[n.axis] < np.Features[n.axis])
            {
                double minDist = np.Features[n.axis] - searchFeatureVector.Features[n.axis];
                if (minDist < smDist)
                {
                    double dist = searchFeatureVector.DistTo2(np);
                    if (dist < smDistSq)
                    {
                        smDistSq = dist;
                        smDist = Math.Sqrt(dist);
                        nearest = n.point;
                    }
                }
                if (n.left != null)
                {
                    SearchSubtree(n.left);
                }
                if (n.right != null)
                {
                    if (minDist < smDist)
                        SearchSubtree(n.right);
                }
            }
            else
            {
                double minDist = searchFeatureVector.Features[n.axis] - np.Features[n.axis];
                if (minDist < smDist)
                {
                    double dist = searchFeatureVector.DistTo2(np);
                    if (dist < smDistSq)
                    {
                        smDist = Math.Sqrt(dist);
                        smDistSq = dist;
                        nearest = n.point;
                    }
                }
                if (n.right != null)
                {
                    SearchSubtree(n.right);
                }
                if (n.left != null)
                {
                    if (minDist < smDist)
                        SearchSubtree(n.left);
                }
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="n"></param>
        /// <param name="list"></param>
        /// <param name="axis"></param>
        private void AddChildren(Node n, List<int> list, int axis)
        {
            n.axis = axis;

            if (list.Count == 1)
            {
                n.point = list[0];
                return;
            }

            int med = Median(list, axis);
            n.point = list[med];

            List<int> left = new List<int>();
            List<int> right = new List<int>();

            for (int i = 0; i < list.Count; i++)
            {
                if (i == med)
                    continue;
                if (fVectors[list[i]].Features[axis] < fVectors[list[med]].Features[axis])
                    left.Add(list[i]);
                else
                    right.Add(list[i]);
            }

            if (left.Count > 0)
            {
                
                n.left = new Node();
                AddChildren(n.left, left, (axis + 1) % fVectors[fVectors.Length-1].Features.Length);
            }
            if (right.Count > 0)
            {
                n.right = new Node();
                AddChildren(n.right, right, (axis + 1) % fVectors[fVectors.Length - 1].Features.Length);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="points"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private int Median(List<int> points, int axis)
        {
            return FindMedian(points, 0, points.Count, axis);
        }

        /// <summary>
        /// Finds median for construction of balanced KD-Tree
        /// </summary>
        /// <param name="points"></param>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <param name="axis"></param>
        /// <returns></returns>
        private int FindMedian(List<int> points, int min, int max, int axis)
        {
            if (min == max)
                return (min);
            if (min == (max - 1))
            {
                if (fVectors[points[min]].Features[axis] > fVectors[points[max]].Features[axis])
                {
                    int tmp = points[min];
                    points[min] = points[max];
                    points[max] = tmp;
                }
                return (points.Count / 2);
            }
            int pivot = min + rnd.Next(max - min);
            int l = min;
            int r = max - 1;
            while (l < r)
            {
                while (fVectors[points[l]].Features[axis] < fVectors[points[pivot]].Features[axis])
                    l++;
                while (fVectors[points[r]].Features[axis] > fVectors[points[pivot]].Features[axis])
                    r--;
                if (l < r)
                {
                    int tmp = points[l];
                    points[l] = points[r];
                    points[r] = tmp;
                    l++;
                    r--;
                }
            }
            if (l < points.Count / 2)
                return (FindMedian(points, l, max, axis));
            else
                return (FindMedian(points, min, l, axis));
        }

    }
}