using Aurora;
using Aurora.Devices;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net;
using System.Diagnostics;
using RGBFusion390Sender;

public class RGBFusionNativeDeviceScript
{
    public string devicename = "RGB Fusion MB Direct";
    public bool enabled = true; //Switch to True, to enable it in Aurora
    ArgsPipeInterOp _pipeInterOp = null;
    private Color device_color = Color.Black;

    public bool Initialize()
    {
        try
        {
			//TODO: Check if RGBFusionsetcolor is up
            _pipeInterOp = new ArgsPipeInterOp();
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
		 _pipeInterOp = new ArgsPipeInterOp();
    }
    
    public void Shutdown()
    {
		_pipeInterOp = null;
    }
    
    public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced)
    {
        try
        {
            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
                //Iterate over each key and color and send them to your device
				if(key.Key == DeviceKeys.G)
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
		if (_pipeInterOp == null)
			return;
        if (!device_color.Equals(color) || forced)
        {
			_pipeInterOp.SendArgs(new string[] { string.Format("--setarea:-1:0:{0}:{1}:{2}", color.R.ToString(), color.G.ToString(), color.B.ToString()) });
			device_color=color;			
			Global.logger.LogLine(string.Format("[C# Script] Sent a color, {0} to RGBFusion390SetColor", color));			
        }
	}
}