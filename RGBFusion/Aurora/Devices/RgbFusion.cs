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
using CSScriptLibrary;

public class RGBFusion
{
    public string devicename = "RGB Fusion";
    public bool enabled = true; //Switch to True, to enable it in Aurora

    public bool Initialize()
    {
        try
        {
            //TODO: Check if RGBFusionsetcolor is up and fire if off
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
	
    public void SendArgs(string[] args)
    {
        using (var pipe = new NamedPipeClientStream(".", "RGBFusion390SetColor", PipeDirection.Out))
        using (var stream = new StreamWriter(pipe))
        {
            pipe.Connect(timeout: 100);
            stream.Write(string.Join(separator: " ", value: args));
        }
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

    public void Reset()
    {
        Shutdown();
        Thread.Sleep(1000);
        Initialize();
    }

    public void Shutdown()
    {
        SendArgs(new string[] { "--shutdown" });
        Thread.Sleep(1000);
    }

    private struct DeviceMapState
    {
        public byte led;
        public Color color;
        public DeviceKeys deviceKey;
        public DeviceMapState(byte led, Color color, DeviceKeys deviceKeys)
        {
            this.led = led;
            this.color = color;
            this.deviceKey = deviceKeys;
        }
    }

	private static Color _initialColor = Color.FromArgb(0, 0, 0);
	
    private List<DeviceMapState> deviceMap = new List<DeviceMapState>
    {
		//             To Area/Key				   From DeviceKey
		new DeviceMapState(1, _initialColor, DeviceKeys.MBAREA_6),
		new DeviceMapState(2, _initialColor, DeviceKeys.MBAREA_3),
		new DeviceMapState(3, _initialColor, DeviceKeys.MBAREA_2),
		new DeviceMapState(5, _initialColor, DeviceKeys.MBAREA_0),
		new DeviceMapState(6, _initialColor, DeviceKeys.MBAREA_4),
		new DeviceMapState(8, _initialColor, DeviceKeys.MBAREA_1),
		new DeviceMapState(9, _initialColor, DeviceKeys.MBAREA_5)
    };

    bool _deviceChanged = true;
    public bool UpdateDevice(Dictionary<DeviceKeys, Color> keyColors, bool forced)
    {
        try
        {
            foreach (KeyValuePair<DeviceKeys, Color> key in keyColors)
            {
                if (key.Key == DeviceKeys.MBAREA_0)
                {
					if (_deviceChanged)
						SendArgs(new string[] { "--transactioncommit" });
                    SendArgs(new[] { "--transactionstart" });
					_deviceChanged = false;
                }
                for (byte d = 0; d < deviceMap.Count; d++)
                {
                    if ((deviceMap[d].deviceKey == key.Key) && (key.Value != deviceMap[d].color))
                    {
                        SendColorToDevice(key.Value, deviceMap[d].led);
                        deviceMap[d] = new DeviceMapState(deviceMap[d].led, key.Value, deviceMap[d].deviceKey);
                        _deviceChanged = true;
                    }
                }
            }
            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    //Custom method to send the color to the device
    private void SendColorToDevice(Color color, int area = -1)
    {
		string command = string.Format(" --sa:{3}:0:{0}:{1}:{2}", Convert.ToInt32(color.R * color.A / 255).ToString(), Convert.ToInt32(color.G * color.A / 255).ToString(), Convert.ToInt32(color.B * color.A / 255).ToString(), area.ToString());
		SendArgs(new string[] { command });
    }
}