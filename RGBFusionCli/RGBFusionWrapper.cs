using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media;
using Gigabyte.ULightingEffects.Devices;
using LedLib2;
using LedLib2.GBPeripheralsLedAPI;
using RGBFusionCli.Wrappers;
using SelLEDControl;

namespace RGBFusionCli
{
    public class RgbFusion
    {
        private const int REPEAT_LAST_COMMAND_TIMEOUT = 70; //ms to wait to re send last color to rams to ensure it change after a command flow
        public Comm_LED_Fun _ledFun;
        public Comm_LED_Fun _ledFunSlow;
        private bool _areaChangeApplySuccess;
        private bool _scanDone;
        private List<CommUI.Area_class> _allAreaInfo;
        private List<CommUI.Area_class> _allExtAreaInfo;
        private List<LedCommand> _commands;
        private bool _initialized;
        private Color _RAMNewColor = Color.FromArgb(0, 0, 0, 0);
        private Thread _MainBoardRingCommandsThread;
        readonly ManualResetEvent _MainBoardRingCommandEvent = new ManualResetEvent(false);
        private List<CommUI.Area_class> MainboardCommandsCommands = new List<CommUI.Area_class>();
        private Timer _repeatLastCommandTimer;

        public bool IsInitialized()
        {
            return _ledFun != null && _initialized;
        }

        public void LoadProfile(int profileId)
        {
            _ledFun.Adv_mode_Apply(GetAllAreaInfo(profileId), GetAllExtAreaInfo(profileId));
            do
            {
                Thread.Sleep(10);
            }
            while (!_areaChangeApplySuccess);
        }

        private void FillAllAreaInfo()
        {
            _allAreaInfo = GetAllAreaInfo();
        }

        public void Shutdown()
        {
            _MainBoardRingCommandsThread.Abort();
        }

        private List<CommUI.Area_class> GetAllAreaInfo(int profileId = -1)
        {
            if (profileId < 1)
            {
                profileId = _ledFun.Current_Profile;
            }
            var str = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "\\GIGABYTE\\RGBFusion\\Pro", profileId.ToString(), ".xml");
            var color = CommUI.Int_To_Color((uint)CommUI.Get_Default_Color_from_Appcenter());
            if (!File.Exists(str))
            {
                Creative_Profile(str, _ledFun.Get_MB_Area_number(), color, string.Concat("Profile ", profileId.ToString()));
            }
            return CommUI.Inport_from_xml(str, null);
        }

        public static void Creative_Profile(string proXmlFilePath, int areaCount, Color defaultColor, string profileName = "")
        {
            var lang = new Lang();
            var areaClasses = new List<CommUI.Area_class>();
            for (var i = 0; i < areaCount; i++)
            {
                if (areaCount != 8 && areaCount != 9)
                {
                    var patternCombItem = new CommUI.Pattern_Comb_Item
                    {
                        Type = 0,
                        Bg_Brush_Solid = { Color = defaultColor },
                        Sel_Item = { Style = null }
                    };
                    patternCombItem.Sel_Item.Background = patternCombItem.Bg_Brush_Solid;
                    patternCombItem.Sel_Item.Content = lang.Get_Lang_Resource("Still");
                    patternCombItem.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(patternCombItem.Bg_Brush_Solid);
                    areaClasses.Add(new CommUI.Area_class(patternCombItem, i, null));
                }
                else if (i == 7 || i == 8)
                {
                    var mPatternInfo = new CommUI.Pattern_Comb_Item
                    {
                        Type = 0,
                        Bg_Brush_Solid = { Color = defaultColor },
                        Sel_Item = { Style = null }
                    };
                    mPatternInfo.Sel_Item.Background = mPatternInfo.Bg_Brush_Solid;
                    mPatternInfo.Sel_Item.Content = lang.Get_Lang_Resource("Still");
                    mPatternInfo.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(mPatternInfo.Bg_Brush_Solid);
                    areaClasses.Add(new CommUI.Area_class(mPatternInfo, i, null));
                }
                else
                {
                    var bgBrushSolid = new CommUI.Pattern_Comb_Item
                    {
                        Type = 0,
                        Bg_Brush_Solid = { Color = defaultColor },
                        Sel_Item = { Style = null }
                    };
                    bgBrushSolid.Sel_Item.Background = bgBrushSolid.Bg_Brush_Solid;
                    bgBrushSolid.Sel_Item.Content = lang.Get_Lang_Resource("Still");
                    bgBrushSolid.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(bgBrushSolid.Bg_Brush_Solid);
                    areaClasses.Add(new CommUI.Area_class(bgBrushSolid, i, null));
                }
            }
            CommUI.Export_to_xml(areaClasses, proXmlFilePath, profileName);
        }

