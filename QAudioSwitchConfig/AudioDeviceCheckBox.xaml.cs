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

        public delegate void DeviceCheckedHandler(object sender, bool isChecked);

        public event DeviceCheckedHandler OnDeviceChecked;

        public static readonly DependencyProperty IsAudioDeviceCheckedProperty = DependencyProperty.Register(
            "IsAudioDeviceChecked", 
            typeof(bool),
            typeof(AudioDeviceCheckBox), 
            new FrameworkPropertyMetadata(
                false,
                FrameworkPropertyMetadataOptions.AffectsMeasure,
                new PropertyChangedCallback(OnDeviceCheckedDependencyChanged)));

        private static void OnDeviceCheckedDependencyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            AudioDeviceCheckBox checkBox = (AudioDeviceCheckBox)d;
            if (checkBox != null && checkBox.OnDeviceChecked != null)
            {
                checkBox.OnDeviceChecked(checkBox, (bool)e.NewValue);
            }
        }

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
                case DeviceState.Active:
                    stateString = "Active";
                    break;
                case DeviceState.Disabled:
                    stateString = "Disabled";
                    break;
                case DeviceState.Unplugged:
                    stateString = "Unplugged";
                    break;
                case DeviceState.NotPresent:
                    stateString = "Disconnected";
                    break;
                default: break;
            }

            this.NameLabel.Content = device.FriendlyName;
            this.StatusLabel.Content = stateString;
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
