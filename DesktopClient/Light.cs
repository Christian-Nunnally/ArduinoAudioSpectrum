using System.Collections.Generic;
using System.Windows.Media;

namespace AudioSpectrum
{
    class Light
    {
        public List<byte[]> Pixels { get; set; } = new List<byte[]>();

        public Color Color { get; set; }
        public byte Brightness { get; set; }
    }
}
