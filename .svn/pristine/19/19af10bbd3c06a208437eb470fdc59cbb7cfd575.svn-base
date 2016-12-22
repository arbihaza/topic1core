using System;

public class GAIndividual
{

	public static float[][] adj_matrix; // the adjacency matrix representation of graph

	private static Random randg = new Random(); // Random Generator
	// What if we choose a static random generator?
	// A unique random generator for all individuals!?

	public int nvert;
	public int[] left; // a subset of graph vertices
	public int[] right; // another subset of graph vertices
	// left and right are disjoint sets
	// left.length + right.length == nvert
	// left.length >= 1
	// right.length >= 1
	public int[] picklist;
	// picklist.length == left.length + right.length
	// picklist = left UNION right
	// picklist is a pick list! such that picklist[i] < picklist.length - i
	// for example {0, 0, 2, 2, 0, 0, 0} is a picklist and represents:
	// {0, 1, 4, 5, 2, 3, 6}

	public float fitness;

	public GAIndividual(int nvert)
	{
	// Create a random individual of size numv,
	// numv: number of vertices of the grapg

		this.nvert = nvert;
		int leftsize = randg.Next(nvert - 1) + 1;
		left = new int[leftsize];
		right = new int[nvert - leftsize];

		int[] tmp = GAUtils.randperm(nvert);
		for (int i = 0; i < left.Length; i++)
		{
			left[i] = tmp[i];
		}
		for (int i = left.Length; i < nvert; i++)
		{
			right[i - left.Length] = tmp[i];
		}

		picklist = GAUtils.perm2picklist(tmp);

		evalFitness(); // evaluate fitness of this new individual
	}
	public GAIndividual(int[] l, int[] r)
	{

		nvert = l.Length + r.Length;
		int[] tmp = new int[nvert];

		left = new int[l.Length];
		for (int i = 0; i < l.Length; i++)
		{
			left[i] = l[i];
			tmp[i] = l[i];
		}
		right = new int[r.Length];
		for (int i = 0; i < r.Length; i++)
		{
			right[i] = r[i];
			tmp[l.Length + i] = r[i];
		}

		picklist = GAUtils.perm2picklist(tmp);

		evalFitness(); // evaluate fitness of this new individual
	}

    private static bool nextBoolean()
    {
        return randg.Next(0, 2) == 0;
    }

	public virtual GAIndividual mutate()
	{
		int[] child_picklist = new int[nvert];
		for (int i = 0; i < nvert; i++)
		{
			child_picklist[i] = picklist[i];
		}

		// mutate one point:
		int m_point = randg.Next(nvert - 1); // mutation point
		child_picklist[m_point] = randg.Next(nvert - m_point);

		int[] tmp = GAUtils.picklist2perm(child_picklist);
		int[] l, r;

		if (nextBoolean() == true)
		{
		// this mutation preserves structure:
			if (nextBoolean() == true)
			{
				l = new int[left.Length];
				r = new int[right.Length];
			}
			else
			{
				l = new int[right.Length];
				r = new int[left.Length];
			}
		}
		else
		{
		// mutate structure:
			int dpoint = 1 + randg.Next(nvert - 1); // tmp division point
			l = new int[dpoint];
			r = new int[nvert - dpoint];
		}

		for (int i = 0; i < l.Length; i++)
		{
			l[i] = tmp[i];
		}
		for (int i = 0; i < r.Length; i++)
		{
			r[i] = tmp[l.Length + i];
		}

		 return new GAIndividual(l,r);
	}

    public static GAIndividual xover1p(GAIndividual f, GAIndividual m)
    {
        // 1-point cross over
        int xpoint = 1 + randg.Next(f.nvert - 1);
        int[] child_picklist = new int[f.nvert];
        for (int i = 0; i < xpoint; i++)
        {
            child_picklist[i] = f.picklist[i];
        }

        for (int i = xpoint; i < f.nvert; i++)
        {
            child_picklist[i] = m.picklist[i];
        }

        int[] tmp = GAUtils.picklist2perm(child_picklist);
        int[] l, r;

        if (nextBoolean() == true)
        {

            l = new int[f.left.Length]; // WHY?
            r = new int[tmp.Length - f.left.Length];
        }
        else
        {

            l = new int[m.left.Length]; // WHY?
            r = new int[tmp.Length - m.left.Length];
        }

        for (int i = 0; i < l.Length; i++)
        {
            l[i] = tmp[i];
        }
        for (int i = 0; i < r.Length; i++)
        {
            r[i] = tmp[l.Length + i];
        }

        return new GAIndividual(l, r);
    }

	public string ToString()
	{

		string s = "{";
        for (int i = 0; i < left.Length - 1; i++)
		{
			s += left[i] + ", ";
		}
        s += left[left.Length - 1] + "}";
		s += " {";
        for (int i = 0; i < right.Length - 1; i++)
		{
			s += right[i] + ", ";
		}
		s += right[right.Length - 1] + "}";

		s += "\nnumber of edges in between = " + (1.0f / fitness - 1);

		return s;
	}

	private void evalFitness()
	{
	// greater fitness means better individual
		int sum = 0; // number of edges between left and right
        for (int i = 0; i < left.Length; i++)
		{
            for (int j = 0; j < right.Length; j++)
/*->*/
			{	if (adj_matrix[left[i]][right[j]] == 1.0f) // it is assumed adj_matrix is symmetric
	{
					sum++;
	}
			}
		}

		fitness = 1.0f / (1.0f + sum);
	}

  public static void main(string[] args)
  {

		GAIndividual.adj_matrix = GAUtils.readMatrix("graph.txt");
		int nvert = GAIndividual.adj_matrix.Length; // number of vertices

		GAIndividual i1 = new GAIndividual(nvert);
		GAIndividual i2 = new GAIndividual(nvert);

		for (int i = 0; i < 1000; i++)
		{
			Console.WriteLine(new GAIndividual(nvert));
		}
  }
}