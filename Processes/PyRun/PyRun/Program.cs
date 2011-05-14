using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACSR.PythonScripting;
using ACSR.Core.Processes;
using PyRun.Properties;
using System.IO;

namespace PyRun
{
    class Program
    {
        PyRunner _runner;

        void TestMode()
        {
          //  -script "TestScript.py" -args "status" -command "hg" -workingDirectory "d:\Dev\Openbox\Change Controls\GBS Reforecasts Integration"

            Console.OutputEncoding = Encoding.ASCII;
            var cmd = new CmdLineHelper();
            
            _runner = new PyRunner("notepad.exe",
                "TestScript.py",
                "test.txt",
                @"c:\temp", cmd);
            _runner.Run();
           // Console.ReadKey();

        }

        void Run(string[] args)
        {
            //TestMode();
            //return;
            Console.OutputEncoding = Encoding.ASCII;
            var cmd = new CmdLineHelper();
            _runner = new PyRunner(cmd.ParamAfterSwitch("command"),
                cmd.ParamAfterSwitch("script"),
                cmd.ParamAfterSwitch("args"),
                cmd.ParamAfterSwitch("workingDirectory"), cmd);
            _runner.Run();
            //Console.ReadKey();
        }

        static void Main(string[] args)
        {
            new Program().Run(args);
        }
    }
}
