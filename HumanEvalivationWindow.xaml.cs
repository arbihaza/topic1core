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


namespace HDictInduction.Console
{
    /// <summary>
    /// Interaction logic for HumanEvalivationWindow.xaml
    /// </summary>
    public partial class HumanEvalivationWindow : Window
    {
        string[] lines;
        string[] sampleLines;
        int currentIndex = 0;
        bool[] sampleResult;
        string fileName = string.Empty;
        public HumanEvalivationWindow()
        {
            InitializeComponent();
            this.label_source.Content = this.label_index.Content = string.Empty;
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.OpenFileDialog of = new System.Windows.Forms.OpenFileDialog();
            of.DereferenceLinks = false;
            of.ShowDialog();
            if (string.IsNullOrEmpty(of.FileName))
                return;
            fileName = System.IO.Path.GetFileName(of.FileName);
            lines = System.IO.File.ReadAllLines(of.FileName);
            this.label_source.Content = string.Format("File Name:{0}    Total pairs: {1}", fileName, lines.Length);
        }

        private void button_selectRandom_Click(object sender, RoutedEventArgs e)
        {

            int randomCount = lines.Length;
            if (this.comboBox1.Text.ToString().ToLower() != "all")
               randomCount= int.Parse(this.comboBox1.Text.ToString());
          
            if (lines == null || lines.Length == 0 )
            {
                System.Windows.MessageBox.Show("Not enough items for sample selection!");
                return;
            }
            randomCount = (randomCount < lines.Length ? randomCount : lines.Length);
            Dictionary<int, string> randomDict = new Dictionary<int, string>();

            if (randomCount == lines.Length)
            {
                for (int i = 0; i < lines.Length; i++)
                {
                    randomDict.Add(i, lines[i]);
                }
            }
            else
            {
                Random random = new Random();
                int counter = 0;
                while (randomDict.Count < (randomCount))
                {
                    int index = -1;
                    do
                    {
                        index = random.Next(0, lines.Length);
                    } while (randomDict.ContainsKey(index));
                    randomDict.Add(index, lines[index]);
                }
            }
            sampleLines = randomDict.Select(t => t.Value).ToArray();
            sampleResult = new bool[sampleLines.Length];
            this.label_source.Content = string.Format("File Name:{0}    Pair count: {1}  Sample count:{2}",fileName, lines.Length, sampleLines.Length);
            this.setCurrentIndex(0);
        }

        private void setCurrentIndex(int index)
        {
            this.currentIndex = index;
            this.label_u.Content = this.sampleLines[index].Split('\t')[0];
            this.label_k.Content = this.sampleLines[index].Split('\t')[1];
            this.label_index.Content = string.Format("({0}/{1})",sampleLines.Length,currentIndex+1);
        }

        private void button_Yes_Click(object sender, RoutedEventArgs e)
        {
            if (this.sampleLines == null || this.sampleLines.Length == 0)
                return;
            sampleResult[this.currentIndex] = true;
            if (this.currentIndex < sampleLines.Length - 1)
                this.setCurrentIndex(this.currentIndex + 1);
            else
                showCorrect();
        }

        private void button_no_Click(object sender, RoutedEventArgs e)
        {
            if (this.sampleLines == null || this.sampleLines.Length == 0)
                return;
            sampleResult[this.currentIndex] = false;
            if (this.currentIndex < sampleLines.Length - 1)
                this.setCurrentIndex(this.currentIndex + 1);
            else
                showCorrect();
        }

        private void showCorrect()
        {
            MessageBox.Show(string.Format("{0} correct out of {1}",sampleResult.Where(t=>t==true).Count(),sampleResult.Length));
        }

        private void label_u_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                if (e.RightButton == MouseButtonState.Pressed)
                    Clipboard.SetText((sender as Label).Content.ToString());
            }
            catch { }
        }

        private void button2_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Clipboard.SetText(label_u.Content.ToString());
            }
            catch { }
        }

        private void button3_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                Clipboard.SetText(label_k.Content.ToString());
            }
            catch { }
        }
    }
}
