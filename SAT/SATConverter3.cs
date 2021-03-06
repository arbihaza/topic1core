﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;
using QuickGraph.Algorithms.MaximumFlow;
using QuickGraph.Algorithms.RankedShortestPath;
using QuickGraph.Algorithms.ShortestPath;
using System.Threading.Tasks;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace HDictInduction.Console.SAT
{
    class SATConverter3 
    {
        //catche
        private static Dictionary<int, bool> Cache1 = new Dictionary<int, bool>();
        private static Dictionary<int, SLink> LinkCache = new Dictionary<int, SLink>();
        private static Dictionary<SLink, float> LinkWeightCache = new Dictionary<SLink, float>();

        //maps
        private Dictionary<SLink, string> linkVarMap;
        private Dictionary<WordPair, int> pairVarMap;
        private Dictionary<WordPair, double> newPairVarMap = new Dictionary<WordPair, double>();
        private Dictionary<int,WordPair > varPairMap;
        private Dictionary<WordPair, bool> generatedPairMap;

        BidirectionalGraph<Word, Edge<Word>> semiCompleteGraph;
        BidirectionalGraph<Word, Edge<Word>> completeGraph;
        BidirectionalGraph<Word, Edge<Word>> graph;

        private bool enableComment = true;
        private string maxCost = "100000000000000000000000000000000000000000";
        private string maxCost2 = "99999999999999999999999999999999999999999";
        private long totalCost;
        private long totalCostHistory;
        private long totalCostHistory2;
        private long currentCost;
        public static long highestCost = 0;

        public static int symmetryCycle = 1;
        public static int currentCycle = 1;
        public static int languageOption = 2;
        //public static double omega2Threshold = 0;
        public static double autoThreshold = 0;
        //public static double omega3Threshold = 0;
        //public static bool acceptOmega3NewPair = true;
        public static List<string> allPairsNoRank = new List<string>();
        
        private List<WordPair> GeneratePossiblePairs(BidirectionalGraph<Word, Edge<Word>> g)
        {
            LinkCache = new Dictionary<int, SLink>();
            LinkWeightCache = new Dictionary<SLink, float>();
            graph = new BidirectionalGraph<Word, Edge<Word>>(false);
            semiCompleteGraph = new BidirectionalGraph<Word, Edge<Word>>(false);
            completeGraph = new BidirectionalGraph<Word, Edge<Word>>(false);
            #region Prepare graph
            foreach (var item in g.Vertices)
            {
                graph.AddVertex(item);
                semiCompleteGraph.AddVertex(item);
                completeGraph.AddVertex(item);
            }

            foreach (var item in g.Edges) 
            {
                if (item.Source.Language == Console.Language.Chinese && item.Target.Language == Console.Language.Uyghur)
                {
                    graph.AddEdge(new Edge<Word>(item.Target, item.Source));
                    semiCompleteGraph.AddEdge(new Edge<Word>(item.Target, item.Source));
                    completeGraph.AddEdge(new Edge<Word>(item.Target, item.Source));
                }
                else
                {
                    graph.AddEdge(new Edge<Word>(item.Source, item.Target));
                    semiCompleteGraph.AddEdge(new Edge<Word>(item.Source, item.Target));
                    completeGraph.AddEdge(new Edge<Word>(item.Source, item.Target));
                }
            }
            #endregion

            List<WordPair> ooPairs = new List<WordPair>();
            Dictionary<WordPair, bool> ooPairsDict = new Dictionary<WordPair, bool>();

            var uWords = graph.Vertices.Where(t => t.Language == Language.Uyghur);
            var kWords = graph.Vertices.Where(t => t.Language == Language.Kazak);
            var cWords = graph.Vertices.Where(t => t.Language == Language.Chinese);

            //int uWordCount = uWords.Count();
            //int kWordCount = kWords.Count();
            //int cWordCount = cWords.Count();
            int u =0,k=0;

            /*foreach (var uWord in uWords)
            {
                foreach (var edge1 in graph.OutEdges(uWord))
                {
                    foreach (var  edge2 in graph.OutEdges(edge1.Target))
                    {
                        Word kWord = edge2.Target;
                        WordPair ooPair = new WordPair(uWord, kWord);
                        ooPair = createNewEdges(uWord, kWord, graph);//, uWordCount, kWordCount);
                        if (ooPairsDict.ContainsKey(ooPair))
                            continue;
                        else
                        {
                            ooPairsDict.Add(ooPair, true);
                            ooPairs.Add(ooPair);
                        } 
                    }
                }
 
            }*/
            currentCycle = 1;
            while (currentCycle <= symmetryCycle)
            {
                Debug.WriteLine(currentCycle);
                if (currentCycle > 1)
                {
                    //semiCompleteGraph.Clear();
                    semiCompleteGraph = completeGraph;
                }                
                ooPairsDict.Clear();
                ooPairs.Clear();
                foreach (var uWord in uWords)
                {
                    Word cWord;
                    Word kWord;

                    foreach (var edge1 in graph.OutEdges(uWord))
                    {
                        cWord = edge1.Target;
                        foreach (var edge2 in graph.OutEdges(cWord))
                        {
                            kWord = edge2.Target;

                            WordPair pair = new WordPair(uWord, kWord);
                            pair = createNewEdges(uWord, kWord, semiCompleteGraph, completeGraph);
                            if (ooPairsDict.ContainsKey(pair))
                                continue;
                            else
                            {
                                ooPairsDict.Add(pair, true);
                                ooPairs.Add(pair);
                            }
                        }
                    }
                }
                
                currentCycle++;                
            }
            /*//2nd cycle to add new blue edge and generate more pairs
            //Add new pair candidate from the semiCompleteGraph
            if (false)//languageOption == 2 && symmetryCycle > 1)
            {
                ooPairsDict.Clear();
                ooPairs.Clear();

                foreach (var uWord in uWords)
                {
                    Word cWord; 
                    Word kWord;

                    foreach (var edge1 in semiCompleteGraph.OutEdges(uWord))
                    {
                        cWord = edge1.Target;
                        foreach (var edge2 in semiCompleteGraph.OutEdges(cWord))
                        {
                            kWord = edge2.Target;

                            WordPair pair = new WordPair(null, null);
                            pair = createNewBlueEdges(uWord, kWord, semiCompleteGraph);
                            if (ooPairsDict.ContainsKey(pair))
                                continue;
                            else
                            {
                                ooPairsDict.Add(pair, true);
                                ooPairs.Add(pair);
                            }                            
                        }
                    }
                }
            }

            //3rd cycle to add new green edge and generate more pairs
            //Add new pair candidate from the semiCompleteGraph
            if (false)//languageOption == 2 && symmetryCycle > 2)
            {
                ooPairsDict.Clear();
                ooPairs.Clear();

                foreach (var uWord in uWords)
                {
                    Word cWord;
                    Word kWord;

                    foreach (var edge1 in completeGraph.OutEdges(uWord))
                    {
                        cWord = edge1.Target;
                        foreach (var edge2 in completeGraph.OutEdges(cWord))
                        {
                            kWord = edge2.Target;

                            WordPair pair = new WordPair(null, null);
                            pair = createNewGreenEdges(uWord, kWord, completeGraph);
                            if (ooPairsDict.ContainsKey(pair))
                                continue;
                            else
                            {
                                ooPairsDict.Add(pair, true);
                                ooPairs.Add(pair);
                            }                            
                        }
                    }
                }
            }*/
            float maxWeight = 0;
            return ooPairs;
        }
        
        public List<WordPair> GenerateAllNaivePairs(BidirectionalGraph<Word, Edge<Word>> graph)
        {
            /*BidirectionalGraph<Word, Edge<Word>> graph = new BidirectionalGraph<Word, Edge<Word>>(false);
            #region Prepare graph
            foreach (var item in g.Vertices)
                graph.AddVertex(item);
            #endregion*/
            List<WordPair> pairs = new List<WordPair>();
            //List<KeyValuePair<Word, Word>> pairs = new List<KeyValuePair<Word, Word>>();
            //List<KeyValuePair<List<KeyValuePair<Word, Word>>, float>> pairsProb = new List<KeyValuePair<List<KeyValuePair<Word, Word>>, float>>();

            var uWords = graph.Vertices.Where(t => t.Language == Language.Uyghur);
            var kWords = graph.Vertices.Where(t => t.Language == Language.Kazak);
            var cWords = graph.Vertices.Where(t => t.Language == Language.Chinese);
            foreach (var uWord in uWords)
            {
                float connectedUC = (float)graph.InDegree(uWord);
                foreach (var kWord in kWords)
                {
                    float connectedCK = (float)graph.InDegree(kWord);
                    WordPair pair = new WordPair(uWord, kWord);
                    pair.Prob = (connectedUC + connectedCK) / (2 * cWords.Count());
                    pairs.Add(pair);
                }
            }    

            /*var output = pairs.Select(t => string.Format("{0},{1}", t.Key, t.Value));
            System.IO.File.WriteAllLines(@"buffer\NaiveCombination.txt", output);
            System.Media.SoundPlayer simpleSound = new System.Media.SoundPlayer(@"c:\Windows\Media\Ring03.wav");
            simpleSound.Play();
            Debug.WriteLine("Generate All Naive pairs is done");
            */
            return pairs;
        }

        private WordPair createNewEdges(Word uWord, Word kWord, BidirectionalGraph<Word, Edge<Word>> semiCompleteGraph, BidirectionalGraph<Word, Edge<Word>> completeGraph)//, int uCount, int kCount)
        {
            Cache1.Clear();
            List<SPath> paths = new List<SPath>();
            WordPair pair = new WordPair(uWord, kWord);
            pair.Polysemy = 0f;
            foreach (var item in semiCompleteGraph.OutEdges(uWord)) // Using the updated graph with new edges
            {
                int inDegreePivot = (int)semiCompleteGraph.InDegree(item.Target);
                int outDegreePivot = (int)semiCompleteGraph.OutDegree(item.Target);
                int totalSenseEdge = (Math.Max(inDegreePivot, outDegreePivot) - 1) * (inDegreePivot + outDegreePivot);
                pair.Polysemy += totalSenseEdge;
                //Word pivot_sense = item.Target;
                //for (int sense = 1; sense <= totalSense; sense++)
                //{
                //pivot_sense.Value = pivot_sense.Value + "_sense" + sense; 

                SLink linkCU = new SLink(item.Target, uWord);
                linkCU.Exists = true;// semiCompleteGraph.ContainsEdge(uWord, pivot_sense);

                SLink linkCK = new SLink(item.Target, kWord);
                linkCK.Exists = semiCompleteGraph.ContainsEdge(item.Target, kWord);

                SPath path = new SPath(linkCU, linkCK);
                paths.Add(path);
                
                Cache1.Add(item.Target.ID, true);
                //}               
            }

            foreach (var item in semiCompleteGraph.InEdges(kWord)) // Using the updated graph with new edges
            {
                if (Cache1.ContainsKey(item.Source.ID))
                    continue;
                int inDegreePivot = (int)semiCompleteGraph.InDegree(item.Source);
                int outDegreePivot = (int)semiCompleteGraph.OutDegree(item.Source);
                int totalSenseEdge = (Math.Max(inDegreePivot, outDegreePivot) - 1) * (inDegreePivot + outDegreePivot);
                pair.Polysemy += totalSenseEdge;

                SLink linkCK = new SLink(item.Source, kWord);
                linkCK.Exists = true;// semiCompleteGraph.ContainsEdge(item.Source, kWord);

                SLink linkCU = new SLink(item.Source, uWord);
                linkCU.Exists = semiCompleteGraph.ContainsEdge(uWord, item.Source);

                SPath path = new SPath(linkCU, linkCK);
                paths.Add(path);
            }

            //calculate probability

            //float couverage = Math.Min(uCount, kCount) / (float)Math.Max(uCount, kCount);
            float pUK = 0;
            float pKU = 0;
            float probUK = 0;
            float probKU = 0;
            //bool hasPolysemy = false;
            foreach (var item in paths)
            {
                //if (!item.LinkCU.Exists || !item.LinkCK.Exists) //containning non-existance link
                if (!item.LinkCU.Exists)
                {
                    if (languageOption == 2)
                        completeGraph.AddEdge(new Edge<Word>(item.LinkCU.WordNonPivot, item.LinkCU.WordPivot));
                    continue;
                }
                if (!item.LinkCK.Exists)
                {
                    if (languageOption == 2)
                        completeGraph.AddEdge(new Edge<Word>(item.LinkCK.WordPivot, item.LinkCK.WordNonPivot));
                    continue;
                }
                //if ((float)graph.InDegree(item.LinkCU.WordPivot) > 1 || (float)graph.OutDegree(item.LinkCK.WordPivot) > 1)
                //    hasPolysemy = true;                    
                if (currentCycle == 1)
                {
                    float PrCU = 1.0f / (float)semiCompleteGraph.OutDegree(item.LinkCU.WordNonPivot); //P(C|U) = P(C&U)/P(U)
                    float PrKC = 1.0f / (float)semiCompleteGraph.OutDegree(item.LinkCK.WordPivot);    //P(K|C) = P(K&C)/P(C)
                    float PrCK = 1.0f / (float)semiCompleteGraph.InDegree(item.LinkCK.WordNonPivot);  //P(C|K) = P(C&K)/P(K)
                    float PrUC = 1.0f / (float)semiCompleteGraph.InDegree(item.LinkCU.WordPivot);     //P(U|C) = P(U&C)/P(C)

                    pKU += PrCU * PrKC;
                    pUK += PrCK * PrUC;
                }
                else
                {
                    float PrCU = 0.0f;
                    float PrKC = 0.0f;
                    float PrCK = 0.0f;
                    float PrUC = 0.0f;
                    foreach (var downEdgeCU in semiCompleteGraph.OutEdges(item.LinkCU.WordNonPivot)) //Loop through down-path from nonpivot1 to pivot
                    {
                        PrCU += 1.0f / LinkWeightCache[new SLink(downEdgeCU.Target, downEdgeCU.Source)]; //P(C|U) = P(C&U)/P(U)
                    }
                    foreach (var downEdgeCK in semiCompleteGraph.OutEdges(item.LinkCK.WordPivot)) //Loop through down-path from pivot to nonpivot2
                    {
                        PrKC += 1.0f / LinkWeightCache[new SLink(downEdgeCK.Source, downEdgeCK.Target)]; //P(K|C) = P(K&C)/P(C)
                    }
                    foreach (var upEdgeCK in semiCompleteGraph.InEdges(item.LinkCK.WordNonPivot)) //Loop through up-path from nonpivot2 to pivot
                    {
                        PrCK += 1.0f / LinkWeightCache[new SLink(upEdgeCK.Target, upEdgeCK.Source)]; //P(C|K) = P(C&K)/P(K)
                    }
                    foreach (var upEdgeCU in semiCompleteGraph.InEdges(item.LinkCU.WordPivot)) //Loop through up-path from pivot to nonpivot1
                    {
                        PrUC += 1.0f / LinkWeightCache[new SLink(upEdgeCU.Source, upEdgeCU.Target)]; //P(U|C) = P(U&C)/P(C)
                    }

                    PrCU = 1.0f / PrCU;
                    PrKC = 1.0f / PrKC;
                    PrCK = 1.0f / PrCK;
                    PrUC = 1.0f / PrUC;
                    pUK += PrUC * PrCK;
                    pKU += PrKC * PrCU;
                }                
            }
            probUK = pUK * pKU;

            //WordPair pair = new WordPair(uWord, kWord);
            //pair.HasMissingEdge = hasPolysemy;
            pair.Paths = paths;
            pair.Prob = (float)probUK;
            pair.Polysemy *= (1 - pair.Prob);

            //set link weights
            foreach (var item in pair.Paths)
            {
                //CU
                //float polysemyCost = 1 / ((float)graph.InDegree(item.LinkCU.WordPivot) * (float)graph.OutDegree(item.LinkCK.WordPivot));
                if (item.LinkCU.Exists)
                {
                    item.LinkCU.Pr = 1f; //polysemyCost; 
                    if (!LinkWeightCache.ContainsKey(item.LinkCU))
                        LinkWeightCache.Add(item.LinkCU, item.LinkCU.Pr);
                }
                else
                {
                    //pair.HasMissingCUEdge = true;
                    float value = 0;
                    if (LinkWeightCache.TryGetValue(item.LinkCU, out value))
                    {
                        if (pair.Prob > value)
                            item.LinkCU.Pr = LinkWeightCache[item.LinkCU] = pair.Prob; //polysemyCost *
                        else
                            item.LinkCU.Pr = value;
                    }
                    else
                    {
                        item.LinkCU.Pr = pair.Prob; //polysemyCost * 
                        LinkWeightCache.Add(item.LinkCU, pair.Prob);
                    }
                }

                //CK
                if (item.LinkCK.Exists)//false)//
                {
                    item.LinkCK.Pr = 1f; //polysemyCost
                    if (!LinkWeightCache.ContainsKey(item.LinkCK))
                        LinkWeightCache.Add(item.LinkCK, item.LinkCK.Pr);
                }
                else
                {
                    float value = 0;
                    if (LinkWeightCache.TryGetValue(item.LinkCK, out value))
                    {
                        if (pair.Prob > value)
                            LinkWeightCache[item.LinkCK] = item.LinkCK.Pr = pair.Prob; //polysemyCost * 
                        else
                            item.LinkCK.Pr = value;
                    }
                    else
                    {
                        item.LinkCK.Pr = pair.Prob; //polysemyCost * 
                        LinkWeightCache.Add(item.LinkCK, pair.Prob);
                    }
                }
            }
            return pair;
        }

        private WordPair createNewBlueEdges(Word uWord, Word kWord, BidirectionalGraph<Word, Edge<Word>> graph)
        {
            Cache1.Clear();
            List<SPath> paths = new List<SPath>();

            foreach (var item in graph.OutEdges(uWord))
            {
                SLink linkCU = new SLink(item.Target, uWord);
                linkCU.Exists = graph.ContainsEdge(uWord, item.Target);

                SLink linkCK = new SLink(item.Target, kWord);
                linkCK.Exists = graph.ContainsEdge(item.Target, kWord);

                SPath path = new SPath(linkCU, linkCK);
                paths.Add(path);

                Cache1.Add(item.Target.ID, true);
            }

            foreach (var item in graph.InEdges(kWord))
            {
                if (Cache1.ContainsKey(item.Source.ID))
                    continue;
                SLink linkCK = new SLink(item.Source, kWord);
                linkCK.Exists = graph.ContainsEdge(item.Source, kWord);

                SLink linkCU = new SLink(item.Source, uWord);
                linkCU.Exists = graph.ContainsEdge(uWord, item.Source);

                SPath path = new SPath(linkCU, linkCK);
                paths.Add(path);
            }

            //calculate probability

            float pUK = 0;
            float pKU = 0;
            float probUK = 0;
            float probKU = 0;
            foreach (var item in paths)
            {
                if (!item.LinkCU.Exists)
                {
                    if (languageOption == 2)
                        completeGraph.AddEdge(new Edge<Word>(item.LinkCU.WordNonPivot, item.LinkCU.WordPivot));
                    continue;
                }
                if (!item.LinkCK.Exists)
                {
                    if (languageOption == 2)
                        completeGraph.AddEdge(new Edge<Word>(item.LinkCK.WordPivot, item.LinkCK.WordNonPivot));
                    continue;
                }
                    
                float PrCU = 0.0f;
                float PrKC = 0.0f;
                float PrCK = 0.0f;
                float PrUC = 0.0f;
                foreach (var downEdgeCU in graph.OutEdges(item.LinkCU.WordNonPivot)) //Loop through down-path from nonpivot1 to pivot
                {
                    PrCU += 1.0f / LinkWeightCache[new SLink(downEdgeCU.Target, downEdgeCU.Source)]; //P(C|U) = P(C&U)/P(U)
                }
                foreach (var downEdgeCK in graph.OutEdges(item.LinkCK.WordPivot)) //Loop through down-path from pivot to nonpivot2
                {
                    PrKC += 1.0f / LinkWeightCache[new SLink(downEdgeCK.Source, downEdgeCK.Target)]; //P(K|C) = P(K&C)/P(C)
                }
                foreach (var upEdgeCK in graph.InEdges(item.LinkCK.WordNonPivot)) //Loop through up-path from nonpivot2 to pivot
                {
                    PrCK += 1.0f / LinkWeightCache[new SLink(upEdgeCK.Source, upEdgeCK.Target)]; //P(C|K) = P(C&K)/P(K)
                }
                foreach (var upEdgeCU in graph.InEdges(item.LinkCU.WordPivot)) //Loop through up-path from pivot to nonpivot1
                {
                    PrUC += 1.0f / LinkWeightCache[new SLink(upEdgeCU.Target, upEdgeCU.Source)]; //P(U|C) = P(U&C)/P(C)
                }
                
                PrCU = 1.0f / PrCU;
                PrKC = 1.0f / PrKC;
                PrCK = 1.0f / PrCK;
                PrUC = 1.0f / PrUC;
                pUK += PrUC * PrCK;
                pKU += PrKC * PrCU;                
            }
            probUK = pUK * pKU;

            WordPair pair = new WordPair(uWord, kWord);
            pair.Paths = paths;
            pair.Prob = (float)probUK;

            //set link weights
            foreach (var item in pair.Paths)
            {
                //CU
                if (!item.LinkCU.Exists)
                {
                    float value = 0;
                    if (LinkWeightCache.TryGetValue(item.LinkCU, out value))
                    {
                        if (pair.Prob > value)
                            item.LinkCU.Pr = LinkWeightCache[item.LinkCU] = pair.Prob;
                        else
                            item.LinkCU.Pr = value;
                    }
                    else
                    {
                        item.LinkCU.Pr = pair.Prob;
                        LinkWeightCache.Add(item.LinkCU, pair.Prob);
                    }
                }

                //CK
                if (!item.LinkCK.Exists)
                {
                    float value = 0;
                    if (LinkWeightCache.TryGetValue(item.LinkCK, out value))
                    {
                        if (pair.Prob > value)
                            LinkWeightCache[item.LinkCK] = item.LinkCK.Pr = pair.Prob;
                        else
                            item.LinkCK.Pr = value;
                    }
                    else
                    {
                        item.LinkCK.Pr = pair.Prob;
                        LinkWeightCache.Add(item.LinkCK, pair.Prob);
                    }
                }
            }
            return pair;
        }

        private WordPair createNewGreenEdges(Word uWord, Word kWord, BidirectionalGraph<Word, Edge<Word>> graph)
        {
            Cache1.Clear();
            List<SPath> paths = new List<SPath>();

            foreach (var item in graph.OutEdges(uWord))
            {
                SLink linkCU = new SLink(item.Target, uWord);
                linkCU.Exists = graph.ContainsEdge(uWord, item.Target);

                SLink linkCK = new SLink(item.Target, kWord);
                linkCK.Exists = graph.ContainsEdge(item.Target, kWord);

                SPath path = new SPath(linkCU, linkCK);
                paths.Add(path);

                Cache1.Add(item.Target.ID, true);
            }

            foreach (var item in graph.InEdges(kWord))
            {
                if (Cache1.ContainsKey(item.Source.ID))
                    continue;
                SLink linkCK = new SLink(item.Source, kWord);
                linkCK.Exists = graph.ContainsEdge(item.Source, kWord);

                SLink linkCU = new SLink(item.Source, uWord);
                linkCU.Exists = graph.ContainsEdge(uWord, item.Source);

                SPath path = new SPath(linkCU, linkCK);
                paths.Add(path);
            }

            //calculate probability

            float pUK = 0;
            float pKU = 0;
            float probUK = 0;
            float probKU = 0;
            foreach (var item in paths)
            {
                if (!item.LinkCU.Exists || !item.LinkCK.Exists) //containing non-existance link
                    continue;
                float PrCU = 0.0f;
                float PrKC = 0.0f;
                float PrCK = 0.0f;
                float PrUC = 0.0f;
                foreach (var downEdgeCU in graph.OutEdges(item.LinkCU.WordNonPivot)) //Loop through down-path from nonpivot1 to pivot
                {
                    PrCU += 1.0f / LinkWeightCache[new SLink(downEdgeCU.Target, downEdgeCU.Source)]; //P(C|U) = P(C&U)/P(U)
                }
                foreach (var downEdgeCK in graph.OutEdges(item.LinkCK.WordPivot)) //Loop through down-path from pivot to nonpivot2
                {
                    PrKC += 1.0f / LinkWeightCache[new SLink(downEdgeCK.Source, downEdgeCK.Target)]; //P(K|C) = P(K&C)/P(C)
                }
                foreach (var upEdgeCK in graph.InEdges(item.LinkCK.WordNonPivot)) //Loop through up-path from nonpivot2 to pivot
                {
                    PrCK += 1.0f / LinkWeightCache[new SLink(upEdgeCK.Source, upEdgeCK.Target)]; //P(C|K) = P(C&K)/P(K)
                }
                foreach (var upEdgeCU in graph.InEdges(item.LinkCU.WordPivot)) //Loop through up-path from pivot to nonpivot1
                {
                    PrUC += 1.0f / LinkWeightCache[new SLink(upEdgeCU.Target, upEdgeCU.Source)]; //P(U|C) = P(U&C)/P(C)
                }

                PrCU = 1.0f / PrCU;
                PrKC = 1.0f / PrKC;
                PrCK = 1.0f / PrCK;
                PrUC = 1.0f / PrUC;
                pUK += PrUC * PrCK;
                pKU += PrKC * PrCU;
            }
            probUK = pUK * pKU;

            WordPair pair = new WordPair(uWord, kWord);
            pair.Paths = paths;
            pair.Prob = (float)probUK;

            //set link weights
            foreach (var item in pair.Paths)
            {
                //CU
                if (!item.LinkCU.Exists)
                {
                    float value = 0;
                    if (LinkWeightCache.TryGetValue(item.LinkCU, out value))
                    {
                        if (pair.Prob > value)
                            item.LinkCU.Pr = LinkWeightCache[item.LinkCU] = pair.Prob;
                        else
                            item.LinkCU.Pr = value;
                    }
                    else
                    {
                        item.LinkCU.Pr = pair.Prob;
                        LinkWeightCache.Add(item.LinkCU, pair.Prob);
                    }
                }

                //CK
                if (!item.LinkCK.Exists)
                {
                    float value = 0;
                    if (LinkWeightCache.TryGetValue(item.LinkCK, out value))
                    {
                        if (pair.Prob > value)
                            LinkWeightCache[item.LinkCK] = item.LinkCK.Pr = pair.Prob;
                        else
                            item.LinkCK.Pr = value;
                    }
                    else
                    {
                        item.LinkCK.Pr = pair.Prob;
                        LinkWeightCache.Add(item.LinkCK, pair.Prob);
                    }
                }
            }
            return pair;
        }

        private string encode(List<WordPair> pairs, Dictionary<WordPair, int> ooPairs, FileInfo file)
        {
            if (pairVarMap == null)
            {
                pairVarMap = new Dictionary<WordPair, int>();
                varPairMap = new Dictionary<int, WordPair>();
            }
            StringBuilder cnfBuffer = new StringBuilder();
            StringBuilder buffer = new StringBuilder();

            float maxProb = 0f;
            int varCount = 0;
            int varHelperCount = 0;
            int clauseCount = 0;
            bool overSized = false;
            bool populateVarPairMap = pairVarMap.Count == 0;
            
            if (linkVarMap == null)
            {
                linkVarMap = new Dictionary<SLink, string>();
                List<SLink> links = LinkWeightCache.Keys.OrderBy(t => t.WordNonPivot.Language).OrderByDescending(t => t.Exists).ToList();
                for (int i = 0; i < links.Count; i++)
                {
                    linkVarMap.Add(links[i], (i + 1).ToString());
                }
            }

            varCount = linkVarMap.Count;
            foreach (var item in linkVarMap)
            {
                string cost = string.Empty;
                //double realCost = 0;
                if (item.Key.Pr == 1)//item.Key.Exists)
                {
                    cost = maxCost;
                    cnfBuffer.AppendLine(string.Format("{0} {1} 0", cost, item.Value));
                    clauseCount++;                                        
                }
                else
                {
                    //realCost = Math.Round((1 - item.Key.Pr) * 1000000000); //realCost = Math.Round((1 - (item.Key.Pr / maxProb)) * 1000000000);
                    cost = Math.Round((1 - item.Key.Pr ) * 1000000000).ToString(); //cost = Math.Round((1 - (item.Key.Pr / maxProb)) * 1000000000).ToString();
                    cnfBuffer.AppendLine(string.Format("{0} {1} 0", cost, "-" + item.Value));
                    clauseCount++;                    
                }                                              
            }

            if (enableComment)
            {
                cnfBuffer.AppendLine("c ## Var-Link Mapping ##");
                foreach (var item in linkVarMap)
                {
                    //if (item.Key.Exists)
                        cnfBuffer.AppendLine(string.Format("c {0} {1}-->{2}", item.Value, item.Key.WordPivot.Value, item.Key.WordNonPivot.Value));
                }
            }

            //Symmetry Constraint
            int counter = 0;
            //Dictionary<WordPair, int> newInferedPair = new Dictionary<WordPair, int>();
            Dictionary<WordPair, string> polysemyPair = new Dictionary<WordPair, string>();            
            foreach (var pair in pairs)
            {
                if (enableComment)
                    cnfBuffer.AppendLine(string.Format("c Symmetry Constraint:{0}   {1}<->{2}", varCount + 1, pair.WordU, pair.WordK));

                //connect path and its links
                ++varCount;
                /*bool polysemyFound = false;
                foreach (SPath spath in pair.Paths)
                {
                    if ((float)graph.InDegree(spath.LinkCU.WordPivot) > 1 || (float)graph.OutDegree(spath.LinkCK.WordPivot) > 1)
                    {
                        polysemyFound = true;
                        break;
                    }
                }
                */
                
                if (true)//pair.HasMissingEdge)//polysemyFound) Symmetry constraint only if polysemyFound
                {
                    foreach (SPath spath in pair.Paths)
                    {
                        cnfBuffer.AppendLine(string.Format("{0} {1} -{2} 0", maxCost, linkVarMap[spath.LinkCU], varCount));
                        cnfBuffer.AppendLine(string.Format("{0} {1} -{2} 0", maxCost, linkVarMap[spath.LinkCK], varCount));
                        clauseCount = clauseCount + 2;
                    }
                }

                if (pair.Polysemy > 0)
                {
                    polysemyPair[pair] = varCount.ToString() + "," + Math.Round(pair.Polysemy * 1000000000).ToString();
                    //if (enableComment)
                    //    cnfBuffer.AppendLine(string.Format("c Polysemy-pivot Constraint:{0}   {1}<->{2}", varCount + 1, pair.WordU, pair.WordK));
                    //string polysemyCost = Math.Round(pair.Polysemy * 1000000000).ToString();
                    //cnfBuffer.AppendLine(string.Format("{0} -{1} 0", polysemyCost, varCount));
                    //clauseCount++;                                        
                }
                //Add alpha to ensure pair from existing edges are prioritized first
                //if (pair.HasMissingEdge)
                //    newInferedPair[pair] = varCount;

                if (populateVarPairMap)
                {
                    pairVarMap.Add(pair, varCount);
                    varPairMap.Add(varCount, pair);
                }
                
            }
            //Uniqueness constraint
            if (languageOption == 1){
                counter = 0;
                if (enableComment)
                    cnfBuffer.AppendLine(string.Format("c {0}", "==Start of one-to-one contraint 1 =="));
                Dictionary<int, bool> pairing = new Dictionary<int, bool>();
                foreach (var pair in pairs)
                {
                    int pairVar1 = pairVarMap[pair];
                    var exclusivePairs = pairs.Where(t => (t.WordU == pair.WordU || t.WordK == pair.WordK) && t != pair);
                    foreach (var pair2 in exclusivePairs)
                    {
                        int pairVar2 = pairVarMap[pair2];
                        int pairIdentifier = string.Format("{0}-{1}", Math.Max(pairVar1, pairVar2), Math.Min(pairVar1, pairVar2)).GetHashCode();
                        if (pairing.ContainsKey(pairIdentifier))
                            continue;
                        pairing.Add(pairIdentifier, true);
                        cnfBuffer.AppendLine(string.Format("{0} -{1} -{2} 0", maxCost, pairVarMap[pair], pairVarMap[pair2]));
                        clauseCount++;
                    }
                    if (enableComment)
                        cnfBuffer.AppendLine(string.Format("c "));
                }
            }

            /*if (enableComment)
                cnfBuffer.AppendLine(string.Format("c {0}", "==Start of Soft Constraint 2: New Induced Pair=="));

            foreach (KeyValuePair<WordPair, int> newPair in newInferedPair)                
            {
                varCount++;
                double cost = 100000000000 + Math.Round((1 - newPairVarMap[newPair.Key]) * 1000000000); 
                cnfBuffer.AppendLine(string.Format("{0} {1} -{2} 0", maxCost, varCount, newPair.Value));
                cnfBuffer.AppendLine(string.Format("{0} -{1} 0", cost, varCount));
                clauseCount = clauseCount + 2;                
            }*/

            if (enableComment)
                cnfBuffer.AppendLine(string.Format("c {0}", "==Start of Polysemy Pivot Constraint=="));

            foreach (KeyValuePair<WordPair, string> polyPair in polysemyPair)                
            {
                string[] polyPairInfo = polyPair.Value.Split(',');
                varCount++;
                cnfBuffer.AppendLine(string.Format("{0} {1} -{2} 0", maxCost, varCount, polyPairInfo[0]));
                cnfBuffer.AppendLine(string.Format("{0} -{1} 0", polyPairInfo[1], varCount));
                clauseCount = clauseCount + 2;                
            }

            if (enableComment)
                cnfBuffer.AppendLine(string.Format("c {0}", "==Start of Exclusive constraint =="));

            buffer.Clear();
            foreach (var item in pairs)
                if (!ooPairs.ContainsKey(item))
                    buffer.Append(pairVarMap[item] + " ");

            cnfBuffer.AppendLine(string.Format("{0} {1} 0", maxCost, buffer.ToString().Trim()));
            clauseCount++;

            foreach (var item in ooPairs.Keys)
            {
                cnfBuffer.AppendLine(string.Format("{0} {1} 0", maxCost, pairVarMap[item]));
                clauseCount++;
            }

            cnfBuffer.Insert(0, string.Format("p wcnf {0} {1} {2}{3}", varCount + varHelperCount, clauseCount, maxCost, Environment.NewLine));

            return cnfBuffer.ToString();
        }
        
        public string SolveGraph(BidirectionalGraph<Word, Edge<Word>> graph, FileInfo file)
        {
            long time = DateTime.Now.Ticks;

            List<SAT.WordPair> pairs = GeneratePossiblePairs(graph);
            //List<SAT.WordPair> pairs = GenerateAllPossiblePairs(graph);
            Dictionary<WordPair, int> ooPairs = new Dictionary<WordPair, int>();
            Dictionary<int, WordPair> iterOOPairMap = new Dictionary<int, WordPair>();
            Dictionary<int, long> iterSolvingTimeMap = new Dictionary<int, long>();
            Dictionary<int, string> iterObjectiveMap = new Dictionary<int, string>();

            bool solveNext = true;
            int solveCounter = 1;
            int rank = 0;

            while (solveNext)
            {
                bool pairInduced = false;
                int counter = 0;
                string objectiveFunctionValue = "-";
                //write encode
                string cnf = encode(pairs, ooPairs, file);
                //string cnf = encodeAll(pairs, ooPairs, file);
                StreamWriter cnfWriter = File.CreateText(file.FullName);
                cnfWriter.Write(cnf);
                cnfWriter.Flush();
                cnfWriter.Close();

                //solve
                KeyValuePair<Dictionary<int, bool>, bool> currentResult = solveCNF(file);
                foreach (var item in currentResult.Key)
                {
                    if (ooPairs.ContainsKey(varPairMap[item.Key]))
                        continue;
                    if (item.Value)
                        ooPairs.Add(varPairMap[item.Key], ++rank);
                    else
                        ooPairs.Add(varPairMap[item.Key], -(++rank));
                    iterOOPairMap.Add(solveCounter, varPairMap[item.Key]);
                    if (counter > 1)
                        throw new Exception("More than one pair induced!");
                    pairInduced = true;                    
                }
                solveNext = pairInduced;
                
                //read object function value
                string infoFileName = file.FullName+ ".inf";
                string[] lines = System.IO.File.ReadAllLines(infoFileName);
                if (lines.Length > 1)
                    iterObjectiveMap.Add(solveCounter, lines[1]);
                else
                    iterObjectiveMap.Add(solveCounter, "-1");
                solveCounter++;
            }
            
            List<string> ooPairString = new List<string>();
            //List<string> ooPairsNoRank = new List<string>();
            foreach (var item in ooPairs.Keys)
                if (ooPairs[item] > 0)
                {
                    allPairsNoRank.Add(string.Format("{0},{1}", item.WordU, item.WordK));
                    ooPairString.Add(string.Format("{0},{1},{2}", ooPairs[item], item.WordU, item.WordK));
                }
            string ooFileName = file.FullName.Replace(".wcnf", ".oo");
            System.IO.File.WriteAllLines(ooFileName, ooPairString.ToArray());
            //string ooFileNameNoRank = file.FullName.Replace(".wcnf", ".oo2");
            //System.IO.File.WriteAllLines(ooFileNameNoRank, ooPairsNoRank.ToArray());
            int pairCount = ooPairString.Count;

            return string.Format("Done in {0}ms", (DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond);
        }

        private KeyValuePair<Dictionary<int, bool>, bool> solveCNF(FileInfo file)
        {
            //For omega3, threshold only works on existing pair, currently the new pair in D_N will all have constant cost, 
            //so, no need to filter, either accept them all or not at all
            //Thus, we can use one threshold for both omega2 and omega3 (with option to accept new pair in D_N or not)                                         
            //if (autoThreshold > 0)
            //    omega2Threshold = autoThreshold;
            double threshold2 = 1000000000 * autoThreshold;// omega2Threshold;
            //double threshold3 = 100000000000 + (1000000000 * omega3Threshold);

            Dictionary<int, bool> inducedPairs = new Dictionary<int, bool>();
            long time = DateTime.Now.Ticks; 

            //solve
            string solverPath = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "SATSolverConsole.jar");
            string strCmdText = string.Format("/C java -jar {0} {1}", solverPath, file.FullName);
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            System.Diagnostics.ProcessStartInfo startInfo = new System.Diagnostics.ProcessStartInfo();
            startInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = strCmdText;
            process.StartInfo = startInfo;
            time = DateTime.Now.Ticks;
            process.Start();
            process.WaitForExit();
            long timeToSolve = (DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond;
            bool decision = false;

            System.IO.FileInfo solFile = new System.IO.FileInfo(file.FullName + ".sol");
            System.IO.FileInfo costFile = new System.IO.FileInfo(file.FullName + ".inf");
            string solution = System.IO.File.ReadAllText(solFile.FullName);
            if (string.IsNullOrEmpty(solution.Trim()))
                return new KeyValuePair<Dictionary<int, bool>, bool>(new Dictionary<int, bool>(), decision); //unsatisfiable
            else
            {
                string[] lines = System.IO.File.ReadAllLines(file.FullName + ".inf");
                if (lines.Length > 1)
                {
                    totalCost = long.Parse(lines[1]); 

                    currentCost = totalCost - totalCostHistory;
                    totalCostHistory = totalCost;
                    /*if (omega3Threshold > 0)
                        if (currentCost <= threshold3)
                            Debug.WriteLine("ACCEPTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                        else
                            Debug.WriteLine("REJECTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                    else if (omega2Threshold > 0)
                        if (currentCost <= threshold2)
                            Debug.WriteLine("ACCEPTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                        else
                            Debug.WriteLine("REJECTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                    */
                }                                    
            }

            foreach (var item in solution.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int varValue = int.Parse(item);
                bool b = varValue > 0 ? true : false;
                double currentThreshold = threshold2;
                /*bool useThreshold = false;
                if (omega3Threshold > 0 && omega2Threshold == 0)
                {
                    currentThreshold = threshold3;
                    useThreshold = true;// totalCost < omega3Threshold ? false : true;
                }
                else if (omega2Threshold > 0 && omega3Threshold == 0)
                {
                    currentThreshold = threshold2;
                    useThreshold = true;// totalCost < omega2Threshold ? false : true;
                }
                if (b && varPairMap.ContainsKey(varValue))
                {
                    Debug.WriteLine("CurrentCost: " + currentCost);
                    if (!useThreshold && currentCost == 100000000000)
                    {
                        inducedPairs[varValue] = true;
                        Debug.WriteLine("#ACCEPTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                    }
                    else// if (currentCost <= currentThreshold)
                    {
                        inducedPairs[varValue] = false;
                        //Debug.WriteLine("ACCEPTED PAIR: " + currentCost);
                        Debug.WriteLine("REJECTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                    }
                    //else
                    //    Debug.WriteLine("REJECTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                    //if (totalCost > 100000000000)
                    //    Debug.WriteLine(totalCost);
                }*/
                if (b && varPairMap.ContainsKey(varValue))
                {
                    //if (languageOption == 2) //Change threshold
                    //    currentThreshold = 100000000000 + (1000000000 * omega2Threshold);
                    if (currentCost > highestCost)
                        highestCost = currentCost;
                    if (autoThreshold < 0 || (autoThreshold >= 0 && currentCost <= currentThreshold))
                        inducedPairs[varValue] = true;
                    else
                        inducedPairs[varValue] = false;

                    /*if (autoThreshold == 0 || (autoThreshold > 0 && currentCost <= currentThreshold))
                    {
                        if (acceptOmega3NewPair) //Expecting Omega3 results with new pairs
                            inducedPairs[varValue] = true;
                        else //Expecting Omega2 results
                        {
                            double newPairsThreshold = 100000000000 + (1000000000 * autoThreshold);
                            if (currentCost <= newPairsThreshold)
                                inducedPairs[varValue] = true;
                            else
                                inducedPairs[varValue] = false;
                        }
                    }*/
                }
            }

            return new KeyValuePair<Dictionary<int, bool>, bool>(inducedPairs, decision);
        }
    }
}
