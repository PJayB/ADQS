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

namespace QAudioSwitch
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            foreach (var device in AudioController.GetAllPlaybackDevices())
            {
                ActivePlaybackDevicesListBox.Items.Add(new AudioDeviceListItem(device));
            }

            AudioController.DeviceAdded += AudioController_DeviceAdded;
            AudioController.DeviceRemoved += AudioController_DeviceRemoved;
        }

        private void AddAudioDevice(IAudioDevice device)
        {
            // Add the device to the list
            if (device.Type == AudioDeviceType.Playback)
            {
                var items = ActivePlaybackDevicesListBox.Items;

                // Make sure it isn't already in the list
                for (int i = 0; i < items.Count; ++i)
                {
                    AudioDeviceListItem item = (AudioDeviceListItem)items[i];
                    if (item != null && item.AudioDevice != null && item.AudioDevice.Id == device.Id)
                    {
                        return;
                    }
                }

                items.Add(new AudioDeviceListItem(device));
            }
        }

        private void RemoveAudioDevice(IAudioDevice device)
        {
            var items = ActivePlaybackDevicesListBox.Items;
            for (int i = 0; i < items.Count; ++i)
            {
                AudioDeviceListItem item = (AudioDeviceListItem)items[i];
                if (item != null && item.AudioDevice != null && item.AudioDevice.Id == device.Id)
                {
                    items.RemoveAt(i--);
                }
            }
        }

        private void AudioController_DeviceAdded(object sender, DeviceAddedEvent e)
        {
            Utils.ScheduleUIAction(Dispatcher, delegate
            {
                AddAudioDevice(e.device);
            });            
        }

        private void AudioController_DeviceRemoved(object sender, DeviceRemovedEvent e)
        {
            Utils.ScheduleUIAction(Dispatcher, delegate
            {
                RemoveAudioDevice(e.device);
            });
        }

        private void ActivePlaybackDevicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            AudioDeviceListItem item = (AudioDeviceListItem) e.AddedItems[0];
            if (item == null)
                return;

            // Set the audio device as default
            IAudioDevice device = item.AudioDevice;
            if (device == null || device.DeviceState != DeviceState.Active || device.Type != AudioDeviceType.Playback)
                return;

            try
            {
                device.SetAsDefault(Role.Multimedia);
            }
            catch (Exception ex)
            {
                MessageBox.Show("There was an error setting this audio device as the default for playback. " + ex.Message,
                    "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
