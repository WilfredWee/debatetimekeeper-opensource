using D.Timekeeper_Universal.Common;
using D.Timekeeper_Universal.DataModel;
using D.Timekeeper_Universal.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Serialization;
using System.Text;
using Windows.Data.Json;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Phone.UI.Input;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace D.Timekeeper_Universal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewFormatPage : Page
    {
        private readonly NavigationHelper navigationHelper;

        private ObservableCollection<Speaker> speakers = new ObservableCollection<Speaker>();

        public NewFormatPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;


        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableCollection<Speaker> Speakers
        {
            get { return this.speakers; }
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="sender">
        /// The source of the event; typically <see cref="NavigationHelper"/>
        /// </param>
        /// <param name="e">Event data that provides both the navigation parameter passed to
        /// <see cref="Frame.Navigate(Type, object)"/> when this page was initially requested and
        /// a dictionary of state preserved by this page during an earlier
        /// session.  The state will be null the first time a page is visited.</param>
        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            
            // Order matters. We are checking for both conditions in the second if.
            if(e.NavigationParameter is Speaker)
            {
                Speaker newSpeaker = (Speaker)e.NavigationParameter;
                if(!speakers.Contains(newSpeaker))
                {
                    speakers.Add(newSpeaker);
                }
            }
            //If navigated from the HubPage
            else if (e.PageState == null)
            {
                speakers.Clear();
                FormatNameTextBox.Text = String.Empty;
            }

            if(speakers.Count > 0 && FormatNameTextBox.Text.Length > 0)
            {
                SaveAppBarButton.IsEnabled = true;
            }
            else
            {
                SaveAppBarButton.IsEnabled = false;
            }
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="sender">The source of the event; typically <see cref="NavigationHelper"/></param>
        /// <param name="e">Event data that provides an empty dictionary to be populated with
        /// serializable state.</param>
        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {
            e.PageState["speakers"] = speakers;
        }


        ///// <summary>
        ///// Shows the details of an item clicked on in the <see cref="ItemPage"/>
        ///// </summary>
        ///// <param name="sender">The source of the click event.</param>
        ///// <param name="e">Defaults about the click event.</param>
        //private void ItemView_ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    var itemId = ((SampleDataItem)e.ClickedItem).UniqueId;
        //    if (!Frame.Navigate(typeof(ItemPage), itemId))
        //    {
        //        throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
        //    }
        //}

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
            // Override NavigationHelper's navigation by requiring 
            // NewFormatPage to always navigate back to HubPage
            HardwareButtons.BackPressed += (sender, backPressedEvent) =>
            {
                if (Frame.CurrentSourcePageType == typeof(NewFormatPage))
                {
                    backPressedEvent.Handled = true;
                    if (!Frame.Navigate(typeof(HubPage), null))
                    {
                        throw new Exception("NavigationFailedException");
                    }
                }
            };

            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private async void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            LoadingRing.IsActive = true;
            LoadingRing.Visibility = Visibility.Visible;

            DebateFormat df = new DebateFormat(FormatNameTextBox.Text, speakers.ToList());

            await DebateFormatsSource.SaveNewDebateFormat(df);

            LoadingRing.IsActive = false;
            LoadingRing.Visibility = Visibility.Collapsed;

            if (!Frame.Navigate(typeof(HubPage), null))
            {
                throw new Exception("NavigationFailedException");
            }

        }

        private void AddSpeakerButton_Click(object sender, RoutedEventArgs e)
        {
            if (!Frame.Navigate(typeof(NewSpeakerPage), null))
            {
                throw new Exception("NavigationFailedException");
            }
        }

        private async void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            Speaker selectedSpeker = ((Speaker)e.ClickedItem);
            StringBuilder sb = new StringBuilder();

            foreach(KeyValuePair<int, int> ringPair in selectedSpeker.ringPairs)
            {
                int minutes = ringPair.Key/60;
                int seconds = ringPair.Key % 60;

                sb.Append("Minutes: " + minutes.ToString() + " ");
                sb.Append("Seconds: " + seconds.ToString() + " ");
                sb.Append("Rings: " + ringPair.Value.ToString() + "\n");
            }

            ContentDialog speakerInfoDialog = new ContentDialog()
            {
                Title = selectedSpeker.name,
                Content = sb.ToString(),
                PrimaryButtonText = "ok"
            };

            await speakerInfoDialog.ShowAsync();
        }

        private void SpeakerStackPanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private void SpeakerDeleteFlyoutItem_Click(object sender, RoutedEventArgs e)
        {
            Speaker speakerToDelete = (Speaker)((MenuFlyoutItem)sender).DataContext;
            speakers.Remove(speakerToDelete);
        }

    }
}
