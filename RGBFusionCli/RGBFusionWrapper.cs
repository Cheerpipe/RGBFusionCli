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
        private Dictionary<int, CommUI.Area_class> _allAreaInfo = new Dictionary<int, CommUI.Area_class>();

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
            _allAreaInfo.Clear();
            foreach (CommUI.Area_class area in GetAllAreaInfo())
            {
                var patternCombItem = new CommUI.Pattern_Comb_Item
                {
                    Bg_Brush_Solid = { Color = area.Pattern_info.Bg_Brush_Solid.Color },
                    Sel_Item = { Style = null }
                };

                patternCombItem.Sel_Item.Background = patternCombItem.Bg_Brush_Solid;
                patternCombItem.Sel_Item.Content = string.Empty;
                patternCombItem.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(patternCombItem.Bg_Brush_Solid);
                patternCombItem.Bri = area.Pattern_info.Bri;
                patternCombItem.Speed = area.Pattern_info.Speed;
                patternCombItem.Type = area.Pattern_info.Type;
                CommUI.Area_class newArea = new CommUI.Area_class(patternCombItem, area.Area_index, null);
                _allAreaInfo.Add(area.Area_index, newArea);
            }

            var y = GetAllExtAreaInfo();
            foreach (CommUI.Area_class extArea in GetAllExtAreaInfo())
            {

                var patternCombItem = new CommUI.Pattern_Comb_Item
                {
                    Bg_Brush_Solid = { Color = extArea.Pattern_info.Bg_Brush_Solid.Color },
                    Sel_Item = { Style = null }
                };

                patternCombItem.Sel_Item.Background = patternCombItem.Bg_Brush_Solid;
                patternCombItem.Sel_Item.Content = string.Empty;
                patternCombItem.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(patternCombItem.Bg_Brush_Solid);
                patternCombItem.Bri = extArea.Pattern_info.Bri;
                patternCombItem.Speed = extArea.Pattern_info.Speed;
                patternCombItem.Type = extArea.Pattern_info.Type;
                CommUI.Area_class newExtArea = new CommUI.Area_class(patternCombItem, extArea.Area_index, null);
                newExtArea.Ext_Area_id = extArea.Ext_Area_id;
                _allAreaInfo.Add(newExtArea.Area_index, newExtArea);
            }
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

        private void CreateAreaCommands()
        {
            if (_commands.Count <= 0)
                return;

            MainboardCommandsCommands.Clear();
            foreach (var command in _commands)
            {

                if (command.AreaId == -1)
                {
                    foreach (CommUI.Area_class setAllArea in _allAreaInfo.Values)
                    {
                        setAllArea.Pattern_info.Bg_Brush_Solid.Color = command.NewColor;
                        setAllArea.Pattern_info.Type = command.NewMode;
                        setAllArea.Pattern_info.Bri = command.Bright;
                        setAllArea.Pattern_info.Speed = command.Speed;
                        setAllArea.Pattern_info.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(setAllArea.Pattern_info.Bg_Brush_Solid);
                        MainboardCommandsCommands.Add(setAllArea);
                    }
                    continue;
                }

                CommUI.Area_class area = _allAreaInfo[command.AreaId];
                area.Pattern_info.Bg_Brush_Solid.Color = command.NewColor;
                area.Pattern_info.Type = command.NewMode;
                area.Pattern_info.Bri = command.Bright;
                area.Pattern_info.Speed = command.Speed;
                area.Pattern_info.But_Args = CommUI.Get_Color_Sceenes_class_From_Brush(area.Pattern_info.Bg_Brush_Solid);

                if (area.Ext_Area_id == ExtLedDev.None) //Mainboard
                {
                    //TODO: Find lower level call and use it
                    MainboardCommandsCommands.Add(area);
                }
                else if (area.Ext_Area_id == ExtLedDev.Kingston_RAM && _ledObject.MB_Id == MBIdentify.I_Z390)
                {
                    _RAMNewColor = command.NewColor;
                    new Task(() => { SetRamColor(command.NewColor); }).Start(); //Direct using MCU. Faster than using RGBFusion HAL.
                }
                else if (area.Ext_Area_id == ExtLedDev.GB_VGACard && _gb_led_periphs?.GraphicsName == "GV-N2080GAMING OC-8GC") //KinstoM RAM by Nvidia i2C. A lot faster than setting ram but still slow using rgbfusion methods.
                {
                    new Task(() => { AorusVGA.SetDirect(System.Drawing.Color.FromArgb(255, command.NewColor.R, command.NewColor.G, command.NewColor.B)); }).Start(); //Direct GvLedLib lib. A lot faster than RGBFusion HAL.
                }
                else
                {
                    MainboardCommandsCommands.Add(area); // Left devices, for now i left this just for compatibility but it may be slow on some rigs.
                }
                _allAreaInfo[command.AreaId] = area;
            }
        }

        public void SetMainboardRingAreas()
        {
            FillAllAreaInfo();
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
            SetRamColor(_RAMNewColor);
        }

        public void SetRamColor(Color color)
        {
            KingstonFury.SetDirect(color);
        }

        public string GetAreasReport()
        {
            var areasReport = string.Empty;
            if (_allAreaInfo.Count > 0)
            {
                areasReport += "Areas detected on " + _ledFun.Product_Name + Environment.NewLine;
                areasReport = _allAreaInfo.Aggregate(areasReport, (current, area) => current + ("    Area ID: " + area.Value.Area_index + Environment.NewLine));
            }
            areasReport += Environment.NewLine;
            /*
            if (_allExtAreaInfo.Count > 0)
                foreach (var areaInfo in _ledFun.LEd_Layout.Ext_Led_Array)
                {
                    var extDevice = _ledFun.Get_Ext_Led_Tip(areaInfo.extLDev);
                    areasReport += "Areas detected on " + extDevice + Environment.NewLine;
                    areasReport = _allExtAreaInfo.Where(area => area.Ext_Area_id == areaInfo.extLDev).Aggregate(areasReport, (current, area) => current + ("    Area ID: " + area.Area_index + Environment.NewLine));
                }
                */
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

            _ledObject = (LedObject)typeof(Comm_LED_Fun).GetField("LedObj", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(this._ledFun);
            _gb_led_periphs = (GBLedPeripherals)typeof(LedObject).GetField("gb_led_periphs", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_ledObject);
            _ram_led_obj = (PeripheralDeviceManagement)typeof(LedObject).GetField("ram_led_obj", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(_ledObject);
            //Initialize mode and bright.
            _ram_led_obj?.Software_Control(1, -1, 0, 9, true);
            _ledObject.bSyncRAM = false;
            _ledObject.bSyncVGA = false;
            _initialized = true;
        }


        private LedObject _ledObject;
        private PeripheralDeviceManagement _ram_led_obj;
        private GBLedPeripherals _gb_led_periphs;
    }
}
