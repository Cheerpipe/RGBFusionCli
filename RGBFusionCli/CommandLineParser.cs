using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Windows;
using System.Windows.Media;

namespace RGBFusionCli
{
    public static class CommandLineParser
    {
        public static List<LedCommand> GetLedCommands(string[] args)
        {

            List<LedCommand> ledCommands = new List<LedCommand>();

            foreach (var arg in args)
            {
                var command = ParseLedCommand(arg);
                if (command != null)
                    ledCommands.Add(command);
            }

            return ledCommands;
        }

        public static LedCommand ParseLedCommand(string arg)
        {
            bool directCommand = (arg.ToLower().Contains("--setarea:") || arg.ToLower().Contains("--sa:"));
            bool nonDirectCommand = false;
            if (!directCommand)
                nonDirectCommand = (arg.ToLower().Contains("--setareand:") || arg.ToLower().Contains("--sand:"));
            LedCommand command = null;
            if (directCommand || nonDirectCommand)
            {
                try
                {
                    command = new LedCommand();
                    var commandParts = arg.Split(':');
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
                        command.Speed = sbyte.Parse(commandParts[6]);
                        command.Bright = sbyte.Parse(commandParts[7]);
                    }

                    command.Direct = !nonDirectCommand;

                }
                catch (Exception Ex)
                {
                    MessageBox.Show(string.Format("Wrong --setarea: command in GetLedCommands: {0}", Ex.ToString()));
                }
            }
            return command;
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

        public static bool GeShutdownCommand(string[] args)
        {
            return args != null && args.Any(s => s.ToLower().Contains("--shutdown") || s.ToLower().Contains("--sd"));
        }

        public static bool NoInstanceCheck(string[] args)
        {
            return args != null && args.Any(s => s.ToLower().Contains("--nocheck") || s.ToLower().Contains("--nc"));
        }

        public static bool GetResetCommand(string[] args)
        {
            return args != null && args.Any(s => s.ToLower().Contains("--reset"));
        }

        public static int GetTransactionStartCommand(string[] args)
        {
            if (args != null)
                return ParseTransactionStartCommand(args);
            else
                return -1;
        }

        public static int ParseTransactionStartCommand(string[] args)
        {
            int transactionMaxAliveTime = -1;
            foreach (var arg in args)
            {
                if (arg.ToLower().Contains("--transactionstart"))
                {
                    if (arg.Split(':').Length == 1)
                    {
                        return 0;
                    }
                    else if (arg.Split(':').Length == 2)
                    {
                        try
                        {
                            transactionMaxAliveTime = int.Parse(arg.Split(':')[1]);
                            break;
                        }
                        catch
                        {
                            throw new Exception("Transaction max alive time incorrect value");
                        }
                    }

                    break;
                }
            }
            return transactionMaxAliveTime;
        }

        public static bool GetTransactionCancel(string[] args)
        {
            return args != null && args.Any(s => s.ToLower().Contains("--transactioncancel"));
        }

        public static bool GetTransactionCommitCommand(string[] args)
        {
            return args != null && args.Any(s => s.ToLower().Contains("--transactioncommit"));
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
