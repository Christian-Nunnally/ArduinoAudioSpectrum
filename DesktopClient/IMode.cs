using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioSpectrum
{
    interface IMode
    {
        List<Light> Lights { get; set; }

        void Display();

    }
}
