using System.ComponentModel;
using System.Windows.Media;

namespace RGBFusionCli
{
    public class LedCommand
    {
        [DefaultValue(-1)]
        public sbyte AreaId { get; set; }
        [DefaultValue(0)]
        public sbyte NewMode { get; set; }
        public Color NewColor { get; set; }
        [DefaultValue(5)]
        public sbyte Speed { get; set; }
        [DefaultValue(2)]
        public sbyte Bright { get; set; }
        [DefaultValue(true)]
        public bool Direct { get; set; }
    }
}
