using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Text.RegularExpressions;
using System.Linq;
using System.Linq.Expressions;
using HDictInduction.Console;
using HDictInduction.Console.zuk_dbSQLDataSetTableAdapters;
using System.IO;
using System.Threading;
using System.ComponentModel;
using System.Windows;
using QuickGraph;
//using HDictInduction.Console.Resources;
//using HDictInduction.Console.Heuristics;
using System.Diagnostics;
using System.Threading.Tasks;
using QuickGraph.Algorithms.ConnectedComponents;

namespace HDictInduction.Console
{
    class DBHelper
    {
        public static BidirectionalGraph<Word, Edge<Word>> WordRelaionGraph = null;

        public enum Language { Uyghur, Kazak, Chinese, Kyrgiz };
        
        private static DictBase _DictBase;

        private static zuk_dbSQLDataSet.zuk_fixedDataTable _ZukTable;

        public static DictBase MyDictBase {
            get { return _DictBase; }
        }

        private static Dictionary<string, float> wnStatDict = new Dictionary<string, float>();

        public static Dictionary<string, float> WordnetStat
        {
            get { return wnStatDict; }
        }

        private static Syntax _Syn = null;

        public static Syntax Syn
        {
            get { if (_Syn == null) _Syn = new Syntax();
            return _Syn;
            }
        }
      
        public System.Data.DataTable XmlToDataTable(string fileName)
        {
            //read into dataset
            DataSet dataSet = new DataSet();
            dataSet.ReadXml(fileName);

            //return single table inside of dataset
            return dataSet.Tables[0];
        }

        public void WriteTextToTempFile(string content)
        {
            string fileName = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "temp.txt");
            System.IO.File.WriteAllText(fileName, content);
        }

