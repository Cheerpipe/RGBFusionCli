using RGBFusionAuroraListener;
using System;
using System.IO;
using System.IO.Pipes;


namespace RGBFusionCli
{
    public class Listener
    {
        private bool _StopListening = false;
        private byte _maxCommandLenght;
        public void StartArgsPipeServer(string pipeName = "RGBFusionAuroraListener", byte maxCommandLenght = 6)
        {
            _maxCommandLenght = maxCommandLenght;
            var s = new NamedPipeServerStream(pipeName, PipeDirection.In);
            Action<NamedPipeServerStream> a = GetArgsCallBack;
            a.BeginInvoke(s, callback: ar => { }, @object: null);
        }

        private void GetArgsCallBack(NamedPipeServerStream pipe)
        {

            while (!_StopListening)
            {
                pipe.WaitForConnection();
                var sr = new BinaryReader(pipe);
                var command = sr.ReadBytes(_maxCommandLenght);
                pipe.Disconnect();
                Processor.ProcessCommand(command);
            }
        }
    }
}
