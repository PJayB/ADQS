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
        Configuration _config;
        AudioSwitchQ _switchQ;
        HashSet<string> _knownDevices = new HashSet<string>();

        private void AddAudioDevice(IAudioDevice device)
        {
            if (!_knownDevices.Contains(device.Id))
            {
                bool initialState = !_config.ExclusionIDs.Contains(device.Id);

                var checkBox = new AudioDeviceCheckBox(device, initialState);

                checkBox.OnDeviceChecked += CheckBox_OnDeviceChecked;

                _knownDevices.Add(device.Id);

                UI.ScheduleAction(this.Dispatcher, delegate
                {
                    AudioDevicesListBox.Items.Add(checkBox);

                    if (device.IsDefault(Role.Multimedia))
                    {
                        AudioDevicesListBox.SelectedIndex = AudioDevicesListBox.Items.Count - 1;
                    }
                });
            }
        }

        public MainWindow()
        {
            // Load the configuration
            _config = Configuration.Load();

            InitializeComponent();

            AudioDevicesListBox.Items.Clear();

            foreach (var device in AudioController.GetAllPlaybackDevices())
            {
                AddAudioDevice(device);
            }
            
            AudioController.DeviceAdded += AudioController_DeviceAdded;

            _switchQ = new AudioSwitchQ();
        }

        private void AudioController_DeviceAdded(object sender, DeviceAddedEvent e)
        {
            AddAudioDevice(e.device);
        }

        private void CheckBox_OnDeviceChecked(object sender, bool isChecked)
        {
            AudioDeviceCheckBox checkBox = (AudioDeviceCheckBox) sender;
            if (isChecked)
            {
                _config.ExclusionIDs.Remove(checkBox.AudioDevice.Id);
            }
            else
            {
                _config.ExclusionIDs.Add(checkBox.AudioDevice.Id);
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            // Save the configuration and quit
            _config.Save();
            this.Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void AudioDevicesListBox_SetAsDefaultNow(object sender, RoutedEventArgs e)
        {
            AudioDeviceCheckBox checkBox = AudioDevicesListBox.SelectedItem as AudioDeviceCheckBox;
            if (_switchQ != null && checkBox != null && checkBox.AudioDevice.DeviceState == DeviceState.Active)
            {
                _switchQ.Push(checkBox.AudioDevice);
            }
        }

        private void AudioDevicesListBox_ToggleVisibility(object sender, RoutedEventArgs e)
        {
            AudioDeviceCheckBox checkBox = AudioDevicesListBox.SelectedItem as AudioDeviceCheckBox;
            if (checkBox != null && checkBox.AudioDevice.DeviceState == DeviceState.Active)
            {
                checkBox.IsAudioDeviceChecked = !checkBox.IsAudioDeviceChecked;
            }
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _switchQ.Dispose();
        }
    }
}
