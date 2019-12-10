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
	
	//Mode is passed in R chanel of G5 Keys
	/*
		0 = Still
		1 = Pulse
		2 = Path
		3 = Rainbown
	*/
	private int mode;
	private int defaultMode = 2;
	
    private ArgsPipeInterOp _pipeInterOp = null;
    private Color device_color = Color.Black;

    public bool Initialize()
    {
        try
        {
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
				else if(key.Key == DeviceKeys.G5)
                {
					mode = key.Value.R; 
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
			switch (mode)
			{
				case 0: //Still
					_pipeInterOp.SendArgs(new string[] { string.Format(" --setarea:-1:0:{0}:{1}:{2}", color.R.ToString(), color.G.ToString(), color.B.ToString()) });			
					break;
				case 1: //Pulse
					_pipeInterOp.SendArgs(new string[] { string.Format(" --setarea:-1:1:{0}:{1}:{2}", color.R.ToString(), color.G.ToString(), color.B.ToString()) });			
					break;
				case 2: //Path
					_pipeInterOp.SendArgs(new string[] { string.Format(" --setarea:1:0:{0}:{1}:{2}  --setarea:2:0:{0}:{1}:{2}  --setarea:3:0:{0}:{1}:{2}  --setarea:5:13:{0}:{1}:{2}  --setarea:6:0:{0}:{1}:{2}  --setarea:8:0:{0}:{1}:{2} ", color.R.ToString(), color.G.ToString(), color.B.ToString()) });			
					break;
				case 3: //Rainbown
					_pipeInterOp.SendArgs(new string[] { string.Format("--setarea:0:0:{0}:{1}:{2} --setarea:1:2:{0}:{1}:{2}  --setarea:2:2:{0}:{1}:{2}  --setarea:3:2:{0}:{1}:{2}  --setarea:4:2:{0}:{1}:{2}  --setarea:5:13:{0}:{1}:{2}  --setarea:6:13:{0}:{1}:{2}  --setarea:7:2:{0}:{1}:{2}  --setarea:8:2:{0}:{1}:{2} ", color.R.ToString(), color.G.ToString(), color.B.ToString()) });
					break;
				default: 
					goto case 2; //Path
					break;
			  }			
			device_color=color;			
			Global.logger.LogLine(string.Format("[C# Script] Sent a color, {0} to RGBFusion390SetColor with mode {1}", color, mode));			
        }
	}
}


/*
AREAS
0
1
2
3
4
5
6
7


*/
