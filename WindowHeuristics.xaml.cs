﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using QuickGraph;
using HDictInduction.Console.Resources;
using HDictInduction.Console.Heuristics;
using System.Data;
using System.ComponentModel;
using GraphSharp.Controls;
using QuickGraph.Algorithms.ConnectedComponents;
using System.Threading.Tasks;

namespace HDictInduction.Console
{
    /// <summary>
    /// Interaction logic for WindowHeuristics.xaml
    /// </summary>
    public partial class WindowHeuristics : Window
    {
        private GridLength _VerticalGridWith;
        private GridLength _HorzintalGridHeight;
        private Dictionary<Word, INodeControl> _NodeDictionary = null;
        private CandidateStore _CandidateStore = null;

        List<BidirectionalGraph<Word, Edge<Word>>> ConnectedComponets = null;

        public bool Latin
        {
            get { return GlobalStore.Latin; }
            set { GlobalStore.Latin = value; }
        }

        public WindowHeuristics()
        {
            InitializeComponent();
            cmbxLayoutAlgorithm.SelectedIndex = 7;
            GraphSharp.Algorithms.Layout.Simple.Hierarchical.EfficientSugiyamaLayoutParameters
                parameter = new GraphSharp.Algorithms.Layout.Simple.Hierarchical.EfficientSugiyamaLayoutParameters();
            parameter.LayerDistance = 100f;
            this.graphLayout.LayoutParameters = parameter;
            this._VerticalGridWith = this.grid0.ColumnDefinitions[0].Width;
            this._HorzintalGridHeight = this.grid1.RowDefinitions[1].Height;

            this.listBox_AllUWords.ItemsSource =
            DBHelper.MyDictBase.GetUWordByIDs(Enumerable.Range(0, DBHelper.MyDictBase.UWords.Length - 1).ToList());
            this.label_AllUWords.Content = this.listBox_AllUWords.Items.Count.ToString();
            if (this.listBox_AllUWords.Items.Count > 0)
                this.listBox_AllUWords.SelectedIndex = 0;


            ParameterProvider.RUNTIME_PARAMS = ParameterProvider.NORMAL_PARAMS;

            this.textBox_w1.Text = ParameterProvider.RUNTIME_PARAMS[0].ToString();
            this.textBox_w2.Text = ParameterProvider.RUNTIME_PARAMS[1].ToString();
            this.textBox_w3.Text = ParameterProvider.RUNTIME_PARAMS[2].ToString();

        }

        private void button_FindCandidates_Click(object sender, RoutedEventArgs e)
        {
            if (this.listBox_AllUWords.SelectedIndex < 0)
                return;
            //float w1 = 0, w2 = 0, w3 = 0;
            //if (!(float.TryParse(this.textBox_w1.Text, out w1) && float.TryParse(this.textBox_w2.Text, out w2) && float.TryParse(this.textBox_w3.Text, out w3)))
            //{
            //    MessageBox.Show("Parameter incorrect!");
            //    return;
            //}

            DBHelper dbHelper = new DBHelper();
            Word uWord = (Word)this.listBox_AllUWords.SelectedItem;

            _CandidateStore = dbHelper.GetHeuristicCandidatesOfU(uWord);
            //_CandidateStore.Parameters = new float[] { w1, w2, w3 };


            BidirectionalGraph<object, IEdge<object>> graph = _CandidateStore.VisualGraph;
            _NodeDictionary = graph.Vertices.Cast<INodeControl>().ToDictionary(t => t.Word, t => t);

            var KNodes = graph.Vertices.Cast<INodeControl>().Where<INodeControl>(t => t.Word.Language == HDictInduction.Console.Language.Kazak);
            this.listBox_KCandidates.ItemsSource = KNodes.Select(t => t.Word);
            this.label_CandidateKWords.Content = this.listBox_KCandidates.Items.Count.ToString();

            foreach (var item in graph.Vertices)
            {
                if (item is UserControl)
                    (item as UserControl).MouseLeftButtonDown += new MouseButtonEventHandler(Node_MouseLeftButtonDown);
            }

            DataTable table = CandidateStore.GetDataTable(_CandidateStore);
            this.dgvSummary.ItemsSource = table.DefaultView;

            if (_CandidateStore.Language == Console.Language.Kazak)
                graph.AddVertex("X");
            if (_CandidateStore.Language == Console.Language.Uyghur)
                graph.AddVertex("Y");
            graph.AddVertex("Z");
            graph.AddVertex(new GraphMatrixLabelControl());

            Dispatcher.BeginInvoke(new Action(delegate
            {
                this.graphLayout.Graph = graph;
                this.graphLayout.UpdateLayout();
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        void Node_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is NodeControl)
            {
                this.textBox_CurrentKWord.Text = (sender as NodeControl).Word.ToString();
            }
            else
            {
                NodeConstrolWithMatrix node = sender as NodeConstrolWithMatrix;
                int index = 0;
                foreach (var item in _CandidateStore.WeightVectors)
                {
                    if (item.Key == node.Word)

                        break;
                    index++;
                }
                int index2 = 0;
                foreach (DataRowView item in this.dgvSummary.ItemsSource)
                {
                    int id = int.Parse(item.Row.ItemArray[0].ToString());
                    if (id == index + 1)
                        break;
                    index2++;
                }
                if (index2 < this.dgvSummary.Items.Count)
                {
                    dgvSummary.SelectedIndex = index2;
                    dgvSummary.ScrollIntoView(this.dgvSummary.SelectedItem);
                }
                if (node.Word.Language == Console.Language.Kazak)
                    this.listBox_KCandidates.SelectedIndex = index;
            }
        }

        private void button_Relayout_Click(object sender, RoutedEventArgs e)
        {
            this.graphLayout.Relayout();
        }

        private void listBox_AllUWords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.listBox_AllUWords.SelectedIndex > -1)
            {
                this.label_CurrentUword.Content = DBHelper.Syn.getUKYFromUy(((Word)this.listBox_AllUWords.SelectedItem).Value);
                this.textBox_CurrentUWord.Text = ((Word)this.listBox_AllUWords.SelectedItem).Value;
            }
            else
                this.label_CurrentUword.Content = string.Empty;
        }

        private void zoomGraph_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (this.grid0.ColumnDefinitions[0].Width.Value == 0)
            {
                this.grid0.ColumnDefinitions[0].Width = this._VerticalGridWith;
                this.grid1.RowDefinitions[1].Height = this._HorzintalGridHeight;
            }
            else
            {
                this.grid0.ColumnDefinitions[0].Width = new GridLength(0);
                this.grid1.RowDefinitions[1].Height = new GridLength(0);
            }
        }

