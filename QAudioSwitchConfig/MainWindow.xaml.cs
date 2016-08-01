using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using AudioEndPointControllerWrapper;
using AudioSwitchCommon;

namespace QAudioSwitchConfig
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AudioDevicesListBox.Items.Clear();

            foreach (var device in AudioController.GetAllPlaybackDevices())
            {
                // TODO: logic to map visible devices 

                bool initialState = device.DeviceState == DeviceState.Active;

                AudioDevicesListBox.Items.Add(new AudioDeviceCheckBox(device, initialState));

                if (device.IsDefault(Role.Multimedia))
                {
                    AudioDevicesListBox.SelectedIndex = AudioDevicesListBox.Items.Count - 1;
                }
            }
        }
    }
}
