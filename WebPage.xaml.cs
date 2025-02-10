using Microsoft.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using App3;
using Microsoft.Web.WebView2.Core;
using static App3.App;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media.Animation;
using Windows.ApplicationModel.Contacts;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App3
{
    public sealed partial class WebPage : Page
    {
        DispatcherTimer Timer;
        bool PageReqFS = false;

        public static MainPage Browser
        {
            get { return (Window.Current.Content as Frame)?.Content as MainPage; }
        }

        public WebPage()
        {
            this.InitializeComponent();

            LinkBox.Focus(FocusState.Keyboard);
        }

        bool LinkTyping = false;
        bool WebNavigating = false;
        int LayoutState = 0;

        private void Timer_Tick(object sender, object e)
        {
            Page_SizeChanged();
            isLoaded = true;

            if (Browser.SelectedTab.Content == this.Frame)
            {
                (Application.Current as App).WebLink = EdgeWebView.Source.ToString();
                if (EdgeWebView.Source.ToString() == "about:blank")
                {
                    Browser.SelectedTab.Header = "新标签页";
                }
                else if(EdgeWebView.DocumentTitle == "")
                {
                    Browser.SelectedTab.Header = "加载中";
                }
                else
                {
                    Browser.SelectedTab.Header = EdgeWebView.DocumentTitle;
                }
            }


            if (WebNavigating == true && (EdgeWebView.Source.ToString() != "about:blank"))
            {
                if (ProgressBar.Visibility == Visibility.Collapsed)
                {
                    ProgressBar.IsIndeterminate = false;
                    ProgressBar.IsIndeterminate = true;
                    ProgressBar.Visibility = Visibility.Visible;
                }
            }
            else
            {
                if (ProgressBar.Visibility == Visibility.Visible)
                {
                    ProgressBar.Visibility = Visibility.Collapsed;
                }
            }

            if (DownloadFrame.Visibility == Visibility.Visible && DownloadFrame.Opacity == 0)
            {
                DownloadFrame.Visibility = Visibility.Collapsed;
            }

            if (LinkTyping == false)
            {
                string LastPage = LinkBox.Text;
                LinkBox.Text = EdgeWebView.Source.ToString();
                if (EdgeWebView.Source.ToString() == "about:blank")
                {
                    LinkBox.Text = "";
                }
                else
                {

                }
            }

            if (EdgeWebView.Source.ToString() != "about:blank")
            {
                if (EdgeWebView.VerticalAlignment != VerticalAlignment.Stretch)
                {
                    EdgeWebView.VerticalAlignment = VerticalAlignment.Stretch;
                }
            }
            else if (EdgeWebView.VerticalAlignment != VerticalAlignment.Top && WebNavigating == false)
            {
                EdgeWebView.VerticalAlignment = VerticalAlignment.Top;
                EdgeWebView.Refresh();
            }

            if (EdgeWebView.VerticalAlignment == VerticalAlignment.Stretch)
            {
                SearchBox.Text = "";
            }
        }

        private void WebNavStart(object sender, WebViewNavigationStartingEventArgs args)
        {
            WebNavigating = true;
        }
        private async void WebNavComplete(WebView sender, WebViewNavigationCompletedEventArgs args)
        {
            WebNavigating = false;
            PageReqFS = false;

            if (EdgeWebView.Source.ToString() != "about:blank")
            {
                Windows.Storage.StorageFolder StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                int HistoryCount = 1;
                try
                {
                    Windows.Storage.StorageFile file;
                    file = await StorageFolder.GetFileAsync("History\\HistoryCount");
                    var HCount = await Windows.Storage.FileIO.ReadLinesAsync(file);
                    HistoryCount += Int32.Parse(HCount[0]);
                }
                catch
                {

                }
                string HistoryTitle = EdgeWebView.DocumentTitle + "\n" + EdgeWebView.Source.ToString();
                string FileHistoryTitle = "History\\HistoryTitle" + HistoryCount.ToString();
                Windows.Storage.StorageFile Count = await StorageFolder.CreateFileAsync("History\\HistoryCount", Windows.Storage.CreationCollisionOption.OpenIfExists);
                Windows.Storage.StorageFile Title = await StorageFolder.CreateFileAsync(FileHistoryTitle, Windows.Storage.CreationCollisionOption.OpenIfExists);
                await Windows.Storage.FileIO.WriteTextAsync(Count, HistoryCount.ToString());
                await Windows.Storage.FileIO.WriteTextAsync(Title, HistoryTitle);

                (Application.Current as App).HistoryList.Clear();
                (Application.Current as App).GetHistory();
            }
        }

        private void LinkToChanging(object sender, RoutedEventArgs e)
        {
            LinkTyping = true;
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }
        private void LinkToChanged(object sender, RoutedEventArgs e)
        {
            LinkTyping = false;
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }
        private void LinkIME(object sender, TextCompositionStartedEventArgs e)
        {
            LinkTyping = true;
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void LinkChanged(object sender, KeyRoutedEventArgs e)
        {
            LinkTyping = true;
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
            if ((EdgeWebView.VerticalAlignment != VerticalAlignment.Stretch || LinkBox.Text.ToString() != EdgeWebView.Source.ToString()) && LinkBox.Text != "" && e.Key == Windows.System.VirtualKey.Enter)
            {
                LinkTyping = false;
                string Link = LinkBox.Text;
                if (Link[0] == '@')
                {
                    char[] CLink = Link.ToCharArray();
                    CLink[0] = ' ';
                    Link = new string(CLink);
                    Link = Link.TrimStart();
                    Link = (Application.Current as App).SearchToolLink + Link;
                    EdgeWebView.Navigate(new Uri(Link));
                }
                else
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
                        Link = (Application.Current as App).SearchToolLink + Link;
                        LinkBox.Text = Link;
                        EdgeWebView.Navigate(new Uri(Link));
                    }
                    else
                    {
                        try
                        {
                            EdgeWebView.Navigate(new Uri(Link));
                        }
                        catch
                        {
                            try
                            {
                                Link = "https://" + Link;
                                EdgeWebView.Navigate(new Uri(Link));
                            }
                            catch
                            {
                                try
                                {
                                    Link = (Application.Current as App).SearchToolLink + Link;
                                    EdgeWebView.Navigate(new Uri(Link));
                                }
                                catch
                                {

                                }
                            }
                        }
                    }
                }
                
            }
            else if(LinkBox.Text == "" && e.Key == Windows.System.VirtualKey.Enter && EdgeWebView.VerticalAlignment == VerticalAlignment.Stretch && WebNavigating == false)
            {
                LinkBox.Text = EdgeWebView.Source.ToString();
            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            if (EdgeWebView.CanGoBack == true)
            {
                EdgeWebView.GoBack();
            }

            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void Forward(object sender, RoutedEventArgs e)
        {
            if (EdgeWebView.CanGoForward == true)
            {
                EdgeWebView.GoForward();
            }

            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            EdgeWebView.Refresh();

            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void Collection(object sender, RoutedEventArgs e)
        {
            Browser.ShowSideWindow(1);
            Browser.ShowTabListWindow(0);
        }

        private void History(object sender, RoutedEventArgs e)
        {
            Browser.ShowSideWindow(2);
            Browser.ShowTabListWindow(0);
        }

        private void Download(object sender, RoutedEventArgs e)
        {
            DownloadFrame.Navigate(typeof(Download), null, new SuppressNavigationTransitionInfo());
            DownloadFrame.Visibility = Visibility.Visible;
            DownloadBackControl.Visibility = Visibility.Visible;

            DownloadStoryBoardDoubleAnimation.From = 0.001;
            DownloadStoryBoardDoubleAnimation.To = 1;
            DownloadStoryBoard.Begin();

            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void FullScreen(object sender, RoutedEventArgs e)
        {
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (!view.IsFullScreenMode)
            {
                view.TryEnterFullScreenMode();
            }
            else
            {
                view.ExitFullScreenMode();
            }
        }

        private void Settings(object sender, RoutedEventArgs e)
        {
            Browser.Settings();
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void NewTabRequest(WebView sender, WebViewNewWindowRequestedEventArgs e)
        {
            (Application.Current as App).TabStartLink = e.Uri.ToString();
            Browser.TabView_AddButtonClick(null , null);
            e.Handled = true;
        }

        private void FullScreenRequest(WebView sender, object args)
        {
            PageReqFS = EdgeWebView.ContainsFullScreenElement;
            ApplicationView view = ApplicationView.GetForCurrentView();
            if (!view.IsFullScreenMode && PageReqFS)
            {
                view.TryEnterFullScreenMode();
            }
            else if (view.IsFullScreenMode && !PageReqFS)
            {
                view.ExitFullScreenMode();
            }    
        }

        private void WebView_PermissionRequested(WebView sender, WebViewPermissionRequestedEventArgs args)
        {
            args.PermissionRequest.Allow();
        }

        private void SearchChanged(object sender, KeyRoutedEventArgs e)
        {
            if (SearchBox.Text != "" && e.Key == Windows.System.VirtualKey.Enter && EdgeWebView.VerticalAlignment != VerticalAlignment.Stretch)
            {
                string Link = (Application.Current as App).SearchToolLink + SearchBox.Text;
                LinkBox.Text = Link;
                EdgeWebView.Navigate(new Uri(Link));
            }
        }

        public void CloseWebView()
        {
            while (EdgeWebView.CanGoBack == true)
            {
                EdgeWebView.GoBack();
            }
            EdgeWebView.Source = (new Uri("about:blank"));
        }

        bool isLoaded = false;
        private void EdgeWebView_Loaded(object sender, RoutedEventArgs e)
        {
            if(!isLoaded)
            {
                isLoaded = true;
                LinkBox.Text = "";
                if ((Application.Current as App).TabStartLink != "about:blank")
                {
                    string Link = (Application.Current as App).TabStartLink;
                    try
                    {
                        EdgeWebView.Navigate(new Uri(Link));
                    }
                    catch
                    {
                        try
                        {
                            Link = "https://" + Link;
                            EdgeWebView.Navigate(new Uri(Link));
                        }
                        catch
                        {
                            try
                            {
                                Link = (Application.Current as App).SearchToolLink + Link;
                                EdgeWebView.Navigate(new Uri(Link));
                            }
                            catch
                            {

                            }
                        }
                    }
                    EdgeWebView.VerticalAlignment = VerticalAlignment.Stretch;
                }
                (Application.Current as App).TabStartLink = "about:blank";

                Timer = new DispatcherTimer();
                Timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                Timer.Tick += Timer_Tick;
                Timer.Start();
            }
        }

        private void DownloadBack_Click(object sender, RoutedEventArgs e)
        {
            (((Frame)DownloadFrame).Content as Download).CloseWebView();
            DownloadBackControl.Visibility = Visibility.Collapsed;

            DownloadStoryBoardDoubleAnimation.From = 1;
            DownloadStoryBoardDoubleAnimation.To = 0;
            DownloadStoryBoard.Begin();
        }



        private void MoreMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewTab_Click(object sender, RoutedEventArgs e)
        {
            Browser.TabView_AddButtonClick(null, null);
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void Page_SizeChanged(object sender = null, SizeChangedEventArgs e = null)
        {
            if ((Application.Current as App).LayoutState == 0)
            {
                if (ActualWidth > 680)
                    LayoutState = 1;
                else
                    LayoutState = 2;
            }
            else
            {
                LayoutState = (Application.Current as App).LayoutState;
            }

            if ((App.Current.RequestedTheme == ApplicationTheme.Light && (Application.Current as App).ThemeSelected == 0) || (Application.Current as App).ThemeSelected == 1)
            {
                SeparateLineDark.StrokeThickness = 0;
                SeparateLineLight.StrokeThickness = 0.5;
            }
            else if ((App.Current.RequestedTheme == ApplicationTheme.Dark && (Application.Current as App).ThemeSelected == 0) || (Application.Current as App).ThemeSelected == 2)
            {
                SeparateLineDark.StrokeThickness = 0.5;
                SeparateLineLight.StrokeThickness = 0;
            }

            SeparateLineDark.X2 = 10 * ActualWidth;
            SeparateLineLight.X2 = 10 * ActualWidth;

            int i = 1;
            if ((Application.Current as App).ShowCollection)
            {
                CollectionButton.Visibility = Visibility.Visible;
                i++;
            }
            else
            {
                CollectionButton.Visibility = Visibility.Collapsed;
            }
            if ((Application.Current as App).ShowHistory)
            {
                HistoryButton.Visibility = Visibility.Visible;
                i++;
            }
            else
            {
                HistoryButton.Visibility = Visibility.Collapsed;
            }
            if ((Application.Current as App).ShowDownload)
            {
                DownloadButton.Visibility = Visibility.Visible;
                i++;
            }
            else
            {
                DownloadButton.Visibility = Visibility.Collapsed;
            }
            if ((Application.Current as App).ShowFullScreen)
            {
                FullScreenButton.Visibility = Visibility.Visible;
                i++;
            }
            else
            {
                FullScreenButton.Visibility = Visibility.Collapsed;
            }


            if (LayoutState == 1)
            {
                EdgeTitleBar.Visibility = Visibility.Visible;
                EdgeBottomBar.Visibility = Visibility.Collapsed;
                EdgeWebView.Margin = new Thickness(0, 50, 0, 0);

                if (EdgeWebView.Source.ToString() == "about:blank")
                {
                    SearchBox.Margin = new Thickness(180, 180, 180, 0);
                }
                else
                {
                    SearchBox.Margin = new Thickness(180, -128, 180, 0);
                }

                LinkBox.Margin = new Thickness(140, 0, 40 * i + 20, 0);
                LinkBox.Height = 32;
                LinkBox.FontSize = 15;
                ProgressBar.VerticalAlignment = VerticalAlignment.Top;
                EdgeLinkGrid.VerticalAlignment = VerticalAlignment.Top;
                SeparateLineLight.Y1 = SeparateLineLight.Y2 = SeparateLineDark.Y1 = SeparateLineDark.Y2 = 50;
            }
            else if (LayoutState == 2)
            {
                EdgeTitleBar.Visibility = Visibility.Collapsed;
                EdgeBottomBar.Visibility = Visibility.Visible;
                EdgeWebView.Margin = new Thickness(0, 0, 0, 100);

                if (EdgeWebView.Source.ToString() == "about:blank")
                {
                    SearchBox.Margin = new Thickness(40, 120, 40, 0);
                }
                else
                {
                    SearchBox.Margin = new Thickness(180, -128, 180, 0);
                }

                LinkBox.Margin = new Thickness(12, 4, 56, 0);
                LinkBox.Height = 36;
                LinkBox.FontSize = 17;
                ProgressBar.VerticalAlignment = VerticalAlignment.Bottom;
                EdgeLinkGrid.VerticalAlignment = VerticalAlignment.Bottom;
                SeparateLineLight.Y1 = SeparateLineLight.Y2 = SeparateLineDark.Y1 = SeparateLineDark.Y2 = ActualHeight - 100;
            }
        }

        private void TabList_Click(object sender, RoutedEventArgs e)
        {
            Browser.ShowTabListWindow(1);
        }
    }
}