        private void listBox_KCandidates_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.listBox_KCandidates.SelectedIndex < 0)
                return;
            INodeControl node = _NodeDictionary[(Word)this.listBox_KCandidates.SelectedItem];
            if (node == null)
                return;
            foreach (object item in this.graphLayout.Graph.Vertices)
            {
                if (!(item is INodeControl))
                    continue;
                if (item == node)
                    graphLayout.HighlightVertex(item, "none");
                else
                    graphLayout.RemoveHighlightFromVertex(item);
            }
            this.textBox_CurrentKWord.Text = ((Word)this.listBox_KCandidates.SelectedItem).Value;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            this.graphLayout.HighlightAlgorithmType = "none";
        }

        private void dgvSummary_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;
            if (!(e.AddedItems[0] is DataRowView))
                return;
            int id = int.Parse((e.AddedItems[0] as DataRowView).Row.ItemArray[0].ToString());
            Word word = _CandidateStore.WeightVectors.ElementAt(id - 1).Key;
            INodeControl node = _NodeDictionary[word];
            foreach (object item in this.graphLayout.Graph.Vertices)
            {
                if (!(item is INodeControl))
                    continue;
                if (item == node)
                    graphLayout.HighlightVertex(item, "none");
                else
                    graphLayout.RemoveHighlightFromVertex(item);
            }
        }

        private void button_Sort_Click(object sender, RoutedEventArgs e)
        {
            ICollectionView view = CollectionViewSource.GetDefaultView(this.dgvSummary.ItemsSource);
            view.SortDescriptions.Clear();
            view.SortDescriptions.Add(new SortDescription((sender as Button).Tag.ToString(), ListSortDirection.Descending));
            view.Refresh();
        }

        private void button_FindAllPairs_Click(object sender, RoutedEventArgs e)
        {
            DBHelper helper = new DBHelper();
            BidirectionalGraph<Word, Edge<Word>> graph = helper.GetFullBidirectionalBestCandidatesGraph().Key;
            int uCount = 0, kCount = 0;

            foreach (Word item in graph.Vertices)
            {
                if (item.Language == Console.Language.Uyghur)
                    uCount++;
                if (item.Language == Console.Language.Kazak)
                    kCount++;
            }

            IMutableVertexAndEdgeSet<Word, Edge<Word>> tmp = graph as IMutableVertexAndEdgeSet<Word, Edge<Word>>;

            IncrementalConnectedComponentsAlgorithm<Word, Edge<Word>>
            a2 = new IncrementalConnectedComponentsAlgorithm<Word, Edge<Word>>(tmp);
            a2.Compute();
            MessageBox.Show(string.Format("U:{0} K:{1} Edges:{2}, Components:{3}", uCount, kCount, graph.EdgeCount, a2.ComponentCount));
        }

        private void button_SamplePairs_Click(object sender, RoutedEventArgs e)
        {
            DBHelper helper = new DBHelper();
            BidirectionalGraph<object, IEdge<object>> graph = helper.GetHeuristicPairsSample(200, this.textBox_ShouldBeIncluded.Text.Trim(), true);

            Dispatcher.BeginInvoke(new Action(delegate
            {
                this.graphLayout.Graph = graph;
                this.graphLayout.UpdateLayout();
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void button_FindAllOnetoOnePairs_Click(object sender, RoutedEventArgs e)
        {
            float w1 = 0, w2 = 0, w3 = 0;
            if (!(float.TryParse(this.textBox_w1.Text, out w1) && float.TryParse(this.textBox_w2.Text, out w2) && float.TryParse(this.textBox_w3.Text, out w3)))
            {
                MessageBox.Show("Parameter incorrect!");
                return;
            }


            DBHelper helper = new DBHelper();
            BidirectionalGraph<Word, Edge<Word>> graph = helper.GetFullUtoKBestCandidatesGraph().Key;
            int ID = 1;

            StringBuilder sb = new StringBuilder();
            string title = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
    "ID", "Uyghur", "Kazak", "H1", "H2", "H3", "Over All");
            sb.AppendLine(title);

            foreach (Word item in graph.Vertices)
            {
                if (item.Language != HDictInduction.Console.Language.Uyghur)
                    continue;
                if (graph.OutDegree(item) != 1)
                    continue;
                Edge<Word> edge = graph.OutEdge(item, 0);
                if (graph.InDegree(edge.Target) != 1)
                    continue;


                Word kWord = (Word)edge.Target;
                Word uWord = item;

                CandidateStore candidateStore = helper.GetHeuristicCandidatesOfU(uWord);
                candidateStore.Parameters = new float[] { w1, w2, w3 };



                float weight = CandidateStore.GetFValue(kWord, candidateStore);
                float h1 = candidateStore.WeightVectors[kWord][0];
                float h2 = candidateStore.WeightVectors[kWord][1];
                float h3 = candidateStore.WeightVectors[kWord][2];
                string line = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                    ID, uWord, kWord, h1, h2, h3, weight);
                sb.AppendLine(line);
                ID++;
            }

            this.textBox_Output.Text = sb.ToString();
        }

        private void button_FindUCandidates_Click(object sender, RoutedEventArgs e)
        {
            if (this.listBox_KCandidates.SelectedIndex < 0)
                return;
            float w1 = 0, w2 = 0, w3 = 0;
            if (!(float.TryParse(this.textBox_w1.Text, out w1) && float.TryParse(this.textBox_w2.Text, out w2) && float.TryParse(this.textBox_w3.Text, out w3)))
            {
                MessageBox.Show("Parameter incorrect!");
                return;
            }

            DBHelper dbHelper = new DBHelper();
            Word word = (Word)this.listBox_KCandidates.SelectedItem;

            _CandidateStore = dbHelper.GetHeuristicCandidatesOfK(word);
            _CandidateStore.Parameters = new float[] { w1, w2, w3 };


            BidirectionalGraph<object, IEdge<object>> graph = _CandidateStore.VisualGraph;
            _NodeDictionary = graph.Vertices.Cast<INodeControl>().ToDictionary(t => t.Word, t => t);

            //var KNodes = graph.Vertices.Cast<INodeControl>().Where<INodeControl>(t => t.Word.Language == HDictInduction.Console.Language.Kazak);
            //this.listBox_KCandidates.ItemsSource = KNodes.Select(t => t.Word);
            //this.label_CandidateKWords.Content = this.listBox_KCandidates.Items.Count.ToString();

            foreach (var item in graph.Vertices)
            {
                if (item is UserControl)
                    (item as UserControl).MouseLeftButtonDown += new MouseButtonEventHandler(Node_MouseLeftButtonDown);
            }

            DataTable table = CandidateStore.GetDataTable(_CandidateStore);
            this.dgvSummary.ItemsSource = table.DefaultView;

            graph.AddVertex("K");
            graph.AddVertex("C");
            graph.AddVertex(new GraphMatrixLabelControl());

            Dispatcher.BeginInvoke(new Action(delegate
            {
                this.graphLayout.Graph = graph;
                this.graphLayout.UpdateLayout();
            }), System.Windows.Threading.DispatcherPriority.Render);
        }

        private void button_SelectRandomPairs_Click(object sender, RoutedEventArgs e)
        {
            float w1 = 0, w2 = 0, w3 = 0;
            if (!(float.TryParse(this.textBox_w1.Text, out w1) && float.TryParse(this.textBox_w2.Text, out w2) && float.TryParse(this.textBox_w3.Text, out w3)))
            {
                MessageBox.Show("Parameter incorrect!");
                return;
            }


            DBHelper helper = new DBHelper();
            int ID = 1;
            BidirectionalGraph<Word, Edge<Word>> graph = helper.GetFullUtoKBestCandidatesGraph().Key;
            List<string> list = new List<string>();
            string title = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}", "ID", "Uyghur", "Kazak", "H1", "H2", "H3", "Over All");
            list.Add(title);




            foreach (Word item in graph.Vertices)
            {
                if (item.Language != HDictInduction.Console.Language.Uyghur)
                    continue;
                if (graph.OutDegree(item) != 1)
                    continue;
                Edge<Word> edge = graph.OutEdge(item, 0);
                if (graph.InDegree(edge.Target) != 1)
                    continue;


                Word kWord = (Word)edge.Target;
                Word uWord = item;

                CandidateStore candidateStore = helper.GetHeuristicCandidatesOfU(uWord);
                candidateStore.Parameters = new float[] { w1, w2, w3 };



                float weight = CandidateStore.GetFValue(kWord, candidateStore);
                float h1 = candidateStore.WeightVectors[kWord][0];
                float h2 = candidateStore.WeightVectors[kWord][1];
                float h3 = candidateStore.WeightVectors[kWord][2];
                string line = string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}",
                    ID, uWord, kWord, h1, h2, h3, weight);
                list.Add(line);
                ID++;
            }

            int limit = 1000;
            List<string> list2 = new List<string>();
            Dictionary<int, bool> ids = new Dictionary<int, bool>();
            Random random = new Random();

            for (int i = 0; i < limit; i++)
            {
                int id = -1;
                do
                {
                    id = random.Next(0, list.Count());
                } while (ids.ContainsKey(id));
                list2.Add(list[id]);
            }

            StringBuilder sb = new StringBuilder();
            foreach (string item in list2)
            {
                sb.AppendLine(item);
            }

            this.textBox_Output.Text = sb.ToString();

        }





































































        private List<KeyValuePair<Word, Word>> getBidirectedPairs(BidirectionalGraph<Word, Edge<Word>> graph)
        {
            StronglyConnectedComponentsAlgorithm<Word, Edge<Word>> cf
               = new StronglyConnectedComponentsAlgorithm<Word, Edge<Word>>(graph as IVertexListGraph<Word, Edge<Word>>);
            cf.Compute();
            IDictionary<Word, int> d1 = cf.Components;
            var d2 = d1.GroupBy(t => t.Value);

            List<KeyValuePair<Word, Word>> bidirectedList = new List<KeyValuePair<Word, Word>>();

            foreach (var item1 in d2)
            {
                if (item1.Count() < 2)
                    continue;
                IList<Word> l1 = item1.Select(t => t.Key).ToList();

                foreach (var item2 in l1)
                {
                    if (item2.Language != Console.Language.Uyghur)
                        continue;
                    foreach (var edge in graph.OutEdges(item2))
                    {
                        Edge<Word> e = null;
                        if (graph.TryGetEdge(edge.Target, edge.Source, out e))
                            bidirectedList.Add(new KeyValuePair<Word, Word>(item2, edge.Target));
                    }
                }
            }


            foreach (var item in bidirectedList)
            {
                Edge<Word> e1;
                if (!graph.TryGetEdge(item.Key, item.Value, out e1))
                    throw new Exception("Error");
                if (!graph.TryGetEdge(item.Value, item.Key, out e1))
                    throw new Exception("Error");
            }
            return bidirectedList;
        }

        private List<BidirectionalGraph<Word,Edge<Word>>> getConnectedComponents(BidirectionalGraph<Word,Edge<Word>> graph)
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

        private bool IsStrongPair(KeyValuePair<Word, Word> pair, List<KeyValuePair<Word, Word>> pairs)
        {

            bool isStrongPair = true;
            foreach (var item in pairs)
            {

                if (pair.Key == item.Key && pair.Value == item.Value)
                    continue;
                if (pair.Key == item.Key || pair.Value == item.Value)
                {
                    isStrongPair = false;
                    break;
                }
            }
            return isStrongPair;
        }



        //temp form weak pair anylisis
        List<BidirectionalGraph<object, IEdge<object>>> listWeekPairGraphs = new List<BidirectionalGraph<object, IEdge<object>>>();
        List<BidirectionalGraph<object, IEdge<object>>> listWeekPairGraphs3 = new List<BidirectionalGraph<object, IEdge<object>>>();
        private void button_temp_Click(object sender, RoutedEventArgs e)
        {
            long tick = DateTime.Now.Ticks;

            DBHelper helper = new DBHelper();
            BidirectionalGraph<Word, Edge<Word>> graph = null;

            graph = helper.GetFullBidirectionalBestCandidatesGraph().Key;

            int uCount = 0, kCount = 0;

            foreach (Word item in graph.Vertices)
            {
                if (item.Language == Console.Language.Uyghur)
                    uCount++;
                if (item.Language == Console.Language.Kazak)
                    kCount++;
            }

            this.ConnectedComponets = getConnectedComponents(graph);


            //Show info
            StringBuilder sb = new StringBuilder();
            sb.AppendLine(string.Format("U:{0} K:{1} Edges:{2}, Components:{3}", uCount, kCount, graph.EdgeCount, ConnectedComponets.Count));

           
            //Find Bidirected Pairs from All subgraphs
            List<KeyValuePair<Word, Word>> bidirectedPairsAll = new List<KeyValuePair<Word, Word>>();
            List<KeyValuePair<Word, Word>> weakPairs = new List<KeyValuePair<Word, Word>>();

            int distinctPair = 0;

            List<int> tempList = new List<int>();
            for (int t = 0; t < ConnectedComponets.Count; t++)
            {
                BidirectionalGraph<Word, Edge<Word>> item = ConnectedComponets[t];
                IList<KeyValuePair<Word, Word>> b = this.getBidirectedPairs(item);
                bidirectedPairsAll.AddRange(b);


                foreach (var item2 in b)
                {
                    bool ok = true;
                    foreach (var item3 in b)
                    {

                        if (item2.Key == item3.Key && item2.Value == item3.Value)
                            continue;
                        if (item2.Key == item3.Key || item2.Value == item3.Value)
                        {
                            ok = false;
                            break;
                        }
                    }
                    if (ok)
                    {
                        distinctPair++;
                    }
                    else
                    {
                        weakPairs.Add(item2);
                        if (!tempList.Contains(t))
                            tempList.Add(t);
                    }
                }
            }

            //temp weak pair statistics
            

            listWeekPairGraphs = new List<BidirectionalGraph<object, IEdge<object>>>();
            listWeekPairGraphs3 = new List<BidirectionalGraph<object, IEdge<object>>>();
            BidirectionalGraph<Word, Edge<Word>> graphWeakPairs = new BidirectionalGraph<Word, Edge<Word>>();
            foreach (var item in weakPairs)
            {
                if(!graphWeakPairs.ContainsVertex(item.Key))
                    graphWeakPairs.AddVertex(item.Key);
                if (!graphWeakPairs.ContainsVertex(item.Value))
                    graphWeakPairs.AddVertex(item.Value);

                graphWeakPairs.AddEdge(new Edge<Word>(item.Key, item.Value));
                graphWeakPairs.AddEdge(new Edge<Word>(item.Value, item.Key));
            }

            StronglyConnectedComponentsAlgorithm<Word, Edge<Word>> cf
   = new StronglyConnectedComponentsAlgorithm<Word, Edge<Word>>(graphWeakPairs as IVertexListGraph<Word, Edge<Word>>);
            cf.Compute();
            IDictionary<Word, int> d1 = cf.Components;
            var d2 = d1.GroupBy(t => t.Value);

            foreach (var item in d2)
            {
                IList<Word> l1 = item.Select(t => t.Key).ToList();
                BidirectionalGraph<object, IEdge<object>> g = new BidirectionalGraph<object, IEdge<object>>();
                foreach (var item2 in l1)
                {
                    if (item2.Language != Console.Language.Uyghur)
                        continue;
                    foreach (var edge in graphWeakPairs.OutEdges(item2))
                    {
                        g.AddVertex(item2);
                        if (!g.ContainsVertex(edge.Target))
                            g.AddVertex(edge.Target);
                        g.AddEdge(new Edge<object>(item2, edge.Target));
                        g.AddEdge(new Edge<object>(edge.Target, item2));
                    }
                }

                listWeekPairGraphs.Add(g);
                if (g.VertexCount > 3)
                    listWeekPairGraphs3.Add(g);
            }



            Dictionary<Word, List<Word>> dict1 = new Dictionary<Word, List<Word>>();
            foreach (var item2 in weakPairs)
            {
                if (dict1.ContainsKey(item2.Key))
                    dict1[item2.Key].Add(item2.Value);
                else
                    dict1.Add(item2.Key, new List<Word>() { item2.Value });
            }
            Dictionary<Word, List<Word>> dict3 = new Dictionary<Word, List<Word>>();
            foreach (var item in dict1)
            {
                if (item.Value.Count >= 2)
                    dict3.Add(item.Key, item.Value);
            }


            Dictionary<int, bool> randomDict = new Dictionary<int, bool>();


            List<int> list20 = randomListGenerator(20, dict3.Count);
            List<int> list40 = randomListGenerator(40, dict3.Count);
            List<int> list60 = randomListGenerator(60, dict3.Count);
            List<int> list80 = randomListGenerator(80, dict3.Count);
            List<int> list100 = randomListGenerator(100, dict3.Count);

            List<string> list20_r = new List<string>();
            List<string> list20_s = new List<string>();
            List<string> list40_r = new List<string>();
            List<string> list40_s = new List<string>();
            List<string> list60_r = new List<string>();
            List<string> list60_s = new List<string>();
            List<string> list80_r = new List<string>();
            List<string> list80_s = new List<string>();
            List<string> list100_r = new List<string>();
            List<string> list100_s = new List<string>();

            foreach (var item in list20)
            {
                KeyValuePair<Word, List<Word>> kv = dict3.ElementAt(item);
                
                var v1 = kv.Value.OrderByDescending(t=>graph.Degree(t)).First();
                var v2 = kv.Value.OrderBy(t => graph.Degree(t)).First();
            
                list20_s.Add(string.Format("{0}\t{1}", kv.Key.Value, v1.Value));
                list20_r.Add(string.Format("{0}\t{1}", kv.Key.Value, v2.Value));
            }

            var vv1 = list20_r.Intersect(list20_s);

            foreach (var item in list40)
            {
                KeyValuePair<Word, List<Word>> kv = dict3.ElementAt(item);

                var v1 = kv.Value.OrderByDescending(t => graph.Degree(t)).First();
                var v2 = kv.Value.OrderBy(t => graph.Degree(t)).First();

                list40_s.Add(string.Format("{0}\t{1}", kv.Key.Value, v1.Value));
                list40_r.Add(string.Format("{0}\t{1}", kv.Key.Value, v2.Value));
            }
            var vv2 = list40_r.Intersect(list40_s);

            foreach (var item in list60)
            {
                KeyValuePair<Word, List<Word>> kv = dict3.ElementAt(item);

                var v1 = kv.Value.OrderByDescending(t => graph.Degree(t)).First();
                var v2 = kv.Value.OrderBy(t => graph.Degree(t)).First();

                list60_s.Add(string.Format("{0}\t{1}", kv.Key.Value, v1.Value));
                list60_r.Add(string.Format("{0}\t{1}", kv.Key.Value, v2.Value));
            }

            var vv3 = list60_r.Intersect(list60_s);

            foreach (var item in list80)
            {
                KeyValuePair<Word, List<Word>> kv = dict3.ElementAt(item);

                var v1 = kv.Value.OrderByDescending(t => graph.Degree(t)).First();
                var v2 = kv.Value.OrderBy(t => graph.Degree(t)).First();

                list80_s.Add(string.Format("{0}\t{1}", kv.Key.Value, v1.Value));
                list80_r.Add(string.Format("{0}\t{1}", kv.Key.Value, v2.Value));
            }

            var vv4 = list80_r.Intersect(list80_s);

            foreach (var item in list100)
            {
                KeyValuePair<Word, List<Word>> kv = dict3.ElementAt(item);

                var v1 = kv.Value.OrderByDescending(t => graph.Degree(t)).First();
                var v2 = kv.Value.OrderBy(t => graph.Degree(t)).First();

                list100_s.Add(string.Format("{0}\t{1}", kv.Key.Value, v1.Value));
                list100_r.Add(string.Format("{0}\t{1}", kv.Key.Value, v2.Value));
            }

            var vv5 = list100_r.Intersect(list100_s);


            string path = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "result");

            System.IO.File.WriteAllLines(System.IO.Path.Combine(path, "20_r.txt"), list20_r.ToArray());
            System.IO.File.WriteAllLines(System.IO.Path.Combine(path, "20_s.txt"), list20_s.ToArray());

            System.IO.File.WriteAllLines(System.IO.Path.Combine(path, "40_s.txt"), list40_s.ToArray());
            System.IO.File.WriteAllLines(System.IO.Path.Combine(path, "40_r.txt"), list40_r.ToArray());

            System.IO.File.WriteAllLines(System.IO.Path.Combine(path, "60_s.txt"), list60_s.ToArray());
            System.IO.File.WriteAllLines(System.IO.Path.Combine(path, "60_r.txt"), list60_r.ToArray());

            System.IO.File.WriteAllLines(System.IO.Path.Combine(path, "80_s.txt"), list80_s.ToArray());
            System.IO.File.WriteAllLines(System.IO.Path.Combine(path, "80_r.txt"), list80_r.ToArray());

            System.IO.File.WriteAllLines(System.IO.Path.Combine(path, "100_s.txt"), list100_s.ToArray());
            System.IO.File.WriteAllLines(System.IO.Path.Combine(path, "100_r.txt"), list100_r.ToArray());
            //--------------------




            this.graphLayout.Tag = tempList;
            int uCount2 = bidirectedPairsAll.Select(t => t.Key).Distinct().Count();
            int kCount2 = bidirectedPairsAll.Select(t => t.Value).Distinct().Count();

            sb.AppendLine(string.Format("Pairs:{0}, U Distinct:{1} K Distinct{2} Distinct Pairs:{3}", bidirectedPairsAll.Count(), uCount2, kCount2, distinctPair));

            this.textBox_Output.Text = sb.ToString();
            MessageBox.Show(string.Format("Done in {0} secs!",(new TimeSpan(DateTime.Now.Ticks-tick)).TotalSeconds));
        }

        private List<int> randomListGenerator(int count, int length)
        {
            List<int> resutl = new List<int>();

            System.Threading.Thread.Sleep(1000);
            Random random = new Random();
            for (int i = 0; i < count; i++)
            {
                int t = -1;
                do
                {
                    t = random.Next(0, length);
                } while (resutl.Contains(t));
                resutl.Add(t);
            }

            return resutl;
        }

        private void buttonRandomConnectedComponents_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConnectedComponets == null)
            {
                MessageBox.Show("Not yet created!");
                return;
            }
            int index = 0;

            do
            {
                index = (new Random()).Next(0, this.ConnectedComponets.Count);
            } while (this.ConnectedComponets[index].VertexCount > 100);

            IList<KeyValuePair<Word, Word>> bidirectedList = this.getBidirectedPairs(this.ConnectedComponets[index]);

            StringBuilder sb = new StringBuilder();
            foreach (var item in bidirectedList)
            {
                sb.AppendLine(string.Format("{0} <--> {1}", item.Key, item.Value));
            }

            this.textBox_Output.Text = sb.ToString();

            //display graph
            BidirectionalGraph<object, IEdge<object>> temp = new BidirectionalGraph<object, IEdge<object>>();
            Dictionary<Word, Label> dict = new Dictionary<Word, Label>();
            Brush blue = new SolidColorBrush(Colors.Blue);
            Brush green = new SolidColorBrush(Colors.Green);
            foreach (var item in this.ConnectedComponets[index].Vertices)
            {
                Label l = new Label();
                l.Width = double.NaN;
                l.Height = double.NaN;
                l.Foreground = item.Language == Console.Language.Uyghur ? blue : green;
                l.Content = item;
                temp.AddVertex(l);
                dict.Add(item, l);
            }

            foreach (var item in this.ConnectedComponets[index].Edges)
            {
                temp.AddEdge(new Edge<object>(dict[item.Source], dict[item.Target]));
            }

            this.graphLayout.Graph = temp;
            this.graphLayout.Relayout();
        }

        private void buttonRandomConnectedComponents_bi_Click(object sender, RoutedEventArgs e)
        {
            if (this.ConnectedComponets == null)
            {
                MessageBox.Show("Not yet created!");
                return;
            }
            int index = 0;

            do
            {
                index = (new Random()).Next(0, (this.graphLayout.Tag as List<int>).Count);
            } while (this.ConnectedComponets[(this.graphLayout.Tag as List<int>)[index]].VertexCount > 100);

            IList<KeyValuePair<Word, Word>> bidirectedList = this.getBidirectedPairs(this.ConnectedComponets[(this.graphLayout.Tag as List<int>)[index]]);

            StringBuilder sb = new StringBuilder();
            foreach (var item in bidirectedList)
            {
                sb.AppendLine(string.Format("{0} <--> {1}", item.Key, item.Value));
            }

            this.textBox_Output.Text = sb.ToString();

            //add to graph
            BidirectionalGraph<object, IEdge<object>> temp = new BidirectionalGraph<object, IEdge<object>>();
            Dictionary<Word, Label> dict = new Dictionary<Word, Label>();
            Dictionary<Word, Label> dict2 = new Dictionary<Word, Label>();
            Brush blue = new SolidColorBrush(Colors.Blue);
            Brush green = new SolidColorBrush(Colors.Green);
            foreach (var item in this.ConnectedComponets[(this.graphLayout.Tag as List<int>)[index]].Vertices)
            {
                Label l = new Label();
                l.Width = double.NaN;
                l.Height = double.NaN;
                l.Foreground = item.Language == Console.Language.Uyghur ? blue : green;
                l.Content = item;
                temp.AddVertex(l);
                dict.Add(item, l);
            }

            foreach (var item in this.ConnectedComponets[(this.graphLayout.Tag as List<int>)[index]].Edges)
            {
                temp.AddEdge(new Edge<object>(dict[item.Source], dict[item.Target]));
            }

            //add bidirected part to graph
           
            List<Word> allWords = bidirectedList.Select(t=>t.Key).ToList();
            allWords.AddRange(bidirectedList.Select(t=>t.Value).ToList());
            foreach (var item in allWords)
	        {
                if(dict2.ContainsKey(item))
                    continue;
		        Label l = new Label();
                l.Width = double.NaN;
                l.Height = double.NaN;
                l.Foreground = item.Language == Console.Language.Uyghur ? blue : green;
                l.Content = item;
                temp.AddVertex(l);
                dict2.Add(item, l);
	        }
            foreach (var item in bidirectedList)
	        {
		        temp.AddEdge(new Edge<object>(dict2[item.Key],dict2[item.Value]));
                temp.AddEdge(new Edge<object>(dict2[item.Value], dict2[item.Key]));
	        }


            this.graphLayout.Graph = temp;
            this.graphLayout.Relayout();
        }

        private void displayConnectedComponent(BidirectionalGraph<Word, IEdge<Word>> graph)
        {
            BidirectionalGraph<object, IEdge<object>> temp = new BidirectionalGraph<object, IEdge<object>>();
            Dictionary<Word, Label> dict = new Dictionary<Word, Label>();
            Dictionary<Word, Label> dict2 = new Dictionary<Word, Label>();
            Brush blue = new SolidColorBrush(Colors.Blue);
            Brush green = new SolidColorBrush(Colors.Green);
            foreach (var item in graph.Vertices)
            {
                Label l = new Label();
                l.Width = double.NaN;
                l.Height = double.NaN;
                l.Foreground = item.Language == Console.Language.Uyghur ? blue : green;
                l.Content = item;
                temp.AddVertex(l);
                dict.Add(item, l);
            }

            foreach (var item in graph.Edges)
            {
                temp.AddEdge(new Edge<object>(dict[item.Source], dict[item.Target]));
            }

            this.graphLayout.Graph = temp;
            this.graphLayout.Relayout();
        }

        private void button_SaveBidrectionalPairs_Click(object sender, RoutedEventArgs e)
        {

            //Find Bidirected Pairs from All subgraphs
            List<KeyValuePair<Word, Word>> distinctBidirectedPairs = new List<KeyValuePair<Word, Word>>();

            int distinctPair = 0;

            for (int t = 0; t < this.ConnectedComponets.Count; t++)
            {
                BidirectionalGraph<Word, Edge<Word>> item = this.ConnectedComponets[t];
                IList<KeyValuePair<Word, Word>> b = this.getBidirectedPairs(item);
                foreach (var item2 in b)
                {
                    bool ok = true;
                    foreach (var item3 in b)
                    {

                        if (item2.Key == item3.Key && item2.Value == item3.Value)
                            continue;
                        if (item2.Key == item3.Key || item2.Value == item3.Value)
                        {
                            ok = false;
                            break;
                        }
                    }
                    if (ok)
                        distinctBidirectedPairs.Add(item2);
                }
            }


            String fileName = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bi_pairs_nextStep.txt");
            System.IO.File.WriteAllLines(fileName, distinctBidirectedPairs.Select(t => t.Key.Value +"\t"+ t.Value.Value).ToArray());
            this.textBox_Output.Clear();
            this.textBox_Output.AppendText(string.Format("Distinct Pairs:{0}", distinctBidirectedPairs.Count));
            this.textBox_Output.AppendText(Environment.NewLine);
            this.textBox_Output.AppendText(fileName);
            MessageBox.Show("Done!");
        }

        private void button_GenerageFinalResult_Click(object sender, RoutedEventArgs e)
        {
            button_GenerageFinalResultForGraphClustering_Click();
            return;
            bool removeWeakPairs = false;
            int iter = 1;
            long tick = DateTime.Now.Ticks;
            DBHelper helper = new DBHelper();
            bool allowNext = true;

            //List<Dictionary<float, int[]>> listHStatisticsUtoK = new List<Dictionary<float, int[]>>();
            //List<Dictionary<float, int[]>> listHStatisticsKtoU = new List<Dictionary<float, int[]>>();
           
            //for heuristic value distribution statistics
            Dictionary<float, int[]> HStatisticsUtoK = new Dictionary<float, int[]>();
            Dictionary<float, int[]> HStatisticsKtoU = new Dictionary<float, int[]>();
            for (int i = 0; i <= 100; i++)
            {
                HStatisticsUtoK.Add(helper.Round((float)i / 100), new int[4] { 0, 0, 0, 0 });
                HStatisticsKtoU.Add(helper.Round((float)i / 100), new int[4] { 0, 0, 0, 0 });
            }
            Dictionary<int, List<KeyValuePair<string, string>>> qualityDistribution = new Dictionary<int, List<KeyValuePair<string, string>>>();
            for (int i = 1; i <= 10; i++)
                qualityDistribution.Add(i, new List<KeyValuePair<string, string>>());
            
            System.IO.File.AppendAllLines(System.IO.Path.Combine(helper.AutoResultfolder(),"stats.txt"), new string[] { string.Format("{0}\t{1}\t{2}", "Iteration", "Strong Pairs", "Weak Pairs") });
            
            Dictionary<int, KeyValuePair<List<KeyValuePair<Word, Word>>, List<KeyValuePair<Word, Word>>>>
                result = new Dictionary<int,KeyValuePair<List<KeyValuePair<Word, Word>>, List<KeyValuePair<Word, Word>>>>();

            do
            {
                if (iter == 1)
                    ParameterProvider.RUNTIME_PARAMS = ParameterProvider.NORMAL_PARAMS;
                else
                    ParameterProvider.RUNTIME_PARAMS = ParameterProvider.NORMAL_PARAMS;

                KeyValuePair<BidirectionalGraph<Word, Edge<Word>>, Dictionary<string, KeyValuePair<float, float[]>>> graph = helper.GetFullBidirectionalBestCandidatesGraph();
                List<BidirectionalGraph<Word, Edge<Word>>> conComponents = getConnectedComponents(graph.Key);

                List<KeyValuePair<Word, Word>> pairsStrong = new List<KeyValuePair<Word, Word>>();
                List<KeyValuePair<Word, Word>> pairsWeak = new List<KeyValuePair<Word, Word>>();


                for (int i = 0; i < conComponents.Count; i++)
                {
                    BidirectionalGraph<Word, Edge<Word>> component = conComponents[i];
                    List<KeyValuePair<Word, Word>> pairs = this.getBidirectedPairs(component);

                    if (pairs.Count > 0)
                    {
                        foreach (var kv in pairs)
                        {
                            bool isStrong = IsStrongPair(kv, pairs);
                            if (isStrong)
                            {
                                var hValuesUK = graph.Value[helper.EdgeIdentifier(new Edge<Word>(kv.Key, kv.Value))];
                                var hValuesKU = graph.Value[helper.EdgeIdentifier(new Edge<Word>(kv.Value, kv.Key))];
                                var average = (hValuesUK.Key + hValuesKU.Key) / 2;

                                if (iter == 1)
                                    if (average < 0.5f)
                                        continue;
                                if (iter == 2)
                                    if (average < 0.4f)
                                        continue;
                                if (iter == 3)
                                    if (average < 0.3f)
                                        continue;
                                if (iter == 4)
                                    if (average < 0.2f)
                                        continue;
                                if (iter == 5)
                                    if (average < 0.1f)
                                        continue;

                                pairsStrong.Add(kv);
                            }
                            else
                                pairsWeak.Add(kv);
                        }
                    }
                }

                if (pairsStrong.Count == 0)
                {
                    allowNext = false;
                }
                else
                {
                    result.Add(iter, new KeyValuePair<List<KeyValuePair<Word, Word>>, List<KeyValuePair<Word, Word>>>(pairsStrong, pairsWeak));
                  
                    //write to file
                    string fileNameStrong = System.IO.Path.Combine(helper.AutoResultfolder(), string.Format("iter_{0}_pairs_strong.txt", iter));
                    string fileNameWeak = System.IO.Path.Combine(helper.AutoResultfolder(), string.Format("iter_{0}_pairs_weak.txt", iter));

                    string[] strong = pairsStrong.Select(t => string.Format("{0}\t{1}\t{2}\t{3}", 
                        t.Key.Value, 
                        t.Value.Value, 
                        graph.Value[helper.EdgeIdentifier(new Edge<Word>(t.Key, t.Value))].Key, 
                        graph.Value[helper.EdgeIdentifier(new Edge<Word>(t.Value, t.Key))].Key)
                        ).ToArray();
                    
                    string[] weak = pairsWeak.Select(t => string.Format("{0}\t{1}\t{2}\t{3}", 
                        t.Key.Value, 
                        t.Value.Value, 
                        graph.Value[helper.EdgeIdentifier(new Edge<Word>(t.Key, t.Value))].Key, 
                        graph.Value[helper.EdgeIdentifier(new Edge<Word>(t.Value, t.Key))].Key)
                        ).ToArray();


                    System.IO.File.WriteAllLines(fileNameStrong, strong);
                    System.IO.File.WriteAllLines(fileNameWeak, weak);
                    System.IO.File.AppendAllLines(System.IO.Path.Combine(helper.AutoResultfolder(), "stats.txt"), new string[] { string.Format("{0}\t{1}\t{2}", iter, strong.Length, weak.Length) });


                    //Create Heuristic value distribution  Statistics
                    foreach (var item in pairsStrong)
                    {
                        var hValuesUK = graph.Value[helper.EdgeIdentifier(new Edge<Word>(item.Key, item.Value))];
                        HStatisticsUtoK[helper.Round(hValuesUK.Value[0])][0] += 1;
                        HStatisticsUtoK[helper.Round(hValuesUK.Value[1])][1] += 1;
                        HStatisticsUtoK[helper.Round(hValuesUK.Value[2])][2] += 1;
                        HStatisticsUtoK[helper.Round(hValuesUK.Key)][3] += 1;
                    }

                    foreach (var item in pairsStrong)
                    {
                        var hValuesKU = graph.Value[helper.EdgeIdentifier(new Edge<Word>(item.Value, item.Key))];
                        HStatisticsKtoU[helper.Round(hValuesKU.Value[0])][0] += 1;
                        HStatisticsKtoU[helper.Round(hValuesKU.Value[1])][1] += 1;
                        HStatisticsKtoU[helper.Round(hValuesKU.Value[2])][2] += 1;
                        HStatisticsKtoU[helper.Round(hValuesKU.Key)][3] += 1;
                    }


                    foreach (var item in pairsStrong)
                    {
                        var hValuesUK = graph.Value[helper.EdgeIdentifier(new Edge<Word>(item.Key, item.Value))];
                        var hValuesKU = graph.Value[helper.EdgeIdentifier(new Edge<Word>(item.Value, item.Key))];
                        var average = (hValuesUK.Key + hValuesKU.Key) / 2;
                        int rangeID = -1;
                        if (average < 0.1f)
                            rangeID = 1;
                        else if (average < 0.2f && average >= 0.1f)
                            rangeID = 2;
                        else if (average < 0.3f && average >= 0.2f)
                            rangeID = 3;
                        else if (average < 0.4f && average >= 0.3f)
                            rangeID = 4;
                        else if (average < 0.5f && average >= 0.4f)
                            rangeID = 5;
                        else if (average < 0.6f && average >= 0.5f)
                            rangeID = 6;
                        else if (average < 0.7f && average >= 0.6f)
                            rangeID = 7;
                        else if (average < 0.8f && average >= 0.7f)
                            rangeID = 8;
                        else if (average < 0.9f && average >= 0.8f)
                            rangeID = 9;
                        else
                            rangeID = 10;

                        qualityDistribution[rangeID].Add(new KeyValuePair<string, string>(item.Key.Value, item.Value.Value));
                    }


                    //reload database
                    List<string> filterUWords = new List<string>();
                    List<string> filterKWords = new List<string>();
                    foreach (var item in result.Values)  //collect U/K words in strong paris from previous iterations
                    {
                        filterUWords.AddRange(item.Key.Select(t => t.Key.Value));
                        filterKWords.AddRange(item.Key.Select(t => t.Value.Value));
                    }

                    if (removeWeakPairs)
                    {
                        foreach (var item in result.Values)  //collect U/K words in weak paris from previous iterations
                        {
                            filterUWords.AddRange(item.Value.Select(t => t.Key.Value).Distinct());
                            filterKWords.AddRange(item.Value.Select(t => t.Value.Value).Distinct());
                        }
                    }
                    DBHelper.GenerateDatabase(filterUWords, filterKWords);
                    helper = new DBHelper();

                    pairsStrong = new List<KeyValuePair<Word, Word>>();
                    pairsWeak = new List<KeyValuePair<Word, Word>>();

                    iter++;
                }
            } while (allowNext);


            //Check overlap
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < result.Count-1; i++)
            {
                List<KeyValuePair<Word, Word>> l1 = result[i+1].Value;
                List<KeyValuePair<Word, Word>> l2 = result[i+2].Value;
                List<KeyValuePair<Word, Word>> v = l1.Intersect(l2, new KVComparer()).ToList();

                sb.AppendLine(string.Format("Overlap between {0} and {1}: {2}.",i+1,i+2,v.Count()));
            }

            var vOverall = result[1].Value;
            for (int i = 1; i < result.Count; i++)
            {
                var intersect = vOverall.Intersect(result[i+1].Value, new KVComparer()).ToList();
                vOverall = intersect;
            }
            sb.AppendLine(string.Format("Common Overlap: {0}", vOverall.Count()));
            System.IO.File.AppendAllLines(System.IO.Path.Combine(helper.AutoResultfolder(), "stats.txt"), new string[] { sb.ToString() });
            string[] weakOverlap = vOverall.Select(t => string.Format("{0}\t{1}", t.Key.Value, t.Value.Value)).ToArray();
            System.IO.File.WriteAllLines(System.IO.Path.Combine(helper.AutoResultfolder(),"weak_overlaps.txt"), weakOverlap);
            System.IO.File.AppendAllText(System.IO.Path.Combine(helper.AutoResultfolder(), "hstats.txt"), this.getHeuristicDistribution(HStatisticsUtoK, HStatisticsKtoU));
            writeQualityDistribution(qualityDistribution);
            MessageBox.Show(string.Format("Done in {0} secs!", (new TimeSpan(DateTime.Now.Ticks - tick)).TotalSeconds));
        }

        private void button_GenerageFinalResultForGraphClustering_Click()
        {


            bool removeWeakPairs = false;
            int iter = 1;
            long tick = DateTime.Now.Ticks;
            DBHelper helper = new DBHelper();
            bool allowNext = true;

       

            //List<Dictionary<float, int[]>> listHStatisticsUtoK = new List<Dictionary<float, int[]>>();
            //List<Dictionary<float, int[]>> listHStatisticsKtoU = new List<Dictionary<float, int[]>>();

            //for heuristic value distribution statistics
            Dictionary<float, int[]> HStatisticsUtoK = new Dictionary<float, int[]>();
            Dictionary<float, int[]> HStatisticsKtoU = new Dictionary<float, int[]>();
            for (int i = 0; i <= 100; i++)
            {
                HStatisticsUtoK.Add(helper.Round((float)i / 100), new int[4] { 0, 0, 0, 0 });
                HStatisticsKtoU.Add(helper.Round((float)i / 100), new int[4] { 0, 0, 0, 0 });
            }
            Dictionary<int, List<KeyValuePair<string, string>>> qualityDistribution = new Dictionary<int, List<KeyValuePair<string, string>>>();
            for (int i = 1; i <= 10; i++)
                qualityDistribution.Add(i, new List<KeyValuePair<string, string>>());

            System.IO.File.AppendAllLines(System.IO.Path.Combine(helper.AutoResultfolder(), "stats.txt"), new string[] { string.Format("{0}\t{1}\t{2}", "Iteration", "Strong Pairs", "Weak Pairs") });

            Dictionary<int, KeyValuePair<List<KeyValuePair<Word, Word>>, List<KeyValuePair<Word, Word>>>>
                result = new Dictionary<int, KeyValuePair<List<KeyValuePair<Word, Word>>, List<KeyValuePair<Word, Word>>>>();

            StringBuilder buffer = new StringBuilder();
            do
            {

                if (iter == 12)
                {
                    //var bigGraph = v2[0];
                    //DBHelper.GenerateDatabase(bigGraph.Vertices.Where(t=>t.Language == Console.Language.Uyghur).Select(t=>t.Value).ToList(),
                    //   bigGraph.Vertices.Where(t => t.Language == Console.Language.Kazak).Select(t=>t.Value).ToList());
                    //var v3 = helper.GetDatabaseConnectedComponents().OrderByDescending(t => t.VertexCount).ToList();

                    //StringBuilder buffer = new StringBuilder();
                    //Dictionary<Word, int> dd = new Dictionary<Word, int>();
                    //foreach (var item in v2[0].Edges)
                    //{
                    //    if (!dd.ContainsKey(item.Source))
                    //        dd.Add(item.Source, dd.Count);
                    //    if (!dd.ContainsKey(item.Target))
                    //        dd.Add(item.Target, dd.Count);
                    //    buffer.AppendLine(string.Format("{0}\t{1}", dd[item.Source], dd[item.Target]));
                    //}

                    //System.IO.File.WriteAllText(@"d:\exchange.edges", buffer.ToString());
                }

                if (iter == 1)
                    ParameterProvider.RUNTIME_PARAMS = ParameterProvider.NORMAL_PARAMS;
                else
                    ParameterProvider.RUNTIME_PARAMS = ParameterProvider.NORMAL_PARAMS;

                KeyValuePair<BidirectionalGraph<Word, Edge<Word>>, Dictionary<string, KeyValuePair<float, float[]>>> graph = helper.GetFullBidirectionalBestCandidatesGraph();
                List<BidirectionalGraph<Word, Edge<Word>>> conComponents = getConnectedComponents(graph.Key);

                List<KeyValuePair<Word, Word>> pairsStrong = new List<KeyValuePair<Word, Word>>();
                List<KeyValuePair<Word, Word>> pairsWeak = new List<KeyValuePair<Word, Word>>();


                for (int i = 0; i < conComponents.Count; i++)
                {
                    BidirectionalGraph<Word, Edge<Word>> component = conComponents[i];
                    List<KeyValuePair<Word, Word>> pairs = this.getBidirectedPairs(component);

                    if (pairs.Count > 0)
                    {
                        foreach (var kv in pairs)
                        {
                            bool isStrong = IsStrongPair(kv, pairs);                            
                            var hValuesUK = graph.Value[helper.EdgeIdentifier(new Edge<Word>(kv.Key, kv.Value))];
                            var hValuesKU = graph.Value[helper.EdgeIdentifier(new Edge<Word>(kv.Value, kv.Key))];
                            var average = (hValuesUK.Key + hValuesKU.Key) / 2;
                            if (isStrong)
                            {
                                //if (iter == 1)
                                //    if (average < 0.7f)
                                //        continue;
                                //if (iter == 2)
                                //    if (average < 0.5f)
                                //        continue;
                                //if (iter == 3)
                                //    if (average < 0.3f)
                                //        continue;
                                //if (iter == 4)
                                //    if (average < 0.2f)
                                //        continue;
                                //    else

                                //if (average < 0.5f)
                                //    continue;
                                pairsStrong.Add(kv);
                            }
                            else
                            {
                                //if (average < 0.5f)
                                //    continue;
                                pairsWeak.Add(kv);
                            }
                        }
                    }
                }

                var v1 = helper.GetDatabaseConnectedComponents().OrderByDescending(t => t.VertexCount).ToList();
                var bigGraph = v1[0];
                var bigGraphCopy = DBHelper.CloneGraph(bigGraph);
                int originalSize = bigGraph.VertexCount;
                foreach (var item in pairsStrong)
                {
                    bigGraphCopy.RemoveVertex(item.Key);
                    bigGraphCopy.RemoveVertex(item.Value);
                }
                buffer.AppendLine(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", iter - 1, bigGraph.VertexCount, bigGraph.EdgeCount, ((bigGraph.VertexCount - bigGraphCopy.VertexCount) / 2), v1.Count-1));

                if (pairsStrong.Count == 0)
                {
                    allowNext = false;
                }
                else
                {
                    result.Add(iter, new KeyValuePair<List<KeyValuePair<Word, Word>>, List<KeyValuePair<Word, Word>>>(pairsStrong, pairsWeak));
                    //write to file
                    string fileNameStrong = System.IO.Path.Combine(helper.AutoResultfolder(), string.Format("iter_{0}_pairs_strong.txt", iter));
                    string fileNameWeak = System.IO.Path.Combine(helper.AutoResultfolder(), string.Format("iter_{0}_pairs_weak.txt", iter));

                    string[] strong = pairsStrong.Select(t => string.Format("{0}\t{1}\t{2}\t{3}",
                        t.Key.Value,
                        t.Value.Value,
                        graph.Value[helper.EdgeIdentifier(new Edge<Word>(t.Key, t.Value))].Key,
                        graph.Value[helper.EdgeIdentifier(new Edge<Word>(t.Value, t.Key))].Key)
                        ).ToArray();

                    string[] weak = pairsWeak.Select(t => string.Format("{0}\t{1}\t{2}\t{3}",
                        t.Key.Value,
                        t.Value.Value,
                        graph.Value[helper.EdgeIdentifier(new Edge<Word>(t.Key, t.Value))].Key,
                        graph.Value[helper.EdgeIdentifier(new Edge<Word>(t.Value, t.Key))].Key)
                        ).ToArray();


                    System.IO.File.WriteAllLines(fileNameStrong, strong);
                    System.IO.File.WriteAllLines(fileNameWeak, weak);
                    System.IO.File.AppendAllLines(System.IO.Path.Combine(helper.AutoResultfolder(), "stats.txt"), new string[] { string.Format("{0}\t{1}\t{2}", iter, strong.Length, weak.Length) });


                    //Create Heuristic value distribution  Statistics
                    foreach (var item in pairsStrong)
                    {
                        var hValuesUK = graph.Value[helper.EdgeIdentifier(new Edge<Word>(item.Key, item.Value))];
                        HStatisticsUtoK[helper.Round(hValuesUK.Value[0])][0] += 1;
                        HStatisticsUtoK[helper.Round(hValuesUK.Value[1])][1] += 1;
                        HStatisticsUtoK[helper.Round(hValuesUK.Value[2])][2] += 1;
                        HStatisticsUtoK[helper.Round(hValuesUK.Key)][3] += 1;
                    }

                    foreach (var item in pairsStrong)
                    {
                        var hValuesKU = graph.Value[helper.EdgeIdentifier(new Edge<Word>(item.Value, item.Key))];
                        HStatisticsKtoU[helper.Round(hValuesKU.Value[0])][0] += 1;
                        HStatisticsKtoU[helper.Round(hValuesKU.Value[1])][1] += 1;
                        HStatisticsKtoU[helper.Round(hValuesKU.Value[2])][2] += 1;
                        HStatisticsKtoU[helper.Round(hValuesKU.Key)][3] += 1;
                    }


                    foreach (var item in pairsStrong)
                    {
                        var hValuesUK = graph.Value[helper.EdgeIdentifier(new Edge<Word>(item.Key, item.Value))];
                        var hValuesKU = graph.Value[helper.EdgeIdentifier(new Edge<Word>(item.Value, item.Key))];
                        var average = (hValuesUK.Key + hValuesKU.Key) / 2;
                        int rangeID = -1;
                        if (average < 0.1f)
                            rangeID = 1;
                        else if (average < 0.2f && average >= 0.1f)
                            rangeID = 2;
                        else if (average < 0.3f && average >= 0.2f)
                            rangeID = 3;
                        else if (average < 0.4f && average >= 0.3f)
                            rangeID = 4;
                        else if (average < 0.5f && average >= 0.4f)
                            rangeID = 5;
                        else if (average < 0.6f && average >= 0.5f)
                            rangeID = 6;
                        else if (average < 0.7f && average >= 0.6f)
                            rangeID = 7;
                        else if (average < 0.8f && average >= 0.7f)
                            rangeID = 8;
                        else if (average < 0.9f && average >= 0.8f)
                            rangeID = 9;
                        else
                            rangeID = 10;
                        qualityDistribution[rangeID].Add(new KeyValuePair<string, string>(item.Key.Value, item.Value.Value));
                    }


                    //reload database
                    List<string> filterUWords = new List<string>();
                    List<string> filterKWords = new List<string>();
                    foreach (var item in result.Values)  //collect U/K words in strong paris from previous iterations
                    {
                        filterUWords.AddRange(item.Key.Select(t => t.Key.Value));
                        filterKWords.AddRange(item.Key.Select(t => t.Value.Value));
                    }

                    if (removeWeakPairs)
                    {
                        foreach (var item in result.Values)  //collect U/K words in weak paris from previous iterations
                        {
                            filterUWords.AddRange(item.Value.Select(t => t.Key.Value).Distinct());
                            filterKWords.AddRange(item.Value.Select(t => t.Value.Value).Distinct());
                        }
                    }
                    DBHelper.GenerateDatabase(filterUWords, filterKWords);
                    helper = new DBHelper();

                    pairsStrong = new List<KeyValuePair<Word, Word>>();
                    pairsWeak = new List<KeyValuePair<Word, Word>>();

                    iter++;
                }
            } while (allowNext);

            System.Diagnostics.Debug.Write(buffer.ToString());
            return;

            //Check overlap
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < result.Count - 1; i++)
            {
                List<KeyValuePair<Word, Word>> l1 = result[i + 1].Value;
                List<KeyValuePair<Word, Word>> l2 = result[i + 2].Value;
                List<KeyValuePair<Word, Word>> v = l1.Intersect(l2, new KVComparer()).ToList();

                sb.AppendLine(string.Format("Overlap between {0} and {1}: {2}.", i + 1, i + 2, v.Count()));
            }

            var vOverall = result[1].Value;
            for (int i = 1; i < result.Count; i++)
            {
                var intersect = vOverall.Intersect(result[i + 1].Value, new KVComparer()).ToList();
                vOverall = intersect;
            }
            sb.AppendLine(string.Format("Common Overlap: {0}", vOverall.Count()));
            System.IO.File.AppendAllLines(System.IO.Path.Combine(helper.AutoResultfolder(), "stats.txt"), new string[] { sb.ToString() });
            string[] weakOverlap = vOverall.Select(t => string.Format("{0}\t{1}", t.Key.Value, t.Value.Value)).ToArray();
            System.IO.File.WriteAllLines(System.IO.Path.Combine(helper.AutoResultfolder(), "weak_overlaps.txt"), weakOverlap);
            System.IO.File.AppendAllText(System.IO.Path.Combine(helper.AutoResultfolder(), "hstats.txt"), this.getHeuristicDistribution(HStatisticsUtoK, HStatisticsKtoU));
            writeQualityDistribution(qualityDistribution);
            MessageBox.Show(string.Format("Done in {0} secs!", (new TimeSpan(DateTime.Now.Ticks - tick)).TotalSeconds));
        }

        private void writeQualityDistribution(Dictionary<int, List<KeyValuePair<string, string>>> qualityDistribution)
        {
            DBHelper helper = new DBHelper();
            foreach (var item in qualityDistribution)
            {
                string fileName = System.IO.Path.Combine(helper.AutoResultfolder(), string.Format("HValueRange-{0}.txt", item.Key));
                System.IO.File.WriteAllLines(fileName, item.Value.Select(t => t.Key + "\t" + t.Value).ToArray());
            }
        }

        private string getHeuristicDistribution(Dictionary<float, int[]> uk, Dictionary<float, int[]> ku)
        {
            StringBuilder result  = new StringBuilder();

                result.AppendLine("Uyghur to Kazak");
                result.AppendLine("Value\tCount");
                foreach (var item in uk)
                {
                    result.AppendLine(string.Format("{0}\t{1}",item.Key.ToString("0.00"),item.Value[3]));
                }

                result.AppendLine("Kazak to Uyghur ==");
                result.AppendLine("Value\tCount");
                foreach (var item in ku)
                {
                    result.AppendLine(string.Format("{0}\t{1}", item.Key.ToString("0.00"), item.Value[3]));
                }
                result.AppendLine(Environment.NewLine);

            return result.ToString();
        }

        private void button1_Click_2(object sender, RoutedEventArgs e)
        {
            (new HumanEvalivationWindow()).Show();
        }


        private float calculateSim(int start, int end, string[] uWords, string[] kWords)
        {
            float sum = 0;
            for (int i = start; i < end; i++)
            {
                string u = uWords[i];
                float maxSim = kWords.Max(t => DBHelper.iLD(u, t));
                sum += maxSim;
            }
            return sum;
        }

        static float ss = 0;

        private void button_AverageSimilarity_Click(object sender, RoutedEventArgs e)
        {
            

            int sum1 = 0, sum2 = 0;
            foreach (var item in DBHelper.MyDictBase.CUDictbase.CtoU)
            {
                sum1 += item.Value.Length;   
            }
            foreach (var item in DBHelper.MyDictBase.CKDictbase.CtoK)
            {
                sum2 += item.Value.Length;
            }

            MessageBox.Show(string.Format("{0} - {1}",sum1,sum2));

            return;


            MessageBoxResult result =  MessageBox.Show("It will take more than a hour to finish. Continue? ", "Coution", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes)
                return;


            string[] uWords = (from u in DBHelper.MyDictBase.UWords
                               let uky = DBHelper.Syn.getUKYFromUy(u)
                               select uky).ToArray();

            string[] kWords = (from k in DBHelper.MyDictBase.KWords
                               let uky = DBHelper.Syn.getLtFromKz(k)
                               select uky).ToArray();



            Parallel.ForEach(uWords, u => {

                float maxSim = kWords.Max(t => DBHelper.iLD(u, t));
                ss+=maxSim;
            });

            MessageBox.Show((ss / uWords.Length).ToString());
        }

        private void textBox_CurrentUWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = this.textBox_CurrentUWord.Text;
            int index = -1;
            for (int i = 0; i < this.listBox_AllUWords.Items.Count; i++)
            {
                if (((Word)this.listBox_AllUWords.Items[i]).Value == keyword)
                    index = i;
            }
            if (index > -1)
            {
                this.listBox_AllUWords.SelectedIndex = index;
                this.listBox_AllUWords.ScrollIntoView(this.listBox_AllUWords.SelectedItem);
            }
        }

        private void textBox_CurrentKWord_TextChanged(object sender, TextChangedEventArgs e)
        {
            string keyword = this.textBox_CurrentKWord.Text;
            int index = -1;
            for (int i = 0; i < this.listBox_KCandidates.Items.Count; i++)
            {
                if (((Word)this.listBox_KCandidates.Items[i]).Value == keyword)
                    index = i;
            }
            if (index > -1)
            {
                this.listBox_KCandidates.SelectedIndex = index;
                this.listBox_KCandidates.ScrollIntoView(this.listBox_KCandidates.SelectedItem);
            }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            BidirectionalGraph<object,IEdge<object>> graph = this.listWeekPairGraphs3[(new Random()).Next(this.listWeekPairGraphs3.Count())];
            this.graphLayout.Graph = graph;
            this.graphLayout.Relayout();
        }
    }
}
