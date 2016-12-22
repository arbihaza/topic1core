using System;

public class GAEvolve
{

	public float[] best_fitness; // best_fitness[i] is the best fitness of i'th generation of this evolution
	public float[] worst_fitness;
	public float[] avr_fitness;

	public GAIndividual best_ind; // best individual of this evolution

	public GAEvolve(int generations, int pop_size, int nvert, int xrate, int mrate)
	{
		// xrate: cross-over rate, the initial value
		// mrate: mutation rate, the initial value
		// xrate and mrate may be adapted over generations
		// nvert: number of graph vertices

		best_fitness = new float[generations];
		worst_fitness = new float[generations];
		avr_fitness = new float[generations];

		GAPopulation gap = new GAPopulation(pop_size,nvert);
		best_fitness[0] = gap.ind[gap.best_index].fitness;
		worst_fitness[0] = gap.ind[gap.worst_index].fitness;
		avr_fitness[0] = gap.avr_fitness;

		for (int i = 1; i < generations; i++)
		{

            gap = GAPopulation.generate(gap, xrate, mrate);
			best_fitness[i] = gap.ind[gap.best_index].fitness;
			worst_fitness[i] = gap.ind[gap.worst_index].fitness;
			avr_fitness[i] = gap.avr_fitness;
		}

		best_ind = gap.ind[gap.best_index];
	}

	public static void evolveAndMakeMFile(string file_name, int runs, int generations, int pop_size, int nvert, int xrate, int mrate)
	{
	// makes a M file (file_name) so that if we run this file in MATLAB
	// the best fitness versus generation, average fitness versus generation
	// and wrost fitness versus generation are plotted.

	// best, average and worst fitness values are the average of "runs" evolutions.

		GAEvolve[] gae = new GAEvolve[runs];
		for (int i = 0; i < runs; i++)
		{
			gae[i] = new GAEvolve(generations,pop_size,nvert,xrate,mrate);
		}

		float[] avr_best_fitness = new float[generations];
		float[] avr_avr_fitness = new float[generations];
		float[] avr_worst_fitness = new float[generations];

		for (int i = 0; i < generations; i++)
		{
			avr_best_fitness[i] = 0;
			avr_avr_fitness[i] = 0;
			avr_worst_fitness[i] = 0;
			for (int j = 0; j < runs; j++)
			{
				avr_best_fitness[i] += gae[j].best_fitness[i] / (float) runs;
				avr_avr_fitness[i] += gae[j].avr_fitness[i] / (float) runs;
				avr_worst_fitness[i] += gae[j].worst_fitness[i] / (float) runs;
			}
		}

		try
		{
            System.IO.StreamWriter @out = new System.IO.StreamWriter(file_name,false);

			@out.WriteLine("\n% M.M.Haji");
            @out.WriteLine("% EC project # 3");
            @out.WriteLine("% GA progression results:\n");

			@out.Write("best_fitness = [");
			for (int i = 0; i < avr_best_fitness.Length; i++)
			{
                @out.Write(avr_best_fitness[i] + " ");
			}
            @out.WriteLine("];");

            @out.Write("average_fitness = [");
			for (int i = 0; i < avr_avr_fitness.Length; i++)
			{
                @out.Write(avr_avr_fitness[i] + " ");
			}
            @out.WriteLine("];");

            @out.Write("worst_fitness = [");
			for (int i = 0; i < avr_worst_fitness.Length; i++)
			{
                @out.Write(avr_worst_fitness[i] + " ");
			}
            @out.WriteLine("];\n");

            @out.WriteLine("plot(1:" + generations + ",best_fitness,1:" + generations + ",average_fitness,1:" + generations + ",worst_fitness)");
            @out.WriteLine("\nlegend('best fitness','average fitness','worst fitness');");
            @out.WriteLine("xlabel('generation');");
            @out.WriteLine("ylabel('fitness');");

            @out.Close();
		}
		catch (Exception ex1)
		{
			Console.WriteLine("error: can't create " + file_name);
		}
		Console.WriteLine(gae[gae.Length - 1].best_ind);
	}

  public static void Main()
  {

		GAIndividual.adj_matrix = GAUtils.readMatrix(@"d:\graph.txt");
		int nvert = GAIndividual.adj_matrix.Length; // number of vertices

		GAEvolve.evolveAndMakeMFile(@"d:\ga.m",1,400,200,nvert,50,50);
  }
}