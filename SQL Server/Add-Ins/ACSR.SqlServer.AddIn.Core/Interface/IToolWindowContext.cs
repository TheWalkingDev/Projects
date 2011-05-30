using System;
namespace PyAddIn.Controller
{
    public interface IToolWindowContext
    {
        dynamic ControlObject { get; set; }
        EnvDTE.Window Window { get; set; }
        EnvDTE.Window Window2 { get; set; }
        
    }
}
