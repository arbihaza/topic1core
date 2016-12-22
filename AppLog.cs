using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;

namespace HDictInduction.Console
{
    // adding this class here because interfaces namespace is referenced by everything else.
    public class AppLog
    {
        static readonly AppLog instance = new AppLog();

        #region Properties
        public Logger LogFile
        {
            get;
            private set;
        }
        #endregion

        static AppLog()
        {
        }

        AppLog()
        {
            this.LogFile = LogManager.GetLogger("asrLogger");
        }

        public static AppLog Instance
        {
            get
            {
                return instance;
            }
        }
    }
}
