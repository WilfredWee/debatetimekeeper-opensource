using D.Timekeeper_Universal.Common;
using D.Timekeeper_Universal.DataModel;
using D.Timekeeper_Universal.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using Windows.ApplicationModel.Resources;
using Windows.ApplicationModel.Store;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.Phone.UI.Input;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

namespace D.Timekeeper_Universal
{
    public sealed partial class HubPage : Page
    {
        private readonly NavigationHelper navigationHelper;
        private IEnumerable<DebateFormat> debateFormats;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");

        public HubPage()
        {
            this.InitializeComponent();

            // Hub is only supported in Portrait orientation
            DisplayInformation.AutoRotationPreferences = DisplayOrientations.Portrait;

            this.NavigationCacheMode = NavigationCacheMode.Required;

            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;
            Hub.Header = "Debate Timekeeper";
        }
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public IEnumerable<DebateFormat> DebateFormats
        {
            get { return this.debateFormats; }
        }

        private async void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {
            this.debateFormats = await DebateFormatsSource.GetDebateFormatsAsync();
        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {

        }

        private void DebateFormatsSection_ItemClick(object sender, ItemClickEventArgs e)
        {
            var debateFormat = ((DebateFormat)e.ClickedItem);
            if (!Frame.Navigate(typeof(TimerPage), debateFormat))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
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
            // Override NavigationHelper's implementation by requiring
            // the application to always exit when it is at HubPage.
            HardwareButtons.BackPressed += (sender, backPressedEvent) =>
                {
                    if (Frame.CurrentSourcePageType == typeof(HubPage))
                    {
                        backPressedEvent.Handled = true;
                        Application.Current.Exit();
                    }
                };
            this.navigationHelper.OnNavigatedTo(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            this.navigationHelper.OnNavigatedFrom(e);
        }

        #endregion

        private void AppBarAddButton_Click(object sender, RoutedEventArgs e)
        {
            if(!Frame.Navigate(typeof(NewFormatPage), null))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }
        }

        private async void DeleteFlyout_Click(object sender, RoutedEventArgs e)
        {
            DebateFormat debateFormatToDelete = (DebateFormat)((MenuFlyoutItem)sender).DataContext;

            TextBlock deleteTextBlock = new TextBlock()
            {
                TextWrapping = TextWrapping.WrapWholeWords,
                Text = "Are you sure you want to delete " + debateFormatToDelete.name + "? This is permanent."
            };

            ContentDialog deleteDialog = new ContentDialog()
            {
                Title = "Please Confirm",
                Content = deleteTextBlock,
                PrimaryButtonText = "Yes",
                SecondaryButtonText = "No"
            };

            deleteDialog.PrimaryButtonClick += async (dialog, clickEvent) =>
                {
                    this.debateFormats = await DebateFormatsSource.DeleteDebateFormat(debateFormatToDelete);
                };

            await deleteDialog.ShowAsync();
        }


        private void StackPanel_Holding(object sender, HoldingRoutedEventArgs e)
        {
            FrameworkElement senderElement = sender as FrameworkElement;
            FlyoutBase flyoutBase = FlyoutBase.GetAttachedFlyout(senderElement);

            flyoutBase.ShowAt(senderElement);
        }

        private async void InfoFlyout_Click(object sender, RoutedEventArgs e)
        {
            DebateFormat debateFormat = (DebateFormat)((MenuFlyoutItem)sender).DataContext;

            StringBuilder contentSB = new StringBuilder();
            foreach(Speaker speaker in debateFormat.speakers)
            {
                contentSB.Append(speaker.name + ": \n");

                foreach(KeyValuePair<int, int> ringPair in speaker.ringPairs)
                {
                    int minutes = ringPair.Key/60;
                    int seconds = ringPair.Key % 60;

                    contentSB.Append("Minutes: " + minutes.ToString() + " ");
                    contentSB.Append("Seconds: " + seconds.ToString() + " ");
                    contentSB.Append("Rings: " + ringPair.Value.ToString() + "\n");
                }
                contentSB.Append("\n");
            }

            TextBlock infoTextBlock = new TextBlock()
            {
                Text = contentSB.ToString()
            };

            ScrollViewer infoScrollViewer = new ScrollViewer()
            {
                Content = infoTextBlock,
                VerticalScrollMode = ScrollMode.Enabled,
                Height = Window.Current.Bounds.Height - 150
            };

            ContentDialog infoContentDialog = new ContentDialog()
            {
                Title = debateFormat.name,
                Content = infoScrollViewer,
                PrimaryButtonText = "ok",
            };

            await infoContentDialog.ShowAsync();
        }

        private async void RateAndReviewTextBlock_Tapped(object sender, TappedRoutedEventArgs e)
        {
            await Launcher.LaunchUriAsync(new Uri("ms-windows-store:reviewapp?appid=" + CurrentApp.AppId));
        }

    }
}