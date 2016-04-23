using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Un4seen.Bass;
using Un4seen.BassWasapi;

namespace AudioSpectrum
{

    internal class Analyzer
    {
        private bool _enable;               //enabled status
        private readonly DispatcherTimer _t;         //timer that refreshes the display
        private readonly float[] _fft;               //buffer for fft data
        private readonly WASAPIPROC _process;        //callback function to obtain data
        private int _lastlevel;             //last output level
        private int _hanctr;                //last output level counter

        private readonly float[] _maxSpectrumValue; // For normalizing the spectrum data.
        private readonly float[] _minSpectrumValue; // For normalizing the spectrum data.

        private readonly List<float> _oldSpectrumdata;//spectrum data buffer
        private readonly List<float> _spectrumdata;   //spectrum data buffer

        private readonly Spectrum _spectrum;         //spectrum dispay control
        private readonly ComboBox _devicelist;       //device list

        private bool _initialized;          //initialized flag
        private int _devindex;               //used device index

        private int _lines = 16;            // number of spectrum lines

        private int _mode = 0;

        private List<byte> ByteSpectrumData
        {
            get
            {
                var byteSpectrumData = new List<byte>();
                for (var i = 0; i < _lines; i++)
                {
                    byteSpectrumData.Add((byte)_spectrumdata[i]);
                }
                return byteSpectrumData;
            }
        }

        // Options
        public bool SudoLowPassFilterEnabled { get; set; }
        public bool AverageSmoothEnabled { get; set; }
        public bool SqaureOutputEnabled { get; set; }
        public bool EnhanceChangeContrastEnabled { get; set; }
        public bool NormalizeEnabled { get; set; }
        public float NormalDecayVelocity { get; set; }
        

        //ctor
        public Analyzer(Spectrum spectrum, ComboBox devicelist)
        {
            _fft = new float[1024];
            _lastlevel = 0;
            _hanctr = 0;
            _t = new DispatcherTimer();
            _t.Tick += _t_Tick;
            _t.Interval = TimeSpan.FromMilliseconds(25); //40hz refresh rate
            _t.IsEnabled = false;
            _process = Process;
            _oldSpectrumdata = new List<float>(); 
            _spectrumdata = new List<float>();
            _spectrum = spectrum;
            _devicelist = devicelist;
            _initialized = false;
            NormalDecayVelocity = 0.0f;

            _maxSpectrumValue = new float[_lines];
            _minSpectrumValue = new float[_lines];

            Init();

            DisplayEnable = true;
        }

        // Serial port for arduino output
        public SerialPort Serial { get; set; }

        // flag for display enable
        public bool DisplayEnable { get; set; }

        //flag for enabling and disabling program functionality
        public bool Enable
        {
            get { return _enable; }
            set
            {
                _enable = value;
                if (value)
                {
                    if (!_initialized)
                    {
                        var s = _devicelist.Items[_devicelist.SelectedIndex] as string;
                        if (s != null)
                        {
                            var array = s.Split(' ');
                            _devindex = Convert.ToInt32(array[0]);
                        }

                        bool result = BassWasapi.BASS_WASAPI_Init(_devindex, 0, 0, BASSWASAPIInit.BASS_WASAPI_BUFFER, 1f, 0.05f, _process, IntPtr.Zero);
                        if (!result)
                        {
                            var error = Bass.BASS_ErrorGetCode();
                            MessageBox.Show(error.ToString());
                        }
                        else
                        {
                            _initialized = true;
                            _devicelist.IsEnabled = false;
                        }
                    }
                    BassWasapi.BASS_WASAPI_Start();
                }
                else BassWasapi.BASS_WASAPI_Stop(true);
                Thread.Sleep(500);
                _t.IsEnabled = value;
            }
        }

        public bool SetModeFlag { get; set; }

