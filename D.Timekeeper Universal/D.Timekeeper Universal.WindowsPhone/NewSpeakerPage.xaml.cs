using D.Timekeeper_Universal.Common;
using D.Timekeeper_Universal.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;
using Windows.Foundation;
using Windows.Foundation.Collections;
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
    public sealed partial class NewSpeakerPage : Page
    {

        public class RingPair
        {
            public int Time { get; set; }
            public int Amount { get; set; }
        }
        private readonly NavigationHelper navigationHelper;
        private readonly ResourceLoader resourceLoader = ResourceLoader.GetForCurrentView("Resources");
        private ObservableCollection<RingPair> ringPairs = new ObservableCollection<RingPair>();
        private ObservableCollection<int> availableTimes = new ObservableCollection<int>();
        private ObservableCollection<int> availableRingAmounts = new ObservableCollection<int>();

        private RingPair selectedRingPair;

        private int selectedMinute;
        private int selectedSecond;


        public NewSpeakerPage()
        {
            this.InitializeComponent();

            //this.NavigationCacheMode = NavigationCacheMode.Required;
            this.navigationHelper = new NavigationHelper(this);
            this.navigationHelper.LoadState += this.NavigationHelper_LoadState;
            this.navigationHelper.SaveState += this.NavigationHelper_SaveState;

            for (int i = 0; i < 60; i++)
            {
                this.availableTimes.Add(i);
            }

            for (int i = 1; i <= 10; i++)
            {
                this.availableRingAmounts.Add(i);
            }

            this.selectedRingPair = new RingPair()
            {
                Time = 0,
                Amount = 1
            };

            this.ringPairs.Add(this.selectedRingPair);
            // We add the ringPairs collection on the ComboBox SelectionChanged event.

        }

        /// <summary>
        /// Gets the <see cref="NavigationHelper"/> associated with this <see cref="Page"/>.
        /// </summary>
        public NavigationHelper NavigationHelper
        {
            get { return this.navigationHelper; }
        }

        public ObservableCollection<RingPair> RingPairs
        {
            get { return this.ringPairs; }
        }

        public ObservableCollection<int> AvailableTimes
        {
            get { return this.availableTimes; }
        }

        public ObservableCollection<int> AvailableRingAmounts
        {
            get { return this.availableRingAmounts; }
        }

        private void NavigationHelper_LoadState(object sender, LoadStateEventArgs e)
        {

        }

        private void NavigationHelper_SaveState(object sender, SaveStateEventArgs e)
        {

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


        private void MinutesComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = ((int)((ComboBox)sender).SelectedItem);
            this.selectedMinute = selected;
            ChangeRingPairTime();

        }

        private void SecondsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = ((int)((ComboBox)sender).SelectedItem);
            this.selectedSecond = selected;
            ChangeRingPairTime();
        }

        private void RingAmountsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int selected = ((int)((ComboBox)sender).SelectedItem);
            ChangeRingPairAmount(selected);
            
        }

        private void ChangeRingPairTime()
        {
            int newKey = (this.selectedMinute * 60) + (this.selectedSecond);
            if (this.selectedRingPair.Time != newKey)
            {
                this.selectedRingPair.Time = newKey;
            }

        }

        private void ChangeRingPairAmount(int newAmount)
        {
            if (this.selectedRingPair.Amount != newAmount)
            {
                this.selectedRingPair.Amount = newAmount;
            }
        }

        private void SaveAppBarButton_Click(object sender, RoutedEventArgs e)
        {
            Speaker newSpeaker = null;
            if(this.ringPairs.Count > 0)
            {
                List<KeyValuePair<int, int>> newRingPairs = new List<KeyValuePair<int, int>>();
                foreach (RingPair rp in this.ringPairs)
                {
                    newRingPairs.Add(new KeyValuePair<int, int>(rp.Time, rp.Amount));
                }
                newRingPairs.Sort((a, b) => 
                {
                    return a.Key.CompareTo(b.Key);
                });
                newSpeaker = new Speaker(PositionTextBox.Text, newRingPairs);
            }

            if (!Frame.Navigate(typeof(NewFormatPage), newSpeaker))
            {
                throw new Exception(this.resourceLoader.GetString("NavigationFailedExceptionMessage"));
            }

        }

        private void AddRingButton_Click(object sender, RoutedEventArgs e)
        {
            RingPair newRingPair = new RingPair() 
            { 
                Time = 0,
                Amount = 1
            };

            this.ringPairs.Add(newRingPair);
        }

        private void RingPairComboBox_DropDownOpened(object sender, object e)
        {
            DependencyObject depObj = (DependencyObject)((RoutedEventArgs)e).OriginalSource;

            while ((depObj != null) && !(depObj is ListViewItem))
            {
                depObj = VisualTreeHelper.GetParent(depObj);
            }
            this.selectedRingPair = (RingPair)((ListViewItem)depObj).Content;
        }

        private void PositionTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SaveAppBarButton.IsEnabled = (((TextBox)sender).Text.Length > 0)? true : false;
        }
    }
}
