using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ApkManager.Lib
{
    class PipeClient
    {
        private string pipename;

        public PipeClient(string pipename)
        {
            this.pipename = pipename;
        }

        public async Task SendMessage(string message)
        {
            using (var client = new NamedPipeClientStream(pipename))
            {
                await Task.Run(()=> client.Connect(1000));
                using (var sw = new StreamWriter(client))
                {
                    sw.AutoFlush = true;
                    sw.WriteLine(message);
                }
            }
        }
    }
}
