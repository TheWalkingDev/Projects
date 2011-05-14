using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACSR.PythonScripting;
using ACSR.Core.Processes;
using PyRun.Properties;

namespace PyRun
{
    public class PyRunner
    {
        ScriptController _controller;
        string command;

        public string Command
        {
            get { return command; }
            set { command = value; }
        }
        string script;

        public string Script
        {
            get { return script; }
            set { script = value; }
        }
        string args;

        public string CommandArguments
        {
            get { return args; }
            set { args = value; }
        }
        string workingDirectory;

        public string WorkingDirectory
        {
            get { return workingDirectory; }
            set { workingDirectory = value; }
        }
        ProcessFactory _processFactory;
        ICommandParameters _commandParameters;
        public PyRunner(string command, string script, string args, string workingDirectory,
            ICommandParameters commandParameters)
        {
            this.command = command;
            this.script = script;
            this.args = args;
            this.workingDirectory = workingDirectory;
             _processFactory = new ProcessFactory();
             _commandParameters = commandParameters;
        }
        PyProcessContext _mainProcess;

        
        public void Run()
        {
            // var code = new StreamReader(Resources.ResourceManager.GetStream("TextFile1.py")).ReadToEnd();
            _controller = new ScriptController(false);
            _controller.OnMessage += (sender, message) =>
                {
                    //Console.WriteLine(message);
                   // Console.WriteLine(String.Format("[{0:HH:MM:ss:fff}][SCRIPT]:{1}", DateTime.Now, message.Trim()));
                    Console.WriteLine(String.Format("{0}", message.Trim()));
                   
                };
            var code = Resources.ResourceManager.GetString("ScriptHeader1");
            dynamic handler = null;
            IScriptContext ctx;
            try
            {

                if (string.IsNullOrEmpty(script))
                {
                    script = code;
                    ctx = _controller.CreateScriptContextFromString(script);
                    ctx.Execute();
                }
                else
                {
                    ctx = _controller.CreateScriptContextFromFile(script);
                    ctx.ExecuteString(code);
                    ctx.Execute();
                }
             
           
               
                
                handler = ctx.Scope.CreateHandler();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                return;
            }
        

            _mainProcess = new PyProcessContext(ctx, handler,
                _processFactory,
                command,
                CommandArguments,
                workingDirectory,
                _commandParameters);

            
            _mainProcess.Start();

            ctx.FlushBuffer();
           // Console.ReadKey();
        }


       
       
    }
    public class PyProcessContext
    {
        dynamic _processHandler;
        ProcessContext _processContext;

        public ProcessContext ProcessContext
        {
            get { return _processContext; }
           
        }
        ProcessFactory _processFactory;
        string _command;
        string _commandArguments;
        string _workingDirectory;
        IScriptContext _scriptContext;
        ICommandParameters _commandParameters;

        public PyProcessContext(IScriptContext scriptContext,
            dynamic processHandler, 
            ProcessFactory processFactory, 
            string command,
            string commandArguments,
            string workingDirectory,
            ICommandParameters commandParameters)
        {
            _commandParameters = commandParameters;
            _scriptContext = scriptContext;
            _command = command;
            _commandArguments = commandArguments;
            _workingDirectory = workingDirectory;
            _processFactory = processFactory;
            var processContext = _processFactory.CreateProcessContext(command, 
                commandArguments, 
                workingDirectory);
            _processHandler = processHandler;
            processContext.OnMessage += (message) =>
            {
                _processHandler.OnMessage(message);
                //  Console.WriteLine(message);
            };
            processContext.OnError += (message) =>
            {
                _processHandler.OnError(message);
            };
            _processHandler.OnInit(this, _commandParameters);
            _processContext = processContext;
        }
        public void Start()
        {
            _processHandler.OnCommandStarting();
            _processContext.Start();
            _processHandler.OnCommandCompleted();
        }

        public PyProcessContext CreateSpawnedProcess(string command,
            string commandArguments)
        {
            return CreateSpawnedProcess(this._command,
                this._commandArguments,
                this._workingDirectory,
                this._processHandler);
        }

        public PyProcessContext CreateSpawnedProcess(string command,
            string commandArguments,
            string workingDirectory,
            dynamic processHandler)
        {
            if (workingDirectory == null)
            {
                workingDirectory = _workingDirectory;
            }
            var cmd = new CmdLineHelper();
            cmd.ParseString(commandArguments);
            return new PyProcessContext(_scriptContext, 
                processHandler, 
                this._processFactory,
                command,
                commandArguments,
                workingDirectory,
                cmd);
        }
    }

    class NullHandler
    {
        dynamic OnInit(object processContext)
        {
            return null;
        }
        dynamic OnError(string message)
        {
            return null;
        }
        dynamic OnMessage(string message)
        {
            return null;
        }
        dynamic OnCommandStarting()
        {
            return null;
        }
        dynamic OnCommandCompleted()
        {
            return null;
        }
        
 
        
   
    }
}
