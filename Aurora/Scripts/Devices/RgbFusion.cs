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
    public string devicename = "RGB Fusion All Zones";
    public bool enabled = true; //Switch to True, to enable it in Aurora
    private Color device_color = Color.Black;
	
    public bool Initialize()
    {
        try
        {
			//TODO: Check if RGBFusionsetcolor is up and fire if off
			Process.Start(@"C:\Program Files (x86)\GIGABYTE\RGBFusion\RGBFusionCli.exe"); 
            return true;
        }
        catch(Exception exc)
        {
            return false;
        }
    }
    
    public void Reset()
    {
		//TODO: Maybe restart RGBFusionsetcolor Process?
    }
    
    public void Shutdown()
    {
		SendColorToDevice(Color.FromArgb(0,0,0), true);
		Thread.Sleep(500);
        SendArgs(new string[] { "shutdown" });
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
				if(key.Key == DeviceKeys.G6)
                {
                    //For example if we're basing our device color on Peripheral colors
                    SendColorToDevice(key.Value, forced);
                }
            }
            return true;
        }
        catch(Exception exc)
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
			device_color=color;	
			string command = string.Format(" --nc --sa:1:{3}:{0}:{1}:{2}  --sa:2:{3}:{0}:{1}:{2}  --sa:3:{3}:{0}:{1}:{2} --sa:5:{5}:{0}:{1}:{2} --sa:6:{5}:{0}:{1}:{2}  --sa:8:{6}:{0}:{1}:{2} --sa:9:{7}:{0}:{1}:{2}", Convert.ToInt32(color.R*color.A/255).ToString(), Convert.ToInt32(color.G*color.A/255).ToString(), Convert.ToInt32(color.B*color.A/255).ToString(), 0 , 0, 0, 0 , 0) ;
			SendArgs(new string[] { command });
        }
	}
}