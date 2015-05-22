using D.Timekeeper_Universal.Common;
using D.Timekeeper_Universal.Model;
using SharpDX.IO;
using SharpDX.Multimedia;
using SharpDX.Toolkit.Audio;
using SharpDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Storage;
using Windows.System.Display;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace D.Timekeeper_Universal
{
    public sealed partial class TimerPage : Page, TimekeeperReceiver, IDisposable
    {
        DisplayRequest displayRequest;

        private DebateFormat debateFormat;
        private Timekeeper timeKeeper;
        private Speaker currentSpeaker;

        private readonly NavigationHelper navigationHelper;
        private readonly ObservableDictionary defaultViewModel = new ObservableDictionary();

        private XAudio2 xAudio;
        private SoundStream stream;
        private WaveFormat waveFormat;
        private AudioBuffer buffer;

        public TimerPage()
        {
            this.InitializeComponent();

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            xAudio = new XAudio2();
            var masteringVoice = new MasteringVoice(xAudio);
            var nativeFileStream = new NativeFileStream("Assets/Ding.wav", NativeFileMode.Open, NativeFileAccess.Read, NativeFileShare.Read);

            stream = new SoundStream(nativeFileStream);
            waveFormat = stream.Format;
            buffer = new AudioBuffer
            {
                Stream = stream.ToDataStream(),
                AudioBytes = (int)stream.Length,
                Flags = BufferFlags.EndOfStream
            };
            
        }

        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableDictionary DefaultViewModel
        {
            get { return this.defaultViewModel; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            if(displayRequest == null)
            {
                displayRequest = new DisplayRequest();
                displayRequest.RequestActive();
            }

            debateFormat = (DebateFormat)e.NavigationParameter;
            this.DefaultViewModel["DebateFormat"] = debateFormat;

            if(this.currentSpeaker == null)
            {
                this.currentSpeaker = debateFormat.speakers[0];
                timeKeeper = Timekeeper.getInstance(this, currentSpeaker);
            }
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            timeKeeper.resetTimer();
            Dispose();
        }

        private void StartPauseButton_Click(object sender, RoutedEventArgs e)
        {
            StartPauseBtn.Content = (timeKeeper.paused) ? "Pause" : "Start";
            timeKeeper.startStopTimer();
        }

        private async void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            await Reset();
        }

        private void RingButton_Click(object sender, RoutedEventArgs e)
        {
            Ring();
        }

        private void Ring()
        {
            var sourceVoice = new SourceVoice(xAudio, waveFormat, true);

            sourceVoice.SubmitSourceBuffer(buffer, stream.DecodedPacketsInfo);
            sourceVoice.Start();
        }

        public async Task updateTimeText(TimeSpan elapsed)
        {
            string minutes = elapsed.ToString("mm");
            string seconds = elapsed.ToString("ss");

            await TimeTextBlock.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, delegate() { TimeTextBlock.Text = minutes + ":" + seconds; });
        }

        private async void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            currentSpeaker = (Speaker)((Pivot)sender).SelectedItem;
            timeKeeper.changeCurrentSpeaker(currentSpeaker);

            await updateUIToInitialState();
        }

        private async Task updateUIToInitialState()
        {
            SpeakerTextBlock.Text = currentSpeaker.name;
            StartPauseBtn.Content = "Start";
            await updateTimeText(new TimeSpan(0));
        }

        private async Task Reset()
        {
            timeKeeper.resetTimer();
            await updateUIToInitialState();
        }

        public async Task autoRing(int amount)
        {
            for(int i=0; i<amount; i++)
            {
                Ring();
                await Task.Delay(400);
            }
        }

        private void PivotNavigationAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            if(((AppBarButton)sender).Name == "ForwardAppBarButton")
            {
                SpeakerPivot.SelectedIndex = (SpeakerPivot.SelectedIndex + 1) % SpeakerPivot.Items.Count;
            }
            else if (((AppBarButton)sender).Name == "BackwardAppBarButton")
            {
                SpeakerPivot.SelectedIndex = (SpeakerPivot.SelectedIndex == 0)? (SpeakerPivot.Items.Count - 1) : (SpeakerPivot.SelectedIndex - 1);
            }
        }

        public void Dispose()
        {
            if(xAudio != null)
            {
                xAudio.Dispose();
                xAudio = null;
            }
            if(stream != null)
            {
                stream.Dispose();
                stream = null;
            }
        }

        #region NavigationHelper registration

        /// <summary>
        /// The methods provided in this section are simply used to allow
        /// NavigationHelper to respond to the page's navigation methods.
        /// <para>
        /// Page specific logic should be placed in event handlers for the
        /// <see cref="NavigationHelper.LoadState"/>
        /// and <see cref="NavigationHelper.SaveState"/>.
        /// The navigation parameter is available in the LoadState method
        /// in addition to page state preserved during an earlier session.
        /// </para>
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion
    }
}