using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;

namespace PortListener
{
    class Program
    {
        
        static void Main(string[] args)
        {
            new Program().Run(args);
           
        }

        private void Run(string[] args)
        {
            int port = Int32.Parse(args[0]);
            Console.WriteLine("Started listening on port: {0}", port);
            var server = new TcpListener(IPAddress.Any, port);
            server.Start();
            server.BeginAcceptSocket(new AsyncCallback(server_onAccept), server);

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();
        }

        void server_onAccept(IAsyncResult ar)
        {
            var server = ((TcpListener)ar.AsyncState);
            var socket = server.EndAcceptSocket(ar);
            
            Console.WriteLine(string.Format("Socket {0} Connected",socket.RemoteEndPoint.ToString()));
            server.BeginAcceptSocket(new AsyncCallback(server_onAccept), server);
        }
    }
}
