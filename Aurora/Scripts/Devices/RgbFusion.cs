using Aurora;
using Aurora.Devices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Threading;

public class RGBFusion
{
    public string devicename = "RGB Fusion";
    public bool enabled = true; //Switch to True, to enable it in Aurora
    private Color device_color = Color.Black;

    public bool Initialize()
    {
        try
        {
			KillProcessByName("RGBFusionCli.exe");
			Thread.Sleep(500);
			Process.Start(@"C:\Program Files (x86)\GIGABYTE\RGBFusion\RGBFusionCli.exe");
            return true;
        }
        catch (Exception exc)
        {
            return false;
        }
    }

    public void Reset()
    {
        Shutdown();
        Thread.Sleep(1000);
        Initialize();
    }

    public void Shutdown()
    {
        SendArgs(new string[] { "--shutdown" });
        Thread.Sleep(500);
    }

    public static void KillProcessByName(string processName)
    {
        Process cmd = new Process();
        cmd.StartInfo.FileName = @"C:\Windows\System32\taskkill.exe";
        cmd.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
        cmd.StartInfo.Arguments = string.Format(@"/f /im {0}", processName);
        cmd.Start();
        cmd.Dispose();
    }

    public void SendArgs(string[] args)
    {
        using (var pipe = new NamedPipeClientStream(".", "RGBFusion390SetColor", PipeDirection.Out))
        using (var stream = new StreamWriter(pipe))
        {
            pipe.Connect(timeout: 100);
            stream.Write(string.Join(separator: " ", value: args));
        }
    }

    public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced)
    {
        try
        {
            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
                //Iterate over each key and color and send them to your device
                if (key.Key == DeviceKeys.ESC) //Device will take ESC key color
                {
                    SendColorToDevice(key.Value, forced);
                }
            }
            return true;
        }
        catch (Exception exc)
        {
            return false;
        }
    }

    //Custom method to send the color to the device
    private void SendColorToDevice(Color color, bool forced)
    {
        //Check if device's current color is the same, no need to update if they are the same		
        if (!device_color.Equals(color) || forced)
        {
            device_color = color;
            string command = string.Format(" --nc --sa:-1:0:{0}:{1}:{2}", Convert.ToInt32(color.R * color.A / 255).ToString(), Convert.ToInt32(color.G * color.A / 255).ToString(), Convert.ToInt32(color.B * color.A / 255).ToString());
            SendArgs(new string[] { command });
        }
    }
}