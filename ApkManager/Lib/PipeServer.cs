using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApkManager.Lib
{
    class PipeServer
    {
        public delegate void MessageEventHander(string message);
        public event MessageEventHander OnMessageReceived;

        private static NamedPipeServerStream server;
        private string pipename;

        public PipeServer(string pipename)
        {
            this.pipename = pipename;
        }

        public async void StartListening()
        {
            if (server != null) return;
            while (true)
            {
                try
                {
                    if (server == null)
                    {
                        server = new NamedPipeServerStream(pipename);
                    }
                    await Task.Run(() => server.WaitForConnection());
                    using (var sr = new StreamReader(server))
                    {
                        OnMessageReceived?.Invoke(sr.ReadLine());
                    }
                    //server?.Disconnect();
                }
                catch(Exception ex)
                {
                    Debug.Print("PipeServer: {0}", ex.Message);
                    //break;
                }
                finally
                {
                    server?.Dispose();
                    server = null;
                }
            }
        }

        public void StopListening()
        {
            server?.Dispose();
            server = null;
        }
    }
}
