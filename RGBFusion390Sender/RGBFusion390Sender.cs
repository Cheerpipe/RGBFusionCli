using System.IO;
using System.IO.Pipes;

namespace RGBFusion390Sender
{
    // ReSharper disable once UnusedMember.Global
    public class ArgsPipeInterOp
    {
        // ReSharper disable once UnusedMember.Global
        public void SendArgs(string[] args)
        {
            using (var pipe = new NamedPipeClientStream(".", "RGBFusion390SetColor", PipeDirection.Out))
            using (var stream = new StreamWriter(pipe))
            {
                pipe.Connect(timeout: 1000);
                stream.Write(string.Join(separator: " ", value: args));
            }
        }
    }
}


