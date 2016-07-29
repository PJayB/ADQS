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
    /// Interaction logic for AudioDeviceListItem.xaml
    /// </summary>
    public partial class AudioDeviceListItem : UserControl
    {
        public readonly IAudioDevice AudioDevice;

        public AudioDeviceListItem(IAudioDevice device)
        {
            InitializeComponent();

            NameLabel.Content = device.FriendlyName;

            try
            {
                IconImage.Source = IconResourceCache.LoadIconAsBitmap(device.DeviceClassIconPath, true);
            }
            catch (Exception)
            {
                // Disregard errors -- we'll just have to make do without an image
            }

            AudioDevice = device;
        }
    }
}
