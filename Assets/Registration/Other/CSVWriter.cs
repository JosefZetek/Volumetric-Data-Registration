using System.Collections.Generic;
using System.IO;

namespace DataView
{
	public class CSVWriter
	{
		/// <summary>
		/// Writes 2D array into a CSV file
		/// </summary>
		/// <param name="fileName">Name of a file (including path and extension)</param>
		/// <param name="labelX">Header for the X axis</param>
		/// <param name="labelY">Header for the Y axis</param>
		/// <param name="listOfPoints">List of points to be printed out</param>
		public static void WriteResult(string fileName, string labelX, string labelY, List<Point2D> listOfPoints)
		{
			StreamWriter writer = new StreamWriter(fileName);
			writer.WriteLine("{0}; {1}", labelX, labelY);

			foreach(Point2D point in listOfPoints)
			{
                writer.WriteLine("{0}; {1}", point.X, point.Y);
            }

			writer.Flush();
			writer.Close();
		}
	}
}

