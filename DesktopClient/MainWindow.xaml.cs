using System;
using System.Collections.Generic;
using System.Windows;
using System.IO.Ports;

namespace AudioSpectrum
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private readonly Analyzer _analyzer;
        private SerialPort _port;


        public MainWindow()
        {
            InitializeComponent();
            Modes.InitModes();
            Un4seen.Bass.BassNet.Registration("lgf.littlegreenflame@gmail.com", "2X5152330152222");
            _analyzer = new Analyzer(Spectrum, DeviceBox);
            //.IsChecked = true;
        }

        private void BtnEnable_Click(object sender, RoutedEventArgs e)
        {
            if (BtnEnable.IsChecked == true)
            {
                BtnEnable.Content = "Disable";
                _analyzer.Enable = true;
            }
            else
            {
                _analyzer.Enable = false;
                BtnEnable.Content = "Enable";
            }
        }

        private void Comports_DropDownOpened(object sender, EventArgs e)
        {
            Comports.Items.Clear();
            var ports = SerialPort.GetPortNames();
            foreach (var port in ports) Comports.Items.Add(port);
        }

        private void CkbSerial_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (CkbSerial.IsChecked == true)
                {
                    Comports.IsEnabled = false;
                    _port = new SerialPort((Comports.Items[Comports.SelectedIndex] as string))
                    {
                        BaudRate = 115200,
                        StopBits = StopBits.One,
                        Parity = Parity.None,
                        DataBits = 8,
                        DtrEnable = true
                    };
                    _port.Open();
                    _analyzer.Serial = _port;
                }
                else
                {
                    Comports.IsEnabled = true;
                    _analyzer.Serial = null;
                    if (_port != null)
                    {
                        _port.Close();
                        _port.Dispose();
                        _port = null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void LpChecked(object sender, RoutedEventArgs e)
        {
            if (_sudoLpCheck.IsChecked != null) _analyzer.SudoLowPassFilterEnabled = (bool) _sudoLpCheck.IsChecked;
        }

        private void AverageSmoothChecked(object sender, RoutedEventArgs e)
        {
            if (_averageSmoothCheck.IsChecked != null)
                _analyzer.AverageSmoothEnabled = (bool) _averageSmoothCheck.IsChecked;
        }

        private void AverageSmoothUnchecked(object sender, RoutedEventArgs e)
        {
            if (_averageSmoothCheck.IsChecked != null)
                _analyzer.AverageSmoothEnabled = (bool) _averageSmoothCheck.IsChecked;
        }

        private void LpUnchecked(object sender, RoutedEventArgs e)
        {
            if (_sudoLpCheck.IsChecked != null) _analyzer.SudoLowPassFilterEnabled = (bool) _sudoLpCheck.IsChecked;
        }

        private void NormDecayVelValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_analyzer == null) return;

            _analyzer.NormalDecayVelocity = (byte) _normDecayVel.Value;
            _normDecayLbl.Content = _normDecayVel.Value;
        }

        private void NormalizeChecked(object sender, RoutedEventArgs e)
        {
            _normDecayVel.IsEnabled = true;
            _analyzer.NormalizeEnabled = true;
        }

        private void NormalizeUnchecked(object sender, RoutedEventArgs e)
        {
            _normDecayVel.IsEnabled = false;
            _analyzer.NormalizeEnabled = false;
        }

        private void SquareOutputUnchecked(object sender, RoutedEventArgs e)
        {
            _analyzer.SqaureOutputEnabled = false;
        }

        private void SqaureOutputChecked(object sender, RoutedEventArgs e)
        {
            _analyzer.SqaureOutputEnabled = true;
        }

        private void EnhanceChangeChecked(object sender, RoutedEventArgs e)
        {
            _analyzer.EnhanceChangeContrastEnabled = true;
        }

        private void EnhanceChangeUnchecked(object sender, RoutedEventArgs e)
        {
            _analyzer.EnhanceChangeContrastEnabled = false;
        }

        /// <summary>
        /// When the send button is clicked the text data in the send text box should be parsed and sent
        /// to the arduino.
        /// </summary>
        /// <param name="sender">Send button.</param>
        /// <param name="e">Event Args.</param>
        private void SendButtonClicked(object sender, RoutedEventArgs e)
        {
            var splitData = sendTextBox.Text.Split(' ');

            var data = new List<byte>();
            foreach (var str in splitData)
            {
                byte b;
                if (byte.TryParse(str, out b))
                {
                    data.Add(b);
                }
            }
            _analyzer?.SendSerialData(data.ToArray());
            sendTextBox.Text = "";
        }

        private void Spectrum_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void setModeButton_Click(object sender, RoutedEventArgs e)
        {
            _analyzer.SetModeFlag = true;
        }
    }
}
