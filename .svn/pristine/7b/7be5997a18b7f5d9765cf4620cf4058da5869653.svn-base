using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using QuickGraph.Algorithms.MaximumFlow;

namespace HDictInduction.Console.SAT
{
    class SATTemp
    {
        static BidirectionalGraph<Word, Edge<Word>> gr;
        static public Dictionary<Edge<Word>, double> edgeCapacitiesDictionary = new Dictionary<Edge<Word>, double>();
        static public Dictionary<Edge<string>, double> edgeCapacitiesDictionaryTest = new Dictionary<Edge<string>, double>();
        
        static Edge<Word> MyEdgeFactory(Word source, Word target)
        {
            //Edge<Word> e =  null;
            //if (gr.TryGetEdge(source, target, out e))
            //    return e;
            return new Edge<Word>(source, target);
        }

        static Edge<string> MyEdgeFactoryTest(string source, string target)
        {
            //Edge<Word> e =  null;
            //if (gr.TryGetEdge(source, target, out e))
            //    return e;
            return new Edge<string>(source, target);
        }

        static double ComputeCapacity(Edge<Word> curEdge)
        {
            if (edgeCapacitiesDictionary.ContainsKey(curEdge) == true)
                return edgeCapacitiesDictionary[curEdge];
            else
                return 1; //you may want to put unused reversed edge to 0
        }

        static double ComputeCapacityTest(Edge<string> curEdge)
        {
            if (edgeCapacitiesDictionaryTest.ContainsKey(curEdge) == true)
                return edgeCapacitiesDictionaryTest[curEdge];
            else
                return 0; //you may want to put unused reversed edge to 0
        }

        public static string Main(BidirectionalGraph<Word, Edge<Word>> g)
        {
            gr = g;
            foreach (var item in g.Edges)
            {
                //edgeCapacitiesDictionary.Add(item, 0);
            }
            var reversedEdgeAugmentor = new ReversedEdgeAugmentorAlgorithm<Word, Edge<Word>>(g, MyEdgeFactory);
            reversedEdgeAugmentor.AddReversedEdges();

            //p.;

            // (other option) new PushRelabelMaximumFlowAlgorithm
            MaximumFlowAlgorithm<Word, Edge<Word>> algo = new EdmondsKarpMaximumFlowAlgorithm<Word, Edge<Word>>(g, ComputeCapacity, reversedEdgeAugmentor.ReversedEdges);

            Dictionary<Edge<Word>, double> d = new Dictionary<Edge<Word>, double>();
            foreach (var item in g.Edges)
            {
                algo = new EdmondsKarpMaximumFlowAlgorithm<Word, Edge<Word>>(g, ComputeCapacity, reversedEdgeAugmentor.ReversedEdges);
                double value = algo.Compute(item.Source, item.Target);
          
                d.Add(item, value);
            }

            //algo.Compute();
            //algo.Compute(source, sink);
            

            //return algo.MaxFlow;

            StringBuilder sb = new StringBuilder();
            foreach (var item in d)
            {
                sb.AppendLine(item.ToString());
            }
            return sb.ToString();
        }

        public static string Test()
        {

         
            ////////////////////////////////////////
            // test maximum flow algorith
            //////////////////////////////////////////
            var g = new BidirectionalGraph<string, Edge<string>>(true);
            string source = "A";
            string sink = "G";

            //Vertices
            //////////////
            g.AddVertex("A"); g.AddVertex("B"); g.AddVertex("C"); g.AddVertex("D");
            g.AddVertex("E"); g.AddVertex("F"); g.AddVertex("G");

            //Edge
            ////////////
            var edgesList = new List<Edge<string>>();
            var ad = new Edge<string>("A", "D"); g.AddEdge(ad); edgeCapacitiesDictionaryTest.Add(ad, 2);
            var ab = new Edge<string>("A", "B"); g.AddEdge(ab); edgeCapacitiesDictionaryTest.Add(ab, 3);
            var bc = new Edge<string>("B", "C"); g.AddEdge(bc); edgeCapacitiesDictionaryTest.Add(bc, 3);
            var ca = new Edge<string>("C", "A"); g.AddEdge(ca); edgeCapacitiesDictionaryTest.Add(ca, 4);
            var cd = new Edge<string>("C", "D"); g.AddEdge(cd); edgeCapacitiesDictionaryTest.Add(cd, 1);
            var de = new Edge<string>("D", "E"); g.AddEdge(de); edgeCapacitiesDictionaryTest.Add(de, 7);
            var df = new Edge<string>("D", "F"); g.AddEdge(df); edgeCapacitiesDictionaryTest.Add(df, 4);
            var eb = new Edge<string>("E", "B"); g.AddEdge(eb); edgeCapacitiesDictionaryTest.Add(eb, 1);
            var ce = new Edge<string>("C", "E"); g.AddEdge(ce); edgeCapacitiesDictionaryTest.Add(ce, 2);
            var eg = new Edge<string>("E", "G"); g.AddEdge(eg); edgeCapacitiesDictionaryTest.Add(eg, 3);
            var fg = new Edge<string>("F", "G"); g.AddEdge(fg); edgeCapacitiesDictionaryTest.Add(fg, 4);

            /////////////////////////////////////
            // creating the augmentor
            ////////////////////////////////////
            var reversedEdgeAugmentor = new ReversedEdgeAugmentorAlgorithm<string, Edge<string>>(g, MyEdgeFactoryTest);
            reversedEdgeAugmentor.AddReversedEdges();

            // (other option)                                                                           new PushRelabelMaximumFlowAlgorithm
            MaximumFlowAlgorithm<string, Edge<string>> algo = new EdmondsKarpMaximumFlowAlgorithm<string, Edge<string>>(g, /*e => 2*/ComputeCapacityTest, reversedEdgeAugmentor.ReversedEdges);



            algo.Compute(source, sink);
            //algo.Compute();
            StringBuilder sb = new StringBuilder();

            sb.AppendLine(string.Format("MaxFlow: {0}", algo.MaxFlow));

            sb.AppendLine("Press <ENTER> to complete");
            return sb.ToString();
        }
    }
}
