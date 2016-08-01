using System;
using System.Windows;
using System.Windows.Controls;

using AudioEndPointControllerWrapper;
using AudioSwitchCommon;

namespace QAudioSwitch
{
    /// <summary>
    /// Interaction logic for AudioDeviceListItem.xaml
    /// </summary>
    public partial class AudioDeviceListItem : UserControl
    {
        public IAudioDevice AudioDevice { get; private set; }

        public AudioDeviceListItem(IAudioDevice device)
        {
            InitializeComponent();

            UpdateDeviceState(device, device.DeviceState);
        }

        private void AudioController_DeviceStateChanged(object sender, DeviceStateChangedEvent e)
        {
            if (e.device.Id == AudioDevice.Id)
            {
                UI.ScheduleAction(Dispatcher, delegate
                    {
                        UpdateDeviceState(e.device, e.newState);
                    });
            }
        }

        private void UpdateDeviceState(IAudioDevice device, DeviceState state)
        {
            this.IsEnabled = state == DeviceState.Active;
            this.AudioDevice = device;

            try
            {
                IconImage.Source = IconResourceCache.LoadIconAsBitmap(device.DeviceClassIconPath, true);
            }
            catch (Exception)
            {
                // Disregard errors -- we'll just have to make do without an image
            }

            string stateString = "";
            switch (state)
            {
                case DeviceState.Disabled:
                    stateString = " [Disabled]";
                    break;
                case DeviceState.Unplugged:
                case DeviceState.NotPresent:
                    stateString = " [Disconnected]";
                    break;
                default: break;
            }

            string name = device.FriendlyName;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = device.Description;
            }

            this.NameLabel.Content = $"{name}{stateString}";
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            // Monitor the status of this device
            AudioController.DeviceStateChanged += AudioController_DeviceStateChanged;
        }

        private void UserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            // Unsubscribe from the device notifications
            AudioController.DeviceStateChanged -= AudioController_DeviceStateChanged;
        }
    }
}
