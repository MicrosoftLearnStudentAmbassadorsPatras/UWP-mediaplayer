using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace Media_Player
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private ButtonStates button_state;

        public MainPage()
        {
            this.InitializeComponent();
        }

        // Buttons ---------------------------------------------------------------------

        // Handles changing states & images
        private void start_Click(object sender, RoutedEventArgs e)
        {
            if(button_state == ButtonStates.PAUSED)
            {
                m.Play(); // play music
                button_state = ButtonStates.PLAYING; // change state
                start_img.Source = new BitmapImage(new Uri("ms-appx:///Pics/pause.png", UriKind.RelativeOrAbsolute)); // change img
            }
            else if (button_state == ButtonStates.PLAYING)
            {
                m.Pause(); // pause music
                button_state = ButtonStates.PAUSED;
                start_img.Source = new BitmapImage(new Uri("ms-appx:///Pics/play.png", UriKind.RelativeOrAbsolute));
            }
            else if (button_state == ButtonStates.STOPED)
            {
                m.Play(); // play music
                button_state = ButtonStates.PLAYING;
                start_img.Source = new BitmapImage(new Uri("ms-appx:///Pics/pause.png", UriKind.RelativeOrAbsolute));
            }
            
        }

        private void stop_Click(object sender, RoutedEventArgs e)
        {
            button_state = ButtonStates.STOPED;
            start_img.Source = new BitmapImage(new Uri("ms-appx:///Pics/play.png", UriKind.RelativeOrAbsolute));
            m.Stop();
            timelineSlider.Value = 0; // reset slider
        }

        // Volume ----------------------------------------------------------------------
        private void ChangeMediaVolume(object sender, RangeBaseValueChangedEventArgs e)
        {
            m.Volume = volumeSlider.Value;
        }

        // Seeker ----------------------------------------------------------------------
        private void SeekToMediaPosition(object sender, RangeBaseValueChangedEventArgs e)
        {
            int SliderValue = (int)timelineSlider.Value;

            // days, hours, minutes, seconds, milliseconds. 
            // use seconds because timelineslider.Maximum is set to display seconds
            TimeSpan ts = new TimeSpan(0, 0, 0, SliderValue, 0);
            m.Position = ts;
        }

        // File Hanlding ---------------------------------------------------------------

        // Executes when media file ends
        private void m_MediaEnded(object sender, RoutedEventArgs e)
        {
            start_img.Source = new BitmapImage(new Uri("ms-appx:///Pics/play.png", UriKind.RelativeOrAbsolute));
            button_state = ButtonStates.STOPED;
            m.Stop();
        }

        // Executes when a media file is opened
        private void m_MediaOpened(object sender, RoutedEventArgs e)
        {
            // Init Slider + calculate total song time
            int temp = (int)m.NaturalDuration.TimeSpan.TotalSeconds;
            int min, sec;
            min = temp / 60;
            sec = temp - min * 60;

            timelineSlider.Maximum = temp; // change slider scale
            time.Text = "Total time: "+ min + ":" + sec; // change textblock's text

            start_img.Source = new BitmapImage(new Uri("ms-appx:///Pics/pause.png", UriKind.RelativeOrAbsolute));
            button_state = ButtonStates.PLAYING;
            m.Play();
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = DataPackageOperation.Copy;

            e.DragUIOverride.Caption = "drop to create a copy of the file and play it.";
            e.DragUIOverride.IsCaptionVisible = true;
        }

        // Executes when user drops a file on the app
        private async void Grid_Drop(object sender, DragEventArgs e)
        {
            if (e.DataView.Contains(StandardDataFormats.StorageItems))
            {
                var items = await e.DataView.GetStorageItemsAsync();

                if (items.Any())
                {
                    var storageFile = items[0] as StorageFile;
                    var contentType = storageFile.ContentType;

                    StorageFolder folder = ApplicationData.Current.LocalFolder;

                    pathTextBox.Text = "File Path: " + folder.Path;
                    title.Text = "Title: " + storageFile.Name;

                    if (contentType == "audio/mpeg")
                    {
                        StorageFile newfile = await storageFile.CopyAsync(folder, storageFile.Name, NameCollisionOption.ReplaceExisting);
                        m.SetSource(await storageFile.OpenAsync(FileAccessMode.Read), contentType);

                    }
                }
            }
        }

        // Auxiliary -------------------------------------------------------------------

        // Enum for the states
        public enum ButtonStates : byte
        {
            PLAYING,
            PAUSED,
            STOPED
        }

    }
}
