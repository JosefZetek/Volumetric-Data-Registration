using UnityEngine;
using DataView;
using MathNet.Numerics.LinearAlgebra;

public class NewMonoBehaviour : MonoBehaviour
{
	// Use this for initialization
	void Start()
	{
        AMockObject mockObject = new EllipsoidMockData(3, 4, 5, new int[] { 10, 6, 8 }, new int[] { 1, 1, 1 });
		MockDataSegment mockObjectTranslated = new MockDataSegment(mockObject, new int[] { 10, 6, 8 }, new int[] { 1, 1, 1 });

		mockObjectTranslated.TransformObject(new Transform3D(Matrix<double>.Build.DenseIdentity(3), Vector<double>.Build.DenseOfArray(new double[] { -1, -2, -3 })));
		Debug.Log("Mock data created");


        FileSaver fileSaver = new FileSaver("/Users/pepazetek/Desktop/", "mockEllipsoidTranslated", mockObjectTranslated);
        fileSaver.MakeFiles();
		Debug.Log("File made");
    }

	// Update is called once per frame
	void Update()
	{
			
	}
}

