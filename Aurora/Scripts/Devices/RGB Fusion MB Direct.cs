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
            CommandAssembler cA = new CommandAssembler();

            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
                //Iterate over each key and color and cache commands
				if(key.Key == DeviceKeys.A)
                {
                    //IO-Shroud
                    cA.addCommand("7", key.Value);
                }
                if(key.Key == DeviceKeys.S)
                {
                    //PCIE-Slots
                    cA.addCommand("3", key.Value);
                }
                if(key.Key == DeviceKeys.D)
                {
                    //RAM Slots
                    cA.addCommand("1", key.Value);
                }
                if(key.Key == DeviceKeys.F)
                {
                    //Sidebar
                    cA.addCommand("2", key.Value);
                } 
                if(key.Key == DeviceKeys.G)
                {
                    cA.addCommand("8", key.Value);
                }
            }
            
            if (cA.hasCommands()){
                SendColorToDevice(cA.returnCommand(), cA.latestColor(), forced);
            }
            return true;
        }
        catch(Exception exc)
        {
            return false;
        }
    }
    
    //Custom method to send the color to the device
    private void SendColorToDevice(String[] commands, Color color, bool forced)
    {
        //Check if device's current color is the same, no need to update if they are the same
		if (_pipeInterOp == null)
			return;
        if (!device_color.Equals(color) || forced)
        {
			_pipeInterOp.SendArgs(commands);
            Global.logger.LogLine(String.Join("; ", commands));
			device_color=color;			
        }
	}
}

internal class CommandAssembler{
    private List<String> commands = new List<String>();
    private Color color = Color.Black;

    public void addCommand(String zone, Color color, String mode = "0"){
        commands.Add(string.Format("--setarea:{0}:{1}:{2}:{3}:{4} ",zone, mode, color.R.ToString(), color.G.ToString(), color.B.ToString()));
        this.color = color;
    }

    public String[] returnCommand(){
        String[] returnString = commands.ToArray();
        commands.Clear();
        return returnString;
    }

    public Color latestColor(){
        return color;
    }

    public bool hasCommands(){
        return commands.Count() != 0;
    }
	
}