        // initialization
        private void Init()
        {
            for (var i = 0; i < BassWasapi.BASS_WASAPI_GetDeviceCount(); i++)
            {
                var device = BassWasapi.BASS_WASAPI_GetDeviceInfo(i);
                if (device.IsEnabled && device.IsLoopback)
                {
                    _devicelist.Items.Add($"{i} - {device.name}");
                }
            }
            _devicelist.SelectedIndex = 0;
            Bass.BASS_SetConfig(BASSConfig.BASS_CONFIG_UPDATETHREADS, false);
            var result = Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            if (!result) throw new Exception("Init Error");
        }

        //timer 
        private void _t_Tick(object sender, EventArgs e)
        {
            var ret = BassWasapi.BASS_WASAPI_GetData(_fft, (int)BASSData.BASS_DATA_FFT2048); //get channel fft data
            if (ret < -1) return;
            int line;
            var b0 = 0;

            //computes the spectrum data, the code is taken from a bass_wasapi sample.
            for (line = 0; line<_lines; line++)
            {
                float peak = 0;
                var b1 = (int)Math.Pow(2, line * 10.0 / (_lines - 1));
                if (b1 > 1023) b1 = 1023;
                if (b1 <= b0) b1 = b0 + 1;
                for (;b0<b1;b0++)
                {
                    if (peak < _fft[1 + b0]) peak = _fft[1 + b0];
                }
                var y = (int)(Math.Sqrt(peak) * 496);
                if (y > 255) y = 255;
                if (y < 0) y = 0;

                _spectrumdata.Add((byte)y);
            }

            // Pre-Process the spectrum data
            ProcessSpectrum(_spectrumdata, _oldSpectrumdata);

            if (NormalizeEnabled)
            {
                for (line = 0; line < _spectrumdata.Count; line++)
                {
                    if (_maxSpectrumValue[line] > _minSpectrumValue[line]+20)
                    {
                        // ReSharper disable once UnusedVariable
                        var precentApart = Math.Abs(_maxSpectrumValue[line] - _minSpectrumValue[line]) / 255;
                        _maxSpectrumValue[line] -= NormalDecayVelocity;
                        //_minSpectrumValue[line] += NormalDecayVelocity;
                    }

                    if (_maxSpectrumValue[line] < _spectrumdata[line]) _maxSpectrumValue[line] = (byte)_spectrumdata[line];
                    //if (_minSpectrumValue[line] > _spectrumdata[line]) _minSpectrumValue[line] = (byte)_spectrumdata[line];

                    _spectrumdata[line] = (byte)ConvertRange(_minSpectrumValue[line], _maxSpectrumValue[line], 0, 255, _spectrumdata[line]);
                    //Console.WriteLine((byte)_minSpectrumValue[line]);
                }
            }

            //_mode.Display(_spectrumdata);

            _oldSpectrumdata.Clear();
            _oldSpectrumdata.AddRange(_spectrumdata.ToArray());

            // Post

            if (DisplayEnable) _spectrum.Set(ByteSpectrumData);
            Serial?.Write(ByteSpectrumData.ToArray(), 0, _spectrumdata.Count);

            _spectrumdata.Clear();


            var level = BassWasapi.BASS_WASAPI_GetLevel();
            if (level == _lastlevel && level != 0) _hanctr++;
            _lastlevel = level;

            if (SetModeFlag && Serial != null)
            {
                SetMode(Modes.ModeList[_mode]);
            }

            //Required, because some programs hang the output. If the output hangs for a 75ms
            //this piece of code re initializes the output so it doesn't make a gliched sound for long.
            if (_hanctr <= 3) return;
            _hanctr = 0;
            Free();
            Bass.BASS_Init(0, 44100, BASSInit.BASS_DEVICE_DEFAULT, IntPtr.Zero);
            _initialized = false;
            Enable = true;
        }

