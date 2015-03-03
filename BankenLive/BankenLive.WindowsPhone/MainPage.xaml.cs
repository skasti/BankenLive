using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Notifications;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace BankenLive
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            AccountInfo.Text = "Laster...";

            if (Session.Current.HasToken && (await Session.Current.Login()))
            {
                var account = await Session.Current.GetInfo();

                if (account == null)
                    return;

                AccountInfo.Text = account.ToString();
            }
            else
            {
                AccountInfo.Text = "Logg inn for å hente data";
            }

            RegisterTask();
        }

        private async void RegisterTask()
        {
            BackgroundExecutionManager.RemoveAccess();

            var backgroundAccessStatus = await BackgroundExecutionManager.RequestAccessAsync();
            if (backgroundAccessStatus == BackgroundAccessStatus.AllowedMayUseActiveRealTimeConnectivity ||
                backgroundAccessStatus == BackgroundAccessStatus.AllowedWithAlwaysOnRealTimeConnectivity)
            {

                foreach (var task in BackgroundTaskRegistration.AllTasks)
                {
                    if ((task.Value.Name == "BankenLiveTimedUpdater") 
                        || (task.Value.Name == "BankenLiveUserPresentUpdater"))
                    {
                        task.Value.Unregister(true);
                    }
                }

                var tileUpdaterBuilder = new BackgroundTaskBuilder();

                tileUpdaterBuilder.Name = "BankenLiveTimedUpdater";
                tileUpdaterBuilder.TaskEntryPoint = "BackgroundTasksWP.TileUpdater";
                tileUpdaterBuilder.SetTrigger(new TimeTrigger(30, false));

                tileUpdaterBuilder.Register();

                tileUpdaterBuilder = new BackgroundTaskBuilder();

                tileUpdaterBuilder.Name = "BankenLiveUserPresentUpdater";
                tileUpdaterBuilder.TaskEntryPoint = "BackgroundTasksWP.TileUpdater";
                tileUpdaterBuilder.SetTrigger(new SystemTrigger(SystemTriggerType.UserPresent, false));

                tileUpdaterBuilder.Register();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof (LoginPage));
        }

        private void UpdateTile_Click(object sender, RoutedEventArgs e)
        {
            var updater = TileUpdateManager.CreateTileUpdaterForApplication();
            var contentXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWidePeekImage06);

            contentXml.GetElementsByTagName("text")[0].InnerText = AccountInfo.Text;

            foreach (XmlElement image in contentXml.GetElementsByTagName("image"))
                image.SetAttribute("src", "ms-appx:///Assets/Wide310x150Logo.png");

            updater.Update(new TileNotification(contentXml));
        }
    }
}
