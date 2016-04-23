using System.Collections.Generic;
using System.Windows.Media;

namespace AudioSpectrum
{
    static class Modes
    {
        public static List<IMode> ModeList { get; set; } = new List<IMode>();

        public static void InitModes()
        {
            var mode1 = new TestMode();

            var l1 = new Light {Color = Color.FromRgb(128, 64, 32)};
            var l2 = new Light {Color = Color.FromRgb(64, 128, 32)};
            var l3 = new Light {Color = Color.FromRgb(64, 32, 128)};

            l1.Pixels.Add(new byte[] { 0, 0 });
            l1.Pixels.Add(new byte[] { 1, 0 });
            l1.Pixels.Add(new byte[] { 2, 0 });
            l1.Pixels.Add(new byte[] { 3, 0 });

            l2.Pixels.Add(new byte[] { 4, 0 });
            l2.Pixels.Add(new byte[] { 5, 0 });
            l2.Pixels.Add(new byte[] { 6, 0 });
            l2.Pixels.Add(new byte[] { 7, 0 });

            l3.Pixels.Add(new byte[] { 16, 0 });
            l3.Pixels.Add(new byte[] { 17, 0 });
            l3.Pixels.Add(new byte[] { 18, 0 });
            l3.Pixels.Add(new byte[] { 19, 0 });

            mode1.Lights.Add(l1);
            mode1.Lights.Add(l2);
            mode1.Lights.Add(l3);

            ModeList.Add(mode1);
        }
    }
}
