using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RGBFusion390SetColor
{
    public class ArgsPipeInterOp
    {
        public void StartArgsPipeServer()
        {
            NamedPipeServerStream s = new NamedPipeServerStream("RGBFusion390SetColor", PipeDirection.In);
            Action<NamedPipeServerStream> a = GetArgsCallBack;
            a.BeginInvoke(s, ar => { }, null);
        }

        private void GetArgsCallBack(NamedPipeServerStream pipe)
        {
            while (true)
            {
                pipe.WaitForConnection();
                StreamReader sr = new StreamReader(pipe);
                string[] args = sr.ReadToEnd().Split(' ');
                Program.Run(args);
                pipe.Disconnect();
            }
        }

        public void SendArgs(string[] args)
        {
            using (var pipe = new NamedPipeClientStream(".", "RGBFusion390SetColor", PipeDirection.Out))
            using (var stream = new StreamWriter(pipe))
            {
                pipe.Connect(1000);
                stream.Write(string.Join(" ", args));
            }
        }
    }
}
