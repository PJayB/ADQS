using System;
using System.Windows;
using System.Windows.Controls;

using AudioEndPointControllerWrapper;
using AudioSwitchCommon;

namespace QAudioSwitchConfig
{
    /// <summary>
    /// Interaction logic for AudioDeviceCheckBox.xaml
    /// </summary>
    public partial class AudioDeviceCheckBox : UserControl
    {
        public IAudioDevice AudioDevice { get; private set; }

        public static readonly DependencyProperty IsAudioDeviceCheckedProperty = DependencyProperty.Register(
            "IsAudioDeviceChecked", 
            typeof(bool),
            typeof(AudioDeviceCheckBox), 
            new FrameworkPropertyMetadata(false));

        public bool IsAudioDeviceChecked 
        {
            get { return (bool)GetValue(IsAudioDeviceCheckedProperty); }
            set { SetValue(IsAudioDeviceCheckedProperty, value); }
        }
        
        public AudioDeviceCheckBox(IAudioDevice device, bool initialCheckedState)
        {
            InitializeComponent();

            UpdateDeviceState(device, device.DeviceState);

            IsAudioDeviceChecked = initialCheckedState;
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

            this.NameLabel.Content = $"{device.FriendlyName}{stateString}";
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