        public void WriteLinesToTempFile(string[] content)
        {
            string fileName = System.IO.Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "temp.txt");
            System.IO.File.WriteAllLines(fileName, content);
        }

        public void WriteDBtoXML()
        {
            string fileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp.xml");
            HDictInduction.Console.zuk_dbSQLDataSetTableAdapters.zuk_fixedTableAdapter zukAdapter = new HDictInduction.Console.zuk_dbSQLDataSetTableAdapters.zuk_fixedTableAdapter();
            zuk_dbSQLDataSet.zuk_fixedDataTable zukTable = new zuk_dbSQLDataSet.zuk_fixedDataTable();
            zukAdapter.Fill(zukTable);

            zukTable.WriteXml(fileName);
        }


        public static void GenerateDatabase(List<string> filterUWords, List<string> filterKWords)
        {
            _DictBase = GenerateDatabaseFull(filterUWords, filterKWords);
        }

        public static zuk_dbSQLDataSet.zuk_fixedDataTable GraphToDatabase(QuickGraph.BidirectionalGraph<Word, Edge<Word>> graph)
        {
            zuk_dbSQLDataSet.zuk_fixedDataTable table = new zuk_dbSQLDataSet.zuk_fixedDataTable();
            foreach (var ver in graph.Vertices.Where(t => t.Language == Console.Language.Chinese))
            {
                var uMeanigns = graph.OutEdges(ver).Select(t => t.Target).Where(t => t.Language == Console.Language.Uyghur).ToArray();
                var kMeanigns = graph.OutEdges(ver).Select(t => t.Target).Where(t => t.Language == Console.Language.Kazak).ToArray();
                zuk_dbSQLDataSet.zuk_fixedRow row = table.Newzuk_fixedRow();
                row.Zh = ver.Value;
                row.Ug = "";
                row.Kz = "";
                foreach (var u in uMeanigns)
                    row.Ug += u + ",";
                foreach (var k in kMeanigns)
                    row.Kz += k + ",";
                row.Ug = row.Ug.TrimEnd(',');
                row.Kz = row.Kz.TrimEnd(',');
                //row.Pn = "-";

                table.Addzuk_fixedRow(row);
            }
            return table;
        }

        public static QuickGraph.UndirectedGraph<string, Edge<string>> DatabaseToGraph(string dbFileName)
        {

            Dictionary<string, string> irpan = new Dictionary<string, string>();
            if (false)
            {
                string[] lines = System.IO.File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "new_CnUy2007.txt"));
                foreach (var item in lines)
                {
                    string c = item.Split('\t')[0];
                    string u = item.Split('\t')[1];

                    if (irpan.ContainsKey(c))
                        continue;
                    irpan.Add(c, u);
                }
            }

            char[] spliter = new char[] { ',' };
            DictBase dictBase = new DictBase();
            Dictionary<string, int> cWordDict = new Dictionary<string, int>();
            Dictionary<string, int> uWordDict = new Dictionary<string, int>();
            Dictionary<string, int> kWordDict = new Dictionary<string, int>();

            Dictionary<int, string> ccWordDict = new Dictionary<int, string>();
            Dictionary<int, string> uuWordDict = new Dictionary<int, string>();
            Dictionary<int, string> kkWordDict = new Dictionary<int, string>();

            QuickGraph.UndirectedGraph<string, Edge<string>> graph = new UndirectedGraph<string, Edge<string>>(false);

            zuk_dbSQLDataSet.zuk_fixedDataTable zukTable = new zuk_dbSQLDataSet.zuk_fixedDataTable();
            if(dbFileName.IndexOf("\\")>-1)
                zukTable.ReadXml(dbFileName);
            else
                zukTable.ReadXml(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, dbFileName));

            foreach (zuk_dbSQLDataSet.zuk_fixedRow row in zukTable)
            {
                string strChinese = row.Zh.Trim();
                string[] strUyghurs;// = row.Ug.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                if (irpan.ContainsKey(strChinese))
                {
                    strUyghurs = irpan[strChinese].Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                    strUyghurs = row.Ug.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                string[] strKazaks = row.Kz.Split(spliter, StringSplitOptions.RemoveEmptyEntries);

                //trim  
                for (int i = 0; i < strUyghurs.Length; i++)
                {
                    strUyghurs[i] = strUyghurs[i].Trim();
                }
                for (int i = 0; i < strKazaks.Length; i++)
                {
                    strKazaks[i] = strKazaks[i].Trim();
                }

                //add to db
                int cID = cWordDict.Count;
                cWordDict.Add(strChinese, cID);
                ccWordDict.Add(cID,strChinese);
                graph.AddVertex("c" + cID);

                //u
                for (int i = 0; i < strUyghurs.Length; i++)
                {
                    if (!uWordDict.ContainsKey(strUyghurs[i]))
                    {
                        int uID = uWordDict.Count;
                        uWordDict.Add(strUyghurs[i], uID);
                        uuWordDict.Add(uID, strUyghurs[i]);
                        graph.AddVertex("u" + uID);
                        graph.AddEdge(new Edge<string>("c" + cID.ToString(), "u" + uID.ToString()));
                    }
                    else
                        graph.AddEdge(new Edge<string>("c" + cID, "u" + uWordDict[strUyghurs[i]]));
                }

                //k
                for (int i = 0; i < strKazaks.Length; i++)
                {
                    if (!kWordDict.ContainsKey(strKazaks[i]))
                    {
                        int kID = kWordDict.Count;
                        kWordDict.Add(strKazaks[i], kID);
                        kkWordDict.Add(kID, strKazaks[i]);
                        graph.AddVertex("k" + kID);
                        graph.AddEdge(new Edge<string>("c" + cID.ToString(), "k" + kID.ToString()));
                    }
                    graph.AddEdge(new Edge<string>("c" + cID, "k" +kWordDict[strKazaks[i]]));
                }
            }



            var maxU = graph.Vertices.Where<string>(t => t.StartsWith("u")).OrderByDescending(t => graph.AdjacentEdges(t).Count());
            var maxU2 = maxU.ToDictionary(t => uuWordDict[int.Parse(t.TrimStart('u'))],t=>graph.AdjacentEdges(t).Count());

            var maxK = graph.Vertices.Where<string>(t => t.StartsWith("k")).OrderByDescending(t => graph.AdjacentEdges(t).Count());
            var maxK2 = maxK.ToDictionary(t => kkWordDict[int.Parse(t.TrimStart('k'))], t => graph.AdjacentEdges(t).Count());
            //Test

            foreach (var item in maxU2)
            {
                if (item.Value >1)
                    continue;
                graph.RemoveVertex("u" + uWordDict[item.Key]);
            }

            foreach (var item in maxK2)
            {
                if (item.Value >1)
                    continue;
                graph.RemoveVertex("k" + kWordDict[item.Key]);
            }


            IncrementalConnectedComponentsAlgorithm<string, Edge<string>>
            a = new IncrementalConnectedComponentsAlgorithm<string, Edge<string>>(graph as IMutableVertexAndEdgeSet<string, Edge<string>>);
            a.Compute();

            KeyValuePair<int, IDictionary<string, int>> components = a.GetComponents();
            List<BidirectionalGraph<string, Edge<string>>> connectedComponents = new List<BidirectionalGraph<string, Edge<string>>>(components.Key);
            var grouped = components.Value.GroupBy(t => t.Value);

            foreach (var group in grouped)
            {
                BidirectionalGraph<string, Edge<string>> g = new BidirectionalGraph<string, Edge<string>>(true, group.Count());

                foreach (var item in group)
                {
                    g.AddVertex(item.Key);
                }

                foreach (var item in g.Vertices)
                {
                    g.AddEdgeRange(graph.AdjacentEdges(item));
                }

                connectedComponents.Add(g);
            }

            var connectedComponentsSorted = connectedComponents.OrderByDescending(t => t.VertexCount).ToList();


            return graph;
        }

        private static DictBase GenerateDatabaseFull(List<string> filterUWords, List<string> filterKWords)
        {
            BidirectionalGraph<string, Edge<string>> graph = new BidirectionalGraph<string, Edge<string>>(false);


            char[] spliter = new char[] { ',' };
            DictBase dictBase = new DictBase(); 
            //string[] lines = new string[] { };// System.IO.File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "new_CnUy2007.txt"));
            //string[] freqWords = System.IO.File.ReadAllLines(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "freqwords.txt"));
            /*Dictionary<string, string> irpan = new Dictionary<string, string>();
            foreach (var item in lines)
            {
                string c = item.Split('\t')[0];
                string u = item.Split('\t')[1];

                if (irpan.ContainsKey(c))
                    continue;
                irpan.Add(c, u);
            }*/

            //string[] lines_preStepResult = System.IO.File.ReadAllLines(System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bi_pairs_preStep.txt"));
            Dictionary<string, bool> filterUWords_dict = filterUWords.Select(t => t.Trim()).Distinct().ToDictionary(t => t, t => true);
            Dictionary<string, bool> filterKWords_dict = filterKWords.Select(t => t.Trim()).Distinct().ToDictionary(t => t, t => true);



            Dictionary<string, int> cWordDict = new Dictionary<string, int>();
            Dictionary<string, int> uWordDict = new Dictionary<string, int>();
            Dictionary<string, int> kWordDict = new Dictionary<string, int>();
            //Dictionary<int, string> cWordPOSDict = new Dictionary<int, string>();
            //Dictionary<int, string> uWordPOSDict = new Dictionary<int, string>();
            //Dictionary<int, string> kWordPOSDict = new Dictionary<int, string>();

            /*
             * 
            HDictInduction.Console.zuk_dbSQLDataSetTableAdapters.zuk_fixedTableAdapter zukAdapter = new zuk_fixedTableAdapter();
            zuk_dbSQLDataSet.zuk_fixedDataTable zukTable = new zuk_dbSQLDataSet.zuk_fixedDataTable();
            zukAdapter.Fill(zukTable);
             */

            zuk_dbSQLDataSet.zuk_fixedDataTable zukTable = new zuk_dbSQLDataSet.zuk_fixedDataTable();
            if (_ZukTable != null)
                zukTable = _ZukTable;
            else
            {
                try
                {
                    zukTable.ReadXml(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zukTable.xml"));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                    throw;
                }
                _ZukTable = zukTable;
            }

            //Debug.Write("Before loop");
            //int inLoop = 0;
            foreach (zuk_dbSQLDataSet.zuk_fixedRow row in zukTable)
            {
                //Debug.WriteLine(++inLoop);
                string strChinese = row.Zh.Trim();
                //string[] strUyghurs = irpan.ContainsKey(strChinese)?irpan[strChinese].Split(spliter, StringSplitOptions.RemoveEmptyEntries)
                //    : row.Ug.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                string[] strUyghurs = row.Ug.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
                string[] strKazaks = row.Kz.Split(spliter, StringSplitOptions.RemoveEmptyEntries);

                strUyghurs = strUyghurs.Select(t => t.Trim()).Distinct().ToArray();
                strKazaks = strKazaks.Select(t => t.Trim()).Distinct().ToArray();

                //commit filtering
                strUyghurs = strUyghurs.Where(t => !filterUWords_dict.ContainsKey(t)).ToArray();
                strKazaks = strKazaks.Where(t => !filterKWords_dict.ContainsKey(t)).ToArray();

                int[] strUyghursIDs = new int[strUyghurs.Length];
                int[] strKazaksIDs = new int[strKazaks.Length];
                int cID = cWordDict.Count;

                /* For data with Part of speech
                string[] idPOS = strChinese.Split(new char[] { '-' }, 2);
                strChinese = idPOS[1];
                */

                if (cWordDict.ContainsKey(strChinese))
                    throw new Exception("Multiple Chinese Word!");
                else
                {
                    //string chPOS = strChinese.Substring(0, 2);
                    //string chWOrd = strChinese.Substring(3);
                    //cWordPOSDict.Add(cID, chPOS);
                    cWordDict.Add(strChinese, cID);                    
                }                

                //u
                for (int i = 0; i < strUyghurs.Length; i++)
                {
                    //string uPOS = strUyghurs[i].Substring(0, 2);
                    //string uWOrd = strUyghurs[i].Substring(3);

                    /* For data with Part of speech
                    string[] mkPOS = strUyghurs[i].Split(new char[] { '-' }, 2);
                    string mk = mkPOS[1];                    

                    if (!uWordDict.ContainsKey(mk))//strUyghurs[i]))
                    */
                    if (!uWordDict.ContainsKey(strUyghurs[i]))
                    {
                        strUyghursIDs[i] = uWordDict.Count;
                        //uWordPOSDict.Add(uWordDict.Count, uPOS);
                        //uWordDict.Add(uWOrd, uWordDict.Count);                        
                        uWordDict.Add(strUyghurs[i], uWordDict.Count);
                        /* For data with Part of speech
                        uWordDict.Add(mk, uWordDict.Count);
                         */
                    }
                    else
                        /* For data with Part of speech
                        strUyghursIDs[i] = uWordDict[mk];//strUyghurs[i]];
                         */
                        strUyghursIDs[i] = uWordDict[strUyghurs[i]];
                }

                //k
                for (int i = 0; i < strKazaks.Length; i++)
                {
                    //string kPOS = strKazaks[i].Substring(0, 2);
                    //string kWOrd = strKazaks[i].Substring(3);
                    //string[] malayPOS = strKazaks[i].Split(new char[] { '-' }, 2);
                    /* For data with Part of speech
                    string[] malayPOS = strKazaks[i].Split(new char[] { '-' }, 3);                     
                    //string statMs = malayPOS[0];
                    string ms = malayPOS[2];
                     */ 
                    //string pair = strChinese + "_" + ms;                    
                    /* For data with Part of speech
                    if (!kWordDict.ContainsKey(ms))//strKazaks[i]))
                     */
                    if (!kWordDict.ContainsKey(strKazaks[i]))
                    {
                        strKazaksIDs[i] = kWordDict.Count;
                        //kWordPOSDict.Add(kWordDict.Count, kPOS);
                        //kWordDict.Add(kWOrd, kWordDict.Count);
                        kWordDict.Add(strKazaks[i], kWordDict.Count);
                        /* For data with Part of speech
                        kWordDict.Add(ms, kWordDict.Count);  
                         */ 
                    }
                    else
                        strKazaksIDs[i] = kWordDict[strKazaks[i]];
                        /* For data with Part of speech
                        strKazaksIDs[i] = kWordDict[ms];//strKazaks[i]];
                         */ 
                    //wnStatDict.Add(pair, float.Parse(statMs)); //Add id-ms translation wordnet statistic                        
                }

                //c
                dictBase.CUDictbase.CtoU.Add(cID, strUyghursIDs);
                dictBase.CKDictbase.CtoK.Add(cID, strKazaksIDs);

                //u
                foreach (int item in strUyghursIDs)
                {
                    if (!dictBase.CUDictbase.UtoC.ContainsKey(item))
                    {
                        dictBase.CUDictbase.UtoC.Add(item, new int[1] { cID });
                    }
                    else
                    {
                        int[] array = dictBase.CUDictbase.UtoC[item];
                        if (!array.Contains(cID))
                        {
                            Array.Resize(ref array, array.Length + 1);
                            array[array.Length - 1] = cID;
                            dictBase.CUDictbase.UtoC[item] = array;
                        }
                    }
                }

                //k
                foreach (int item in strKazaksIDs)
                {
                    if (!dictBase.CKDictbase.KtoC.ContainsKey(item))
                    {
                        dictBase.CKDictbase.KtoC.Add(item, new int[1] { cID });
                    }
                    else
                    {
                        int[] array = dictBase.CKDictbase.KtoC[item];
                        if (!array.Contains(cID))
                        {
                            Array.Resize(ref array, array.Length + 1);
                            array[array.Length - 1] = cID;
                            dictBase.CKDictbase.KtoC[item] = array;
                        }
                    }
                }                
            }
            
            dictBase.CWords = cWordDict.Keys.ToArray();
            dictBase.KWords = kWordDict.Keys.ToArray();
            dictBase.UWords = uWordDict.Keys.ToArray();

            
            //temp check Kyrgiz dictionary ---------------
            //Dictionary<string, string[]> krdb = new Dictionary<string, string[]>();
            //List<string> list = new List<string>();
            //Dictionary<string, bool> krUnique = new Dictionary<string, bool>();
            //String fileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "zh_pn_kr.txt");

            //int index = 0;
            //foreach (var line in File.ReadAllLines(fileName))
            //{
            //    string[] cols = line.Split('\t');
            //    if (cols.Length != 3)
            //        throw new Exception(index.ToString());
            //    string zh = cols[0].Trim();
            //    string[] krs = cols[2].Trim('"').Split('،', ',');
            //    if (krs.Length == 0)
            //        throw new Exception(index.ToString());
            //    list.Clear();
            //    foreach (var kr in krs)
            //    {
            //        if (string.IsNullOrEmpty(kr.Trim()))
            //            continue;
            //        list.Add(kr.Trim());
            //    }
            //    if (list.Count == 0)
            //        throw new Exception(index.ToString());
            //    krdb.Add(zh, list.ToArray());
            //    index++;
            //}

            //int gCount = 0;
            //int gPairCount =0;
            //int gUnique = 0;
            //foreach (var zh in dictBase.CWords)
            //{
            //    if(krdb.ContainsKey(zh))
            //    {
            //        gCount++;
            //        gPairCount += krdb[zh].Length;

            //        foreach (var kr in krdb[zh])
            //        {
            //            if (!krUnique.ContainsKey(kr))
            //                krUnique.Add(kr, true); 
            //        }
            //    }
            //}
            //gUnique = krUnique.Count;

            ////--------------------------------------------



            return dictBase;
        }

        //private static DictBase GenerateDatabaseTrimmed()
        //{
        //    Dictionary<string, bool> aloneDictionary = getCWordsWithOneToOne();

        //    char[] spliter = new char[] { ',' };
        //    DictBase dictBase = new DictBase();


        //    Dictionary<string, int> cWordDict = new Dictionary<string, int>();
        //    Dictionary<string, int> uWordDict = new Dictionary<string, int>();
        //    Dictionary<string, int> kWordDict = new Dictionary<string, int>();


        //    HDictInduction.Console.zuk_dbSQLDataSetTableAdapters.zuk_fixedTableAdapter zukAdapter = new zuk_fixedTableAdapter();
        //    zuk_dbSQLDataSet.zuk_fixedDataTable zukTable = new zuk_dbSQLDataSet.zuk_fixedDataTable();
        //    zukAdapter.Fill(zukTable);

        //    foreach (zuk_dbSQLDataSet.zuk_fixedRow row in zukTable)
        //    {
        //        string strChinese = row.Zh.Trim();

        //        if (aloneDictionary.ContainsKey(strChinese))
        //            continue;

        //        string[] strUyghurs = row.Ug.Split(spliter, StringSplitOptions.RemoveEmptyEntries);
        //        string[] strKazaks = row.Kz.Split(spliter, StringSplitOptions.RemoveEmptyEntries);

        //        int[] strUyghursIDs = new int[strUyghurs.Length];
        //        int[] strKazaksIDs = new int[strKazaks.Length];

        //        int cID = cWordDict.Count;
        //        if (cWordDict.ContainsKey(strChinese))
        //            cID = cWordDict[strChinese];
        //        else
        //            cWordDict.Add(strChinese, cID);

        //        //u
        //        for (int i = 0; i < strUyghurs.Length; i++)
        //        {
        //            strUyghurs[i] = strUyghurs[i].Trim();
        //            if (!uWordDict.ContainsKey(strUyghurs[i]))
        //            {
        //                strUyghursIDs[i] = uWordDict.Count;
        //                uWordDict.Add(strUyghurs[i], uWordDict.Count);
        //            }
        //            else
        //                strUyghursIDs[i] = uWordDict[strUyghurs[i]];
        //        }

        //        //k
        //        for (int i = 0; i < strKazaks.Length; i++)
        //        {
        //            strKazaks[i] = strKazaks[i].Trim();
        //            if (!kWordDict.ContainsKey(strKazaks[i]))
        //            {
        //                strKazaksIDs[i] = kWordDict.Count;
        //                kWordDict.Add(strKazaks[i], kWordDict.Count);
        //            }
        //            else
        //                strKazaksIDs[i] = kWordDict[strKazaks[i]];
        //        }

        //        //c
        //        dictBase.CUDictbase.CtoU.Add(cID, strUyghursIDs);
        //        dictBase.CKDictbase.CtoK.Add(cID, strKazaksIDs);

        //        //u
        //        foreach (int item in strUyghursIDs)
        //        {
        //            if (!dictBase.CUDictbase.UtoC.ContainsKey(item))
        //            {
        //                dictBase.CUDictbase.UtoC.Add(item, new int[1] { cID });
        //            }
        //            else
        //            {
        //                int[] array = dictBase.CUDictbase.UtoC[item];
        //                if (!array.Contains(cID))
        //                {
        //                    Array.Resize(ref array, array.Length + 1);
        //                    array[array.Length - 1] = cID;
        //                    dictBase.CUDictbase.UtoC[item] = array;
        //                }
        //            }
        //        }

        //        //k
        //        foreach (int item in strKazaksIDs)
        //        {
        //            if (!dictBase.CKDictbase.KtoC.ContainsKey(item))
        //            {
        //                dictBase.CKDictbase.KtoC.Add(item, new int[1] { cID });
        //            }
        //            else
        //            {
        //                int[] array = dictBase.CKDictbase.KtoC[item];
        //                if (!array.Contains(cID))
        //                {
        //                    Array.Resize(ref array, array.Length + 1);
        //                    array[array.Length - 1] = cID;
        //                    dictBase.CKDictbase.KtoC[item] = array;
        //                }
        //            }
        //        }
        //    }
        //    dictBase.CWords = cWordDict.Keys.ToArray();
        //    dictBase.KWords = kWordDict.Keys.ToArray();
        //    dictBase.UWords = uWordDict.Keys.ToArray();

        //    return dictBase;
        //}
        
        private static Dictionary<string, bool> getCWordsWithOneToOne()
        {
            Dictionary<string, bool> result = new Dictionary<string, bool>();
            for (int cId = 0; cId < _DictBase.CWords.Length; cId++)
            {
                bool isAlone = true;
                int[] uIds = _DictBase.CUDictbase.CtoU[cId];
                int[] kIds = _DictBase.CKDictbase.CtoK[cId];

                if (uIds.Length > 1)
                    isAlone = false;
                if (kIds.Length > 1)
                    isAlone = false;
                if (isAlone == true)
                    if (_DictBase.CUDictbase.UtoC[uIds[0]].Length > 1)
                        isAlone = false;
                if (isAlone == true)
                    if (_DictBase.CKDictbase.KtoC[kIds[0]].Length > 1)
                        isAlone = false;
                if (isAlone)
                    result.Add(_DictBase.CWords[cId], true);
            }
            return result;
        }

        public static Dictionary<string, string> GetCWordsWithOneToOne()
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            for (int cId = 0; cId < _DictBase.CWords.Length; cId++)
            {
                bool isAlone = true;
                int[] uIds = _DictBase.CUDictbase.CtoU[cId];
                int[] kIds = _DictBase.CKDictbase.CtoK[cId];

                if (uIds.Length > 1)
                    isAlone = false;
                if (kIds.Length > 1)
                    isAlone = false;
                if (isAlone == true)
                    if (_DictBase.CUDictbase.UtoC[uIds[0]].Length > 1)
                        isAlone = false;
                if (isAlone == true)
                    if (_DictBase.CKDictbase.KtoC[kIds[0]].Length > 1)
                        isAlone = false;
                if (isAlone)
                    result.Add(_DictBase.UWords[uIds[0]], _DictBase.KWords[kIds[0]]);
            }
            return result;
        }
        
        public static int LD(string sRow, string sCol)
        {
            int RowLen = sRow.Length;  // نىڭ ئۇزۇنلۇقى sRow
            int ColLen = sCol.Length;  // نىڭ ئۇزۇنلۇقى sCol
            int RowIdx;                // نىڭ زىيارەتچى ئىندىكىسى sRow
            int ColIdx;                // نىڭ زىيارەتچى ئىندىكىسى sCol
            char Row_i;                // نىڭ ئاي ئىنچى ھەرپى sRow
            char Col_j;                // نىڭ جى ئىنچى ھەرپى sCol
            int cost;                  // چىقىم

            //بىرىنچى قەدەم

            /// ئىككى دانە ۋېكتور قۇرۇش
            int[] v0 = new int[RowLen + 1];
            int[] v1 = new int[RowLen + 1];
            int[] vTmp;

            /// ئىككىنچى قەدەم
            /// بىرىنچى ۋېكتورنى دەسلەپلەشتۈرۈش
            for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
                v0[RowIdx] = RowIdx;

            //ئۈچۈنچى قەدەم

            /// ستوندىكى ھەربىر ھەرتپنى زىيارەت قىلىش
            for (ColIdx = 1; ColIdx <= ColLen; ColIdx++)
            {
                /// Set the 0'th element to the column number
                v1[0] = ColIdx;

                Col_j = sCol[ColIdx - 1];

                // تۆتىنچى قەدەم

                // قۇردىكى ھەربىر ھەرپىنى زىيارەت قىلىش
                for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
                {
                    Row_i = sRow[RowIdx - 1];
                    // بەشىنچى قەدەم
                    cost = Row_i == Col_j ? 0 : 1;
                    // ئالتىنچى قەدەم

                    /// كىچىكىنى تېپىش
                    int m_min = v0[RowIdx] + 1;
                    int b = v1[RowIdx - 1] + 1;
                    int c = v0[RowIdx - 1] + cost;

                    if (b < m_min)
                        m_min = b;
                    if (c < m_min)
                        m_min = c;

                    v1[RowIdx] = m_min;
                }

                /// ۋېكتورلارنى ئالماشتۇرۇش
                vTmp = v0;
                v0 = v1;
                v1 = vTmp;
            }


            // يەتتىنچى قەدەم

            //Value between 0 - 100
            //0==perfect match 100==totaly different

            //The vectors where swaped one last time at the end of the last loop,
            //that is why the result is now in v0 rather than in v1
            //int max = System.Math.Max(RowLen, ColLen);
            //return ((100 * v0[RowLen]) / max);

            return v0[RowLen];
            //return v0[RowLen];
        }

        public static float iLD(String sRow, String sCol)
        {
            int RowLen = sRow.Length;  // length of sRow
            //int RowLen = sRow.Length;  // length of sRow
            int ColLen = sCol.Length;  // length of sCol
            int RowIdx;                // iterates through sRow
            int ColIdx;                // iterates through sCol
            char Row_i;                // ith character of sRow
            char Col_j;                // jth character of sCol
            int cost;                   // cost

            // Step 1

            /// Create the two vectors
            int[] v0 = new int[RowLen + 1];
            int[] v1 = new int[RowLen + 1];
            int[] vTmp;

            /// Step 2
            /// Initialize the first vector
            for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
                v0[RowIdx] = RowIdx;

            // Step 3

            /// Fore each column
            for (ColIdx = 1; ColIdx <= ColLen; ColIdx++)
            {
                /// Set the 0'th element to the column number
                v1[0] = ColIdx;

                Col_j = sCol[ColIdx - 1];

                // Step 4

                // Fore each row
                for (RowIdx = 1; RowIdx <= RowLen; RowIdx++)
                {
                    Row_i = sRow[RowIdx - 1];
                    // Step 5
                    cost = Row_i == Col_j ? 0 : 1;
                    // Step 6

                    /// Find minimum
                    int m_min = v0[RowIdx] + 1;
                    int b = v1[RowIdx - 1] + 1;
                    int c = v0[RowIdx - 1] + cost;

                    if (b < m_min)
                        m_min = b;
                    if (c < m_min)
                        m_min = c;

                    v1[RowIdx] = m_min;
                }

                /// Swap the vectors
                vTmp = v0;
                v0 = v1;
                v1 = vTmp;
            }


            // Step 7

            /// Value between 0 - 100
            /// 0==perfect match 100==totaly different
            /// 
            /// The vectors where swaped one last time at the end of the last loop,
            /// that is why the result is now in v0 rather than in v1
            //int max = System.Math.Max(RowLen, ColLen);
            //return ((100 * v0[RowLen]) / max);

            //return v0[RowLen];

            float l = RowLen > ColLen ? RowLen : ColLen;

            float db = 1 - (v0[RowLen] / l);
            float result =  (float)Math.Round(db, 5);
            if (result == 0f)
                return 0.00001f;
            else
                return result;
        }

        public IList<int> GetSubGraph(string clueWord, Language lang)
        {
            GraphFinder finder = new GraphFinder();
            IList<int> result = finder.GetLinkedCWords2(clueWord);
            return result;
        }

        public IList<int> GetSubGraph(int clueWord, Language lang)
        {
            GraphFinder finder = new GraphFinder();
            IList<int> result = finder.GetLinkedCWords2(clueWord);
            return result;
        }

        private class GraphFinder
        {
            int depht1 = 0;
            int depht2 = 0;
            //private List<NtoNDictbase> databse = null;
            private DictBase databse = null;
            private Dictionary<int, bool> cWordList1 = null;
            private Dictionary<int, bool> cWordList2 = null;
            private Dictionary<int, bool> uWordList = null;
            private Dictionary<int, bool> kWordList = null;
            private Thread _thread = null;

            private List<string> Reulst = null;

            public GraphFinder()
            {
                this.databse = DBHelper.MyDictBase;
                cWordList1 = new Dictionary<int, bool>();
                cWordList2 = new Dictionary<int, bool>();
                uWordList = new Dictionary<int, bool>();
                kWordList = new Dictionary<int, bool>();
               
            }

            public IList<int> GetLinkedCWords2(int clueWord)
            {
                int clueWordID = clueWord;

                Dictionary<int, bool> cWordsVizited = new Dictionary<int, bool>();
                Dictionary<int, bool> uWordsVizited = new Dictionary<int, bool>();
                Dictionary<int, bool> kWordsVizited = new Dictionary<int, bool>();

                List<int> uStore1 = databse.CUDictbase.CtoU[clueWordID].ToList();
                List<int> uStore2 = new List<int>();

                List<int> kStore1 = databse.CKDictbase.CtoK[clueWordID].ToList();
                List<int> kStore2 = new List<int>();

                while (uStore1.Count != 0 || kStore1.Count != 0)
                {
                    for (int u = 0, k = 0; u < uStore1.Count || k < kStore1.Count; u++, k++)
                    {
                        //Uyghur
                        if (u < uStore1.Count)
                        {
                            if (!uWordsVizited.ContainsKey(uStore1[u]))
                            {
                                uWordsVizited.Add(uStore1[u], true);
                                foreach (int itemC1 in databse.CUDictbase.UtoC[uStore1[u]])
                                {
                                    if (!cWordsVizited.ContainsKey(itemC1))
                                        cWordsVizited.Add(itemC1, true);

                                    foreach (int itemU1 in databse.CUDictbase.CtoU[itemC1])
                                    {
                                        if (uWordsVizited.ContainsKey(itemU1))
                                            continue;
                                        uStore2.Add(itemU1);
                                    }
                                }
                            }

                        }
                        //Kazak
                        if (k < kStore1.Count)
                        {
                            if (!kWordsVizited.ContainsKey(kStore1[k]))
                            {
                                kWordsVizited.Add(kStore1[k], true);
                                foreach (int itemC1 in databse.CKDictbase.KtoC[kStore1[k]])
                                {
                                    if (!cWordsVizited.ContainsKey(itemC1))
                                        cWordsVizited.Add(itemC1, true);

                                    foreach (int itemK1 in databse.CKDictbase.CtoK[itemC1])
                                    {
                                        if (kWordsVizited.ContainsKey(itemK1))
                                            continue;
                                        kStore2.Add(itemK1);
                                    }
                                }
                            }

                        }
                    }

                    uStore1 = uStore2;
                    kStore1 = kStore2;
                    uStore2 = new List<int>();
                    kStore2 = new List<int>();
                }

                return cWordsVizited.Keys.ToList();
            }

            public IList<int> GetLinkedCWords2(string clueWord)
            {
                int clueWordID= Array.IndexOf<string>(databse.CWords, clueWord);
                return GetLinkedCWords2(clueWordID);
           }
        }
        
       
        
        public string EdgeIdentifier(Edge<Word> edge)
        {
            return string.Format("{0}-{1}-{2}-{3}", edge.Source.ID, (int)edge.Source.Language, edge.Target.ID, (int)edge.Target.Language);
        }

        public string AutoResultfolder()
        {
            return System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AutoResult");
        }

        public float Round(float f)
        {
            return (float)(Math.Round((double)f, 2));
        }

        public List<BidirectionalGraph<Word, Edge<Word>>> GetDatabaseConnectedComponents()
        {
            BidirectionalGraph<Word, Edge<Word>> graph = new BidirectionalGraph<Word, Edge<Word>>(true);

            //C vertexs
            foreach (var item in DBHelper._DictBase.CUDictbase.CtoU)
            {
                graph.AddVertex(DBHelper._DictBase.GetCWordByID(item.Key));
            }

            //U vertexs
            foreach (var item in DBHelper._DictBase.CUDictbase.UtoC)
            {
                graph.AddVertex(DBHelper._DictBase.GetUWordByID(item.Key));
            }

            //K vertexs
            foreach (var item in DBHelper._DictBase.CKDictbase.KtoC)
            {
                graph.AddVertex(DBHelper._DictBase.GetKWordByID(item.Key));
            }

            //add C to U edges
            foreach (var item in DBHelper._DictBase.CUDictbase.CtoU)
            {
                foreach (var item2 in item.Value)
                {
                    graph.AddEdge(new Edge<Word>(DBHelper._DictBase.GetCWordByID(item.Key), DBHelper._DictBase.GetUWordByID(item2)));
                }
            }

            //add C to K edges
            foreach (var item in DBHelper._DictBase.CKDictbase.CtoK)
            {
                foreach (var item2 in item.Value)
                {
                    graph.AddEdge(new Edge<Word>(DBHelper._DictBase.GetCWordByID(item.Key), DBHelper._DictBase.GetKWordByID(item2)));
                }
            }


            if (WordRelaionGraph == null)
            {

                WordRelaionGraph = new BidirectionalGraph<Word, Edge<Word>>(true);
                foreach (var item in graph.Vertices)
                {
                    WordRelaionGraph.AddVertex(item);
                }
                foreach (var item in graph.Edges)
                {
                    WordRelaionGraph.AddEdge(item);
                }
            }

            IncrementalConnectedComponentsAlgorithm<Word, Edge<Word>>
            a = new IncrementalConnectedComponentsAlgorithm<Word, Edge<Word>>(graph as IMutableVertexAndEdgeSet<Word, Edge<Word>>);
            a.Compute();

            KeyValuePair<int, IDictionary<Word, int>> components = a.GetComponents();
            List<BidirectionalGraph<Word, Edge<Word>>> connectedComponents = new List<BidirectionalGraph<Word, Edge<Word>>>(components.Key);
            var grouped = components.Value.GroupBy(t => t.Value);

            foreach (var group in grouped)
            {
                BidirectionalGraph<Word, Edge<Word>> g = new BidirectionalGraph<Word, Edge<Word>>(true, group.Count());

                foreach (var item in group)
                {
                    g.AddVertex(item.Key);
                }

                foreach (var item in g.Vertices)
                {
                    g.AddEdgeRange(graph.OutEdges(item));
                }

                connectedComponents.Add(g);
            }

            return connectedComponents;

        }

        public List<BidirectionalGraph<Word, Edge<Word>>> GetDatabaseConnectedComponents(BidirectionalGraph<Word, Edge<Word>> graph)
        {
            IncrementalConnectedComponentsAlgorithm<Word, Edge<Word>>
            a = new IncrementalConnectedComponentsAlgorithm<Word, Edge<Word>>(graph as IMutableVertexAndEdgeSet<Word, Edge<Word>>);
            a.Compute();

            KeyValuePair<int, IDictionary<Word, int>> components = a.GetComponents();
            List<BidirectionalGraph<Word, Edge<Word>>> connectedComponents = new List<BidirectionalGraph<Word, Edge<Word>>>(components.Key);
            var grouped = components.Value.GroupBy(t => t.Value);

            foreach (var group in grouped)
            {
                BidirectionalGraph<Word, Edge<Word>> g = new BidirectionalGraph<Word, Edge<Word>>(true, group.Count());

                foreach (var item in group)
                {
                    g.AddVertex(item.Key);
                }

                foreach (var item in g.Vertices)
                {
                    g.AddEdgeRange(graph.OutEdges(item));
                }

                connectedComponents.Add(g);
            }

            return connectedComponents;

        }

        public List<BidirectionalGraph<string, Edge<string>>> GetDatabaseConnectedComponents(BidirectionalGraph<string, Edge<string>> graph)
        {
            IncrementalConnectedComponentsAlgorithm<string, Edge<string>>
            a = new IncrementalConnectedComponentsAlgorithm<string, Edge<string>>(graph as IMutableVertexAndEdgeSet<string, Edge<string>>);
            a.Compute();

            KeyValuePair<int, IDictionary<string, int>> components = a.GetComponents();
            List<BidirectionalGraph<string, Edge<string>>> connectedComponents = new List<BidirectionalGraph<string, Edge<string>>>(components.Key);
            var grouped = components.Value.GroupBy(t => t.Value);

            foreach (var group in grouped)
            {
                BidirectionalGraph<string, Edge<string>> g = new BidirectionalGraph<string, Edge<string>>(true, group.Count());

                foreach (var item in group)
                {
                    g.AddVertex(item.Key);
                }

                foreach (var item in g.Vertices)
                {
                    g.AddEdgeRange(graph.OutEdges(item));
                }

                connectedComponents.Add(g);
            }

            return connectedComponents;

        }

        public static BidirectionalGraph<Word, Edge<Word>> CloneGraph(BidirectionalGraph<Word, Edge<Word>> graph)
        {
            BidirectionalGraph<Word, Edge<Word>> result = new BidirectionalGraph<Word, Edge<Word>>(true);
            foreach (var item in graph.Vertices)
            {
                result.AddVertex(item);
            }
            foreach (var item in graph.Edges)
            {
                result.AddEdge(item);
            }
            return result;
        }
    }
}
