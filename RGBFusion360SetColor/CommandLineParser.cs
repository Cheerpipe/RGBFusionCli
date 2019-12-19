using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Media;

namespace RGBFusion390SetColor
{
    public static class CommandLineParser
    {
        public static List<LedCommand> GetLedCommands(string[] args)
        {
            List<LedCommand> ledCommands = new List<LedCommand>();

            foreach (var arg in args)
            {

                bool directCommand = (arg.ToLower().Contains("--setarea:") || arg.ToLower().Contains("--sa:"));
                bool nonDirectCommand = (arg.ToLower().Contains("--setareand:") || arg.ToLower().Contains("--sand:"));

                if (directCommand || nonDirectCommand)
                {
                    try
                    {
                        var commandParts = arg.Split(':');
                        var command = new LedCommand();
                        if (commandParts.Length < 6)
                        {
                            throw new Exception("Wrong value count in " + arg);
                        }

                        if (commandParts.Length >= 6)
                        {

                            command.AreaId = sbyte.Parse(commandParts[1]);
                            command.NewMode = sbyte.Parse(commandParts[2]);
                            command.NewColor = Color.FromRgb(byte.Parse(commandParts[3]), byte.Parse(commandParts[4]), byte.Parse(commandParts[5]));
                            command.Bright = 9;
                            command.Speed = 2;
                        }
                        if (commandParts.Length >= 8)
                        {
                            command.Speed = sbyte.Parse(arg.Split(':')[6]);
                            command.Bright = sbyte.Parse(arg.Split(':')[7]);
                        }
                        if (nonDirectCommand)
                            command.Direct = false;

                        ledCommands.Add(command);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(messageBoxText: "Wrong --setarea: command in GetLedCommands: {ex.Message}");
                    }
                }
            }
            return ledCommands;
        }

        public static int LoadProfileCommand(string[] args)
        {
            var profileId = -1;
            foreach (var arg in args)
            {
                if (arg.ToLower().Contains("--loadprofile:"))
                {
                    try
                    {
                        profileId = sbyte.Parse(arg.Split(':')[1]);
                        break;
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Wrong --loadprofile: command in LoadProfileCommand: {ex.Message}");
                    }
                }
            }
            return profileId;
        }

        public static bool GetAreasCommand(string[] args)
        {
            return args != null && args.Any(s => s.ToLower().Contains("--getareas") || s.ToLower().Contains("--areas"));
        }

        public static bool StartMusicMode(string[] args)
        {
            return args.Any(s => s.ToLower().Contains("--startmusicmode"));
        }
        public static bool StopMusicMode(string[] args)
        {
            return args.Any(s => s.ToLower().Contains("--startmusicmode"));
        }
    }
}
