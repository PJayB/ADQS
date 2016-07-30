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

            ActivePlaybackDevicesListBox.Items.Clear();

            foreach (var device in AudioController.GetActivePlaybackDevices())
            {
                ActivePlaybackDevicesListBox.Items.Add(new AudioDeviceListItem(device));

                if (device.IsDefault(Role.Multimedia))
                {
                    ActivePlaybackDevicesListBox.SelectedIndex = ActivePlaybackDevicesListBox.Items.Count - 1;
                }
            }

            AudioController.DeviceAdded += AudioController_DeviceAdded;
            AudioController.DeviceRemoved += AudioController_DeviceRemoved;
            AudioController.DeviceStateChanged += AudioController_DeviceStateChanged;
            AudioController.DeviceDefaultChanged += AudioController_DeviceDefaultChanged;
        }

        private void SelectNextActive()
        {
            ActivePlaybackDevicesListBox.SelectedIndex = (ActivePlaybackDevicesListBox.SelectedIndex + 1) % ActivePlaybackDevicesListBox.Items.Count;
        }

        private void ListUpdated()
        {
            // If the current selection isn't valid, find the current system default and select that
            AudioDeviceListItem currentSelection = (AudioDeviceListItem)ActivePlaybackDevicesListBox.SelectedItem;
            if (currentSelection == null || !currentSelection.AudioDevice.IsDefault(Role.Multimedia))
            {
                var items = ActivePlaybackDevicesListBox.Items;

                // Make sure it isn't already in the list
                for (int i = 0; i < items.Count; ++i)
                {
                    AudioDeviceListItem item = items[i] as AudioDeviceListItem;
                    if (item != null && item.AudioDevice != null && item.AudioDevice.IsDefault(Role.Multimedia))
                    {
                        ActivePlaybackDevicesListBox.SelectedItem = item;
                        break;
                    }
                }
            }
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
                    AudioDeviceListItem item = items[i] as AudioDeviceListItem;
                    if (item != null && item.AudioDevice != null && item.AudioDevice.Id == device.Id)
                    {
                        return;
                    }
                }

                items.Add(new AudioDeviceListItem(device));

                ListUpdated();
            }
        }

        private void RemoveAudioDevice(IAudioDevice device)
        {
            bool removed = false;

            var items = ActivePlaybackDevicesListBox.Items;
            for (int i = 0; i < items.Count; ++i)
            {
                AudioDeviceListItem item = items[i] as AudioDeviceListItem;
                if (item != null && item.AudioDevice != null && item.AudioDevice.Id == device.Id)
                {
                    items.RemoveAt(i--);
                    removed = true;
                }
            }

            if (removed)
                ListUpdated();
        }

        private void AudioController_DeviceStateChanged(object sender, DeviceStateChangedEvent e)
        {
            Utils.ScheduleUIAction(Dispatcher, delegate
            {
                if (e.newState == DeviceState.Active)
                    AddAudioDevice(e.device);
                else
                    RemoveAudioDevice(e.device);
            });
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

        private void AudioController_DeviceDefaultChanged(object sender, DeviceDefaultChangedEvent e)
        {
            Utils.ScheduleUIAction(Dispatcher, delegate
            {
                ListUpdated();
            });
        }

        private void ActivePlaybackDevicesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 0)
                return;

            AudioDeviceListItem item = e.AddedItems[0] as AudioDeviceListItem;
            if (item == null)
                return;

            // Set the audio device as default
            IAudioDevice device = item.AudioDevice;
            if (device == null || device.DeviceState != DeviceState.Active || device.Type != AudioDeviceType.Playback || device.IsDefault(Role.Multimedia))
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

        private void ActivePlaybackDevicesListBox_KeyUp(object sender, KeyEventArgs e)
        {
            // Close the window on Windows key up
            if (e.Key == Key.LWin || e.Key == Key.RWin || e.Key == Key.LeftAlt || e.Key == Key.RightAlt)
                this.Close();

            // Move the selection on by one
            if (e.Key == Key.Space)
                SelectNextActive();
         
            // Close the window on ESC   
            if (e.Key == Key.Escape)
                Close();
        }

        // Hide but never close
        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.Visibility = Visibility.Hidden;
            e.Cancel = true;
        }

        private void Window_Activated(object sender, EventArgs e)
        {
            ActivePlaybackDevicesListBox.Focus();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
