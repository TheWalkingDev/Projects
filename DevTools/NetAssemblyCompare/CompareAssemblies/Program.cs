using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace CompareAssemblies
{
    public class ResultLog : IResultLog
    {

        int _Count;

        bool _LogOutput;

        public ResultLog(bool logOutput)
        {
            _LogOutput = logOutput;
        }

        public string Context { get; set; }

        #region IResultLog Members

        public void WriteLine(string fmt, params object[] args)
        {
            var st = fmt;
            if (args.NotNull() && args.Length > 0)
            {
                st = string.Format(fmt, args);
            }
            if (_LogOutput)
            {
                Console.WriteLine("{0}: {1}", Context, st);
            }
            _Count++;
        }

        public bool IsDifferent { get { return _Count > 0; } }

        #endregion
    }

    class Program
    {
        static int Main(string[] args)
        {
            var logOutput = args.Length > 2;
            var resultLog = new ResultLog(logOutput);

            var sourceFileName = args[0];
            var targetFileName = args[1];

            sourceFileName = GetFullPathName(sourceFileName);
            targetFileName = GetFullPathName(targetFileName);

            var source = Assembly.LoadFile(sourceFileName);
            var target = Assembly.LoadFile(targetFileName);
            CompareAssemblyMethods.Compare(resultLog, source, target);
            // Not really necessary as get and set methods should be found by CompareAssemblyMethods

            CompareAssemblyProperties.Compare(resultLog, source, target);
            CompareAssemblyFields.Compare(resultLog, source, target);

            if (resultLog.IsDifferent)
            {
                return 1;
            }

            return 0;
        }

        private static string GetFullPathName(string fileName)
        {
            var info = new FileInfo(fileName);
            if (!info.Exists)
            {
                throw new ArgumentException("File not found: " + fileName);
            }
            fileName = info.FullName;
            return fileName;
        }
    }
}
