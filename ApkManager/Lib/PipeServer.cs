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

        NamedPipeServerStream server;
        private string pipename;

        public PipeServer(string pipename)
        {
            this.pipename = pipename;
        }

        public async void StartListening()
        {
            if (server != null) return;
            server = new NamedPipeServerStream(pipename);

            while(true)
            {
                try
                {
                    await Task.Run(() => server.WaitForConnection());
                    using (var sr = new StreamReader(server))
                    {
                        OnMessageReceived?.Invoke(sr.ReadLine());
                    }
                    server?.Disconnect();
                }
                catch(Exception)
                {
                    break;
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
