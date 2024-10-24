using System;
using System.IO;
using System.Text.RegularExpressions;
using DataView;
using MathNet.Numerics.LinearAlgebra;
using UnityEngine;

/// <summary>
/// Class that exports and fetches expected transformation from and to a file
/// Extension - txt
/// </summary>
public class TransformationIO
{
	public static void ExportTransformation(string path, Transform3D transformation)
	{
        StreamWriter streamWriter = new StreamWriter(path + ".txt");

        var rotationMatrix = transformation.RotationMatrix;
        var translationVector = transformation.TranslationVector;

        streamWriter.WriteLine("R {0}x{1}", rotationMatrix.ColumnCount, rotationMatrix.RowCount);

        string elementContent = "";
        for(int i = 0; i < rotationMatrix.ColumnCount; i++)
        {
            for (int j = 0; j < rotationMatrix.RowCount; j++)
                elementContent += string.Format(" {0} ", rotationMatrix[j, i]);

        }
        streamWriter.WriteLine(elementContent);

        streamWriter.WriteLine("t {0}", translationVector.Count);

        elementContent = "";
        for (int i = 0; i < translationVector.Count; i++)
            elementContent += string.Format(" {0} ", translationVector[i]);

        streamWriter.WriteLine(elementContent);
        streamWriter.Close();
    }

    public static Transform3D FetchTransformation(string path)
    {

        return new Transform3D(Matrix<double>.Build.DenseIdentity(3), Vector<double>.Build.Dense(3));
    }

    class StreamAutomaton
    {
        private Regex regex;
        private string remainingBuffer;

        public StreamAutomaton(string pattern)
        {
            // Initialize the regex with a given pattern
            regex = new Regex(pattern, RegexOptions.Compiled);
            remainingBuffer = string.Empty;
        }

        public void ProcessChunk(string chunk)
        {
            // Combine remaining part from the last chunk with the new chunk
            string combined = remainingBuffer + chunk;

            // Find all matches in the current combined text
            MatchCollection matches = regex.Matches(combined);

            foreach (System.Text.RegularExpressions.Match match in matches)
            {
                Console.WriteLine($"Match found: {match.Value}");
            }

            // Save any remaining unmatched text that might be a partial match at the end
            remainingBuffer = GetRemainingBuffer(combined);
        }

        private string GetRemainingBuffer(string text)
        {
            // Get the text that wasn't matched (could be part of the next chunk)
            System.Text.RegularExpressions.Match lastMatch = regex.Match(text, text.Length - 1);
            if (lastMatch.Success && lastMatch.Index + lastMatch.Length == text.Length)
            {
                return lastMatch.Value;
            }

            // If no partial match at the end, clear the remaining buffer
            return string.Empty;
        }
    }
}

