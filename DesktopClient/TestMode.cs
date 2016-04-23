using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace AudioSpectrum
{
    class TestMode : IMode
    {
        public List<Light> Lights { get; set; }

        public TestMode()
        {
            Lights = new List<Light>();

            var cross = new Light { Color = Color.FromRgb(255, 200, 0) };
            var center = new Light { Color = Color.FromRgb(128, 0, 255) };
            var corners1 = new Light { Color = Color.FromRgb(255, 32, 128) };
            var corners2 = new Light { Color = Color.FromRgb(64, 255, 128) };
            var corners3 = new Light { Color = Color.FromRgb(255, 255, 255) };

            for (var y = 0; y < 8; y++)
            {
                cross.Pixels.Add(new byte[] {CordsToByte(3, y), 0});
                cross.Pixels.Add(new byte[] {CordsToByte(4, y), 0});
            }

            for (var x = 0; x < 8; x++)
            {
                cross.Pixels.Add(new byte[] { CordsToByte(3, 3), 0 });
                cross.Pixels.Add(new byte[] { CordsToByte(3, 4), 0 });
            }


            //Lights.Add(l1);
            //Lights.Add(l2);
            //Lights.Add(l3);
        }

        public void Display()
        {
            throw new NotImplementedException();
        }

        private static byte CordsToByte(int x, int y)
        {
            return (byte) ((x << 4) + y);
        }
    }
}
