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
        private NamedPipeServerStream _namedPipeServerStream;
        public bool Listening { get => !_StopListening; }

        public void Start(string pipeName = "RGBFusionAuroraListener", byte maxCommandLenght = 6)
        {
            _maxCommandLenght = maxCommandLenght;
            _namedPipeServerStream = new NamedPipeServerStream(pipeName, PipeDirection.In);
            Action<NamedPipeServerStream> a = GetArgsCallBack;
            a.BeginInvoke(_namedPipeServerStream, callback: ar => { }, @object: null);
        }

        public void Stop()
        {
            _StopListening = true;
            // IAsyncResult result = null;
            
            if (_namedPipeServerStream.IsConnected)
                _namedPipeServerStream.Disconnect();
            _namedPipeServerStream.Close();
            
        }

        private void GetArgsCallBack(NamedPipeServerStream pipe)
        {

            while (!_StopListening)
            {
                pipe.WaitForConnection();
                if (_StopListening)
                    return;
                var sr = new BinaryReader(pipe);
                var command = sr.ReadBytes(_maxCommandLenght);
                pipe.Disconnect();
                Processor.ProcessCommand(command);
            }
        }
    }
}