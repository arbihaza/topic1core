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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HDictInduction.Console
{
    public delegate void SaveImageHandler(object sender, SaveImageEventArgs e);

    /// <summary>
    /// Interaction logic for SaveImageDialog.xaml
    /// </summary>
    public partial class SaveImageDialog
    {
        public string Path
        {
            get;
            set;
        }

        public event SaveImageHandler SaveImage;

        public SaveImageDialog()
        {
            InitializeComponent();
        }

        protected virtual void OnSaveImage(SaveImageEventArgs e)
        {
            if (SaveImage != null)
            {
                SaveImage(this, e);
            }
        }

        private void btnOK_Click(object sender, RoutedEventArgs e)
        {
            // get the file name
            string fileName = txtImageName.Text + ".png";

            bool overwrite = chkbOverwrite.IsChecked.Value;

            SaveImageEventArgs eArgs = new SaveImageEventArgs(Path, fileName, overwrite);
            OnSaveImage(eArgs);
            this.Close();
        }
    }
}
