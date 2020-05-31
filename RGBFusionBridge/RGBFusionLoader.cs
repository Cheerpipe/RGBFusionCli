using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Media;
using SelLEDControl;

namespace RGBFusionBridge
{
    public class RGBFusionLoader
    {
        public Comm_LED_Fun _ledFun;
        private bool _scanDone;
        private Dictionary<byte, CommUI.Area_class> _allAreaInfo = new Dictionary<byte, CommUI.Area_class>();
        private bool _loaded;

        public Dictionary<byte, CommUI.Area_class> Areas
        {
            get
            {
                return _allAreaInfo;
            }
        }

        public Comm_LED_Fun RGBFusion
        {
            get
            {
                return _ledFun;
            }
        }


        public bool IsInitialized()
        {
            return _ledFun != null && _loaded;
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
                _allAreaInfo.Add((byte)area.Area_index, newArea);
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
                _allAreaInfo.Add((byte)newExtArea.Area_index, newExtArea);
            }
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


        public string GetAreasReport()
        {
            var areasReport = string.Empty;
            if (_allAreaInfo.Count > 0)
            {
                areasReport += "Areas detected on " + _ledFun.Product_Name + Environment.NewLine;
                areasReport = _allAreaInfo.Aggregate(areasReport, (current, area) => current + ("    Area ID: " + area.Value.Area_index + Environment.NewLine));
            }
            areasReport += Environment.NewLine;
            areasReport += "Use Area ID -1 to set all areas at the same time.";

            return areasReport;
        }

        private bool _areaChangeApplySuccess;
        private void CallBackLedFunApplyScanPeripheralSuccess() => _scanDone = true;
        private void CallBackLedFunApplyEzSuccess() => _areaChangeApplySuccess = true;
        private void CallBackLedFunApplyAdvSuccess() => _areaChangeApplySuccess = true;
        public void Load()
        {
            _ledFun = new Comm_LED_Fun(false);
            _ledFun.Apply_ScanPeriphera_Scuuess += CallBackLedFunApplyScanPeripheralSuccess;
            _ledFun.ApplyEZ_Success += CallBackLedFunApplyEzSuccess;
            _ledFun.ApplyAdv_Success += CallBackLedFunApplyAdvSuccess;
            _ledFun.Ini_LED_Fun();
            do
            {
                Thread.Sleep(10);
            }
            while (!_scanDone);

            _ledFun.Current_Mode = 1;

            _ledFun.Led_Ezsetup_Obj.PoweronStatus = 0;
            _ledFun.Set_Sync(false);
            FillAllAreaInfo();
            _loaded = true;
        }
    }
}
