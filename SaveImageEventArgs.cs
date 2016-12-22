using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HDictInduction.Console
{
    public class SaveImageEventArgs : EventArgs
    {
        public string FilePath
        {
            get;
            private set;
        }

        public string FileName
        {
            get;
            private set;
        }

        public bool OverWrite
        {
            get;
            private set;
        }

        private SaveImageEventArgs()
        {
        }

        public SaveImageEventArgs(string filePath, string fileName, bool overwrite)
        {
            FilePath = filePath;
            FileName = fileName;
            OverWrite = overwrite;
        }
    }
}
