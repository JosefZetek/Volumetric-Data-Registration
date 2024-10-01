using System;
using System.Collections.Generic;

namespace DataView
{
    class DTNode
    {
        int node;
        DTNode close;
        DTNode far;
        double threshold;
        List<int> items;
        bool leaf;
        public static Candidate[] cands;        

        public DTNode(int n, List<int> children, int depth)
        {
            if (children.Count<5)
            {
                this.leaf = true;
                this.items = new List<int>();
                items.AddRange(children);
                items.Add(n);
            }
            else
            {
                this.node = n;
                this.leaf = false;

                this.threshold = findMedian(node, children);

                

                List<int> closeNodes = new List<int>();

                List<int> farNodes = new List<int>();

                foreach(int i in children)
                {
                    double distance = cands[node].SqrtDistanceTo(cands[i]);
                    if (cands[node].SqrtDistanceTo(cands[i]) >= threshold)
                        farNodes.Add(i);
                    else
                        closeNodes.Add(i);
                }

                if (closeNodes.Count != 0)
                {
                    int cn = closeNodes[closeNodes.Count - 1];
                    closeNodes.RemoveAt(closeNodes.Count - 1);
                    this.close = new DTNode(cn, closeNodes, depth+1);
                }

                if(farNodes.Count != 0)
                {
                    int fn = farNodes[farNodes.Count - 1];
                    farNodes.RemoveAt(farNodes.Count - 1);
                    this.far = new DTNode(fn, farNodes, depth + 1);
                }
            }
        }

        public bool contains(int i)
        {
            if (leaf)
                return items.Contains(i);

            if (node == i)
                return true;

            if (close != null)
            {
                if (close.contains(i))
                    return true;
            }

            if (far != null)
            {
                if (far.contains(i))
                    return true;
            }

            return false;
        }

        /// <summary>
        /// This method outputs the median of distances from a selected node
        /// </summary>
        /// <param name="node">Index of a currently checked node in the original array</param>
        /// <param name="children">List of children of the specified node</param>
        /// <returns>Returns the median of distances</returns>
        private double findMedian(int node, List<int> children)
        {            
            double[] distances = new double[children.Count];
            for (int i = 0; i < distances.Length; i++)
                distances[i] = cands[node].SqrtDistanceTo(cands[children[i]]);
            quickSelect(distances, 0, distances.Length-1, distances.Length / 2);
            return distances[distances.Length / 2];
        }

        private void quickSelect(double[] d, int l, int r, int s)
        {
            int t = split(d, l, r);
            if (t == s)
                return;
            if (t > s)
                quickSelect(d, l, t - 1, s);
            else
                quickSelect(d, t + 1, r, s);            
        }

        private int split(double[] d, int l, int r)
        {
            double pivot = d[r];
            while (true)
            {
                while ((d[l] < pivot)&&(l<r))
                    l++;
                if (l < r)
                {
                    d[r] = d[l];
                    r--;
                } else break;
                while ((d[r] >= pivot)&&(l<r))
                    r--;
                if (l < r)
                {
                    d[l] = d[r];
                    l++;
                } else break;
            }
            d[l] = pivot;
            return l;
        }

        internal void findAllCloserThan(Candidate query, double dist, List<int> result)
        {
            if (leaf)
            {
                foreach(int i in items)
                {
                    double d = query.SqrtDistanceTo(cands[i]);
                    if (d < dist)
                        result.Add(i);
                }
            }
            else
            {
                double d = query.SqrtDistanceTo(cands[node]);
                if (d < dist)
                    result.Add(node);
                if (d <= (threshold + dist))
                    if (close!= null)
                        close.findAllCloserThan(query, dist, result);
                if (d >= (threshold - dist))
                    far.findAllCloserThan(query, dist, result);
            }
        }

        public DTNode getClose()
        {
            return close;
        }

        public DTNode getFar()
        {
            return far;
        }
        public int getNode()
        {
            return node;
        }
    }

    class DistanceTree
    {
        DTNode root;
        Candidate[] cands;
        public DistanceTree(Candidate[] candidates)
        {
            cands = candidates;
            DTNode.cands = candidates;
            List<int> children = new List<int>();

            for (int i = 1; i < candidates.Length; i++)
                children.Add(i);

            root = new DTNode(0, children, 0);
        }

        public List<int> findAllCloserThan(Candidate query, double dist)
        {
            List<int> result = new List<int>();
            root.findAllCloserThan(query, dist, result);
            return result;
        }

        List<int> findAllCloserThanBF(Candidate query, double dist)
        {
            List<int> result = new List<int>();

            for (int i = 0; i < cands.Length; i++)
            {
                if (query.SqrtDistanceTo(cands[i]) < dist)
                    result.Add(i);
            }

            return result;
        }

        public void selfTest()
        {
            Random rnd = new Random();
            double m1 = 0;
            double m2 = 0;
            for (int i = 0;i<10000;i++)
            {
                int query = rnd.Next(cands.Length);
                double dist = rnd.NextDouble()*10;
                DateTime s = DateTime.Now;
                var r1 = findAllCloserThan(cands[query], dist);
                m1 += (DateTime.Now - s).TotalMilliseconds;
                s = DateTime.Now;
                var r2 = findAllCloserThanBF(cands[query], dist);
                m2 += (DateTime.Now - s).TotalMilliseconds;
                if (r1.Count != r2.Count)
                    throw new Exception("Different number of results");
                foreach (int r in r1)
                    if (!r2.Contains(r))
                        throw new Exception("Result mismatch");
                foreach (int r in r2)
                    if (!r1.Contains(r))
                        throw new Exception("Result mismatch");
            }
            Console.WriteLine("m1: {0}, m2:{1}", m1, m2);
        }
    }
}
