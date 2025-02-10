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
using System.Threading;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml.Media.Animation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App3
{
    public sealed partial class WebPage2 : Page
    {
        DispatcherTimer Timer;
        bool IsDownloadPageOpening = false;
        int LayoutState = 0;
        bool PageReqFS = false;

        public static MainPage Browser
        {
            get { return (Window.Current.Content as Frame)?.Content as MainPage; }
        }

        public WebPage2()
        {
            this.InitializeComponent();

            LinkBox.Focus(FocusState.Keyboard);
            Timer = new DispatcherTimer();
        }

        bool LinkTyping = false;
        bool WebNavigating = false;
        private async void Timer_Tick(object sender, object e)
        {
            if(EdgeWebView.CoreWebView2 == null)
            {
                await CoreApplication.RequestRestartAsync(string.Empty);
            }

            Page_SizeChanged();
            isLoaded = true;

            if (EdgeWebView.Opacity == 1)
            {
                EdgeWebView.Visibility = Visibility.Visible;
            }
            else if(EdgeWebView.Opacity == 0)
            {
                EdgeWebView.Visibility = Visibility.Collapsed;
            }

            if (Browser.SelectedTab.Content == this.Frame)
            {
                (Application.Current as App).WebLink = EdgeWebView.Source.ToString();
                if (EdgeWebView.Source.ToString() == "about:blank")
                {
                    Browser.SelectedTab.Header = "新标签页";
                }
                else if (EdgeWebView.CoreWebView2.DocumentTitle == "")
                {
                    Browser.SelectedTab.Header = "加载中";
                }
                else
                {
                    Browser.SelectedTab.Header = EdgeWebView.CoreWebView2.DocumentTitle;
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

            if (LinkTyping == false)
            {
                string LastPage = LinkBox.Text;
                LinkBox.Text = EdgeWebView.Source.ToString();
                if (EdgeWebView.Source.ToString() == "about:blank")
                {
                    LinkBox.Text = "";
                }
            }

            if (EdgeWebView.Source.ToString() != "about:blank")
            {
                if (EdgeWebView.Opacity != 1)
                {
                    EdgeWebView.Opacity = 1;
                }
            }
            else if (EdgeWebView.Opacity != 0 && WebNavigating == false)
            {
                EdgeWebView.Opacity = 0;
                EdgeWebView.CoreWebView2.Reload();
            }

            if (EdgeWebView.Opacity == 1)
            {
                SearchBox.Text = "";
            }
        }

        private void CoreWebView2_NavigationStarting(CoreWebView2 sender, CoreWebView2NavigationStartingEventArgs args)
        {
            WebNavigating = true;
        }
        private async void CoreWebView2_NavigationCompleted(CoreWebView2 sender, CoreWebView2NavigationCompletedEventArgs args)
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
                string HistoryTitle = EdgeWebView.CoreWebView2.DocumentTitle + "\n" + EdgeWebView.Source.ToString();
                string FileHistoryTitle = "History\\HistoryTitle" + HistoryCount.ToString();
                Windows.Storage.StorageFile Count = await StorageFolder.CreateFileAsync("History\\HistoryCount", Windows.Storage.CreationCollisionOption.OpenIfExists);
                Windows.Storage.StorageFile Title = await StorageFolder.CreateFileAsync(FileHistoryTitle, Windows.Storage.CreationCollisionOption.OpenIfExists);
                await Windows.Storage.FileIO.WriteTextAsync(Count, HistoryCount.ToString());
                await Windows.Storage.FileIO.WriteTextAsync(Title, HistoryTitle);

                (Application.Current as App).HistoryList.Clear();
                (Application.Current as App).GetHistory();
            }

            if (!isLoaded)
            {
                LinkBox.Text = "";
                if ((Application.Current as App).TabStartLink != "about:blank")
                {
                    string Link = (Application.Current as App).TabStartLink;
                    try
                    {
                        EdgeWebView.CoreWebView2.Navigate(Link);
                    }
                    catch
                    {
                        try
                        {
                            Link = "https://" + Link;
                            EdgeWebView.CoreWebView2.Navigate(Link);
                        }
                        catch
                        {
                            try
                            {
                                Link = (Application.Current as App).SearchToolLink + Link;
                                EdgeWebView.CoreWebView2.Navigate(Link);
                            }
                            catch
                            {

                            }
                        }
                    }
                    EdgeWebView.Opacity = 1;
                }
                (Application.Current as App).TabStartLink = "about:blank";
            }

            if(!Timer.IsEnabled)
            {
                Timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
                Timer.Tick += Timer_Tick;
                Timer.Start();
            }
        }

        private void LinkToChanging(object sender, RoutedEventArgs e)
        {
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);

            if (isLoaded)
                EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();
            LinkTyping = true;
        }
        private void LinkToChanged(object sender, RoutedEventArgs e)
        {
            LinkTyping = false;
        }
        private void LinkIME(object sender, TextCompositionStartedEventArgs e)
        {
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);

            if (isLoaded)
                EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();

            LinkTyping = true;
        }

        private void LinkChanged(object sender, KeyRoutedEventArgs e)
        {
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);

            if (isLoaded)
                EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();

            LinkTyping = true;
            if ((EdgeWebView.Opacity != 1 || LinkBox.Text.ToString() != EdgeWebView.Source.ToString()) && LinkBox.Text != "" && e.Key == Windows.System.VirtualKey.Enter)
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
                    EdgeWebView.CoreWebView2.Navigate((Link));
                }
                else
                {
                    try
                    {
                        EdgeWebView.CoreWebView2.Navigate(Link);
                    }
                    catch
                    {
                        try
                        {
                            Link = "https://" + Link;
                            EdgeWebView.CoreWebView2.Navigate(Link);
                        }
                        catch
                        {
                            try
                            {
                                Link = (Application.Current as App).SearchToolLink + Link;
                                EdgeWebView.CoreWebView2.Navigate(Link);
                            }
                            catch
                            {

                            }
                        }
                    }
                }
                
            }
            else if(LinkBox.Text == "" && e.Key == Windows.System.VirtualKey.Enter && EdgeWebView.Opacity == 1 && WebNavigating == false)
            {
                LinkBox.Text = EdgeWebView.Source.ToString();
            }
        }

        private void Back(object sender, RoutedEventArgs e)
        {
            if (isLoaded)
                EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();

            if (EdgeWebView.CanGoBack == true)
            {
                EdgeWebView.GoBack();
            }

            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void Forward(object sender, RoutedEventArgs e)
        {
            if (isLoaded)
                EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();

            if (EdgeWebView.CanGoForward == true)
            {
                EdgeWebView.GoForward();
            }

            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void Refresh(object sender, RoutedEventArgs e)
        {
            if (isLoaded)
                EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();

            if (isLoaded)
                EdgeWebView.CoreWebView2.Reload();

            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void Collection(object sender = null, RoutedEventArgs e = null)
        {
            if (isLoaded)
                EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();

            Browser.ShowSideWindow(1);
            Browser.ShowTabListWindow(0);
        }

        private void History(object sender = null, RoutedEventArgs e = null)
        {
            if (isLoaded)
                EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();

            Browser.ShowSideWindow(2);
            Browser.ShowTabListWindow(0);
        }

        private void Download(object sender, RoutedEventArgs e)
        {
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);

            if (isLoaded)
            {
                IsDownloadPageOpening = EdgeWebView.CoreWebView2.IsDefaultDownloadDialogOpen;

                if (EdgeWebView.Visibility == Visibility.Collapsed)
                {
                    EdgeWebView.CoreWebView2.Navigate("edge://downloads");
                }
                else if (!IsDownloadPageOpening)
                {
                    EdgeWebView.CoreWebView2.OpenDefaultDownloadDialog();
                }
                else
                {
                    EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();
                }

            }
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

        private void Settings(object sender = null, RoutedEventArgs e = null)
        {
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);

            if (isLoaded)
                EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();

            Browser.Settings();
        }

        private void CoreWebView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            (Application.Current as App).TabStartLink = args.Uri.ToString();
            Browser.TabView_AddButtonClick(null, null);
            args.Handled = true;
        }

        private void SearchChanged(object sender, KeyRoutedEventArgs e)
        {
            if (SearchBox.Text != "" && e.Key == Windows.System.VirtualKey.Enter && EdgeWebView.Opacity != 1)
            {
                string Link = (Application.Current as App).SearchToolLink + SearchBox.Text;
                LinkBox.Text = Link;
                EdgeWebView.CoreWebView2.Navigate(Link);
            }
        }

        public void CloseWebView()
        {
            Timer.Stop();
            EdgeWebView.Close();
        }

        private void EdgeWebView_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            sender.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            sender.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
            sender.CoreWebView2.NavigationCompleted += CoreWebView2_NavigationCompleted;
            sender.CoreWebView2.ContainsFullScreenElementChanged += CoreWebView2_ContainsFullScreenElementChanged;
        }

        bool isLoaded = false;
        private void EdgeWebView_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void MoreMenuItem_Click(object sender, RoutedEventArgs e)
        {

        }

        private void NewTab_Click(object sender = null, RoutedEventArgs e = null)
        {
            Browser.TabView_AddButtonClick(null, null);
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);
        }

        private void MoreButton_Click(object sender, RoutedEventArgs e)
        {
            Browser.ShowSideWindow(0);
            Browser.ShowTabListWindow(0);

            if (isLoaded)
                EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();
        }

        private void Page_SizeChanged(object sender = null, SizeChangedEventArgs e = null)
        {
            if((Application.Current as App).LayoutState == 0)
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


            if(LayoutState == 1)
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
            else if(LayoutState == 2)
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

        private void CoreWebView2_ContainsFullScreenElementChanged(CoreWebView2 sender, object args)
        {
            PageReqFS = EdgeWebView.CoreWebView2.ContainsFullScreenElement;
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
    }
}
