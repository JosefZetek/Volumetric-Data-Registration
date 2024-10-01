using System;
using System.IO;
using DataView;
using MathNet.Numerics;

class FileSaver
{

    private BinaryWriter binaryWriter;
    private StreamWriter streamWriter;

    private string fileName;
    private IData d;


    public FileSaver(string directory, string fileName, IData d)
    {
        this.fileName = fileName;
        this.d = d;

        if (d == null)
            throw new ArgumentException("No data were passed");

        if (d.Measures == null)
            throw new ArgumentException("Measures are not specified");

        if (d.Measures.Length != 3)
            throw new ArgumentException("Data need to be 3 dimensional");

        if (d.Measures[0] <= 0 || d.Measures[1] <= 0 || d.Measures[2] <= 0)
            throw new ArgumentException("None of the dimensions can be negative or zero");

        try
        {
            binaryWriter = new BinaryWriter(new FileStream(directory + fileName + ".raw", FileMode.Create));
            streamWriter = new StreamWriter(directory + fileName + ".mhd");
        }
        catch (IOException e) { throw e; }
    }

    public void MakeFiles()
    {
        try
        {
            MakeBinaryFile();
            MakeMHDFile();
        }
        catch (IOException e) { throw e; }
    }

    private void MakeMHDFile()
    {
        streamWriter.WriteLine("ObjectType = Image");
        streamWriter.WriteLine("NDims = 3");
        streamWriter.WriteLine("BinaryData = True");
        streamWriter.WriteLine("BinaryDataByteOrderMSB = False");
        streamWriter.WriteLine("CompressedData = False");
        streamWriter.WriteLine("TransformMatrix = 1 0 0 0 1 0 0 0 1");
        streamWriter.WriteLine("Offset = 0 0 0");
        streamWriter.WriteLine("CenterOfRotation = 0 0 0");
        streamWriter.WriteLine("AnatomicalOrientation = RAI");
        streamWriter.WriteLine("ElementSpacing = {0} {1} {2}", d.XSpacing, d.YSpacing, d.ZSpacing);
        streamWriter.WriteLine("DimSize = {0} {1} {2}", d.Measures[2], d.Measures[1], d.Measures[0]);
        streamWriter.WriteLine("ElementType = MET_USHORT");
        streamWriter.WriteLine("ElementDataFile = " + fileName + ".raw");

        streamWriter.Close();
    }

    private void MakeBinaryFile()
    {
        int maxX = d.Measures[0];
        int maxY = d.Measures[1];
        int maxZ = d.Measures[2];

        double currentValue;

        for(double x = 0; x < maxX; x+=d.XSpacing)
        {
            for (double y = 0; y < maxY; y += d.YSpacing)
            {
                for(double z = 0; z < maxZ; z+=d.ZSpacing)
                {
                    //USHORT is used, thus 2^16-1 is used for max value
                    currentValue = Math.Min(d.GetValue(x, y, z), Math.Pow(2, 16) - 1);
                    binaryWriter.Write((ushort)currentValue);
                }
            }
        }

        binaryWriter.Close();
    }
}
