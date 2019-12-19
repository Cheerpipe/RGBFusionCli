using System.ComponentModel;
using System.Windows.Media;

namespace RGBFusion390SetColor
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
        [DefaultValue(9)]
        public sbyte Bright { get; set; }
        [DefaultValue(true)]
        public bool Direct { get; set; }
    }
}
