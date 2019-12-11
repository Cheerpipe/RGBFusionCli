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
    public string devicename = "RGB Fusion Z390 wrapper";
    public bool enabled = true; //Switch to True, to enable it in Aurora
    private ArgsPipeInterOp _pipeInterOp = null;
    private Color device_color = Color.Black;
	private int modeDigital = 0;
	private int modeAnalog = 0;
	private int modeIntegrated = 0;
	
    public bool Initialize()
    {
        try
        {
			//TODO: Check if RGBFusionsetcolor is up and fire if off
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
				if(key.Key == DeviceKeys.G1)
                {
                    //For example if we're basing our device color on Peripheral colors
                    SendColorToDevice(key.Value, forced);
					if (modeDigital > 31)
						modeDigital  = 0;

					if (modeAnalog > 31)
						modeAnalog  = 0;

					if (modeIntegrated > 31)
						modeIntegrated  = 0;					
                }
				else if(key.Key == DeviceKeys.G6)
                {
					modeIntegrated = key.Value.R;					
					modeAnalog = key.Value.G;
					modeDigital = key.Value.B;
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
			_pipeInterOp.SendArgs(new string[] { string.Format(" --setarea:1:{3}:{0}:{1}:{2}  --setarea:2:{3}:{0}:{1}:{2}  --setarea:3:{3}:{0}:{1}:{2} --setarea:5:{5}:{0}:{1}:{2}  --setarea:6:{5}:{0}:{1}:{2}  --setarea:8:{3}:{0}:{1}:{2}", color.R.ToString(), color.G.ToString(), color.B.ToString(), modeIntegrated.ToString(), modeAnalog.ToString(), modeDigital.ToString()) });
			device_color=color;			
			Global.logger.LogLine(string.Format("[C# Script] Sent a color, {0} to RGBFusion390SetColor with integrated mode {1}, analog mode {2} and digital mode {3}", color, modeIntegrated.ToString(), modeAnalog.ToString(), modeDigital.ToString()));			
        }
	}
}