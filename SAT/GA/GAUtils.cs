using System;
using System.Collections;
using System.Collections.Generic;

public class GAUtils
{

	private static Random randg = new Random();

    public static int[] picklist2perm(int[] pl)
    {

        int[] perm = new int[pl.Length];

        ArrayList tmp = new ArrayList(pl.Length);
        for (int i = 0; i < pl.Length; i++)
            tmp.Add(i);

        for (int i = 0; i < pl.Length; i++)
        {
            perm[i] = ((Int32)tmp[pl[i]]);
            tmp.Remove(pl[i]);
        }

        return perm;
    }
    public static int[] perm2picklist(int[] pr)
    {

        int[] pl = new int[pr.Length];

        ArrayList tmp = new ArrayList(pr.Length);
        for (int i = 0; i < pr.Length; i++)
            tmp.Add(i);

        for (int i = 0; i < pr.Length; i++)
        {
            int index = tmp.IndexOf(pr[i]);
            tmp.Remove(index);
            pl[i] = index;
        }
        return pl;
    }

	public static int[] randperm(int n)
	{
	// return a random permutation of size n
	// that is an array containing a random permutation of values 0, 1, ..., n-1
		int[] perm = new int[n];
		for (int i = 0; i < n; i++)
		{
			perm[i] = i;
		}
		for (int i = 0; i < n - 1; i++)
		{
			int j = randg.Next(n - i) + i;
			// sawp perm[i] and perm[j]
			int temp = perm[i];
			perm[i] = perm[j];
			perm[j] = temp;
		}
		return perm;
	}

	public static float[][] readMatrix(string file_name)
	{
	// Read the graph description from file_name, and return the
	// adjacency matrix of the graph.
	// Each line of file_name is a comment (started with //) or
	// otherwise specify an edge of the graph, such that the first
	// number on this line is the starting vertex, the second is the
	// ending vertex and the third is the weight of this edge.
	// EACH LINE SPECIFIES JUST ONE EDGE! (NOT TWO AS IN UNDIRECTED GRAPHS)

		float[][] matrix; // the adjacency matrix

		try
		{
            string[] lines = System.IO.File.ReadAllLines(file_name);
			ArrayList line = new ArrayList();
            foreach (var s in lines)
            {
  				float[] tmp = new float[3];
                string[] st = s.Split(' ');
                if (st.Length!=3)
				{
					continue;
				}
                string token = st[0];
				if (token.Equals("//"))
				{
					continue;
				}

				for (int i = 0; i <= 2; i++)
				{
					tmp[i] = Convert.ToInt32(st[i]);
				}
				line.Add(tmp);
			}

			int max_index = 0; // number of graph vertices == max_index + 1
			for (int i = 0; i < line.Count; i++)
			{
				float[] tmp = (float[])(line[i]);
				if (tmp[0] > max_index)
				{
					max_index = (int) tmp[0];
				}
				if (tmp[1] > max_index)
				{
					max_index = (int) tmp[1];
				}
			}
//JAVA TO C# CONVERTER NOTE: The following call to the 'RectangularArrays' helper class reproduces the rectangular array initialization that is automatic in Java:
//ORIGINAL LINE: matrix = new float[max_index+1][max_index+1];
			matrix = RectangularArrays.ReturnRectangularFloatArray(max_index + 1, max_index + 1);
			for (int i = 0; i <= max_index; i++)
			{
				for (int j = 0; j <= max_index; j++)
				{
					matrix[i][j] = 0.0f;
				}
			}
			for (int i = 0; i < line.Count; i++)
			{
				float[] tmp = (float[])(line[i]);
				matrix[(int)tmp[0]][(int)tmp[1]] = tmp[2];
			}
		}
		catch (Exception ex)
		{
			Console.Error.WriteLine("error in " + file_name);
			return null;
		}
		return matrix;
	}

	public static string array2string(float[] n)
	{

		string s = "[";
		for (int i = 0; i < n.Length - 1; i++)
		{
			s += n[i] + ", ";
		}
		s += n[n.Length - 1] + "]";
		return s;
	}

  public static void Main(string[] args)
  {

		int[] a = new int[]{2,4,3,1,0,5};
		int[] b = GAUtils.perm2picklist(a);

		float[][] matrix = GAUtils.readMatrix("graph.txt");
		for (int i = 0; i < matrix.Length; i++)
		{
			Console.WriteLine(GAUtils.array2string(matrix[i]));
		}
  }
}