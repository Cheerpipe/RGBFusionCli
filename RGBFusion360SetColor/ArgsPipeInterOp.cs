using System;
using System.IO;
using System.IO.Pipes;


namespace RGBFusion390SetColor
{
    public class ArgsPipeInterOp
    {
        public void StartArgsPipeServer()
        {
            var s = new NamedPipeServerStream("RGBFusion390SetColor", PipeDirection.In);
            Action<NamedPipeServerStream> a = GetArgsCallBack;
            a.BeginInvoke(s, callback: ar => { }, @object: null);
        }

        private static void GetArgsCallBack(NamedPipeServerStream pipe)
        {
            while (true)
            {
                pipe.WaitForConnection();
                var sr = new StreamReader(pipe);
                var args = sr.ReadToEnd().Split(' ');
                Program.Run(args);
                pipe.Disconnect();
            }
            // ReSharper disable once FunctionNeverReturns
        }

        public void SendArgs(string[] args)
        {
            using (var pipe = new NamedPipeClientStream(serverName: ".", pipeName: "RGBFusion390SetColor", PipeDirection.Out))
            using (var stream = new StreamWriter(pipe))
            {
                pipe.Connect(timeout: 1000);
                stream.Write(string.Join(separator: " ", args));
            }
        }
    }
}
