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
            Dictionary <DeviceKeys, String> keyMap = new Dictionary<DeviceKeys, String> {
                {DeviceKeys.A, "7"},    //IO-Shroud
                {DeviceKeys.S, "3"},    //PCIE-Slots
                {DeviceKeys.A, "1"},    //RAM-Slots
                {DeviceKeys.A, "2"},    //Sidebar
                {DeviceKeys.A, "8"},    //ARGB-Strip
            };

            //collect commands
            CommandAssembler cA = new CommandAssembler();

            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
                //Iterate over each key and color and cache commands
                if ( keyMap.ContainsKey(key.Key))
                {
                    cA.addCommand(keyMap[key.Key], key.Value);
                }   
            }

            if (cA.hasCommands()){
                SendColorToDevice(cA, forced);
            }
            return true;
        }
        catch (Exception exc)
        {
            return false;
        }
    }

    //Custom method to send the color to the device
    private void SendColorToDevice(CommandAssembler cA, bool forced)
    {
       //TODO: Make it check all individual zone colors and only update current one instead of only checking the most recently changed zone
       color = cA.latestColor();
        //Check if device's current color is the same, no need to update if they are the same		
        if (!device_color.Equals(color) || forced)
        {
            device_color = color;
            SendArgs(cA.returnCommand());
        }
    }
}

internal class CommandAssembler{
    private List<String> commands = new List<String>();
    private Color color;

    public void addCommand(String zone, Color color, String mode = "0"){
        commands.Add(string.Format(" --nc --sa:{0}:{1}:{2}:{3}:{4}", zone, mode, Convert.ToInt32(color.R * color.A / 255).ToString(), Convert.ToInt32(color.G * color.A / 255).ToString(), Convert.ToInt32(color.B * color.A / 255).ToString()));
        this.color = color;
    }

    public String[] returnCommand(){
        String[] returnString = commands.ToArray();
        return returnString;
    }

    public Color latestColor(){
        if (this.hasCommands)
        {
            return color;
        }
        else throw new SystemException();
    }

    public bool hasCommands(){
        return commands.Count() != 0;
    }
	
}