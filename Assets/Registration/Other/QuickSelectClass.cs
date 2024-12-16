using System;
using System.Collections.Generic;

namespace DataView
{
    public class QuickSelectClass
	{

		private Random random;

        public QuickSelectClass()
		{
            random = new Random();
        }

        public T QuickSelect<T>(List<T> givenList, int elementNumber) where T : IComparable<T>
        {
            List<T> listToSort = givenList;

            List<T> smallerList = new List<T>();
            List<T> biggerList = new List<T>();

            while (true)
            {
                int pivotIndex = random.Next(0, listToSort.Count);
                T pivot = listToSort[pivotIndex];

                for (int i = 0; i < listToSort.Count; i++)
                {
                    if (pivotIndex == i)
                        continue;

                    if (listToSort[i].CompareTo(pivot) <= 0)
                    {
                        smallerList.Add(listToSort[i]);
                        continue;
                    }

                    biggerList.Add(listToSort[i]);
                }

                if (elementNumber < smallerList.Count)
                {
                    listToSort = smallerList;
                    smallerList = new List<T>();
                    biggerList.Clear();
                    continue;
                }

                if (elementNumber == smallerList.Count)
                    return pivot;

                elementNumber -= (smallerList.Count + 1);
                listToSort = biggerList;
                smallerList.Clear();
                biggerList = new List<T>();
            }
        }
    }
}

