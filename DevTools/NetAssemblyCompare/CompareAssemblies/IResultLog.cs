using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CompareAssemblies
{
    public interface IResultLog
    {
        string Context { get; set; }
        void WriteLine(string fmt, params object[] args);
        bool IsDifferent { get; }
    }
}
