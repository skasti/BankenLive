﻿using System;
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

            if (!session.HasToken)
                return;

            if (!await session.Login())
                return;

            var account = await session.GetInfo();

            var updater = TileUpdateManager.CreateTileUpdaterForApplication("App");
            var contentXml = TileUpdateManager.GetTemplateContent(TileTemplateType.TileWidePeekImage06);

            contentXml.GetElementsByTagName("text")[0].InnerText = account.ToString();

            foreach (XmlElement image in contentXml.GetElementsByTagName("image"))
                image.SetAttribute("src", "ms-appx:///Assets/Wide310x150Logo.png");

            updater.Update(new TileNotification(contentXml));

            deferral.Complete();
        }
    }
}
