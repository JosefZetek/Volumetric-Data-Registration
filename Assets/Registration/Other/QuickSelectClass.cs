using System;
using System.Collections.Generic;

namespace DataView
{
    public class QuickSelectClass
	{

		Random random;

        public QuickSelectClass()
		{
            random = new Random();
        }
        
        public T QuickSelect<T>(List<T> givenList, int elementNumber) where T : IComparable<T>
		{
            List<T> smallerList = new List<T>();
            List<T> biggerList = new List<T>();

			int pivotIndex = random.Next(0, givenList.Count);
            T pivot = givenList[pivotIndex];

			for(int i = 0; i<givenList.Count; i++)
			{
				if (pivotIndex == i)
					continue;

                if (givenList[i].CompareTo(pivot) < 0)
                {
					smallerList.Add(givenList[i]);
					continue;
                }

                biggerList.Add(givenList[i]);

            }

            if (elementNumber<smallerList.Count)
                return QuickSelect(smallerList, elementNumber);

			if (elementNumber == smallerList.Count)
				return pivot;

			return QuickSelect(biggerList, elementNumber - smallerList.Count - 1);
		}
	}
}

