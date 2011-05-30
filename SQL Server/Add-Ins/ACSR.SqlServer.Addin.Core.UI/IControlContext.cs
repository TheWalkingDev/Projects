using System;
using System.Runtime.InteropServices;
namespace ACSR.SqlServer.Addin.Core.UI
{
    [ComVisible(true)]
    public interface IControlContext
    {
        IntPtr Handle { get; }
    }
}
