using System;
using System.Windows;
using System.Windows.Controls;

using AudioEndPointControllerWrapper;
using ADQSCommon;

namespace ADQSConfigApp
{
    /// <summary>
    /// Interaction logic for AudioDeviceCheckBox.xaml
    /// </summary>
    public partial class AudioDeviceCheckBox : UserControl
    {
        public IAudioDevice AudioDevice { get; private set; }

        public delegate void DeviceCheckedHandler(object sender, bool isChecked);

        public event DeviceCheckedHandler DeviceChecked;

        public static readonly DependencyProperty IsAudioDeviceActiveProperty = DependencyProperty.Register(
            "IsAudioDeviceActive",
            typeof(bool),
            typeof(AudioDeviceCheckBox),
            new FrameworkPropertyMetadata(true));

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
            if (checkBox != null && checkBox.DeviceChecked != null)
            {
                checkBox.DeviceChecked(checkBox, (bool)e.NewValue);
            }
        }

        public bool IsAudioDeviceChecked 
        {
            get { return (bool)GetValue(IsAudioDeviceCheckedProperty); }
            set { SetValue(IsAudioDeviceCheckedProperty, value); }
        }

        public bool IsAudioDeviceActive
        {
            get { return (bool)GetValue(IsAudioDeviceActiveProperty); }
            private set { SetValue(IsAudioDeviceActiveProperty, value); }
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
            this.IsAudioDeviceActive = state == DeviceState.Active;
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

            string name = device.FriendlyName;
            if (string.IsNullOrWhiteSpace(name))
            {
                name = device.Description;
            }

            this.NameLabel.Text = name;
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