        private void SetMode(IMode mode)
        {
            SetModeFlag = false;
            var i = 0;
            foreach (var light in mode.Lights)
            {
                var data = new byte[2 + 3 + 2*light.Pixels.Count];
                data[0] = (i == 0) ? (byte) 0xFE : (byte) 0xFF;
                data[1] = (byte)(3 + 2 * light.Pixels.Count);
                data[2] = light.Color.R;
                data[3] = light.Color.G;
                data[4] = light.Color.B;

                var index = 5;

                foreach (var pixelInfo in light.Pixels)
                {
                    data[index] = pixelInfo[0];
                    data[index + 1] = pixelInfo[1];
                    index += 2;
                }

                foreach (var b in data)
                {
                    Serial?.Write(new[] {b}, 0, 1);
                    Thread.Sleep(20);
                }
                i++;
            }
        }

        // WASAPI callback, required for continuous recording
        private static int Process(IntPtr buffer, int length, IntPtr user)
        {
            return length;
        }

        //cleanup
        private static void Free()
        {
            BassWasapi.BASS_WASAPI_Free();
            Bass.BASS_Free();
        }

        /// <summary>
        /// Runs some algorithums on the spectrum data.
        /// </summary>
        /// <param name="l1"></param>
        /// <param name="l2"></param>
        private void ProcessSpectrum(IList<float> l1, IReadOnlyList<float> l2)
        {
            const float movementThresh = 0.05f;
            for (var i = 0; i < Math.Min(l1.Count, l2.Count); i++)
            {
                if (Math.Abs(l1[i]) > 0.001)
                {
                    if (SqaureOutputEnabled)
                    {
                        l1[i] = ConvertRange(0, 65025, 0, 255, l1[i] * l1[i]);
                    }
                    if (EnhanceChangeContrastEnabled)
                    {
                        EnhanceChangeContrast(l1, l2);
                    }
                    if (SudoLowPassFilterEnabled)
                    {
                        var precentDiff = (Math.Abs(l1[i] - l2[i]) * 1.5)/l1[i];
                        if (precentDiff > 1.0f) precentDiff = 1.0f;

                        if (precentDiff > movementThresh)
                        {
                            l1[i] = (byte) (((precentDiff*l1[i]) + ((1.0f - precentDiff)*l2[i])));
                        }
                        else
                        {
                            l1[i] = l2[i];
                        }
                    }
                    if (AverageSmoothEnabled)
                    {
                        l1[i] = (byte)((l2[i] + l1[i])/2);
                    }
                }

                if (l1[i] > 255) l1[i] = 255;
                if (l1[i] < 0) l1[i] = 0;
            }
        }

        private static void EnhanceChangeContrast(IList<float> l1, IReadOnlyList<float> l2)
        {
            float total = 0;
            // ReSharper disable once LoopCanBeConvertedToQuery
            for (var i = 0; i < l1.Count; i++)
            {
                total += Math.Abs(l1[1] - l2[i]);
            }
            for (var i = 0; i < l1.Count; i++)
            {
                l1[i] = (((l1[1] - l2[i])/total) * l1[i]) + ((1 - ((l1[1] - l2[i]) / total)) * l2[i]);
                
            }
        }


        // Credit to => Aaron Hathaway on http://stackoverflow.com/questions/4229662/convert-numbers-within-a-range-to-numbers-within-another-range 
        private static float ConvertRange(float originalStart, float originalEnd,
                                  float newStart, float newEnd,
                                  float value)
        {
            var originalDiff = originalEnd - originalStart;
            var newDiff = newEnd - newStart;
            var ratio = (Math.Abs(originalDiff) < 0) ? 0 : newDiff / originalDiff;
            var newProduct = value * ratio;
            var finalValue = newProduct + newStart;
            return finalValue;

        }

        /// <summary>
        /// Sends data to the arduino manually.
        /// </summary>
        /// <param name="data">Data to send.</param>
        public void SendSerialData(byte[] data)
        {
            Serial?.Write(data, 0, data.Length);
        }
    }
}
