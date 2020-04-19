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
	private int modeVGA = 0;	
	private int modeRAM = 0;		
	
	
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
		if (_pipeInterOp == null)
			return;
        //Check if device's current color is the same, no need to update if they are the same		
        if (!device_color.Equals(color) || forced)
        {
			device_color=color;	
			string command = string.Format(" --sa:1:{3}:{0}:{1}:{2}  --sa:2:{3}:{0}:{1}:{2}  --sa:3:{3}:{0}:{1}:{2} --sa:5:{5}:{0}:{1}:{2} --sa:6:{5}:{0}:{1}:{2}  --sa:8:{6}:{0}:{1}:{2} --sa:9:{7}:{0}:{1}:{2}", Convert.ToInt32(color.R*color.A/255).ToString(), Convert.ToInt32(color.G*color.A/255).ToString(), Convert.ToInt32(color.B*color.A/255).ToString(), 0 , 0, 0, 0 , 0) ;
			 _pipeInterOp.SendArgs(new string[] { command });
        }
	}
}
