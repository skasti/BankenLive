using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Data.Xml.Dom;
using Windows.UI.Notifications;
using BankenLive;

namespace BackgroundTasksWP
{
    public sealed class TileUpdater : IBackgroundTask
    {
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            var deferral = taskInstance.GetDeferral();
            var session = Session.Current;
            var settings = BankenLiveSettings.Instance;

            if (!settings.HasToken)
                return;

            if (!await session.Login())
                return;

            var snapshot = await session.GetInfo();

            var updater = TileUpdateManager.CreateTileUpdaterForApplication("App");
            var contentXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWide310x150PeekImage06);

            contentXml.GetElementsByTagName("text")[0].InnerText = snapshot.ToString();

            foreach (XmlElement image in contentXml.GetElementsByTagName("image"))
                image.SetAttribute("src", "ms-appx:///Assets/Wide310x150Logo.png");

            updater.Update(new TileNotification(contentXml));

            var balanceChange = session.Account.BalanceChange;

            if (settings.ToastOnIncreasedBalance && balanceChange > settings.ToastOnIncreasedBalanceThreshold)
            {
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                var toastText = toastXml.GetElementsByTagName("text").First();

                toastText.InnerText = String.Format("Du har mottatt {0} kroner", balanceChange);

                IXmlNode toastNode = toastXml.SelectSingleNode("/toast");
                ((XmlElement)toastNode).SetAttribute("duration", "long");

                var toast = new ToastNotification(toastXml);
                ToastNotificationManager.CreateToastNotifier().Show(toast);
            }

            deferral.Complete();
        }
    }
}
