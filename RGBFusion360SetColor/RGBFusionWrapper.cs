
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using SelLEDControl;

namespace RGBFusion390SetColor
{
    public class RgbFusion
    {
        private Comm_LED_Fun _ledFun;
        private bool _areaChangeApplySuccess;
        private bool _scanDone;
        private List<CommUI.Area_class> _allAreaInfo;
        private List<CommUI.Area_class> _allExtAreaInfo;
        private List<LedCommand> _commands;
        private Thread _newChangeThread;
        private readonly sbyte _changeOperationDelay = 60;
        private bool _initialized;

        public bool IsInitialized()
        {
            return _ledFun != null && _initialized;
        }

        readonly ManualResetEvent _commandEvent = new ManualResetEvent(initialState: false);

        public void LoadProfile(int profileId)
        {
            _ledFun.Adv_mode_Apply(Area_info: GetAllAreaInfo(profileId), GetAllExtAreaInfo(profileId));
            do
            {
                Thread.Sleep(millisecondsTimeout: 10);
            }
            while (!_areaChangeApplySuccess);
        }

        private void FillAllAreaInfo()
        {
            _allAreaInfo = GetAllAreaInfo();
        }


        private List<CommUI.Area_class> GetAllAreaInfo(int profileId = -1)
        {
            if (profileId < 1)
            {
                profileId = _ledFun.Current_Profile;
            }
            var str = string.Concat(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "\\GIGABYTE\\RGBFusion\\Pro", profileId.ToString(), ".xml");
            var color = CommUI.Int_To_Color(Argb: (uint)CommUI.Get_Default_Color_from_Appcenter());
            if (!File.Exists(str))
            {
                Creative_Profile(proXmlFilePath: str, _ledFun.Get_MB_Area_number(), color, profileName: string.Concat("Profile ", profileId.ToString()));
            }
            return CommUI.Inport_from_xml(str, But_Style: null);
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
                var areaClass = new CommUI.Area_class(patternCombItem, areaInfo.DivsNum, mBut_Style: null)
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
            var color = CommUI.Int_To_Color(Argb: (uint)CommUI.Get_Default_Color_from_Appcenter());
            if (!File.Exists(str))
            {
                Creative_Profile_Ext(proXmlFilePath: str, _ledFun.LEd_Layout.Ext_Led_Array, color, profileName: string.Concat("ExProfile ", profileId.ToString()));
                allExtAreaInfo = CommUI.Inport_from_xml(str, But_Style: null);
                return allExtAreaInfo;
            }
            Creative_Profile_Ext(proXmlFilePath: str1, _ledFun.LEd_Layout.Ext_Led_Array, color, profileName: string.Concat("ExProfile ", profileId.ToString()));
            allExtAreaInfo = ReImport_ExtInfo(orgExtArea: CommUI.Inport_from_xml(str, But_Style: null), newExtArea: CommUI.Inport_from_xml(str1, But_Style: null));
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

        public void Init()
        {
            var initThread = new Thread(DoInit);
            initThread.SetApartmentState(ApartmentState.STA);
            initThread.Start();
            initThread.Join();
            _newChangeThread = new Thread(SetAreas);
            _newChangeThread.SetApartmentState(ApartmentState.STA);
            _newChangeThread.Start();
            _newChangeThread.Join();
        }

        public void ChangeColorForAreas(List<LedCommand> commands)
        {
            _commands = commands;
            _commandEvent.Set();
            _commandEvent.Reset();
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

            var allAreaInfo = _allAreaInfo.Select(areaInfo => new CommUI.Area_class(patternCombItem, areaInfo.Area_index, mBut_Style: null)).ToList();

            var allExtAreaInfo = _allExtAreaInfo.Select(areaInfo => new CommUI.Area_class(patternCombItem, areaInfo.Area_index, mBut_Style: null) { Ext_Area_id = areaInfo.Ext_Area_id }).ToList();

            allAreaInfo.AddRange(allExtAreaInfo);
            _ledFun.Set_Adv_mode(allAreaInfo, Run_Direct: true);
            Thread.Sleep(millisecondsTimeout: _changeOperationDelay);
        }

        public void StartMusicMode()
        {
            _ledFun.Start_music_mode();
        }

        public void StopMusicMode()
        {
            _ledFun.Stop_music_mode();
        }

        public void SetAreas()
        {
            while (_newChangeThread.IsAlive)
            {
                _commandEvent.WaitOne();
                _commandEvent.Reset();

                var areaInfo = new List<CommUI.Area_class>();

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
                    var area = new CommUI.Area_class(patternCombItem, command.AreaId, mBut_Style: null);

                    foreach (var extAreaInfo in _allExtAreaInfo.Where(extAreaInfo => extAreaInfo.Area_index == area.Area_index))
                    {
                        area.Ext_Area_id = extAreaInfo.Ext_Area_id;
                    }

                    areaInfo.Add(area);
                }

                if (_commands.Count > 0)
                {
                    _ledFun.Set_Sync(false);
                    _ledFun.Set_Adv_mode(areaInfo, Run_Direct: true);
                    var requireNonDirectMode = _commands.FindAll(i => i.NewMode > 0).Count > 0;
                    if (requireNonDirectMode)
                    {
                        _ledFun.Set_Adv_mode(areaInfo);
                        do
                        {
                            Thread.Sleep(millisecondsTimeout: 10);
                        }
                        while (!_areaChangeApplySuccess);
                    }
                }
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
            if (_ledFun != null)
                return;

            _ledFun = new Comm_LED_Fun(false);
            _ledFun.Apply_ScanPeriphera_Scuuess += CallBackLedFunApplyScanPeripheralSuccess;
            _ledFun.ApplyEZ_Success += CallBackLedFunApplyEzSuccess;
            _ledFun.ApplyAdv_Success += CallBackLedFunApplyAdvSuccess;

            _ledFun.Ini_LED_Fun();

           _ledFun = CommUI.Get_Easy_Pattern_color_Key(_ledFun);

            _ledFun.LEd_Layout.Set_Support_Flag();
            do
            {
                Thread.Sleep(millisecondsTimeout: 10);
            }
            while (!_scanDone);

            _ledFun.Current_Mode = 0; // 1= Advanced 0 = Simple or Ez

            _ledFun.Led_Ezsetup_Obj.PoweronStatus = 1;
            StopMusicMode();
            _initialized = true;
            _ledFun.Set_Sync(false);
            StopMusicMode();
            FillAllAreaInfo();
            Fill_ExtArea_info();
        }
    }
}