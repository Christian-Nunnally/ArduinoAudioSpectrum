using System.Collections.Generic;
using System.Windows.Controls;

namespace AudioSpectrum
{
    /// <summary>
    /// Interaction logic for Spectrum.xaml
    /// </summary>
    public partial class Spectrum : UserControl
    {
        public Spectrum()
        {
            InitializeComponent();
        }

        public void Set(List<byte> data)
        {
            if (data.Count < 1) return;
            Bar01.Value = data[0];
            if (data.Count < 2) return;
            Bar02.Value = data[1];
            if (data.Count < 3) return;
            Bar03.Value = data[2];
            if (data.Count < 4) return;
            Bar04.Value = data[3];
            if (data.Count < 5) return;
            Bar05.Value = data[4];
            if (data.Count < 6) return;
            Bar06.Value = data[5];
            if (data.Count < 7) return;
            Bar07.Value = data[6];
            if (data.Count < 8) return;
            Bar08.Value = data[7];
            if (data.Count < 9) return;
            Bar09.Value = data[8];
            if (data.Count < 10) return;
            Bar10.Value = data[9];
            if (data.Count < 11) return;
            Bar11.Value = data[10];
            if (data.Count < 12) return;
            Bar12.Value = data[11];
            if (data.Count < 13) return;
            Bar13.Value = data[12];
            if (data.Count < 14) return;
            Bar14.Value = data[13];
            if (data.Count < 15) return;
            Bar15.Value = data[14];
            if (data.Count < 16) return;
            Bar16.Value = data[15];
        }
    }
}
