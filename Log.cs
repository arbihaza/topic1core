using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NLog;
using NLog.Config;
using NLog.Targets;

namespace HDictInduction.Console
{
    internal static class Log
    {
        public static Logger Instance { get; private set; }
        static Log()
        {
            Instance = LogManager.GetCurrentClassLogger();
            return;
            // Setup the logging view for Sentinel - http://sentinel.codeplex.com
            var sentinalTarget = new NLogViewerTarget()
            {
                Name = "dictionary",
                Address = "udp://127.0.0.1:9999"
            };
            var sentinalRule = new LoggingRule("*", LogLevel.Trace, sentinalTarget);
            LogManager.Configuration.AddTarget("dictionary", sentinalTarget);
            LogManager.Configuration.LoggingRules.Add(sentinalRule);

            LogManager.ReconfigExistingLoggers();

            Instance = LogManager.GetCurrentClassLogger();
        }
    }
}
