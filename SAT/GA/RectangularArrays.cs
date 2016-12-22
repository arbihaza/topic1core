//----------------------------------------------------------------------------------------
//	Copyright c 2007 - 2013 Tangible Software Solutions Inc.
//	This class can be used by anyone provided that the copyright notice remains intact.
//
//	This class provides the logic to simulate Java rectangular arrays, which are jagged
//	arrays with inner arrays of the same length.
//----------------------------------------------------------------------------------------
internal static partial class RectangularArrays
{
    internal static float[][] ReturnRectangularFloatArray(int Size1, int Size2)
    {
        float[][] Array = new float[Size1][];
        for (int Array1 = 0; Array1 < Size1; Array1++)
        {
            Array[Array1] = new float[Size2];
        }
        return Array;
    }
}