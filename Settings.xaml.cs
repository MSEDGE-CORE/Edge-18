using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.ViewManagement;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage.Streams;
using Windows.Storage.Provider;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.UI.Xaml.Media.Animation;
using System.Text.RegularExpressions;
using Windows.ApplicationModel;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App3
{
    public sealed partial class Settings : Page
    {
        int SearchTool = 0;

        public static MainPage Browser
        {
            get { return (Window.Current.Content as Frame)?.Content as MainPage; }
        }

        public Settings()
        {
            this.InitializeComponent();
            TextVersion.Text = string.Format("名称：Edge 18\n版本：{0}.{1}.{2}", Package.Current.Id.Version.Major, Package.Current.Id.Version.Minor, Package.Current.Id.Version.Build);


            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue History_Items = (ApplicationDataCompositeValue)LocalSettings.Values["HistoryItems"];
            if (History_Items != null)
            {
                HistoryItems_Selection.SelectedIndex = (int)History_Items["HistoryItems"];
            }
            else
            {
                HistoryItems_Selection.SelectedIndex = 0;
            }

            Theme_Selection.SelectedIndex = (Application.Current as App).ThemeSelected;

            Windows.Storage.ApplicationDataCompositeValue WebView_Mode = (ApplicationDataCompositeValue)LocalSettings.Values["WebView_Mode"];
            if (WebView_Mode != null)
            {
                WebView_Selection.SelectedIndex = (int)WebView_Mode["WebView_Mode"];
            }
            else
            {
                WebView_Selection.SelectedIndex = 0;
            }

            ShowCollection_Switch.IsOn = (Application.Current as App).ShowCollection;
            ShowHistory_Switch.IsOn = (Application.Current as App).ShowHistory;
            ShowDownload_Switch.IsOn = (Application.Current as App).ShowDownload;
            ShowFullScreen_Switch.IsOn = (Application.Current as App).ShowFullScreen;
            Restart_Button.Visibility = Visibility.Collapsed;

            if ((Application.Current as App).SearchToolLink == "https://cn.bing.com/search?q=")
            {
                Search_Selection.SelectedIndex = 0;
                SetSearch_Button.Visibility = Visibility.Collapsed;
            }
            else if ((Application.Current as App).SearchToolLink == "https://www.baidu.com/s?wd=")
            {
                Search_Selection.SelectedIndex = 1;
                SetSearch_Button.Visibility = Visibility.Collapsed;
            }
            else if ((Application.Current as App).SearchToolLink == "https://www.google.com/search?q=")
            {
                Search_Selection.SelectedIndex = 2;
                SetSearch_Button.Visibility = Visibility.Collapsed;
            }
            else if ((Application.Current as App).SearchToolLink == "https://www.sogou.com/web?query=")
            {
                Search_Selection.SelectedIndex = 3;
                SetSearch_Button.Visibility = Visibility.Collapsed;
            }
            else if ((Application.Current as App).SearchToolLink == "https://www.so.com/s?q=")
            {
                Search_Selection.SelectedIndex = 4;
                SetSearch_Button.Visibility = Visibility.Collapsed;
            }
            else
            {
                Search_Selection.SelectedIndex = 5;
                SetSearch_Button.Visibility = Visibility.Visible;
            }
        }

        private void Theme_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int ThemeSelected = Theme_Selection.SelectedIndex;
            if (ThemeSelected == 0)
            {
                (Window.Current.Content as Frame).RequestedTheme = ElementTheme.Default;
                ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
                if (App.Current.RequestedTheme == ApplicationTheme.Light)
                {
                    TitleBar.ButtonForegroundColor = Colors.Black;
                    TitleBar.ButtonHoverForegroundColor = Colors.Black;
                    TitleBar.ButtonPressedForegroundColor = Colors.Black;
                    TitleBar.ButtonInactiveForegroundColor = Colors.Gray;
                    TitleBar.ButtonHoverBackgroundColor = Colors.White;
                    TitleBar.ButtonPressedBackgroundColor = Colors.LightGray;
                }
                else if (App.Current.RequestedTheme == ApplicationTheme.Dark)
                {
                    TitleBar.ButtonForegroundColor = Colors.White;
                    TitleBar.ButtonHoverForegroundColor = Colors.White;
                    TitleBar.ButtonPressedForegroundColor = Colors.White;
                    TitleBar.ButtonInactiveForegroundColor = Colors.DarkGray;
                    TitleBar.ButtonHoverBackgroundColor = Colors.Black;
                    TitleBar.ButtonPressedBackgroundColor = Colors.Gray;
                }
            }
            else if (ThemeSelected == 1)
            {
                (Window.Current.Content as Frame).RequestedTheme = ElementTheme.Light;
                ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
                TitleBar.ButtonForegroundColor = Colors.Black;
                TitleBar.ButtonHoverForegroundColor = Colors.Black;
                TitleBar.ButtonPressedForegroundColor = Colors.Black;
                TitleBar.ButtonInactiveForegroundColor = Colors.Gray;
                TitleBar.ButtonHoverBackgroundColor = Colors.White;
                TitleBar.ButtonPressedBackgroundColor = Colors.LightGray;
            }
            else if (ThemeSelected == 2)
            {
                (Window.Current.Content as Frame).RequestedTheme = ElementTheme.Dark;
                ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
                TitleBar.ButtonForegroundColor = Colors.White;
                TitleBar.ButtonHoverForegroundColor = Colors.White;
                TitleBar.ButtonPressedForegroundColor = Colors.White;
                TitleBar.ButtonInactiveForegroundColor = Colors.DarkGray;
                TitleBar.ButtonHoverBackgroundColor = Colors.Black;
                TitleBar.ButtonPressedBackgroundColor = Colors.Gray;
            }
            (Application.Current as App).ThemeSelected = ThemeSelected;

            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue App_Theme = new Windows.Storage.ApplicationDataCompositeValue();
            App_Theme["App_Theme"] = Theme_Selection.SelectedIndex;
            LocalSettings.Values["App_Theme"] = App_Theme;
        }

        private void WebView_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int WebViewSelected = WebView_Selection.SelectedIndex;
            //(Application.Current as App).WebViewSelected = WebViewSelected;

            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue WebView_Mode = new Windows.Storage.ApplicationDataCompositeValue();
            WebView_Mode["WebView_Mode"] = WebView_Selection.SelectedIndex;
            LocalSettings.Values["WebView_Mode"] = WebView_Mode;

            Restart_Button.Visibility = Visibility.Visible;
        }

        private async void Collection_Import(object sender, RoutedEventArgs e)
        {
            Collection_Import_Button.IsEnabled = false;

            List<string> CollectionFile = new List<string> { };
            Windows.Storage.Pickers.FileOpenPicker OpenPicker = new Windows.Storage.Pickers.FileOpenPicker();
            OpenPicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            OpenPicker.FileTypeFilter.Add(".txt");
            Windows.Storage.StorageFile file = await OpenPicker.PickSingleFileAsync();
            if (file != null)
            {
                int CollectionCount = 0;

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    using (var tw = new StreamReader(stream))
                    {
                        CollectionCount = Int32.Parse(tw.ReadLine());
                        for (int i = 1; i <= CollectionCount; i++)
                        {
                            CollectionFile.Add(tw.ReadLine());
                            CollectionFile.Add(tw.ReadLine());
                        }
                    }
                }

                Windows.Storage.StorageFolder StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile Count = await StorageFolder.CreateFileAsync("Collection\\CollectionCount", Windows.Storage.CreationCollisionOption.OpenIfExists);
                await Windows.Storage.FileIO.WriteTextAsync(Count, (CollectionCount + (Application.Current as App).CollectionList.Count()).ToString());
                for (int i = 1; i <= CollectionCount; i++)
                {
                    string CollectionTitle = CollectionFile[i * 2 - 2] + "\n" + CollectionFile[i * 2 - 1] + "\n";
                    string FileCollectionTitle = "Collection\\CollectionTitle" + (i + (Application.Current as App).CollectionList.Count()).ToString();
                    Windows.Storage.StorageFile Title = await StorageFolder.CreateFileAsync(FileCollectionTitle, Windows.Storage.CreationCollisionOption.OpenIfExists);
                    await Windows.Storage.FileIO.WriteTextAsync(Title, CollectionTitle);
                }

                (Application.Current as App).GetCollection();
            }
            else
            {

            }

            Collection_Import_Button.IsEnabled = true;
        }

        private async void Collection_Export(object sender, RoutedEventArgs e)
        {
            Collection_Export_Button.IsEnabled = false;

            int CollectionCount = 0;
            Windows.Storage.StorageFolder StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile file;
            try
            {
                file = await StorageFolder.GetFileAsync("Collection\\CollectionCount");
                var Count = await Windows.Storage.FileIO.ReadLinesAsync(file);
                CollectionCount = Int32.Parse(Count[0]);
            }
            catch
            {

            }
            string CollectionFile = CollectionCount.ToString() + "\n";
            for (int i = 0; i < CollectionCount; i++)
            {
                CollectionFile += (Application.Current as App).CollectionList[i].CollectionTitle + "\n" + (Application.Current as App).CollectionList[i].CollectionUri + "\n";
            }

            Windows.Storage.Pickers.FileSavePicker SavePicker = new Windows.Storage.Pickers.FileSavePicker();
            SavePicker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.Downloads;
            SavePicker.FileTypeChoices.Add("文本文档", new List<string>() { ".txt" });
            SavePicker.DefaultFileExtension = ".txt";
            SavePicker.SuggestedFileName = "EdgeCollections";
            file = await SavePicker.PickSaveFileAsync();
            if (file != null)
            {
                using (var stream = await file.OpenStreamForWriteAsync())
                {
                    using (var tw = new StreamWriter(stream))
                    {
                        tw.WriteLine(CollectionFile);
                    }
                }
                FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);

                if (status == Windows.Storage.Provider.FileUpdateStatus.Complete)
                {

                }
                else
                {

                }
            }
            else
            {

            }

            Collection_Export_Button.IsEnabled = true;
        }

        private async void History_Clear(object sender, RoutedEventArgs e)
        {
            HistoryClearFlyout.Hide();

            (Application.Current as App).HistoryList.Clear();

            Windows.Storage.StorageFolder StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            Windows.Storage.StorageFile Count = await StorageFolder.CreateFileAsync("History\\HistoryCount", Windows.Storage.CreationCollisionOption.OpenIfExists);
            await Windows.Storage.FileIO.WriteTextAsync(Count, "0");
        }

        private void Collection_Toggled(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).ShowCollection = ShowCollection_Switch.IsOn;
            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue ShowCollectionSwitch = new Windows.Storage.ApplicationDataCompositeValue();
            ShowCollectionSwitch["ShowCollectionSwitch"] = ShowCollection_Switch.IsOn;
            LocalSettings.Values["ShowCollectionSwitch"] = ShowCollectionSwitch;
        }
        private void History_Toggled(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).ShowHistory = ShowHistory_Switch.IsOn;
            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue ShowHistorySwitch = new Windows.Storage.ApplicationDataCompositeValue();
            ShowHistorySwitch["ShowHistorySwitch"] = ShowHistory_Switch.IsOn;
            LocalSettings.Values["ShowHistorySwitch"] = ShowHistorySwitch;
        }
        private void Download_Toggled(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).ShowDownload = ShowDownload_Switch.IsOn;
            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue ShowDownloadSwitch = new Windows.Storage.ApplicationDataCompositeValue();
            ShowDownloadSwitch["ShowDownloadSwitch"] = ShowDownload_Switch.IsOn;
            LocalSettings.Values["ShowDownloadSwitch"] = ShowDownloadSwitch;
        }

        private void FullScreen_Toggled(object sender, RoutedEventArgs e)
        {
            (Application.Current as App).ShowFullScreen = ShowFullScreen_Switch.IsOn;
            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue ShowFullScreenSwitch = new Windows.Storage.ApplicationDataCompositeValue();
            ShowFullScreenSwitch["ShowFullScreenSwitch"] = ShowFullScreen_Switch.IsOn;
            LocalSettings.Values["ShowFullScreenSwitch"] = ShowFullScreenSwitch;
        }

        private void HistoryItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int HistoryItems = HistoryItems_Selection.SelectedIndex;
            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue History_Items = new Windows.Storage.ApplicationDataCompositeValue();
            History_Items["HistoryItems"] = HistoryItems_Selection.SelectedIndex;
            LocalSettings.Values["HistoryItems"] = History_Items;

            (Application.Current as App).HistoryList.Clear();
            (Application.Current as App).GetHistory();
        }

        private async void Restart(object sender, RoutedEventArgs e)
        {
            await CoreApplication.RequestRestartAsync(string.Empty);
        }

        private void Search_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SearchTool = Search_Selection.SelectedIndex;
            if (Search_Selection.SelectedIndex == 5)
            {
                SetSearch_Button.Visibility = Visibility.Visible;
            }
            else
            {
                SetSearch_Button.Visibility = Visibility.Collapsed;
            }

            if(Search_Selection.SelectedIndex == 0)
            {
                (Application.Current as App).SearchToolLink = "https://cn.bing.com/search?q=";
            }
            else if (Search_Selection.SelectedIndex == 1)
            {
                (Application.Current as App).SearchToolLink = "https://www.baidu.com/s?wd=";
            }
            else if (Search_Selection.SelectedIndex == 2)
            {
                (Application.Current as App).SearchToolLink = "https://www.google.com/search?q=";
            }
            else if (Search_Selection.SelectedIndex == 3)
            {
                (Application.Current as App).SearchToolLink = "https://www.sogou.com/web?query=";
            }
            else if (Search_Selection.SelectedIndex == 4)
            {
                (Application.Current as App).SearchToolLink = "https://www.so.com/s?q=";
            }
            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue SearchLink = new Windows.Storage.ApplicationDataCompositeValue();
            SearchLink["SearchLink"] = (Application.Current as App).SearchToolLink;
            LocalSettings.Values["SearchLink"] = SearchLink;
        }

        private void SetSearch_Click(object sender, RoutedEventArgs e)
        {
            Search_Link.Text = (Application.Current as App).SearchToolLink;
        }

        private void SetSearch_Complete(object sender, RoutedEventArgs e)
        {
            string Link = Search_Link.Text;

            MatchCollection IsMatch = Regex.Matches(Link, @"^(https?)://");

            int CanWebNav = 0;
            foreach (Match m in IsMatch)
            {
                CanWebNav++;
            }

            if (CanWebNav > 0)
            {
                Set_SearchLink_Flyout.Hide();
                (Application.Current as App).SearchToolLink = Link;
            }
            else
            {
                IsMatch = Regex.Matches(Link, @"\.");
                foreach (Match m in IsMatch)
                {
                    CanWebNav++;
                }
                if (CanWebNav > 0)
                {
                    Link = "https://" + Search_Link.Text;

                    Set_SearchLink_Flyout.Hide();
                    (Application.Current as App).SearchToolLink = Link;
                }
                else
                {

                }
            }
            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue SearchLink = new Windows.Storage.ApplicationDataCompositeValue();
            SearchLink["SearchLink"] = (Application.Current as App).SearchToolLink;
            LocalSettings.Values["SearchLink"] = SearchLink;
        }

        private void Frame_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SetTitleBar(AppTitleBar);
            EntranceAnimation.Begin();
        }

        private void ProjectHome_Click(object sender, RoutedEventArgs e)
        {
            Browser.SettingsBack_Click(sender, e);
            (Application.Current as App).TabStartLink = "https://github.com/MSEDGE-CORE/Edge18";
            MainPage.Browser.TabView_AddButtonClick(null, null);
        }

        private void CheckForUpdate_Click(object sender, RoutedEventArgs e)
        {
            Browser.SettingsBack_Click(sender, e);
            (Application.Current as App).TabStartLink = "https://github.com/MSEDGE-CORE/Edge18/releases";
            MainPage.Browser.TabView_AddButtonClick(null, null);
        }

        private void SendFeedback_Click(object sender, RoutedEventArgs e)
        {
            Browser.SettingsBack_Click(sender, e);
            (Application.Current as App).TabStartLink = "https://github.com/MSEDGE-CORE/Edge18/issues";
            MainPage.Browser.TabView_AddButtonClick(null, null);
        }
    }
}
