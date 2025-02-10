using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
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
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.CustomAttributes;
using System.Collections.ObjectModel;
using static App3.App;
using Windows.UI.Notifications;
using Windows.ApplicationModel.Core;
using Windows.UI.Core;
using Windows.System;
using System.Text.RegularExpressions;


namespace App3
{
    public class Collection_List
    {
        public string CollectionTitle { get; set; }
        public string CollectionUri { get; set; }
    }
    public class History_List
    {
        public string HistoryTitle { get; set; }
        public string HistoryUri { get; set; }
    }
    public class Tab_List
    {
        public string Title { get; set; }
    }

    sealed partial class App : Application
    {
        public ObservableCollection<Collection_List> CollectionList { get; } = new ObservableCollection<Collection_List>();
        public ObservableCollection<History_List> HistoryList { get; } = new ObservableCollection<History_List>();
        public ObservableCollection<Tab_List> TabList { get; } = new ObservableCollection<Tab_List>();

        public Frame RootFrame;
        DispatcherTimer Timer;
        public string WebLink = "";
        public string TabStartLink = "about:blank";
        public int ThemeSelected = 0;
        public bool ShowCollection = true;
        public bool ShowHistory = true;
        public bool ShowDownload = true;
        public bool ShowFullScreen = false;
        public int WebViewSelected = 0;
        public string SearchToolLink = "https://cn.bing.com/search?q=";
        public int LayoutState = 0;

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;

            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue App_Theme = (ApplicationDataCompositeValue)LocalSettings.Values["App_Theme"];
            if (App_Theme != null)
            {
                ThemeSelected = (int)App_Theme["App_Theme"];
            }

            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 0, 1, 0);
            Timer.Tick += Timer_Tick;
            Timer.Start();
        }

        private void Timer_Tick(object sender, object e)
        {
            if (ThemeSelected == 0)
            {
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
        }

        /// <summary>
        /// 在应用程序由最终用户正常启动时进行调用。
        /// 将在启动应用程序以打开特定文件等情况下使用。
        /// </summary>
        /// <param name="e">有关启动请求和过程的详细信息。</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            RootFrame = Window.Current.Content as Frame;

            // 不要在窗口已包含内容时重复应用程序初始化，
            // 只需确保窗口处于活动状态
            if (RootFrame == null)
            {
                // 创建要充当导航上下文的框架，并导航到第一页
                RootFrame = new Frame();

                RootFrame.NavigationFailed += OnNavigationFailed;

                if (e.PreviousExecutionState == ApplicationExecutionState.Terminated)
                {
                    //TODO: 从之前挂起的应用程序加载状态
                }

                // 将框架放在当前窗口中
                GetSettings();
                Window.Current.Content = RootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (RootFrame.Content == null)
                {
                    // 当导航堆栈尚未还原时，导航到第一页，
                    // 并通过将所需信息作为导航参数传入来配置
                    // 参数
                    RootFrame.Navigate(typeof(MainPage), e.Arguments);
                }
                // 确保当前窗口处于活动状态
                Window.Current.Activate();
            }
        }

        protected override void OnActivated(IActivatedEventArgs args)
        {
            bool NeedNewTab = true;
            if (RootFrame == null)
            {
                NeedNewTab = false;

                RootFrame = Window.Current.Content as Frame;
                // Create a Frame to act as the navigation context and navigate to the first page
                RootFrame = new Frame();

                RootFrame.NavigationFailed += OnNavigationFailed;
                RootFrame.Navigate(typeof(MainPage));

                Window.Current.Activate();

                // Place the frame in the current Window
                GetSettings();
                Window.Current.Content = RootFrame;
            }

            ProtocolActivatedEventArgs eventArgs = args as ProtocolActivatedEventArgs;
            string Link = eventArgs.Uri.ToString();
            if (Link.StartsWith("msedge-edge18:"))
            {
                Link = Link.Remove(0, 14);
            }

            if(Link.Length > 0)
            {
                MatchCollection IsMatch;
                int CanWebNav = 0;

                IsMatch = Regex.Matches(Link, @" ");
                foreach (Match m in IsMatch)
                {
                    CanWebNav++;
                }
                if (CanWebNav > 0)
                {
                    Link = SearchToolLink + Link;
                }
                TabStartLink = Link;
            }
            else
            {
                Link = "about:blank";
                TabStartLink = Link;
            }

            if(NeedNewTab)
            {
                MainPage.Browser.TabView_AddButtonClick(null, null);
            }
        }

        private void GetSettings()
        {
            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue App_Theme = (ApplicationDataCompositeValue)LocalSettings.Values["App_Theme"];
            if (App_Theme != null)
            {
                int ThemeSelected = (int)App_Theme["App_Theme"];
                if (ThemeSelected == 0)
                {
                    RootFrame.RequestedTheme = ElementTheme.Default;
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
                    RootFrame.RequestedTheme = ElementTheme.Light;
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
                    RootFrame.RequestedTheme = ElementTheme.Dark;
                    ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
                    TitleBar.ButtonForegroundColor = Colors.White;
                    TitleBar.ButtonHoverForegroundColor = Colors.White;
                    TitleBar.ButtonPressedForegroundColor = Colors.White;
                    TitleBar.ButtonInactiveForegroundColor = Colors.DarkGray;
                    TitleBar.ButtonHoverBackgroundColor = Colors.Black;
                    TitleBar.ButtonPressedBackgroundColor = Colors.Gray;
                }
            }

            Windows.Storage.ApplicationDataCompositeValue WebView_Mode = (ApplicationDataCompositeValue)LocalSettings.Values["WebView_Mode"];
            if (WebView_Mode != null)
            {
                WebViewSelected = (int)WebView_Mode["WebView_Mode"];
            }

            Windows.Storage.ApplicationDataCompositeValue ShowCollectionSwitch = (ApplicationDataCompositeValue)LocalSettings.Values["ShowCollectionSwitch"];
            if (ShowCollectionSwitch != null)
            {
                ShowCollection = (bool)ShowCollectionSwitch["ShowCollectionSwitch"];
            }
            Windows.Storage.ApplicationDataCompositeValue ShowHistorySwitch = (ApplicationDataCompositeValue)LocalSettings.Values["ShowHistorySwitch"];
            if (ShowHistorySwitch != null)
            {
                ShowHistory = (bool)ShowHistorySwitch["ShowHistorySwitch"];
            }
            Windows.Storage.ApplicationDataCompositeValue ShowDownloadSwitch = (ApplicationDataCompositeValue)LocalSettings.Values["ShowDownloadSwitch"];
            if (ShowDownloadSwitch != null)
            {
                ShowDownload = (bool)ShowDownloadSwitch["ShowDownloadSwitch"];
            }
            Windows.Storage.ApplicationDataCompositeValue ShowFullScreenSwitch = (ApplicationDataCompositeValue)LocalSettings.Values["ShowFullScreenSwitch"];
            if (ShowFullScreenSwitch != null)
            {
                ShowFullScreen = (bool)ShowFullScreenSwitch["ShowFullScreenSwitch"];
            }

            Windows.Storage.ApplicationDataCompositeValue SearchLink = (ApplicationDataCompositeValue)LocalSettings.Values["SearchLink"];
            if (SearchLink != null)
            {
                SearchToolLink = SearchLink["SearchLink"].ToString();
            }

            GetCollection();
            GetHistory();
        }

        public async void GetCollection()
        {
            Windows.Storage.StorageFolder StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            int CollectionCount = 0;
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

            this.CollectionList.Clear();
            if (CollectionCount > 0)
            {
                for (int i = 1; i <= CollectionCount; i++)
                {
                    string FileCollectionTitle = "Collection\\CollectionTitle" + i.ToString();
                    StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    try
                    {
                        file = await StorageFolder.GetFileAsync(FileCollectionTitle);
                        var FileCollectionList = await Windows.Storage.FileIO.ReadLinesAsync(file);
                        this.CollectionList.Add(new Collection_List() { CollectionTitle = FileCollectionList[0], CollectionUri = FileCollectionList[1] });
                    }
                    catch
                    {

                    }
                }
            }
        }
        public async void GetHistory()
        {
            Windows.Storage.StorageFolder StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            int HistoryCount = 0;
            int HistorySearchMin = 0;
            int HistoryMaxSet = 0;
            Windows.Storage.StorageFile file;
            try
            {
                file = await StorageFolder.GetFileAsync("History\\HistoryCount");
                var Count = await Windows.Storage.FileIO.ReadLinesAsync(file);
                HistoryCount = Int32.Parse(Count[0]);
            }
            catch
            {

            }
            ApplicationDataContainer LocalSettings = Windows.Storage.ApplicationData.Current.LocalSettings;
            Windows.Storage.ApplicationDataCompositeValue HistoryItems = (ApplicationDataCompositeValue)LocalSettings.Values["HistoryItems"];
            if (HistoryItems != null)
            {
                HistoryMaxSet = Int32.Parse(HistoryItems["HistoryItems"].ToString());
            }
            if (HistoryMaxSet == 0)
            {
                HistorySearchMin = HistoryCount - 99;
            }
            else if(HistoryMaxSet == 1)
            {
                HistorySearchMin = HistoryCount - 199;
            }
            else if(HistoryMaxSet == 2)
            {
                HistorySearchMin = HistoryCount - 399;
            }
            else if(HistoryMaxSet == 3)
            {
                HistorySearchMin = HistoryCount - 799;
            }
            if (HistorySearchMin < 1)
            {
                HistorySearchMin = 1;
            }
            this.HistoryList.Clear();
            if (HistoryCount > 0)
            {
                for (int i = HistoryCount; i >= HistorySearchMin; i--)
                {
                    string FileHistoryTitle = "History\\HistoryTitle" + i.ToString();
                    StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                    try
                    {
                        file = await StorageFolder.GetFileAsync(FileHistoryTitle);
                        var FileHistoryList = await Windows.Storage.FileIO.ReadLinesAsync(file);
                        this.HistoryList.Add(new History_List() { HistoryTitle = FileHistoryList[0], HistoryUri = FileHistoryList[1] });
                    }
                    catch
                    {

                    }
                }
            }
        }

        /// <summary>
        /// 导航到特定页失败时调用
        /// </summary>
        ///<param name="sender">导航失败的框架</param>
        ///<param name="e">有关导航失败的详细信息</param>
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }

        /// <summary>
        /// 在将要挂起应用程序执行时调用。  在不知道应用程序
        /// 无需知道应用程序会被终止还是会恢复，
        /// 并让内存内容保持不变。
        /// </summary>
        /// <param name="sender">挂起的请求的源。</param>
        /// <param name="e">有关挂起请求的详细信息。</param>
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();
            //TODO: 保存应用程序状态并停止任何后台活动
            deferral.Complete();
        }
    }
}
