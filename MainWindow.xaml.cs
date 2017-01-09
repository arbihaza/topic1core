using System;
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
using System.ComponentModel;
using QuickGraph;
using GraphSharp.Controls;
//using HDictInduction.Console.Resources;
using QuickGraph.Algorithms.ConnectedComponents;
using System.Diagnostics;

using System.Threading.Tasks;
//using yWorks.yFiles.Algorithms;
using HDictInduction.Console.SAT;


namespace HDictInduction.Console
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string ApplicationPath
        {
            get;
            set;
        }

        //private Dictionary<string, NodeControl> _nodeStore = null;

        //private IL.ILSolver3 _ilsolver = null;
        
        private IBidirectionalGraph<object, IEdge<object>> _graph = null;

        public MainWindow()
        {
            //Word u1 = new Word(1, "u1", Console.Language.Uyghur);
            //Word u2 = new Word(2, "u2", Console.Language.Uyghur);


            //Word k1 = new Word(1, "k1", Console.Language.Kazak);
            //Word k2 = new Word(2, "k2", Console.Language.Kazak);
            //Word k3 = new Word(3, "k3", Console.Language.Kazak);

            //List<KeyValuePair<Word, Word>> l1 = new List<KeyValuePair<Word, Word>>();
            //List<KeyValuePair<Word, Word>> l2 = new List<KeyValuePair<Word, Word>>();

            //l1.Add(new KeyValuePair<Word,Word>(u1,k1));
            //l1.Add(new KeyValuePair<Word, Word>(u1, k2));

            //l2.Add(new KeyValuePair<Word, Word>(u1, k1));
            //l2.Add(new KeyValuePair<Word, Word>(u2, k3));

            //var v = l1.Intersect(l2, new KVComparer());
            //return;



            //System.Windows.Forms.OpenFileDialog of = new System.Windows.Forms.OpenFileDialog();
            //of.ShowDialog();
            //string[] lines = System.IO.File.ReadAllLines(of.FileName);

            //Array.Sort(lines);

            //StringBuilder sb = new StringBuilder();
            //var v = lines.GroupBy(t => t);
            //foreach (var item in v)
            //{
            //    sb.AppendLine(item.First() + "\t" + item.Count());
            //}

            //System.IO.File.WriteAllText("temp.txt", sb.ToString());

            //_nodeStore = new Dictionary<string, NodeControl>();
            ApplicationPath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            InitializeComponent();
            this.InitializeComponent();

            GraphSharp.Algorithms.Layout.Simple.Hierarchical.EfficientSugiyamaLayoutParameters
    parameter = new GraphSharp.Algorithms.Layout.Simple.Hierarchical.EfficientSugiyamaLayoutParameters();
            parameter.LayerDistance = 100f;
            this.graphLayout.LayoutParameters = parameter;

            this.IsEnabled = false;
            cmbxLayoutAlgorithm.SelectedIndex = 7;
            BackgroundWorker worker = new BackgroundWorker();
            worker.DoWork += new DoWorkEventHandler(delegate
            {
                //DBHelper.GenerateDatabase(new List<string>(), new List<string>());
                //return;
                /*
                List<string> uFilter = new List<string>();
                List<string> kFilter = new List<string>();
                string[] lines1 = System.IO.File.ReadAllLines("HValueRange-9.txt");
                string[] lines2 = System.IO.File.ReadAllLines("HValueRange-10.txt");

                foreach (var item in lines1)
                {
                    uFilter.Add(item.Split('\t')[0]);
                    kFilter.Add(item.Split('\t')[1]);
                }

                foreach (var item in lines2)
                {
                    uFilter.Add(item.Split('\t')[0]);
                    kFilter.Add(item.Split('\t')[1]);
                }
                 */
                //foreach (var item in lines3)
                //{
                //    uFilter.Add(item.Split('\t')[0]);
                //    kFilter.Add(item.Split('\t')[1]);
                //}
                //DBHelper.GenerateDatabase(uFilter,kFilter);
                DBHelper.GenerateDatabase(new List<string>(), new List<string>());
                //DBHelper.GenerateDatabaseFull();
                
                
            });
            worker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(delegate
            {
                //this.listBoxAllCWords.ItemsSource = DBHelper.Database[0].AtoBdataset.Keys;
                this.listBoxAllCWords.ItemsSource = DBHelper.MyDictBase.CWords;
                this.IsEnabled = true;
            });

            worker.RunWorkerAsync();
            //this.tabControl1.IsEnabled = true;
            this.graphLayout.Graph = createSampleGraph2();
            this.graphLayout.Relayout();

       

        }

        private void updateGraphInfo(BidirectionalGraph<Word, Edge<Word>> graph)
        {
            this.label_Graph_Domain.Content = string.Format("|A word|={0}, |C word|={1}, |Pivot|={2}   Vertex Count={3}  Edge Count={4}",
                graph.Vertices.Where(t => t.Language == Console.Language.Uyghur).Count(),
                graph.Vertices.Where(t => t.Language == Console.Language.Kazak).Count(),
                graph.Vertices.Where(t => t.Language == Console.Language.Chinese).Count(),
                graph.Vertices.Count(),
                graph.Edges.Count());
        }

        /*private BidirectionalGraph<object, IEdge<object>> createSampleGraph()
        {
            _nodeStore.Clear();
            BidirectionalGraph<object, IEdge<object>> g = new BidirectionalGraph<object, IEdge<object>>();
            for (int i = 1; i <= 5; i++)
            {
                NodeControl nu = new NodeControl(string.Format("{0}{1}", "x", i), new Word(i, string.Format("{0}{1}", "x", i), HDictInduction.Console.Language.Uyghur));
                NodeControl nk = new NodeControl(string.Format("{0}{1}", "y", i), new Word(i, string.Format("{0}{1}", "y", i), HDictInduction.Console.Language.Kazak));


                _nodeStore.Add(nu.NodeMark, nu);
                _nodeStore.Add(nk.NodeMark, nk);

                g.AddVertex(nu);
                g.AddVertex(nk);

                if (i < 4)
                {
                    NodeControl nc = new NodeControl(string.Format("{0}{1}", "b", i), new Word(i, string.Format("{0}{1}", "b", i), HDictInduction.Console.Language.Chinese));
                    _nodeStore.Add(nc.NodeMark, nc);
                    g.AddVertex(nc);
                }
            }

            g.AddEdge(new Edge<object>(_nodeStore["c1"], _nodeStore["u1"]));
            g.AddEdge(new Edge<object>(_nodeStore["u1"], _nodeStore["c1"]));
            g.AddEdge(new Edge<object>(_nodeStore["c1"], _nodeStore["u2"]));
            g.AddEdge(new Edge<object>(_nodeStore["c1"], _nodeStore["u3"]));
            g.AddEdge(new Edge<object>(_nodeStore["c2"], _nodeStore["u3"]));
            g.AddEdge(new Edge<object>(_nodeStore["c2"], _nodeStore["u4"]));
            g.AddEdge(new Edge<object>(_nodeStore["c3"], _nodeStore["u5"]));

            g.AddEdge(new Edge<object>(_nodeStore["k1"], _nodeStore["c1"]));
            g.AddEdge(new Edge<object>(_nodeStore["k2"], _nodeStore["c1"]));
            g.AddEdge(new Edge<object>(_nodeStore["k3"], _nodeStore["c1"]));
            g.AddEdge(new Edge<object>(_nodeStore["k3"], _nodeStore["c2"]));
            g.AddEdge(new Edge<object>(_nodeStore["k4"], _nodeStore["c2"]));
            g.AddEdge(new Edge<object>(_nodeStore["k5"], _nodeStore["c3"]));

            //Button btn = new Button();
            //btn.Width = 20;
            //btn.Height = 50;
            //btn.Content = "hello";

            //g.AddVertex(btn);


            return g;
        }*/


        private BidirectionalGraph<object, IEdge<object>> createSampleGraph2()
        {

            BidirectionalGraph<object, IEdge<object>> g = new BidirectionalGraph<object, IEdge<object>>();

            g.AddVertex("u0");
            g.AddVertex("u1");
            g.AddVertex("u2");
            g.AddVertex("u3");
            g.AddVertex("u4");

            g.AddVertex("k0");
            g.AddVertex("k1");
            g.AddVertex("k2");
            g.AddVertex("k3");
            g.AddVertex("k4");

            g.AddEdge(new Edge<object>("u0", "k0"));
            g.AddEdge(new Edge<object>("k0", "u0"));
            g.AddEdge(new Edge<object>("u1", "k1"));
            g.AddEdge(new Edge<object>("k1", "u1"));
            g.AddEdge(new Edge<object>("k2", "u3"));
            g.AddEdge(new Edge<object>("u2", "k1"));
            g.AddEdge(new Edge<object>("u2", "k2"));

            g.AddEdge(new Edge<object>("k3", "u4"));
            g.AddEdge(new Edge<object>("u4", "k3"));
            g.AddEdge(new Edge<object>("u4", "k4"));

            g.AddEdge(new Edge<object>("k4", "u4"));



            return g;
        }

        private void button_SearchCWord_Click(object sender, RoutedEventArgs e)
        {
            int index = -1;
            index = this.listBoxAllCWords.Items.IndexOf(this.textBox_SearchCWord.Text.Trim());
            if (index > -1)
            {
                this.listBoxAllCWords.SelectedIndex = index;
                this.listBoxAllCWords.UpdateLayout();
            }
            else
            {
                MessageBox.Show("Not found!");
            }
        }

        private void listBoxAllCWords_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.listBoxAllCWords.SelectedIndex > -1)
            {
                string cWord = this.listBoxAllCWords.SelectedItem.ToString();
                IList<int> cWords = new DBHelper().GetSubGraph(cWord, DBHelper.Language.Chinese);
                this.listBox_LinkedCWords.ItemsSource = null;
                this.listBox_LinkedCWords.ItemsSource = DBHelper.MyDictBase.GetCWordByIDs(cWords);
                this.label_LinkedCWordCount.Content = "( " + cWords.Count.ToString() + " )";
            }
            ((ListBox)sender).ScrollIntoView(e.AddedItems[0]);


            //if (this.listBoxAllCWords.SelectedIndex > -1)
            //{
            //    string cWord = this.listBoxAllCWords.SelectedItem.ToString();
            //    new DBHelper().GetSubGraphAyn(cWord, DBHelper.Language.Chinese);
            //    while (DBHelper.LinkedCWords == null)
            //    { 
            //    }
                
            //    this.listBox_LinkedCWords.ItemsSource = null;
            //    this.listBox_LinkedCWords.ItemsSource = DBHelper.LinkedCWords;
            //    this.label_LinkedCWordCount.Content = "(" + DBHelper.LinkedCWords.Count.ToString() + ")";
            //    ((ListBox)sender).ScrollIntoView(e.AddedItems[0]);
            //}
        }

        private void LoadFullGraph()
        {
            //BidirectionalGraph<object, IEdge<object>> graph = null;

            //System.Runtime.Serialization.Formatters.Binary.BinaryFormatter f = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
            //string file = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "graph.dat");
            //graph = (BidirectionalGraph<object, IEdge<object>>)f.Deserialize(new System.IO.FileStream(file, System.IO.FileMode.Open));
            //MessageBox.Show("done");
        }

        /*private void button_CreateGraph_Click(object sender, RoutedEventArgs e)
        {
            if (this.listBox_LinkedCWords.Items.Count == 0)
            {
                MessageBox.Show("Not now!");
                return;
            }
            else if (this.listBox_LinkedCWords.Items.Count > 100)
            {
                MessageBoxResult result = MessageBox.Show("Too many C nodes! do it?", "", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                    return;
            }
            _nodeStore = new Dictionary<string, NodeControl>();
            BidirectionalGraph<object, IEdge<object>> graph = new BidirectionalGraph<object, IEdge<object>>(false);
            List<Word> cWords = this.listBox_LinkedCWords.Items.Cast<Word>().ToList<Word>();
            Dictionary<int, string> uWords = new Dictionary<int, string>();
            Dictionary<int, string> kWords = new Dictionary<int, string>();
            for (int i = 0; i < cWords.Count; i++)
            {
                NodeControl node = new NodeControl("b" + i.ToString(), cWords[i]);
                _nodeStore.Add(node.NodeMark, node);
                graph.AddVertex(node);
            }
            int kCounter = 1;
            int uCounter = 1;
            for (int i = 0; i < cWords.Count; i++)
            {
                Word item = cWords[i];
                foreach (int item2 in DBHelper.MyDictBase.CKDictbase.CtoK[item.ID])
                {
                    if (!kWords.ContainsKey(item2))
                    {
                        string k = "y" + kCounter.ToString();
                        kWords.Add(item2, k);

                        NodeControl node = new NodeControl(k, DBHelper.MyDictBase.GetKWordByID(item2));
                        _nodeStore.Add(node.NodeMark, node);
                        graph.AddVertex(node);
                        graph.AddEdge(new Edge<object>(_nodeStore["b" + i.ToString()], node));
                        kCounter++;
                    }
                    else
                    {
                        graph.AddEdge(new Edge<object>(_nodeStore["b" + i.ToString()], _nodeStore[kWords[item2]]));
                    }
                }
                foreach (int item2 in DBHelper.MyDictBase.CUDictbase.CtoU[item.ID])
                {
                    if (!uWords.ContainsKey(item2))
                    {
                        string u = "x" + uCounter.ToString();
                        uWords.Add(item2, u);

                        NodeControl node = new NodeControl(u, DBHelper.MyDictBase.GetUWordByID(item2));
                        _nodeStore.Add(node.NodeMark, node);
                        graph.AddVertex(node);
                        graph.AddEdge(new Edge<object>(node,_nodeStore["b" + i.ToString()]));
                        uCounter++;
                    }
                    else
                    {
                        graph.AddEdge(new Edge<object>(_nodeStore[uWords[item2]],_nodeStore["b" + i.ToString()]));
                    }
                }
            }

            foreach (KeyValuePair<string, NodeControl> item in _nodeStore)
            {
                item.Value.MouseEnter += new EventHandler(node_MouseEnter);
                item.Value.MouseLeave += new EventHandler(node_MouseLeave);
            }

            //this.zoomGraph.

            graph.AddVertex("X");
            graph.AddVertex("B");
            graph.AddVertex("Y");

            try
            {

                this._graph = graph;
                this.graphLayout.Graph = graph;
            }
            catch (Exception exp) {
                MessageBox.Show("Exception!");
            }
        }

        void node_MouseLeave(object sender, EventArgs e)
        {
            NodeControl node = (sender as NodeControl);
            if (node == null)
                return;
            this.label_CurrentWord.Content = string.Empty;
        }

        void node_MouseEnter(object sender, EventArgs e)
        {
            NodeControl node = (sender as NodeControl);
            if (node == null)
                return;

            string newValue = string.Empty;
            if (node.Word.Language != HDictInduction.Console.Language.Chinese && node.Word.Language != HDictInduction.Console.Language.None)
            {
                newValue = node.Word.Language == HDictInduction.Console.Language.Uyghur ? GlobalStore.Syn.getUKYFromUy(node.Word.Value) :
                    GlobalStore.Syn.getLtFromKz(node.Word.Value);
            }
            else
                newValue = node.Word.Value;
            this.label_CurrentWord.Content = newValue;
        }*/


        private void sid_SaveImage(object sender, SaveImageEventArgs e)
        {
            ExportToPng(graphLayout, e.FilePath, e.FileName, e.OverWrite);
        }
        /// <summary>
        /// Method exports the graphlayout to an png image.
        /// </summary>
        /// <param name="path">destination of image</param>
        /// <param name="surface">graphlayout you want to print</param>
        private void ExportToPng(GraphLayout surface, string path, string fileName, bool overwrite)
        {
            try
            {
                //Set resolution of image.
                const double dpi = 96d;

                //Set pixelformat of image.
                PixelFormat pixelFormat = PixelFormats.Pbgra32;

                //Save current canvas transform
                Transform transform = surface.LayoutTransform;

                //Reset current transform (in case it is scaled or rotated)
                surface.LayoutTransform = null;

                //Get the size of canvas
                Size size = new Size(surface.ActualWidth, surface.ActualHeight);

                //Measure and arrange the surface
                //VERY IMPORTANT
                surface.Measure(size);
                surface.Arrange(new Rect(size));

                //Create a render bitmap and push the surface to it
                RenderTargetBitmap renderBitmap =
                  new RenderTargetBitmap(
                    (int)size.Width,
                    (int)size.Height,
                    dpi,
                    dpi,
                    pixelFormat);

                //Render the graphlayout onto the bitmap.
                renderBitmap.Render(surface);

                //System.Windows.MessageBox.Show("Saving in path "+path);
                if (!System.IO.Directory.Exists(path))
                {
                    System.IO.Directory.CreateDirectory(path);
                }

                //Create a file stream for saving image
                string fullName = System.IO.Path.Combine(path, fileName);

                if (!System.IO.File.Exists(fullName) || (System.IO.File.Exists(fullName) && overwrite))
                {
                    using (System.IO.FileStream outStream = new System.IO.FileStream(fullName, System.IO.FileMode.Create))
                    {
                        //Use png encoder for our data
                        PngBitmapEncoder encoder = new PngBitmapEncoder();

                        //Push the rendered bitmap to it
                        encoder.Frames.Add(BitmapFrame.Create(renderBitmap));

                        //Save the data to the stream
                        encoder.Save(outStream);
                    }

                    //Restore previously saved layout
                    surface.LayoutTransform = transform;
                }
            }
            catch (Exception ex)
            {
                AppLog.Instance.LogFile.Error(ex.Message);
            }
        }

        private void button_Relayout_Click(object sender, RoutedEventArgs e)
        {
            this.graphLayout.Relayout();
        }

        private void button_Output_Copy_Click(object sender, RoutedEventArgs e)
        {
            this.textBox_Output.Copy();
        }

        private void button_Output_Clear_Click(object sender, RoutedEventArgs e)
        {
            this.textBox_Output.Clear();
        }

        private void MenuItem_SaveToImage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (this.graphLayout.Graph != null)
                {
                    SaveImageDialog sid = new SaveImageDialog();
                    sid.Path = System.IO.Path.Combine(ApplicationPath, "Graph Images");
                    sid.SaveImage += new SaveImageHandler(sid_SaveImage);
                    sid.Show();
                }
            }
            catch (Exception ex)
            {
                AppLog.Instance.LogFile.Info(ex.Message + "\n" + ex.StackTrace);
            }
        }

        private void MenuItem_Click_2(object sender, RoutedEventArgs e)
        {
            this.graphLayout.Relayout();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            DBHelper helper = new DBHelper();
            Dictionary<int, int> resutl = new Dictionary<int, int>();
            List<string> list = new List<string>();
            StringBuilder sb = new StringBuilder();
            Dictionary<int, bool> dict = new Dictionary<int, bool>();
            foreach (int item in Enumerable.Range(0,DBHelper.MyDictBase.CWords.Length-1))
            {
                if (dict.ContainsKey(item))
                    continue;
                IList<int> l = helper.GetSubGraph(item, DBHelper.Language.Chinese);
                if (l.Count > 100)
                {
                    foreach (int item2 in l)
                    {
                        if (!dict.ContainsKey(item2))
                            dict.Add(item2, true);
                    }
                }
                else
                {
                    if (l.Count > 3)
                    {
                        foreach (int item3 in l)
                        {
                            if (!resutl.ContainsKey(item3))
                                resutl.Add(item3, l.Count);
                        }
                    }
                }
            }

            resutl = resutl.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            foreach (KeyValuePair<int, int> item in resutl)
            {
                sb.AppendLine(string.Format("{0}\t{1}", DBHelper.MyDictBase.GetCWordByID(item.Key).Value, item.Value));
            }
            this.textBox_Output.Text = sb.ToString();
        }

        StringBuilder sb = new StringBuilder();
        //List sb = new List();
        /*private void MenuItem_Temp_Click(object sender, RoutedEventArgs e)
        {
            IL.ILSolver3 solver = new IL.ILSolver3();
            _ilsolver = solver;
            solver.LoadData();
            dataBaseConnectedComponents2 = solver.ZUKGGraphList;
            int id = 1;
            this.listBox1.ItemsSource = dataBaseConnectedComponents2.Select(t => string.Format("{0}, Vertex:{1} Edge:{2}", id++, t.VertexCount, t.EdgeCount));
        }*/

        private void MenuItem_Click_3(object sender, RoutedEventArgs e)
        {
            (new HumanEvalivationWindow()).Show();
        }

        private void MenuItem_Click_6(object sender, RoutedEventArgs e)
        {
            (new HumanEvalivationWindow()).Show();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }



        private void button_FullGraph_Click(object sender, RoutedEventArgs e)
        {
           
        }

        private List<BidirectionalGraph<Word, Edge<Word>>> dataBaseConnectedComponents = null;
        private List<BidirectionalGraph<Word, Edge<Word>>> dataBaseConnectedComponents2 = null;
        private void LoadConnectedComponets(BidirectionalGraph<Word, Edge<Word>> graph)
        {
            int totalEdges = 0;
            Dictionary<int, int> stat = new Dictionary<int, int>();
            if(graph == null)
                dataBaseConnectedComponents = (new DBHelper()).GetDatabaseConnectedComponents();
            else
                dataBaseConnectedComponents = (new DBHelper()).GetDatabaseConnectedComponents(graph);

            foreach (var item in dataBaseConnectedComponents)
            {
                int pivotCount = item.Vertices.Where(t => t.Language == Console.Language.Chinese).Count();
                if (!stat.ContainsKey(pivotCount))
                    stat.Add(pivotCount, 1);
                else
                    stat[pivotCount] = stat[pivotCount] + 1;
                totalEdges += item.EdgeCount;
            }

            var v = stat.OrderBy(t => t.Key).OrderBy(t => t.Value).ToList();

            v.ForEach(t => this.textBox_Output.AppendText(string.Format("{0}\t{1}{2}", t.Key, t.Value, Environment.NewLine)));
            //dataBaseConnectedComponents2 = dataBaseConnectedComponents.Where(t => t.Vertices.Where(p => p.Language == Console.Language.Chinese).Count() > 1 && t.Vertices.Count() > 6).OrderBy(t => t.Vertices.Count()).ToList();
            dataBaseConnectedComponents2 = dataBaseConnectedComponents.Where(t => t.Vertices.Where(p => p.Language == Console.Language.Chinese).Count() > 0 && t.Vertices.Count() > 0).OrderBy(t => t.Vertices.Count()).ToList();
            int id = 1;

            this.listBox1.ItemsSource = dataBaseConnectedComponents2.Select(t => string.Format("[{0}] P:{1} U:{2} K:{3} E:{4}", id++, t.Vertices.Where(k => k.Language == Console.Language.Chinese).Count(), t.Vertices.Where(k => k.Language == Console.Language.Uyghur).Count(), t.Vertices.Where(k => k.Language == Console.Language.Kazak).Count(), t.EdgeCount));  

            this.labelGraphCount.Content = string.Format("( {0} ) ", this.listBox1.Items.Count);

            //statistics
            String statis = "Cn\tUg\tKz\tEdges";            
            statis+= Environment.NewLine + String.Join(Environment.NewLine, dataBaseConnectedComponents2.Select(
                t => t.Vertices.Where(t1 => t1.Language == Console.Language.Chinese).Count().ToString() + "\t" +
                t.Vertices.Where(t2 => t2.Language == Console.Language.Uyghur).Count().ToString() + "\t" +
                t.Vertices.Where(t3 => t3.Language == Console.Language.Kazak).Count().ToString() + "\t" +
                t.EdgeCount.ToString()));           
            /*statis += Environment.NewLine + String.Join(Environment.NewLine, dataBaseConnectedComponents2.Select(
                t => t.Vertices.Where(t1 => t1.Language == Console.Language.Chinese).Value + "\t" +
                t.Vertices.Where(t2 => t2.Language == Console.Language.Uyghur) + "\t" +
                t.Vertices.Where(t3 => t3.Language == Console.Language.Kazak)));
        */
            textBox_Output.Text = "Total Number of Edges: " + totalEdges.ToString() + Environment.NewLine + statis;
            //MessageBox.Show("Total Number of Edges: " + totalEdges.ToString());
        }

        /*private void displayGraph2(BidirectionalGraph<Word, Edge<Word>> graph)
        {
            Dictionary<Word, NodeControl> wnDict = new Dictionary<Word, NodeControl>();
            BidirectionalGraph<object, IEdge<object>> graphToShow = new BidirectionalGraph<object, IEdge<object>>(false);
            int uCounter = 0, kCounter = 0, zCounter = 0;
            foreach (var item in graph.Vertices)
            {
                NodeControl node = null;
                if (item.Language == Console.Language.Chinese)
                    node = new NodeControl("b" + (++zCounter).ToString(), item);
                if (item.Language == Console.Language.Uyghur)
                    node = new NodeControl("a" + (++uCounter).ToString(), item);
                if (item.Language == Console.Language.Kazak)
                    node = new NodeControl("c" + (++kCounter).ToString(), item);
                wnDict.Add(item, node);
                graphToShow.AddVertex(node);
            }

            foreach (var item in graph.Edges)
                if (item.Target.Language == Console.Language.Uyghur)
                    graphToShow.AddEdge(new Edge<object>(wnDict[item.Target], wnDict[item.Source]));
                else
                    graphToShow.AddEdge(new Edge<object>(wnDict[item.Source], wnDict[item.Target]));

            graphToShow.AddVertex("A");
            graphToShow.AddVertex("B");
            graphToShow.AddVertex("C");


            this._graph = graphToShow;
            this.updateGraphInfo(graph);
            this.graphLayout.Graph = graphToShow;

        }*/

        private void displayGraph(BidirectionalGraph<Word, Edge<Word>> graph, Dictionary<Word, string> uDict, Dictionary<Word, string> kDict)
        {
            BidirectionalGraph<object, IEdge<object>> graph2 = new BidirectionalGraph<object, IEdge<object>>(false);
            Dictionary<Word, WrapPanel> wlDict = new Dictionary<Word, WrapPanel>();
            int u = 0, k = 0, z = 0, g=0;
            foreach (var item in graph.Vertices)
            {
                WrapPanel panel = new WrapPanel();
                panel.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                panel.VerticalAlignment = System.Windows.VerticalAlignment.Center;
                panel.Orientation = Orientation.Vertical;

                Label l = new Label();
                l.Content = item.Value;                
                Color color = item.Language == Console.Language.Chinese ? Colors.OrangeRed : item.Language == Console.Language.Uyghur ? Colors.Blue : Colors.Green;
                if (item.Language == Console.Language.Kiygiz)
                    color = Colors.Purple;
                l.Foreground = new SolidColorBrush(Colors.White);
                l.Background = new SolidColorBrush(color);
                panel.Tag = item;
                panel.Children.Add(l);
                // display pair mark
                if (uDict != null && kDict != null)
                {
                    Label pairMark = new Label();
                    pairMark.HorizontalAlignment = System.Windows.HorizontalAlignment.Center;
                    pairMark.FontSize = 16f;
                    if (item.Language == Console.Language.Uyghur)
                    {
                        //string mark = string.Format("a{0}{1}", ++u, uDict.ContainsKey(item) ? "  " + uDict[item].ToString() : "");
                        string mark = "";
                        if (uDict.ContainsKey(item))
                            mark = string.Format("a{0}", ++u);
                        pairMark.Content = mark;
                        panel.Children.Insert(0, pairMark);
                    }
                    if (item.Language == Console.Language.Kazak)
                    {
                        string mark = string.Format("{0}", kDict.ContainsKey(item) ? kDict[item].ToString() : "");
                        pairMark.Content = mark;
                        panel.Children.Add(pairMark);
                    }
                }

                wlDict.Add(item, panel);
                graph2.AddVertex(panel);
            }

            foreach (var item in graph.Edges)
            {
                if (item.Target.Language == Console.Language.Uyghur)
                    graph2.AddEdge(new Edge<object>(wlDict[item.Target], wlDict[item.Source]));
                else
                    graph2.AddEdge(new Edge<object>(wlDict[item.Source], wlDict[item.Target]));
            }

            this._graph = graph2;
            this.updateGraphInfo(graph);
            this.graphLayout.Graph = graph2;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            if (dataBaseConnectedComponents2 == null)
                return;
            if (this.listBox1.SelectedIndex < 0)
                return;
            BidirectionalGraph<Word, Edge<Word>> graph = dataBaseConnectedComponents2[this.listBox1.SelectedIndex];
            if (checkBoxDisplayType.IsChecked.Value)
                MessageBox.Show("displayGraph2(graph)");
            else displayGraph(graph, new Dictionary<Word,string>(), new Dictionary<Word,string>());
        }

        /*private void buttonConvertToSAT_Click(object sender, RoutedEventArgs e)
        {
            long time = DateTime.Now.Ticks;
           //string retult =  SAT.SATTemp.Main(dataBaseConnectedComponents2[this.listBox1.SelectedIndex]);
           //this.textBox_Output.Text = retult;
           ////this.textBox_Output.Text =  SAT.SATTemp.Test();

            //return;

            if (dataBaseConnectedComponents2 == null)
                return; 
            if (this.listBox1.SelectedIndex < 0)
                return;
            BidirectionalGraph<Word, Edge<Word>> graph1 = dataBaseConnectedComponents2[this.listBox1.SelectedIndex];
       
            SAT.SATConverter2 converter = new SAT.SATConverter2();
            converter.Encode(graph1, new System.IO.FileInfo(@"d:\graph_1.wcnf"));
            MessageBox.Show(string.Format("Done in {0}ms", (DateTime.Now.Ticks-time) / TimeSpan.TicksPerMillisecond));
        }*/



        private void buttonSATAnalyse_Click(object sender, RoutedEventArgs e)
        {
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(@"D:\clusters\t150");


            List<string> pairCountStats = new List<string>();
            List<string> sizeAndTimeizeStats = new List<string>();
            List<string> ooPairs = new List<string>();
            
            //add titles
            pairCountStats.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", "ID", "GraphID","Canidates","MaxExpected","Induced"));
            sizeAndTimeizeStats.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", "ID", "GraphID", "B", "A", "C","PossiblePairs", "Link", "Size", "Time"));
            int i = 1;
            foreach (var file in directory.GetFiles("*.wcnf"))
            {
                string solvingInfoFile = file.FullName.Replace(".wcnf", ".inf2");
                string mapFile = file.FullName.Replace(".wcnf", ".map");
                string ooFile = file.FullName.Replace(".wcnf", ".oo");


                string graphId = System.IO.Path.GetFileNameWithoutExtension(file.FullName).Split('_')[1];
                string infoLine = System.IO.File.ReadAllLines(solvingInfoFile)[2];
                string[] infoLineItems = infoLine.Split('\t');

                //pair count
                int possblePairCount = System.IO.File.ReadAllLines(mapFile).Length;
                int expectedPairCount = Math.Min(int.Parse(infoLineItems[1]), int.Parse(infoLineItems[2]));
                int inducedPairCount = int.Parse(infoLineItems[4]);
                pairCountStats.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}", i++, graphId, possblePairCount, expectedPairCount, inducedPairCount));

                //File Size
                sizeAndTimeizeStats.Add(string.Format("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}", i, graphId, infoLineItems[0], infoLineItems[1], infoLineItems[2], possblePairCount, infoLineItems[3], (new System.IO.FileInfo(file.FullName+"2")).Length, infoLineItems[5]));

                //one-to-one pairs

                string[] lines = System.IO.File.ReadAllLines(ooFile);
                foreach (var item in lines)
                {
                    ooPairs.Add(item.Split('\t')[1]+"\t"+item.Split('\t')[2]);
                }

            }

            string pairCountStatsFile = System.IO.Path.Combine(directory.FullName,"pairCountStats.txt");
            string sizeAndTimeizeStatsFile = System.IO.Path.Combine(directory.FullName,"sizeAndTimeizeStats.txt");
            string ooPairsFile = System.IO.Path.Combine(directory.FullName, "ooPairs.txt");

            System.IO.File.WriteAllLines(pairCountStatsFile,pairCountStats.ToArray());
            System.IO.File.WriteAllLines(sizeAndTimeizeStatsFile,sizeAndTimeizeStats.ToArray());
            System.IO.File.WriteAllLines(ooPairsFile, ooPairs.ToArray());

            BidirectionalGraph<string, Edge<string>> ooGraph = new BidirectionalGraph<string, Edge<string>>(false);
            foreach (var oo in ooPairs)
            {
                string u = "u_" + oo.Split('\t')[0];
                string k = "k_" + oo.Split('\t')[1];

                if(!ooGraph.ContainsVertex(u))
                    ooGraph.AddVertex(u);
                if (!ooGraph.ContainsVertex(k))
                    ooGraph.AddVertex(k);
                ooGraph.AddEdge(new Edge<string>(u, k));
            }

            List<BidirectionalGraph<string, Edge<string>>> graphs = (new DBHelper()).GetDatabaseConnectedComponents(ooGraph);

            List<BidirectionalGraph<string, Edge<string>>> graphs2 = graphs.Where(t => t.VertexCount == 2).ToList();


            MessageBox.Show("Done");
        }

        //private class WeightDataProvider : yWorks.yFiles.Algorithms.IDataProvider
        //{
        //    private Dictionary<yWorks.yFiles.Algorithms.Edge, int> _edgeWeight;

        //    public WeightDataProvider(Dictionary<yWorks.yFiles.Algorithms.Edge, int> edgeWeight)
        //    {
        //        _edgeWeight = edgeWeight;
        //    }

        //    public int GetInt(object dataHolder)
        //    {
        //        return _edgeWeight[(yWorks.yFiles.Algorithms.Edge)dataHolder];
        //    }

        //    public object Get(object dataHolder)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public bool GetBool(object dataHolder)
        //    {
        //        throw new NotImplementedException();
        //    }

        //    public double GetDouble(object dataHolder)
        //    {
        //        return 1.0;
        //        //return _edgeWeight[(Edge)dataHolder];
        //    }
        //}

        Dictionary<int, int> similarity(List<string> uWordsStr, List<string> kWordsStr, int fromIndex, int length)
        {
            Dictionary<int, int> ddd = new Dictionary<int, int>();

            float threashhold = 4.0f;
            for (int i = fromIndex; i < fromIndex + length && i<uWordsStr.Count; i++)
            {
                int maxIndex = -1;
                float maxSimilarity = -1;
                for (int j = 0; j < kWordsStr.Count; j++)
                {
                    if (Math.Abs(uWordsStr[i].Length - kWordsStr[j].Length) > 4)
                        continue;
                    float similarity = DBHelper.iLD(uWordsStr[i], kWordsStr[j]);
                    if (similarity >= threashhold)
                    {
                        if (similarity > maxSimilarity)
                        {
                            maxSimilarity = similarity;
                            maxIndex = j;
                        }
                    }
                }

                if (maxIndex > -1)
                {
                    ddd.Add(i, maxIndex);
                }
            }
            return ddd;
        }

        /*private void buttonCreateViz_Click(object sender, RoutedEventArgs e)
        {
            if (dataBaseConnectedComponents2 == null)
                return;
            if (this.listBox1.SelectedIndex < 0)
                return;
            BidirectionalGraph<Word, Edge<Word>> fullGraph = dataBaseConnectedComponents2[this.listBox1.SelectedIndex];
            Clusterer clusterer = new Clusterer(fullGraph);
            //clusterer.WritePivotGraphFile(@"d:\pivgraph.b");
            clusterer.WriteFullGraphFile(@"d:\transgraph.b");
            MessageBox.Show("Click Ok after \"output_clusters.txt\" is generated...");


            //List<BidirectionalGraph<Word, TaggedEdge<Word, int>>> pivotGraphs =
            //    clusterer.LoadPivotGraphClustersFromFile(@"d:\output_clusters.txt");
            //List<BidirectionalGraph<Word, Edge<Word>>> fullGraphs
            //    = clusterer.PivotGraphsToFullGraphs(pivotGraphs);

    //        List<BidirectionalGraph<Word, Edge<Word>>> fullGraphs =
    //clusterer.LoadFullGraphClustersFromFile(@"d:\output_clusters.txt");

            List<BidirectionalGraph<Word, Edge<Word>>> fullGraphs =
clusterer.LoadFullGraphClustersFromFile(@"d:\transgraph_clusters_150.txt");


            int e1 = fullGraphs.Sum(t => t.EdgeCount);
            int e2 = fullGraph.EdgeCount;

            dataBaseConnectedComponents2 = fullGraphs;
            int id = 1;
            this.listBox1.ItemsSource = dataBaseConnectedComponents2.Select(t => string.Format("[{0}] P:{1} U:{2} K:{3} E:{4}",
                id++, t.Vertices.Where(k => k.Language == Console.Language.Chinese).Count(), t.Vertices.Where(k => k.Language == Console.Language.Uyghur).Count(), t.Vertices.Where(k => k.Language == Console.Language.Kazak).Count(),t.EdgeCount));

            int vcount = fullGraphs.Sum(t => t.VertexCount);
            
      

            //int s = 1;
            //var stats1 = fullGraphs.Select(t => string.Format("{0}\t{1}",s++,t.VertexCount));
            //var stats2 = string.Join( Environment.NewLine,stats1);

            //textBox_Output.Text = stats2;

            var maxValue = fullGraphs.Max(x => x.VertexCount);
            var minValue = fullGraphs.Min(x => x.VertexCount);
            var biggest = fullGraphs.First(x => x.VertexCount == maxValue);
            var smallest = fullGraphs.First(x => x.VertexCount == minValue);

            string strBiggest = string.Format("{0}\t{1}\t{2}\t{3}",
                biggest.Vertices.Where(t=>t.Language == Console.Language.Chinese).Count(),
                biggest.Vertices.Where(t=>t.Language == Console.Language.Uyghur).Count(),
                biggest.Vertices.Where(t=>t.Language == Console.Language.Kazak).Count(),
                biggest.EdgeCount
                );
            string strSmallest = string.Format("{0}\t{1}\t{2}\t{3}",
                 smallest.Vertices.Where(t => t.Language == Console.Language.Chinese).Count(),
                 smallest.Vertices.Where(t => t.Language == Console.Language.Uyghur).Count(),
                 smallest.Vertices.Where(t => t.Language == Console.Language.Kazak).Count(),
                 smallest.EdgeCount
                 );




            MessageBox.Show("Done!");

        }*/

        /*private void buttonCreateViz_Click2()
        {
            if (dataBaseConnectedComponents2 == null)
                return;
            if (this.listBox1.SelectedIndex < 0)
                return;


            bool usepivotGraph = true;
            bool usefullGraph = false;
            bool displayPivotGraphLock = false;
            SAT.SATConverter2 s = new SAT.SATConverter2();
            //Creating connectivity between Chinese words
            BidirectionalGraph<Word, Edge<Word>> fullGraph = dataBaseConnectedComponents2[this.listBox1.SelectedIndex];
            BidirectionalGraph<int, Edge<int>> fullGraphInt = new BidirectionalGraph<int,Edge<int>>(true);
            Dictionary<Word, int> fgWordToId = new Dictionary<Word, int>() ;
            Dictionary<int, Word> fgIdToWord= new Dictionary<int,Word>();


            BidirectionalGraph<Word, TaggedEdge<Word, int>> pivotGraph = new BidirectionalGraph<Word,TaggedEdge<Word,int>>(true);
            BidirectionalGraph<int, TaggedEdge<int, int>> pivotGraphInt = new BidirectionalGraph<int, TaggedEdge<int, int>>(true);
            Dictionary<Word, int> pgWordToId = new Dictionary<Word,int>();
            Dictionary<int, Word> pgIdToWord = new Dictionary<int,Word>();
            

            // convert Chinese connectivity to int graph
            if (usepivotGraph)
            {
                pivotGraph = s.ExtractPivotGraph(fullGraph);
                int id = 1;
                foreach (var item in pivotGraph.Vertices)
                {
                    pivotGraphInt.AddVertex(id);
                    pgWordToId.Add(item, id);
                    pgIdToWord.Add(id, item);
                    id++;
                }
                foreach (var item in pivotGraph.Edges)
                {
                    pivotGraphInt.AddEdge(new TaggedEdge<int, int>(pgWordToId[item.Source], pgWordToId[item.Target], item.Tag));
                }

                StringBuilder sb2 = new StringBuilder();
                foreach (var item in pivotGraphInt.Vertices)
                {
                    //sb2.AppendLine(string.Format("{0}\t{1}", item, pgIdToWord[item].Value));
                }
                //this.textBox_Output.Text = sb2.ToString();

                //write grpah type 2
                sb2 = new StringBuilder();
                sb2.AppendLine(string.Format("{0} {1} 1", pivotGraphInt.VertexCount, pivotGraphInt.EdgeCount));
                for (int i = 1; i <= pivotGraphInt.VertexCount; i++)
                {
                    string line = string.Empty;
                    foreach (var e in pivotGraphInt.InEdges(i))
                        line = string.Format("{0} {1} {2}", line, e.Source,e.Tag);
                    foreach (var e in pivotGraphInt.OutEdges(i))
                        line = string.Format("{0} {1} {2}", line, e.Target, e.Tag);
                    line = line.Trim();
                    sb2.AppendLine(line);
                }
                System.IO.File.WriteAllText(@"d:\pivgraph.b", sb2.ToString().TrimEnd('\r','\n'));

                #region display pivot graph
                if (displayPivotGraphLock)
                {
                    //===================Dispaly Chinese Connectiviy=======================
                    BidirectionalGraph<object, IEdge<object>> tt = new BidirectionalGraph<object, IEdge<object>>(true);
                    Dictionary<Word, Label> wlDict = new Dictionary<Word, Label>();
                    foreach (var item in pivotGraph.Vertices)
                    {
                        Label l = new Label();
                        l.Content = item.Value;
                        Color color = item.Language == Console.Language.Chinese ? Colors.Orange : item.Language == Console.Language.Uyghur ? Colors.Blue : Colors.Green;
                        l.Foreground = new SolidColorBrush(Colors.White);
                        l.Background = new SolidColorBrush(color);

                        wlDict.Add(item, l);
                        tt.AddVertex(l);
                    }

                    foreach (var item in pivotGraph.Edges)
                    {
                        tt.AddEdge(new Edge<object>(wlDict[item.Source], wlDict[item.Target]));
                        tt.AddEdge(new Edge<object>(wlDict[item.Target], wlDict[item.Source]));
                    }

                    this.graphLayout.Graph = tt;
                    this.graphLayout.Relayout();
                }
                #endregion
            }
            

            //convert full graph to int graph
            if (usefullGraph)
            {
                int id = 1;
                foreach (var item in fullGraph.Vertices)
                {
                    fullGraphInt.AddVertex(id);
                    fgWordToId.Add(item, id);
                    fgIdToWord.Add(id, item);
                    id++;
                }
                foreach (var item in fullGraph.Edges)
                {
                    fullGraphInt.AddEdge(new Edge<int>(fgWordToId[item.Source], fgWordToId[item.Target]));
                    //fullGraphInt.AddEdge(new Edge<int>(fgWordToId[item.Target], fgWordToId[item.Source]));
                }


                //write grpah type 1

                //Dictionary<string, bool> tempBuffer = new Dictionary<string, bool>();
                StringBuilder sb2 = new StringBuilder();
                foreach (var item in fullGraphInt.Edges)
                {
                    //string reversePair = string.Format("{0}-{1}", item.Target, item.Source);
                    //if (tempBuffer.ContainsKey(reversePair))
                    //    continue;
                    //tempBuffer.Add(string.Format("{0}-{1}", item.Source, item.Target), true);
                    sb2.AppendLine(string.Format("{0}\t{1}", item.Source, item.Target));
                }

                System.IO.File.WriteAllText(@"d:\graph.a", sb2.ToString());
                sb2.Clear();

                foreach (var item in fullGraphInt.Vertices)
                {
                    sb2.AppendLine(string.Format("{0}\t{1}", item, fgIdToWord[item].Value));
                }

                this.textBox_Output.Text = sb2.ToString();

                //write grpah type 2
                sb2 = new StringBuilder();
                sb2.AppendLine(string.Format("{0} {1}", fullGraphInt.VertexCount, fullGraphInt.EdgeCount));
                for (int i = 1; i <= fullGraphInt.VertexCount; i++)
                {
                    string line = string.Empty;
                    foreach (var e in fullGraphInt.InEdges(i))
                        line = string.Format("{0} {1}", line, e.Source);
                    foreach (var e in fullGraphInt.OutEdges(i))
                        line = string.Format("{0} {1}", line, e.Target);
                    line = line.Trim();
                    sb2.AppendLine(line);
                }
                System.IO.File.WriteAllText(@"d:\graph.b", sb2.ToString());

                //read clusters
                string[] lines = System.IO.File.ReadAllLines(@"d:\graph.b.part.10");
                Dictionary<int, List<int>> clusters = new Dictionary<int, List<int>>();
                for (int i = 0 ; i < lines.Length; i++)
                {
                    int v = i + 1;
                    int clusterID = int.Parse(lines[i]);
                    if (clusters.ContainsKey(clusterID))
                        clusters[clusterID].Add(v);
                    else
                    {
                        List<int> vertexList = new List<int>();
                        vertexList.Add(v);
                        clusters.Add(clusterID, vertexList);
                    }
                }

                List<BidirectionalGraph<int, IEdge<int>>> subGraphs = new List<BidirectionalGraph<int, IEdge<int>>>(clusters.Count);
                foreach (var cluster in clusters)
                {
                    BidirectionalGraph<int, IEdge<int>> g = new BidirectionalGraph<int, IEdge<int>>();
                    foreach (var v in cluster.Value)
                        g.AddVertex(v);
                    foreach (var e in fullGraphInt.Edges)
                    {
                        if (g.ContainsVertex(e.Source) && g.ContainsVertex(e.Target))
                            g.AddEdge(new Edge<int>(e.Source, e.Target));
                    }

                    subGraphs.Add(g);

                }

                string ss = string.Empty;
                 ss= string.Join(Environment.NewLine, subGraphs.Select(t => string.Format("{0}\t{1}", t.VertexCount, t.EdgeCount)).ToArray());
            }



            return;

            if (true)
            {
                //write cluster result
                string[] lines = System.IO.File.ReadLines(@"d:\exchange1_result_big.txt").ToArray();
                List<List<int>> clustersInt = new List<List<int>>();
                List<int> clusterInt = new List<int>();
                StringBuilder stats = new StringBuilder();
                for (int i = 0; i < lines.Length; i++)
                {
                    string item = lines[i];
                    if (item.StartsWith("Set")) //new cluster begin
                    {
                        if (clusterInt.Count > 0)
                        {
                            clustersInt.Add(clusterInt);
                            clusterInt = new List<int>();
                        }
                    }
                    else
                    {
                        clusterInt.Add(int.Parse(item));
                        if (i == lines.Length - 1)
                            clustersInt.Add(clusterInt);
                    }
                }


                BidirectionalGraph<int, Edge<int>> gTemp = new BidirectionalGraph<int, Edge<int>>();
                foreach (var item in clustersInt[2])
                {
                    gTemp.AddVertex(item);
                }

                foreach (var id in clustersInt[2])
                {
                    Word w = fgIdToWord[id];
                    if (w.Language != Console.Language.Chinese)
                        continue;
                    foreach (var e in fullGraph.InEdges(w))
                    {
                        if(gTemp.ContainsVertex(fgWordToId[e.Source]))
                            gTemp.AddEdge(new Edge<int>(fgWordToId[e.Source],id));
                    }

                    foreach (var e in fullGraph.OutEdges(w))
                    {
                        if (gTemp.ContainsVertex(fgWordToId[e.Target]))
                            gTemp.AddEdge(new Edge<int>(id,fgWordToId[e.Target]));
                    }
                }


                StringBuilder sb2 = new StringBuilder();
                foreach (var item in gTemp.Edges)
                {
                    sb2.AppendLine(string.Format("{0}\t{1}",item.Source,item.Target));
                }

                System.IO.File.WriteAllText(@"d:\exchange2.txt",sb2.ToString());


                int clusterID = 1;
                stats.AppendLine("Cluster ID\tPivot Word Count\tEdge Count");
                foreach (var item in clustersInt)
                {
                    BidirectionalGraph<Word, Edge<Word>> fullGraphTemp = DBHelper.CloneGraph(fullGraph);
                    foreach (var item2 in clustersInt)
                    {
                        if (item == item2)
                            continue;

                        foreach (var item3 in item2)
                        {
                            fullGraphTemp.RemoveVertex(fgIdToWord[item3]);
                        }
                    }

                    int removed = 0;
                    List<string> removedWords = new List<string>();
                    zuk_dbSQLDataSet.zuk_fixedDataTable table = DBHelper.GraphToDatabase(fullGraphTemp);

                    stats.AppendLine(string.Format("Cluster {0}:\t{1}\t{2}\t{3}",
                        clusterID + 1,
                        fullGraphTemp.Vertices.Where(t => t.Language == Console.Language.Chinese).Count(),
                        fullGraphTemp.Vertices.Where(t => t.Language == Console.Language.Uyghur || t.Language == Console.Language.Kazak).Count(),
                        fullGraphTemp.EdgeCount
                        ));
                    System.IO.File.WriteAllLines(string.Format(@"d:\buffer\Revmoed{0}.txt", clusterID), removedWords.ToArray());
                    //table.WriteXml(string.Format(@"d:\buffer\cluster{0}.xml", clusterID));
                    clusterID++;

                }

                textBox_Output.Text = stats.ToString();

                MessageBox.Show("Done!");
                return;
            }

            return;

            //============Clustering=============================
            //prepare for clustering 
            //Dictionary<yWorks.yFiles.Algorithms.Node, int> nodeToIdDict = new Dictionary<yWorks.yFiles.Algorithms.Node, int>();
            //Dictionary<int, yWorks.yFiles.Algorithms.Node> idToNodeDict = new Dictionary<int, yWorks.yFiles.Algorithms.Node>();
            //Dictionary<yWorks.yFiles.Algorithms.Edge, Edge<int>> edgeDict = new Dictionary<yWorks.yFiles.Algorithms.Edge, Edge<int>>();
            //yWorks.yFiles.Algorithms.Graph yGraph = new yWorks.yFiles.Algorithms.Graph();
            //Dictionary<yWorks.yFiles.Algorithms.Edge, int> edgeWeight = new Dictionary<yWorks.yFiles.Algorithms.Edge, int>();


            //foreach (var item in fullGraphInt.Vertices)
            //{
            //    yWorks.yFiles.Algorithms.Node node = yGraph.CreateNode();
            //    nodeToIdDict.Add(node, item);
            //    idToNodeDict.Add(item, node);

            //}
            //foreach (var item in fullGraphInt.Edges)
            //{
            //    yWorks.yFiles.Algorithms.Edge edge = yGraph.CreateEdge(idToNodeDict[item.Source], idToNodeDict[item.Target]);
            //    edgeDict.Add(edge, item);
            //    edgeWeight.Add(edge, 1);
            //}



            //start clustering
            //yWorks.yFiles.Algorithms.IDataProvider weighProvider = new WeightDataProvider(edgeWeight);
            //yWorks.yFiles.Algorithms.INodeMap nodeMap = yGraph.CreateNodeMap();
            //int c = yWorks.yFiles.Algorithms.Groups.EdgeBetweennessClustering(yGraph, nodeMap, true, 1000, 2000, weighProvider);
            //StringBuilder sb = new StringBuilder();
            //sb.Append("Clusters:" + c.ToString());
            //foreach (var item in nodeToIdDict.Keys)
            //{
            //    int id = nodeMap.GetInt(item);
            //    sb.AppendLine(string.Format("{0}  {1}   {2}", nodeToIdDict[item], id, fgIdToWord[nodeToIdDict[item]].Value));
            //}
            //System.IO.File.AppendAllText("ppp.txt", sb.ToString());
            //MessageBox.Show("done");



            /*

            Dictionary<Word, int> dict = new Dictionary<Word, int>();
            int index = 0;
            foreach (var item in cConnectivitygraph.Vertices)
            {
                dict.Add(item, index++);
            }

            StringBuilder buffer = new StringBuilder();
            foreach (var item in cConnectivitygraph.Edges)
            {
                buffer.AppendLine(string.Format("{0} {1} {2}", dict[item.Source], dict[item.Target], item.Tag));
                buffer.AppendLine(string.Format("{0} {1} {2}", dict[item.Target], dict[item.Source], item.Tag));
            }
            System.IO.File.WriteAllText(@"d:\graph.txt", buffer.ToString());

            MessageBox.Show("Done!");
             * 
             * * /
        }

        private void buttonCreateViz_Click3()
        {
            if (dataBaseConnectedComponents2 == null||this.listBox1.SelectedIndex < 0||this.listBox1.SelectedIndex < 0)
                return;
            int pivotthreshhold = 50;
            BidirectionalGraph<Word, Edge<Word>> graph = dataBaseConnectedComponents2[this.listBox1.SelectedIndex];
            SAT.SATConverter2 sat = new SAT.SATConverter2();
            List<BidirectionalGraph<Word, Edge<Word>>> graphs = sat.Cluster(graph, pivotthreshhold, true);
            dataBaseConnectedComponents2 = graphs.ToList();

            int id = 1;
            this.listBox1.ItemsSource = dataBaseConnectedComponents2.Select(t => string.Format("[{0}] V:{1} E:{2} P:{3}", id++, t.VertexCount, t.EdgeCount,t.Vertices.Where(v=>v.Language == Console.Language.Chinese).Count()));
            MessageBox.Show("Done!");    
        }*/

        private void ICTest(List<BidirectionalGraph<Word, Edge<Word>>> graphs)
        {
            Dictionary<Word, List<KeyValuePair<Word, int>>> pairs = new Dictionary<Word, List<KeyValuePair<Word, int>>>();
            foreach (var graph in graphs)
            {
                var uwords = graph.Vertices.Where(t => t.Language == Console.Language.Uyghur);
                foreach (var uword in uwords)
                {
                    Dictionary<Word, int> candidates = new Dictionary<Word, int>();
                    var E = graph.InEdges(uword).Select(t => t.Source).ToList();
                    foreach (var pivot in E)
                    {
                        var kWords = graph.OutEdges(pivot).Select(t => t.Target).Where(t=>t.Language == Console.Language.Kazak);
                        foreach (var kword in kWords)
                        {
                            if (candidates.ContainsKey(kword))
                                continue;
                            int weight = graph.InEdges(kword).Select(t => t.Source).Intersect(E).Count();
                            candidates.Add(kword, weight);
                        }
                    }

                    pairs.Add(uword, candidates.Select(t => new KeyValuePair<Word, int>(t.Key, t.Value)).OrderByDescending(t => t.Value).ToList());
                }

            }

            var pairs2 = pairs.Select(t => new KeyValuePair<Word, Word>(t.Key, t.Value[0].Key)).ToList();

            var output = pairs2.Select(t => string.Format("{0},{1}", t.Key, t.Value));
            System.IO.File.WriteAllLines(@"buffer3\mm.txt", output);
            MessageBox.Show("done");
        }

        private void NaiveCombination(List<BidirectionalGraph<Word, Edge<Word>>> graphs)
        {
            List<KeyValuePair<Word, Word>> pairs = new List<KeyValuePair<Word, Word>>();
            foreach (var graph in graphs)
            {
                var uwords = graph.Vertices.Where(t => t.Language == Console.Language.Uyghur);
                var kwords = graph.Vertices.Where(t => t.Language == Console.Language.Kazak);
                foreach (var uword in uwords)
                    foreach (var kword in kwords)
                        pairs.Add(new KeyValuePair<Word, Word>(uword, kword));                
            }
            var output = pairs.Select(t => string.Format("{0},{1}", t.Key, t.Value));
            System.IO.File.WriteAllLines(@"buffer4\NaiveCombination.txt", output);
            System.Media.SoundPlayer simpleSound = new System.Media.SoundPlayer(@"c:\Windows\Media\Ring03.wav");
            simpleSound.Play();
            MessageBox.Show("done");
        }
        
        private void buttonTest_Click(object sender, RoutedEventArgs e)
        {
            //int pivotThreshhold = 50;
            //List<BidirectionalGraph<Word, Edge<Word>>> allGraphs = (new DBHelper()).GetDatabaseConnectedComponents();
            //allGraphs = allGraphs.OrderBy(t => t.Vertices.Where(p => p.Language == Console.Language.Chinese).Count()).ToList();
            //List<BidirectionalGraph<Word, Edge<Word>>> subGraphsBig = allGraphs.Where(t => t.Vertices.Where(p => p.Language == Console.Language.Chinese).Count() > pivotThreshhold).ToList();

            //if (subGraphsBig.Count > 1)
            //    throw new Exception("Many Big Subgraphs!");
            //SAT.SATConverter2 sat = new SAT.SATConverter2();
            //List<BidirectionalGraph<Word, Edge<Word>>> subGraphsClustered = new List<BidirectionalGraph<Word, Edge<Word>>>();
            //Siplit
            //foreach (var graph in subGraphsBig)
            //{
            //    //string tem = TempOperations.Fun3(graph);
            //    //this.textBox_Output.Text = tem;
            //    //return;
            //    List<BidirectionalGraph<Word, Edge<Word>>> subGraphs1 = sat.Cluster(graph, pivotThreshhold, false);
            //    List<BidirectionalGraph<Word, Edge<Word>>> subGraphs2 = subGraphs1.Where(t => t.Vertices.Where(p => p.Language == Console.Language.Chinese).Count() > 1).ToList();
            //    subGraphsClustered.AddRange(subGraphs2);
            //}


            //System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo( @"d:\buffer\");

            //for (int i = 0; i < subGraphsClustered.Count; i++)
            //{
            //    BidirectionalGraph<Word, Edge<Word>> g = subGraphsClustered[i];
            //    System.IO.FileInfo cnfFile = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, string.Format("graph_{0}.wcnf", i + 1)));
            //    new SATConverter3().SolveGraph(g, cnfFile);
            //    // .SolveGraph(g, cnfFile);
            //}

            //for (int i = 0; i < dataBaseConnectedComponents2.Count - 1; i++)
            //{
            //    BidirectionalGraph<Word, Edge<Word>> g = dataBaseConnectedComponents2[i];
            //    System.IO.FileInfo cnfFile = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, string.Format("graph_{0}.wcnf", i + 1)));
            //    new SATConverter3().SolveGraph(g, cnfFile);
            //    // .SolveGraph(g, cnfFile);
            //}

            //ICTest(dataBaseConnectedComponents2.Take(dataBaseConnectedComponents2.Count - 1).ToList());
            var watch = System.Diagnostics.Stopwatch.StartNew();
            solveAll();
            System.Media.SoundPlayer simpleSound = new System.Media.SoundPlayer(@"c:\Windows\Media\tada.wav");
            simpleSound.Play();
            watch.Stop();
            var elapsedMs = watch.Elapsed;
            MessageBox.Show(string.Format("Done in {0} minutes", elapsedMs.ToString()));
        }

        private void solveAll()
        {
            int startindex = int.Parse(this.textBoxIndex.Text.Trim().Split('-')[0].Trim())-1;
            int endindex = int.Parse(this.textBoxIndex.Text.Trim().Split('-')[1].Trim())-1;
            
            if (startindex < 0)
                throw new Exception("Index out of range!");
            if (endindex >= dataBaseConnectedComponents2.Count)
                throw new Exception("Index out of range!");

            List<KeyValuePair<Word, Word>> allPairs = new List<KeyValuePair<Word, Word>>();

            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(@"buffer2\");
            for (int i = startindex; i <= endindex; i++)
            {
                BidirectionalGraph<Word, Edge<Word>> g = dataBaseConnectedComponents2[i];
                System.IO.FileInfo cnfFile = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, string.Format("graph_{0}.wcnf", i + 1)));
                new SATConverter3().SolveGraph(g, cnfFile);
                //allPairs.AddRange(new SATConverter3().GenerateAllNaivePairs(g));
            }
            //var output = allPairs.Select(t => string.Format("{0},{1}", t.Key, t.Value));
            //System.IO.File.WriteAllLines(@"buffer4\NaiveCombination.txt", output);
            System.Media.SoundPlayer simpleSound = new System.Media.SoundPlayer(@"c:\Windows\Media\tada.wav");
            simpleSound.Play();
            //Debug.WriteLine("Generate All Naive pairs is done");

            //Parallel.For(0, dataBaseConnectedComponents2.Count, 
            //    (i)=> new SATConverter3().SolveGraph(dataBaseConnectedComponents2[i],
            //        new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, string.Format("graph_{0}.wcnf", i + 1)))));
        }

        /*private void buttonDisplayPivotGraph_Click(object sender, RoutedEventArgs e)
        {
            if (dataBaseConnectedComponents2 == null)
                return;
            if (this.listBox1.SelectedIndex < 0)
                return;

            //Creating connectivity between Chinese words
            BidirectionalGraph<Word, Edge<Word>> graph1 = dataBaseConnectedComponents2[this.listBox1.SelectedIndex];

            SAT.SATConverter2 s = new SAT.SATConverter2();
            BidirectionalGraph<Word, TaggedEdge<Word, int>> cConnectivitygraph = s.ExtractPivotGraph(graph1);

            if (cConnectivitygraph.Vertices.Count() == 0)
            {
                MessageBox.Show("Only one pivot!");
                return;
            }

            BidirectionalGraph<object, IEdge<object>> tt = new BidirectionalGraph<object, IEdge<object>>();
            Dictionary<Word, Label> wlDict = new Dictionary<Word, Label>();
            foreach (var item in cConnectivitygraph.Vertices)
            {
                Label l = new Label();
                l.Content = item.Value;
                Color color = item.Language == Console.Language.Chinese ? Colors.OrangeRed : item.Language == Console.Language.Uyghur ? Colors.Blue : Colors.Green;
                l.Foreground = new SolidColorBrush(Colors.White);
                l.Background = new SolidColorBrush(color);
  
                wlDict.Add(item, l);
                tt.AddVertex(l);
            }

            foreach (var item in cConnectivitygraph.Edges)
            {
                tt.AddEdge(new Edge<object>(wlDict[item.Source], wlDict[item.Target]));
                tt.AddEdge(new Edge<object>(wlDict[item.Target], wlDict[item.Source]));
            }

            this.graphLayout.Graph = tt;
            this.graphLayout.Relayout();
     
       }*/

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            if (dataBaseConnectedComponents2 == null || dataBaseConnectedComponents2.Count == 0)
                return;
            BidirectionalGraph<Word, Edge<Word>> g = new BidirectionalGraph<Word, Edge<Word>>();
            foreach (var item in dataBaseConnectedComponents2)
            {
                foreach (var vertex in item.Vertices)
                {
                    g.AddVertex(vertex);
                }

                foreach (var edge in item.Edges)
                {
                    g.AddEdge(edge);
                }
            }

            displayGraph(g, new Dictionary<Word,string>(),new Dictionary<Word,string>());
        }

        /*private void buttonTest1_Click(object sender, RoutedEventArgs e)
        {
            if (_ilsolver == null)
            {
                MessageBox.Show("triple graphs have been loaded!");
                return;
            }

            //_ilsolver.BatchSolveTripleGraphs(new List<BidirectionalGraph<Word, Edge<Word>>>{_ilsolver.ZUKGGraphList[this.listBox1.SelectedIndex]}, "x");
            MessageBox.Show("Done");
            //IL.ILSolver3 s = new IL.ILSolver3();
            //s.LoadData();
            //s.Solve3TupleGraphs();
            //MessageBox.Show("Done");

            //for (int j = 0; j < dataBaseConnectedComponents2.Count; j++)
            //{
            //    if (dataBaseConnectedComponents2[j].Vertices.Count() == 10 && dataBaseConnectedComponents2[j].Edges.Count() == 9)
            //    {
            //        System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(@"d:\tt\");
            //        System.IO.FileInfo cnfFile = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, string.Format("graph_{0}.wcnf", j+1)));
            //        string info = TempOperations.SolveGraph(dataBaseConnectedComponents2[j], cnfFile);
            //    }
            //}
            //return;
            //BidirectionalGraph<object, IEdge<object>> graph = this.graphLayout.Graph as BidirectionalGraph<object, IEdge<object>>;
            //int i = 1;
            //foreach (var item in graph.Vertices)
            //{
            //    WrapPanel p = item as WrapPanel;
            //    Label l = new Label();
            //    l.Content = "a" + (i++).ToString();
            //    p.Children.Add(l);
            //}

            //return;
        }*/


        private void buttonSolveSelected_Click(object sender, RoutedEventArgs e)
        {
            if (dataBaseConnectedComponents2 == null)
                return;
            if (this.listBox1.SelectedIndex < 0)
                return;
            BidirectionalGraph<Word, Edge<Word>> graph1 = dataBaseConnectedComponents2[this.listBox1.SelectedIndex];
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(@"buffer\");
            System.IO.FileInfo cnfFile = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, string.Format("graph_Single.wcnf", 1)));
            string info = new SATConverter3().SolveGraph(graph1, cnfFile);

            System.IO.FileInfo resultFile = new System.IO.FileInfo(cnfFile.FullName.Replace(".wcnf", ".oo"));
            if (!resultFile.Exists)
                return;

            //display graph and pair marks
            string[] lines = System.IO.File.ReadAllLines(resultFile.FullName);
            Dictionary<int, string> d0 = new Dictionary<int, string>();
            Dictionary<int, string> d1 = new Dictionary<int, string>();
            Dictionary<int, string> d2 = new Dictionary<int, string>();
            Dictionary<string, string> a = new Dictionary<string, string>();
            Dictionary<Word, string> d3 = new Dictionary<Word, string>();
            Dictionary<Word, string> d4 = new Dictionary<Word, string>();
            int countU = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                d0.Add(i, lines[i].Split(',')[0]);
                d1.Add(i, lines[i].Split(',')[1]);
                d2.Add(i, lines[i].Split(',')[2]);
            }
            foreach (var item in graph1.Vertices.Where(t => t.Language == Console.Language.Uyghur))
            {
                if (d1.ContainsValue(item.Value) && !d3.ContainsKey(item))
                {
                    d3.Add(item, "");
                    a.Add(item.Value, "a" + (++countU).ToString());
                }                    
                /*for (int i = 0; i < lines.Length; i++)
                {
                    string currentU = lines[i].Split(',')[1];
                    if (currentU == item.Value && !d3.ContainsKey(item))
                        d3.Add(item, "");
                }*/
            }
            foreach (var item in graph1.Vertices.Where(t => t.Language == Console.Language.Kazak))
            {
                string currentRank = "";                    
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].Split(',')[2] == item.Value)
                        currentRank += a[lines[i].Split(',')[1]] + "-" + lines[i].Split(',')[0] + "; ";                    
                }
                currentRank = currentRank.TrimEnd(';',' ');
                d4.Add(item, currentRank);                
            }
            displayGraph(graph1, d3, d4);
            textBox_Output.Text = info;
        }

        private void MenuItem_Click_4(object sender, RoutedEventArgs e)
        {
            LoadConnectedComponets(null);
        }

        private void buttonNaiveCombination_Click(object sender, RoutedEventArgs e)
        {
            int startindex = int.Parse(this.textBoxIndex.Text.Trim().Split('-')[0].Trim()) - 1;
            int endindex = int.Parse(this.textBoxIndex.Text.Trim().Split('-')[1].Trim()) - 1;

            if (startindex < 0)
                throw new Exception("Index out of range!");
            if (endindex >= dataBaseConnectedComponents2.Count)
                throw new Exception("Index out of range!");

            //List<KeyValuePair<Word, Word>> allPairs = new List<KeyValuePair<Word, Word>>();
            List<WordPair> allPairs = new List<WordPair>();
            System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(@"buffer2\");
            for (int i = startindex; i <= endindex; i++)
            {
                BidirectionalGraph<Word, Edge<Word>> g = dataBaseConnectedComponents2[i];
                allPairs.AddRange(new SATConverter3().GenerateAllNaivePairs(g));
            }
            var output = allPairs.Select(t => string.Format("{0},{1},{2}", t.Prob, t.WordU, t.WordK));
            System.IO.File.WriteAllLines(@"buffer4\NaiveCombination.txt", output);
            System.Media.SoundPlayer simpleSound = new System.Media.SoundPlayer(@"c:\Windows\Media\tada.wav");
            simpleSound.Play();
            Debug.WriteLine("Generate All Naive pairs is done");
        }

        private void buttonTraditionalTanaka_Click(object sender, RoutedEventArgs e)
        {
            //int pivotThreshhold = 50;
            //List<BidirectionalGraph<Word, Edge<Word>>> allGraphs = (new DBHelper()).GetDatabaseConnectedComponents();
            //allGraphs = allGraphs.OrderBy(t => t.Vertices.Where(p => p.Language == Console.Language.Chinese).Count()).ToList();
            //List<BidirectionalGraph<Word, Edge<Word>>> subGraphsBig = allGraphs.Where(t => t.Vertices.Where(p => p.Language == Console.Language.Chinese).Count() > pivotThreshhold).ToList();

            //if (subGraphsBig.Count > 1)
            //    throw new Exception("Many Big Subgraphs!");
            //SAT.SATConverter2 sat = new SAT.SATConverter2();
            //List<BidirectionalGraph<Word, Edge<Word>>> subGraphsClustered = new List<BidirectionalGraph<Word, Edge<Word>>>();
            //Siplit
            //foreach (var graph in subGraphsBig)
            //{
            //    //string tem = TempOperations.Fun3(graph);
            //    //this.textBox_Output.Text = tem;
            //    //return;
            //    List<BidirectionalGraph<Word, Edge<Word>>> subGraphs1 = sat.Cluster(graph, pivotThreshhold, false);
            //    List<BidirectionalGraph<Word, Edge<Word>>> subGraphs2 = subGraphs1.Where(t => t.Vertices.Where(p => p.Language == Console.Language.Chinese).Count() > 1).ToList();
            //    subGraphsClustered.AddRange(subGraphs2);
            //}


            //System.IO.DirectoryInfo directory = new System.IO.DirectoryInfo(@"d:\buffer\");

            //for (int i = 0; i < subGraphsClustered.Count; i++)
            //{
            //    BidirectionalGraph<Word, Edge<Word>> g = subGraphsClustered[i];
            //    System.IO.FileInfo cnfFile = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, string.Format("graph_{0}.wcnf", i + 1)));
            //    new SATConverter3().SolveGraph(g, cnfFile);
            //    // .SolveGraph(g, cnfFile);
            //}

            //for (int i = 0; i < dataBaseConnectedComponents2.Count - 1; i++)
            //{
            //    BidirectionalGraph<Word, Edge<Word>> g = dataBaseConnectedComponents2[i];
            //    System.IO.FileInfo cnfFile = new System.IO.FileInfo(System.IO.Path.Combine(directory.FullName, string.Format("graph_{0}.wcnf", i + 1)));
            //    new SATConverter3().SolveGraph(g, cnfFile);
            //    // .SolveGraph(g, cnfFile);
            //}

            ICTest(dataBaseConnectedComponents2.Take(dataBaseConnectedComponents2.Count - 1).ToList());
        }

        private void sliderThresholdOmega2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliderThresholdOmega2Value = (double)sliderThresholdOmega2.Value;
            SATConverter3.omega2Threshold = sliderThresholdOmega2Value;// int.Parse(this.textBoxOmega2Threshold.Text);   
            //Debug.WriteLine("sliderThresholdOmega2Value:"+ sliderThresholdOmega2Value);
        }

        /*private void sliderThresholdOmega3_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            double sliderThresholdOmega3Value = (double)sliderThresholdOmega3.Value;
            SATConverter3.omega3Threshold = sliderThresholdOmega3Value;// int.Parse(this.textBoxOmega3Threshold.Text);
            //Debug.WriteLine("sliderThresholdOmega3Value:" + sliderThresholdOmega3Value);
        }*/

        private void WPMaxSAT_Checked(object sender, RoutedEventArgs e)
        {
            if (LC1.IsChecked == true)
            {
                //Debug.WriteLine("LC1");
                SATConverter3.languageOption = 1;
            }
            else if (LC2.IsChecked == true)
            {
                //Debug.WriteLine("LC2");
                SATConverter3.languageOption = 2;
            }
            else if (LC2b.IsChecked == true)
            {
                //Debug.WriteLine("LC2b");
                SATConverter3.languageOption = 3;
            }
        }

        /*private void useNewPairs_Click(object sender, RoutedEventArgs e)
        {
            SATConverter3.acceptOmega3NewPair = useNewPairs.IsChecked == true ? true : false;
        }*/

        private void uniqenessConstraint_Click(object sender, RoutedEventArgs e)
        {
            SATConverter3.languageOption = uniqenessConstraint.IsChecked == true ? 1 : 3;
        }

        /*private void MenuItem_Click_5(object sender, RoutedEventArgs e)
        {
            if (dataBaseConnectedComponents2 == null)
                return;
            if (this.listBox1.SelectedIndex < 0)
                return;
            BidirectionalGraph<Word, Edge<Word>> graph = dataBaseConnectedComponents2[this.listBox1.SelectedIndex];

            IL.ILSolver solver = new IL.ILSolver();
            //string info = solver.Solve(graph);
            //textBox_Output.Text = info;
            MessageBox.Show("Done");


            //StringBuilder sb = new StringBuilder();
            //for (int i = 0; i < dataBaseConnectedComponents2.Count; i++)
            //{
            //    string t = (new IL.ILSolver()).Solve(dataBaseConnectedComponents2[i]);
            //    sb.AppendLine((i + 1).ToString() + "\t" + t);

            //}
            //textBox_Output.Text = sb.ToString();
        }*/
    }
}
