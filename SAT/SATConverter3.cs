using System;
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
        private Dictionary<int,WordPair > varPairMap;
        private Dictionary<WordPair, bool> generatedPairMap;

        BidirectionalGraph<Word, Edge<Word>> semiCompleteGraph;
        
        private bool enableComment = true;
        private string maxCost = "100000000000000000000000000000000000000000";
        private string maxCost2 = "99999999999999999999999999999999999999999";
        private long totalCost;
        private long totalCostHistory;
        private long totalCostHistory2;
        private long currentCost;    

        public static int languageOption = 1;
        public static double omega2Threshold = 0;
        //public static double omega3Threshold = 0;
        public static bool acceptOmega3NewPair = true;

        private List<WordPair> GeneratePossiblePairs(BidirectionalGraph<Word, Edge<Word>> g)
        {
            //Direction: Uyghur--> Chinese <--Kazahk
            LinkCache = new Dictionary<int, SLink>();
            LinkWeightCache = new Dictionary<SLink, float>();
            BidirectionalGraph<Word, Edge<Word>> graph = new BidirectionalGraph<Word, Edge<Word>>(false);
            semiCompleteGraph = new BidirectionalGraph<Word, Edge<Word>>(false);
            #region Prepare graph
            foreach (var item in g.Vertices)
            {
                graph.AddVertex(item);
                semiCompleteGraph.AddVertex(item);
            }

            foreach (var item in g.Edges) 
            {
                if (item.Source.Language == Console.Language.Chinese && item.Target.Language == Console.Language.Uyghur)
                {
                    graph.AddEdge(new Edge<Word>(item.Target, item.Source));
                    semiCompleteGraph.AddEdge(new Edge<Word>(item.Target, item.Source));
                }
                else
                {
                    graph.AddEdge(new Edge<Word>(item.Source, item.Target));
                    semiCompleteGraph.AddEdge(new Edge<Word>(item.Source, item.Target));
                }
            }
            #endregion

            List<WordPair> ooPairs = new List<WordPair>();
            Dictionary<WordPair, bool> ooPairsDict = new Dictionary<WordPair, bool>();

            var uWords = graph.Vertices.Where(t => t.Language == Language.Uyghur);
            var kWords = graph.Vertices.Where(t => t.Language == Language.Kazak);
            var cWords = graph.Vertices.Where(t => t.Language == Language.Chinese);

            int uWordCount = uWords.Count();
            int kWordCount = kWords.Count();
            int cWordCount = cWords.Count();
            int u =0,k=0;

            //foreach (var uWord in uWords)
            //{
            //    foreach (var kWord in kWords)
            //    {
            //        OOPair ooPair = new OOPair(uWord, kWord);
            //        ooPair = createPair(uWord, kWord, graph, uWordCount,kWordCount);
            //        ooPairs.Add(ooPair);
            //     }
            //    u++;
            //}


            foreach (var uWord in uWords)
            {
                foreach (var edge1 in graph.OutEdges(uWord))
                {
                    foreach (var  edge2 in graph.OutEdges(edge1.Target))
                    {
                        Word kWord = edge2.Target;
                        WordPair ooPair = new WordPair(uWord, kWord);
                        ooPair = createNewEdges(uWord, kWord, graph, uWordCount, kWordCount);
                        //ooPair = createPairV2(uWord, kWord, graph, uWordCount, kWordCount);
                        if (ooPairsDict.ContainsKey(ooPair))
                            continue;
                        else
                        {
                            //ooPair.Prob = 1f;
                            ooPairsDict.Add(ooPair, true);
                            ooPairs.Add(ooPair);
                        } 
                    }
                }
 
            }

            /*foreach (var pair1 in ooPairs)
            {
                Word uWord = pair1.WordU;
                Word kWord = pair1.WordK;
                Debug.WriteLine(uWord.Value + " --> " + kWord.Value + ", totalProb: " + pair1.Prob);
            }*/

            //Debug.WriteLine(languageOption);
            //Add new pair candidate (blue dotted line) from the semiCompleteGraph
            if (languageOption == 3)
            {
                foreach (var uWord in uWords)
                {
                    Cache1.Clear();
                    Word cWord;
                    Word kWord;

                    foreach (var edge1 in semiCompleteGraph.OutEdges(uWord))
                    {
                        cWord = edge1.Target;
                        foreach (var edge2 in semiCompleteGraph.OutEdges(cWord))
                        {
                            kWord = edge2.Target;

                            //Cache1.Add(cWord.ID, true);
                            List<SPath> paths = new List<SPath>();
                            WordPair ooPair = new WordPair(null, null);
                            ooPair = new WordPair(uWord, kWord);
                            string newUC = "-";
                            string newCK = "-";
                            //float pUC = 1;
                            //float pCK = 1;
                            //float pUK = 1;

                            /*if (ooPairsDict.ContainsKey(ooPair))
                            {
                                //Debug.WriteLine(uWord.Value + " --> " + kWord.Value + " is EXIST, totalProb: ");
                                //Prob = 1f; continue; as long as there is existing path, give Prob 1
                            }
                            else */
                            if (!ooPairsDict.ContainsKey(ooPair))
                            {
                                //Add path for new generated pairs
                                SLink linkCU = new SLink(cWord, uWord);
                                linkCU.Exists = graph.ContainsEdge(uWord, cWord);

                                SLink linkCK = new SLink(cWord, kWord);
                                linkCK.Exists = graph.ContainsEdge(cWord, kWord);

                                if (linkCU.Exists || linkCK.Exists) // At least one edge exist in a path -> Hard Constraint for Traveling the Paths
                                {
                                    SPath path = new SPath(linkCU, linkCK);
                                    paths.Add(path);
                                
                                    //WordPair ooPair2 = new WordPair(null, null);
                                    ooPair = new WordPair(uWord, kWord);
                                    ooPair.Paths = paths;
                                    ooPair.HasMissingEdge = true;
                                    ooPairsDict.Add(ooPair, true);
                                    ooPairs.Add(ooPair);
                                }
                            }
                        }
                    }
                }
            }              
            float maxWeight = 0;
            return ooPairs;
        }
        
        private WordPair createNewEdges(Word uWord, Word kWord, BidirectionalGraph<Word, Edge<Word>> graph, int uCount, int kCount)
        {
            Cache1.Clear();
            List<SPath> paths = new List<SPath>();

            foreach (var item in graph.OutEdges(uWord))
            {
                SLink linkCU = new SLink(item.Target, uWord);
                //SLink linkCU = createLink(item.Target, uWord);
                //linkCU.Exists = true;
                linkCU.Exists = graph.ContainsEdge(uWord, item.Target);

                SLink linkCK = new SLink(item.Target, kWord);
                //SLink linkCK = createLink(item.Target, kWord);
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
                //SLink linkCK = createLink(item.Source, kWord);
                //linkCK.Exists = true;
                linkCK.Exists = graph.ContainsEdge(item.Source, kWord);

                SLink linkCU = new SLink(item.Source, uWord);
                //SLink linkCU = createLink(item.Source, uWord);
                //linkCU.Exists = false; // graph.ContainsEdge(uWord, item.Source);
                linkCU.Exists = graph.ContainsEdge(uWord, item.Source);

                SPath path = new SPath(linkCU, linkCK);
                paths.Add(path);
            }

            //calculate probability

            float couverage = Math.Min(uCount, kCount) / (float)Math.Max(uCount, kCount);
            float pUK = 0;
            float pKU = 0;
            float probUK = 0;
            float probKU = 0;
            foreach (var item in paths)
            {
                //if (!item.LinkCU.Exists || !item.LinkCK.Exists) //containning non-existance link
                if (!item.LinkCU.Exists)
                {
                    if (languageOption == 3)
                        semiCompleteGraph.AddEdge(new Edge<Word>(item.LinkCU.WordNonPivot, item.LinkCU.WordPivot));
                    continue;
                }
                if (!item.LinkCK.Exists)
                {
                    if (languageOption == 3) 
                        semiCompleteGraph.AddEdge(new Edge<Word>(item.LinkCK.WordPivot, item.LinkCK.WordNonPivot));
                    continue;
                }
                //string currPair = item.LinkCK.WordPivot + "_" + item.LinkCK.WordNonPivot;
                /*ifloat msStat = 0;
                float wnStatCK = 0.0001f;
                f (DBHelper.WordnetStat.TryGetValue(currPair, out msStat))
                {
                    //wnStatCK = msStat * 6000000000000 / 117; //maks statistic is 117
                    wnStatCK = msStat > 0 ? msStat : 0.0001f;
                }*/
                float PrUC = 1.0f / (float)graph.OutDegree(item.LinkCU.WordNonPivot);
                float PrCK = 1.0f / (float)graph.OutDegree(item.LinkCK.WordPivot);
                float PrCU = 1.0f / (float)graph.InDegree(item.LinkCU.WordPivot);
                float PrKC = 1.0f / (float)graph.InDegree(item.LinkCK.WordNonPivot);

                //float pUC = 1 / (float)graph.InDegree(item.LinkCU.WordPivot);//.Count();
                //float pCK = 1 / (float)graph.OutDegree(item.LinkCK.WordPivot);//.Count();
                //pUK = pUC * pCK;
                pUK += PrUC * PrCK;// *wnStatCK;
                pKU += PrKC * PrCU;
                //probUK += (pUK);
                //probKU += (pKU);
                //couverage++;
            }
            probUK = pUK * pKU;//probUK = couverage * pUK * pKU;
            /*if (probUK >= 1)
            {
                Debug.WriteLine(probUK);
                probUK = 0.999999999999f;
            }
             */ 
            //probUK = (probUK / couverage); //Probability of best path
            //probUK = probUK / 100;
            //probUK = probUK >= 1f ? 0.9999999999f : probUK;

            WordPair pair = new WordPair(uWord, kWord);
            pair.Paths = paths;
            pair.Prob = (float)probUK;

            //set link weights
            foreach (var item in pair.Paths)
            {
                //CU
                if (item.LinkCU.Exists)
                {
                    item.LinkCU.Pr = 1f;
                    /*float PrUC = 1.0f / (float)graph.OutDegree(item.LinkCU.WordNonPivot);
                    float PrCU = 1.0f / (float)graph.InDegree(item.LinkCU.WordPivot);
                    item.LinkCU.Pr = PrUC * PrCU;
                    */
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
                if (item.LinkCK.Exists)//false)//
                {
                    item.LinkCK.Pr = 1f;
                    /*float PrCK = 1.0f / (float)graph.OutDegree(item.LinkCK.WordPivot);
                    float PrKC = 1.0f / (float)graph.InDegree(item.LinkCK.WordNonPivot);
                    string currPair = item.LinkCK.WordPivot + "_" + item.LinkCK.WordNonPivot;
                    float msStat = 0;
                    float wnStatCK = 0.0001f;
                    if (DBHelper.WordnetStat.TryGetValue(currPair, out msStat))
                    {
                        wnStatCK = msStat > 0 ? msStat : 0.0001f;
                        //wnStatCK = 1000 * msStat / maxStat; //maks statistic is 117
                    }
                    
                    item.LinkCK.Pr = PrCK * PrKC;// *wnStatCK;
                    */if (!LinkWeightCache.ContainsKey(item.LinkCK))
                        LinkWeightCache.Add(item.LinkCK, item.LinkCK.Pr);
                }
                else
                {
                    /*if (pair.HasMissingCUEdge)
                        pair.HasMissingUKEdge = true;
                    pair.HasMissingCKEdge = true;

                     */ 
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
            //int threshold = 1000000000;

            if (linkVarMap == null)
            {
                linkVarMap = new Dictionary<SLink, string>();
                List<SLink> links = LinkWeightCache.Keys.OrderBy(t => t.WordNonPivot.Language).OrderByDescending(t => t.Exists).ToList();
                for (int i = 0; i < links.Count; i++)
                {
                    linkVarMap.Add(links[i], (i + 1).ToString());
                }
            }

            /*/find max prob
            foreach (var item in linkVarMap.Keys)
                maxProb = item.Pr > maxProb ? item.Pr : maxProb;
            */
            varCount = linkVarMap.Count;
            foreach (var item in linkVarMap)
            {
                string cost = string.Empty;
                double realCost = 0;
                if (item.Key.Exists)
                {
                    cost = maxCost;
                    /*cnfBuffer.AppendLine(string.Format("{0} {1} 0",
                    cost,
                    item.Key.Exists ? item.Value : "-" + item.Value));*/
                    cnfBuffer.AppendLine(string.Format("{0} {1} 0", cost, item.Value));
                    clauseCount++;
                    //varCount++; //only when best path constraint is used
                    //  Add Edge Existance Hard Constraint  //
                    if (false)//languageOption == 3)
                    {
                        long varEdge = 1000000 + Int32.Parse(item.Value);
                        cnfBuffer.AppendLine(string.Format("{0} {1} 0", maxCost, varEdge.ToString()));
                        clauseCount++;
                    }                    
                }
                else
                {
                    //float c = item.Key.Pr / maxProb;
                    //c = (1 - c) ;
                    //c = c * 1000000000;
                    //cost = c.ToString("0");
                    //cost = Math.Round(((1 - (item.Key.Pr / maxProb)) * 200000000)).ToString();                    
                    
                    //realCost = Math.Round((1 - item.Key.Pr) * 1000000000);
                    //cost = Math.Round((1 - item.Key.Pr) * 1000000000).ToString();

                    realCost = Math.Round((1 - item.Key.Pr) * 1000000000); //realCost = Math.Round((1 - (item.Key.Pr / maxProb)) * 1000000000);
                    cost = Math.Round((1 - item.Key.Pr ) * 1000000000).ToString(); //cost = Math.Round((1 - (item.Key.Pr / maxProb)) * 1000000000).ToString();
                    cnfBuffer.AppendLine(string.Format("{0} {1} 0", cost, "-" + item.Value));
                    clauseCount++;
                    //if (realCost < threshold)
                    //{
                    // It is OK that in each iteration, previous cost is recalculated, just implementation issue, the logic still the same.
                    /*if (System.IO.File.Exists(file.FullName + ".sol"))
                    {
                        bool newEdgeFound = false;
                        string solution = System.IO.File.ReadAllText(file.FullName + ".sol");
                        foreach (var item2 in solution.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
                        {
                            int varValue = int.Parse(item2);
                            bool b = varValue > 0 ? true : false;
                            if (b && varValue == int.Parse(item.Value))
                            {
                                newEdgeFound = true;
                                break;
                            }
                        }
                        if (newEdgeFound)
                        {
                            cnfBuffer.AppendLine(string.Format("c {0} {1} 0", cost, "-" + item.Value));
                            cnfBuffer.AppendLine(string.Format("{0} {1} 0", maxCost, item.Value));
                            clauseCount++;
                        }
                        else
                        {
                            cnfBuffer.AppendLine(string.Format("{0} {1} 0", cost, "-" + item.Value));
                            clauseCount++;
                        }
                    }
                    else
                    {
                        cnfBuffer.AppendLine(string.Format("{0} {1} 0", cost, "-" + item.Value));
                        clauseCount++;
                    }*/
                                          
                        //varCount++; //only when best path constraint is used                        
                    /*}
                    else
                    {
                        cnfBuffer.AppendLine(string.Format("{0} {1} 0", maxCost, "-" + item.Value));
                        clauseCount++;
                        //varCount++; //only when best path constraint is used                            
                    }*/
                    //  Add Edge Existance Hard Constraint  //
                    if (false)//languageOption == 3)
                    {
                        long varEdge = 1000000 + Int32.Parse(item.Value);
                        cnfBuffer.AppendLine(string.Format("{0} {1} 0", maxCost, "-" + varEdge.ToString()));
                        clauseCount++;
                    }
                }
                /*cnfBuffer.AppendLine(string.Format("{0} {1} 0",
                    cost,
                    item.Key.Exists ? item.Value : "-" + item.Value));
                clauseCount++;
                varCount++; //only when best path constraint is used       
                 */ 
                /*if (item.Key.Exists)
                {
                    cnfBuffer.AppendLine(string.Format("{0} {1} 0", cost, item.Value));
                    clauseCount++;
                } */                               
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

            //Start of New Pair
            int counter = 0;
            List<int> newInferedPair = new List<int>();
            foreach (var pair in pairs)
            {
                //if (counter % 10 == 0)
                //    Debug.WriteLine(string.Format("Pair resolving {0}/{1}", pairs.Count, ++counter));

                //int pairLen = pair.Paths.Count;

                //if (pair.HasMissingUKEdge)
                //    continue;
                //if (pair.Paths.Count == 1)
                //    continue;

                if (enableComment)
                {
                    cnfBuffer.AppendLine(string.Format("c Start of new pair:{0}   {1}<->{2}", varCount + 1, pair.WordU, pair.WordK));
                    //Debug.WriteLine(pair.WordU.Value + " " + pair.WordK.Value);
                }
                
                //connect path and its links
                ++varCount;
                //string bestPath = "";
                
                foreach (SPath spath in pair.Paths)
                {
                    //++varCount;
                    cnfBuffer.AppendLine(string.Format("{0} {1} -{2} 0", maxCost, linkVarMap[spath.LinkCU], varCount));
                    cnfBuffer.AppendLine(string.Format("{0} {1} -{2} 0", maxCost, linkVarMap[spath.LinkCK], varCount));                    
                    //cnfBuffer.AppendLine(string.Format("{0} -{1} -{2} {3} 0", maxCost, linkVarMap[spath.LinkCU], linkVarMap[spath.LinkCK], varCount));
                    clauseCount = clauseCount + 2;
                    if (false)//languageOption == 3)
                    {
                        long varEdgeCU = 1000000 + Int32.Parse(linkVarMap[spath.LinkCU]);
                        long varEdgeCK = 1000000 + Int32.Parse(linkVarMap[spath.LinkCK]);
                        cnfBuffer.AppendLine(string.Format("{0} {1} {2} -{3} 0", maxCost, varEdgeCU.ToString(), varEdgeCK.ToString(), varCount));
                        clauseCount++;
                    }
                    //bestPath += varCount + " ";
                    //++varCount;
                }

                //Add alpha to ensure pair from existing edges are prioritized first
                if (pair.HasMissingEdge)
                {
                    newInferedPair.Add(varCount);
                    /*cnfBuffer.AppendLine(string.Format("{0} {1} -{2} 0", maxCost, varCount + 1000, varCount));
                    cnfBuffer.AppendLine(string.Format("{0} -{1} 0", 100000000000, varCount + 1000));
                    clauseCount = clauseCount + 2;
                    varHelperCount++;*/
                }

                //++varCount;
                //cnfBuffer.AppendLine(string.Format("{0} {1}-{2} 0", maxCost, bestPath, varCount));
                //clauseCount++;
                //varCount++;

                /*/add similarity constraint

                if (false)
                {
                    string kWordLatin = DBHelper.Syn.getLtFromKz(pair.WordU.Value);
                    string uWordLatin = DBHelper.Syn.getUKYFromUy(pair.WordK.Value);
                    string kWordOnly = kWordLatin.Substring(3);
                    string uWordOnly = uWordLatin.Substring(3);
                    float similarity = DBHelper.iLD(uWordOnly, kWordOnly);
                    cnfBuffer.AppendLine(string.Format("c Similarity constraint of:{0}   {1}<->{2}", varCount, pair.WordU, pair.WordK));
                    string dissimilarityCost = string.Empty;
                    dissimilarityCost = Math.Round(((1 - similarity) * 1000000000)).ToString();
                    cnfBuffer.AppendLine(string.Format("{0} -{1} 0", dissimilarityCost, varCount));
                    clauseCount++;
                }

                //add part-of-speech constraint
                if (false)
                {
                    string kWordLatin = DBHelper.Syn.getLtFromKz(pair.WordU.Value);
                    string uWordLatin = DBHelper.Syn.getUKYFromUy(pair.WordK.Value);
                    string kPOSOnly = kWordLatin.Substring(0, 2);
                    string uPOSOnly = uWordLatin.Substring(0, 2);
                    string kWordOnly = kWordLatin.Substring(3);
                    string uWordOnly = uWordLatin.Substring(3);
                    float matchPOS = 1;
                    if (kPOSOnly != uPOSOnly)
                        matchPOS = matchPOS / 2;
                    cnfBuffer.AppendLine(string.Format("c Part-of-Speech constraint of:{0}   {1}<->{2}", varCount, pair.WordU, pair.WordK));
                    string unmatchedPOSCost = string.Empty;
                    unmatchedPOSCost = Math.Round(((1 - matchPOS) * 1000000000)).ToString();
                    cnfBuffer.AppendLine(string.Format("{0} -{1} 0", unmatchedPOSCost, varCount));
                    clauseCount++;
                }
                */
                if (populateVarPairMap)
                {
                    pairVarMap.Add(pair, varCount);
                    varPairMap.Add(varCount, pair);
                }
                

                //mapWriter.WriteLine(string.Format("{0}\t{1}\t{2}", varCount, pair.WordU.Value, pair.WordK.Value));
            }
            //Uniqueness constraint
            if (languageOption == 1){
                counter = 0;
                if (enableComment)
                    cnfBuffer.AppendLine(string.Format("c {0}", "==Start of one-to-one contraint 1 =="));
                Dictionary<int, bool> pairing = new Dictionary<int, bool>();
                foreach (var pair in pairs)
                {
                    //if (counter % 10 == 0)
                    //    Debug.WriteLine(string.Format("one-to-one constraint {0}/{1}", pairs.Count, ++counter));
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

            if (enableComment)
                cnfBuffer.AppendLine(string.Format("c {0}", "==Start of Soft Constraint 2=="));

            foreach (int item in newInferedPair)
            {
                varCount++;
                cnfBuffer.AppendLine(string.Format("{0} {1} -{2} 0", maxCost, varCount, item));
                cnfBuffer.AppendLine(string.Format("{0} -{1} 0", 100000000000, varCount));
                clauseCount = clauseCount + 2;                
            }


            if (enableComment)
                cnfBuffer.AppendLine(string.Format("c {0}", "==Start of Exclusive constraint 2 =="));

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
                //string fullName = "'" + file.FullName + "'";
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
                    //Debug.WriteLine("varPairMap[item]: " + varPairMap[item.Key] + " decision: " + item.Value);

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
                //iterSolvingTimeMap.Add(solveCounter, currentResult.Value);

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
            foreach (var item in ooPairs.Keys)
                if (ooPairs[item] > 0)
                    ooPairString.Add(string.Format("{0},{1},{2}", ooPairs[item], item.WordU, item.WordK));
            string ooFileName = file.FullName.Replace(".wcnf", ".oo");
            System.IO.File.WriteAllLines(ooFileName, ooPairString.ToArray());
            int pairCount = ooPairString.Count;


            /*List<string> mapPairString = new List<string>();
            foreach (var item in pairs)
                mapPairString.Add(string.Format("{0}\t{1}\t{2}", pairVarMap[item], item.WordU, item.WordK));
            string mapFileName = file.FullName.Replace(".wcnf", ".map");
            System.IO.File.WriteAllLines(mapFileName, mapPairString.ToArray());
            int possiblePairCount = mapPairString.Count;
            */

           

            /*/write statistic
            int uWordCount = graph.Vertices.Where(t => t.Language == Console.Language.Uyghur).Count();
            int kWordCount = graph.Vertices.Where(t => t.Language == Console.Language.Kazak).Count();
            int zWordCount = graph.Vertices.Where(t => t.Language == Console.Language.Chinese).Count();
            int linkCount = graph.EdgeCount;

            string solvingInfoFileName = file.FullName.Replace(".wcnf", ".inf2");
            List<string> infoTale = new List<string>(2);
            infoTale.Add("# overall statistics");
            infoTale.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", "Z", "U", "K", "Links",  "1-1 Pairs",  "Solving TIme"));
            infoTale.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}", zWordCount, uWordCount, kWordCount, linkCount, pairCount,  iterSolvingTimeMap.Values.Sum()));

            infoTale.Add("# iteration statistics");
            infoTale.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", "IterID", "U", "K", "SolvingTime", "Objective"));
            for (int i = 1; i < solveCounter; i++)
            {
                infoTale.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", i, 
                    iterObjectiveMap.Count == i ? "-" : iterOOPairMap[i].WordU.Value, 
                    iterObjectiveMap.Count == i ? "-" : iterOOPairMap[i].WordK.Value, 
                    iterSolvingTimeMap[i], 
                    iterObjectiveMap[i]));
            }
            
            System.IO.File.WriteAllLines(solvingInfoFileName, infoTale.ToArray());
            */
            return string.Format("Done in {0}ms", (DateTime.Now.Ticks - time) / TimeSpan.TicksPerMillisecond);
        }

        private KeyValuePair<Dictionary<int, bool>, bool> solveCNF(FileInfo file)
        {
            //For omega3, threshold only works on existing pair, currently the new pair in D_N will all have constant cost, 
            //so, no need to filter, either accept them all or not at all
            //Thus, we can use one threshold for both omega2 and omega3 (with option to accept new pair in D_N or not)                                         
            double threshold2 = 1000000000 * omega2Threshold;
            //double threshold3 = 1000000000 * omega3Threshold;
            
            //List<int> inducedPairs = new List<int>();
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
                //totalCost = long.Parse(System.IO.File.ReadAllText(costFile.FullName).Substring(15));
                string[] lines = System.IO.File.ReadAllLines(file.FullName + ".inf");
                if (lines.Length > 1)
                {
                    totalCost = long.Parse(lines[1]);
                    currentCost = totalCost - totalCostHistory;
                    /*Debug.WriteLine(currentCost + " is the currentCost");
                    Debug.WriteLine(totalCost + "is the totalCost");
                    if (currentCost == 100000000000)
                    {
                        //currentCost = totalCost - totalCostHistory2;
                        totalCostHistory2 = totalCostHistory;                        
                    }*/
                    //Debug.WriteLine("totalCostHistory2:" + totalCostHistory2);
                    //Debug.WriteLine("totalCostHistory:" + totalCostHistory);
                    //Debug.WriteLine("currentCost:" + currentCost);
                    totalCostHistory = totalCost;
                    /*if (omega3Threshold > 0)
                        if (currentCost <= threshold3)
                            Debug.WriteLine("ACCEPTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                        else
                            Debug.WriteLine("REJECTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                    else */if (omega2Threshold > 0)
                        if (currentCost <= threshold2)
                            Debug.WriteLine("ACCEPTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                        else
                            Debug.WriteLine("REJECTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                    //else
                    //    Debug.WriteLine("#ACCEPTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                }                    
                //if(totalCost > threshold) // Ignored lowest ranked based on threshold
                //    return new KeyValuePair<List<int>, long>(new List<int>(), timeToSolve); //unsatisfiable
                //Debug.WriteLine(totalCost);
            }

            //Dictionary<int, bool> variables = new Dictionary<int, bool>();

            foreach (var item in solution.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries))
            {
                int varValue = int.Parse(item);
                bool b = varValue > 0 ? true : false;
                double currentThreshold = threshold2;
                //bool useThreshold = false;
                /*if (omega3Threshold > 0 && omega2Threshold == 0)
                {
                    currentThreshold = threshold3;
                    useThreshold = true;// totalCost < omega3Threshold ? false : true;
                }
                else if (omega2Threshold > 0 && omega3Threshold == 0)
                {
                    currentThreshold = threshold2;
                    useThreshold = true;// totalCost < omega2Threshold ? false : true;
                }*/
                if (b && varPairMap.ContainsKey(varValue))
                {
                    //Debug.WriteLine("CurrentCost: " + currentCost);
                    if (omega2Threshold == 0 || (omega2Threshold > 0 && currentCost <= currentThreshold))
                    {
                        if (acceptOmega3NewPair)
                            inducedPairs[varValue] = true;
                        else
                        {
                            if (currentCost != 100000000000)
                                inducedPairs[varValue] = true;
                            else
                                inducedPairs[varValue] = false;
                        }
                    }                        
                    /*else if (currentCost <= currentThreshold)
                    {
                        inducedPairs[varValue] = false;
                        //Debug.WriteLine("ACCEPTED PAIR: " + currentCost);
                        Debug.WriteLine("REJECTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                    }                        
                    //else
                    //    Debug.WriteLine("REJECTED PAIR: " + currentCost + "     totalCost: " + totalCost);
                    //if (totalCost > 100000000000)
                    //    Debug.WriteLine(totalCost);
                    */
                }                    
            }

            return new KeyValuePair<Dictionary<int, bool>, bool>(inducedPairs, decision);
        }
    }
}