        public static void Creative_Profile_Ext(string proXmlFilePath, List<Comm_LED_Fun.Ext_Led_class> extAreaInfo, Color defaultColor, string profileName = "")
        {
            var areaClasses = new List<CommUI.Area_class>();
            foreach (var areaInfo in extAreaInfo)
            {
                var patternCombItem = new CommUI.Pattern_Comb_Item
                {
                    Type = 0,
                    Bg_Brush_Solid = { Color = defaultColor },
                    Sel_Item = { Style = null }
                };
                patternCombItem.Sel_Item.Background = patternCombItem.Bg_Brush_Solid;
                patternCombItem.Sel_Item.Content = string.Empty;
                patternCombItem.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(patternCombItem.Bg_Brush_Solid);
                var areaClass = new CommUI.Area_class(patternCombItem, areaInfo.DivsNum, null)
                {
                    Ext_Area_id = areaInfo.extLDev
                };
                areaClasses.Add(areaClass);
            }
            CommUI.Export_to_xml(areaClasses, proXmlFilePath, profileName);
        }

        private void Fill_ExtArea_info()
        {
            _allExtAreaInfo = GetAllExtAreaInfo();
        }

        private List<CommUI.Area_class> GetAllExtAreaInfo(int profileId = -1)
        {
            List<CommUI.Area_class> allExtAreaInfo;
            if (profileId < 1)
            {
                profileId = _ledFun.Current_Profile;
            }
            var str = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "\\GIGABYTE\\RGBFusion\\ExtPro", profileId.ToString(), ".xml");
            var str1 = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "\\GIGABYTE\\RGBFusion\\TempP.xml");
            var color = CommUI.Int_To_Color((uint)CommUI.Get_Default_Color_from_Appcenter());
            if (!File.Exists(str))
            {
                Creative_Profile_Ext(str, _ledFun.LEd_Layout.Ext_Led_Array, color, string.Concat("ExProfile ", profileId.ToString()));
                allExtAreaInfo = CommUI.Inport_from_xml(str, null);
                return allExtAreaInfo;
            }
            Creative_Profile_Ext(str1, _ledFun.LEd_Layout.Ext_Led_Array, color, string.Concat("ExProfile ", profileId.ToString()));
            allExtAreaInfo = ReImport_ExtInfo(CommUI.Inport_from_xml(str, null), CommUI.Inport_from_xml(str1, null));
            File.Delete(str1);
            return allExtAreaInfo;
        }

        private static List<CommUI.Area_class> ReImport_ExtInfo(List<CommUI.Area_class> orgExtArea, IEnumerable<CommUI.Area_class> newExtArea)
        {
            var areaClasses = new List<CommUI.Area_class>();
            foreach (var area in newExtArea)
            {
                var num = 0;
                while (num < orgExtArea.Count)
                {
                    if (area.Ext_Area_id != orgExtArea[num].Ext_Area_id)
                    {
                        num++;
                    }
                    else
                    {
                        area.Pattern_info = new CommUI.Pattern_Comb_Item(orgExtArea[num].Pattern_info);
                        orgExtArea.RemoveAt(num);
                        break;
                    }
                }
                areaClasses.Add(area);
            }
            return areaClasses;
        }

        public void Init(bool startListening = true)
        {
            var initThread = new Thread(DoInit);
            initThread.SetApartmentState(ApartmentState.STA);
            initThread.Start();
            initThread.Join();

            if (startListening)
                StartListening();
        }

        public void StartListening()
        {
            _MainBoardRingCommandsThread = new Thread(SetMainboardRingAreas);
            _MainBoardRingCommandsThread.SetApartmentState(ApartmentState.STA);
            _MainBoardRingCommandsThread.Start();
            _MainBoardRingCommandsThread.Join();
        }

        public void ChangeColorForAreas(List<LedCommand> commands)
        {
            _commands = commands;
            Thread.Sleep(1); //Give time to set variables beffore kick in events
            _repeatLastCommandTimer.Change(Timeout.Infinite, Timeout.Infinite);
            _MainBoardRingCommandEvent.Set();
            _repeatLastCommandTimer.Change(REPEAT_LAST_COMMAND_TIMEOUT, REPEAT_LAST_COMMAND_TIMEOUT);
        }

        public void Reset()
        {
            Init();
        }

        public void SetAllAreas(object obj)
        {
            var patternCombItem = new CommUI.Pattern_Comb_Item
            {
                Bg_Brush_Solid = { Color = (Color)obj },
                Sel_Item = { Style = null }
            };

            patternCombItem.Sel_Item.Background = patternCombItem.Bg_Brush_Solid;
            patternCombItem.Sel_Item.Content = string.Empty;
            patternCombItem.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(patternCombItem.Bg_Brush_Solid);
            patternCombItem.But_Args[0].Scenes_type = 0;
            patternCombItem.But_Args[1].Scenes_type = 0;
            patternCombItem.But_Args[0].TransitionsTeime = 10;
            patternCombItem.But_Args[1].TransitionsTeime = 10;
            patternCombItem.Bri = 9;
            patternCombItem.Speed = 2;
            patternCombItem.Type = 0;
            var allAreaInfo = _allAreaInfo.Select(areaInfo => new CommUI.Area_class(patternCombItem, areaInfo.Area_index, null)).ToList();
            var allExtAreaInfo = _allExtAreaInfo.Select(areaInfo => new CommUI.Area_class(patternCombItem, areaInfo.Area_index, null) { Ext_Area_id = areaInfo.Ext_Area_id }).ToList();

            allAreaInfo.AddRange(allExtAreaInfo);
            _ledFun.Set_Adv_mode(allAreaInfo, true);
        }

        private void CreateAreaCommands()
        {

            if (_commands.Count <= 0)
                return;

            MainboardCommandsCommands.Clear();
            foreach (var command in _commands)
            {

                if (command.AreaId == -1)
                {
                    SetAllAreas(command.NewColor);
                    continue;
                }

                var patternCombItem = new CommUI.Pattern_Comb_Item
                {
                    Bg_Brush_Solid = { Color = command.NewColor },
                    Sel_Item = { Style = null }
                };

                patternCombItem.Sel_Item.Background = patternCombItem.Bg_Brush_Solid;
                patternCombItem.Sel_Item.Content = string.Empty;
                patternCombItem.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(patternCombItem.Bg_Brush_Solid);

                patternCombItem.Bri = command.Bright;
                patternCombItem.Speed = command.Speed;

                patternCombItem.Type = command.NewMode;
                var area = new CommUI.Area_class(patternCombItem, command.AreaId, null);

                foreach (var extAreaInfo in _allExtAreaInfo.Where(extAreaInfo => extAreaInfo.Area_index == area.Area_index))
                {
                    area.Ext_Area_id = extAreaInfo.Ext_Area_id;
                }

                if (area.Ext_Area_id == ExtLedDev.None) //Mainboard
                {
                    MainboardCommandsCommands.Add(area);
                }
                else if (area.Ext_Area_id == ExtLedDev.Kingston_RAM) //KinstoM RAM by SMBUS
                {
                    // _RAMNewColor = ((command.NewColor.R & 0x0ff) << 16) | ((command.NewColor.G & 0x0ff) << 8) | (command.NewColor.B & 0x0ff);
                    new Task(() => { SetRamColor(command.NewColor); }).Start();
                }

                else if (area.Ext_Area_id == ExtLedDev.GB_VGACard) //KinstoM RAM by Nvidia i2C. A lot faster than setting ram but still slow using rgbfusion methods.
                {
                    //Set VGA in special mode or get low framerate and very high cpu usage. GvLedLib shit
                    _RAMNewColor = command.NewColor;
                    new Task(() => { GvledLib.SetColorToVga(System.Drawing.Color.FromArgb(255, command.NewColor.R, command.NewColor.G, command.NewColor.B)); }).Start();
                }
                else
                {
                    MainboardCommandsCommands.Add(area); // Left devices, for now i left this just for compatibility but it may be slow on some rigs.
                }
            }
        }
        public void SetMainboardRingAreas()
        {
            while (_MainBoardRingCommandsThread.IsAlive)
            {
                _MainBoardRingCommandEvent.WaitOne();
                CreateAreaCommands();
                if (MainboardCommandsCommands.Count > 0)
                {
                    _ledFun.Set_Adv_mode(MainboardCommandsCommands, true); // Just Mainboard can work in direct mode with RGBFusion gigabyte dlls. Managed to get direct mode on VGA with lower level dll.
                }
                _MainBoardRingCommandEvent.Reset();
            }
        }


        private void SetLastRamColorTimerTick(object state)
        {
            _repeatLastCommandTimer.Change(Timeout.Infinite, Timeout.Infinite);
            SetRamColor(_RAMNewColor); // Setting ram color is slow, some times it won't set last color so i will re-set RAM after it stops last update.
        }

        private bool _SettingRAMLeds = false;
        public void SetRamColor(Color color)
        {
            if (!_SettingRAMLeds)
            {
                _SettingRAMLeds = true;
                // _ram_led_obj?.Software_Control(-1, color, -1, -1, true);
                SetKinstomRamDirect(color);
                Thread.Sleep(1);
                _SettingRAMLeds = false;
            }
        }

        public string GetAreasReport()
        {
            var areasReport = string.Empty;
            if (_allAreaInfo.Count > 0)
            {
                areasReport += "Areas detected on " + _ledFun.Product_Name + Environment.NewLine;
                areasReport = _allAreaInfo.Aggregate(areasReport, (current, area) => current + ("    Area ID: " + area.Area_index + Environment.NewLine));
            }
            areasReport += Environment.NewLine;

            if (_allExtAreaInfo.Count > 0)
                foreach (var areaInfo in _ledFun.LEd_Layout.Ext_Led_Array)
                {
                    var extDevice = _ledFun.Get_Ext_Led_Tip(areaInfo.extLDev);
                    areasReport += "Areas detected on " + extDevice + Environment.NewLine;
                    areasReport = _allExtAreaInfo.Where(area => area.Ext_Area_id == areaInfo.extLDev).Aggregate(areasReport, (current, area) => current + ("    Area ID: " + area.Area_index + Environment.NewLine));
                }
            areasReport += Environment.NewLine;
            areasReport += "Use Area ID -1 to set all areas at the same time.";

            return areasReport;
        }

        private void CallBackLedFunApplyScanPeripheralSuccess() => _scanDone = true;
        private void CallBackLedFunApplyEzSuccess() => _areaChangeApplySuccess = true;
        private void CallBackLedFunApplyAdvSuccess() => _areaChangeApplySuccess = true;

        private void DoInit()
        {
            _repeatLastCommandTimer = new Timer(new TimerCallback(SetLastRamColorTimerTick), null, Timeout.Infinite, Timeout.Infinite); //70ms apperas to be the lower vald value. 100 is even better but dont look good.
            // _repeatLastCommandTimmer.Elapsed += OnTimedEvent;
            // _repeatLastCommandTimmer.AutoReset = false;
            _ledFun = new Comm_LED_Fun(false);
            _ledFun.Apply_ScanPeriphera_Scuuess += CallBackLedFunApplyScanPeripheralSuccess;
            _ledFun.ApplyEZ_Success += CallBackLedFunApplyEzSuccess;
            _ledFun.ApplyAdv_Success += CallBackLedFunApplyAdvSuccess;

            _ledFun.Ini_LED_Fun();

            _ledFun = CommUI.Get_Easy_Pattern_color_Key(_ledFun);

            _ledFun.LEd_Layout.Set_Support_Flag();
            do
            {
                Thread.Sleep(10);
            }
            while (!_scanDone);

            _ledFun.Current_Mode = 1; 

            _ledFun.Led_Ezsetup_Obj.PoweronStatus = 1;
            _ledFun.Set_Sync(false);
            FillAllAreaInfo();
            Fill_ExtArea_info();


            _ledObject = (LedObject)typeof(Comm_LED_Fun).GetField("LedObj", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this._ledFun);
            _gb_led_periphs = (GBLedPeripherals)typeof(LedObject).GetField("gb_led_periphs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_ledObject);
            _ram_led_obj = (PeripheralDeviceManagement)typeof(LedObject).GetField("ram_led_obj", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_ledObject);

            //Initialize mode and bright.
            _ram_led_obj?.Software_Control(1, 55555, 0, 9, true);

            _ledObject.bSyncRAM = false;
            _ledObject.bSyncVGA = false;
            _initialized = true;
        }

        private void SetKinstomRamDirect(Color color)
        {
            byte pNull = 0;
            byte mcuAddr = 78;//Specific for Aorus Master Z390 and Kinstom RAM
            int commandDelay = 2;
            SMBusCtrl.MCU_Rw(mcuAddr, 225, 1, ref pNull, 0, 0); //Start command
            SMBusCtrl.MCU_Rw(mcuAddr, 236, (byte)color.R, ref pNull, 0, 0);
            Thread.Sleep(commandDelay);
            SMBusCtrl.MCU_Rw(mcuAddr, 237, (byte)color.G, ref pNull, 0, 0);
            Thread.Sleep(commandDelay);
            SMBusCtrl.MCU_Rw(mcuAddr, 238, (byte)color.B, ref pNull, 0, 0);
            Thread.Sleep(commandDelay);
            SMBusCtrl.MCU_Rw(mcuAddr, 225, 2, ref pNull, 0, 0); //Close command
            SMBusCtrl.MCU_Rw(mcuAddr, 225, 3, ref pNull, 0, 0); // Apply command
        }

        private LedObject _ledObject;
        private PeripheralDeviceManagement _ram_led_obj;
        private GBLedPeripherals _gb_led_periphs;
    }
}
