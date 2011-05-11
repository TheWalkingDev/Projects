using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ACSR.Dev.NetCmpLib;

namespace TestNetCmpLib
{
    class Program
    {
        static void Main(string[] args)
        {
            new NetCmp().Compare(
                //@"\\ctbuildvm01\PerFIX\Delivery\Current_RC\Web\bin\PerfixEMS.dll",
                //@"x:\Build\PerFIX\Delivery\PerfixEMS_RC\Web\bin\PerfixEMS.dll",
                //@"s:\PerfixEMS_RC\Core\Common\Peresys.PerfixEMS.Common\bin\Debug\Peresys.PerfixEMS.Common.dll",
                //@"s:\PerfixEMS_RC\Core\Common\Peresys.PerfixEMS.Common\bin\Debug\Peresys.PerfixEMS.Common.dll.old",
                @"s:\PerfixEMS_RC\Core\Common\Peresys.PerfixEMS.Common\bin\Debug\Peresys.PerfixEMS.Common.dll",
                @"\\CTPERFIXBUILDVM.sys.co.za\c$\Build\PerFIX\Development\PerfixEMS_RC\Core\Common\Peresys.PerfixEMS.Common\bin\Debug\Peresys.PerfixEMS.Common.dll",
                @"c:\temp\ildasmtest\Temp\ILDasm\",
                @"c:\temp\ildasmtest\ildasm.exe");

        }
    }
}
 