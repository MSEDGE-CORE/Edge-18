using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using System.Threading;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App3
{
    public sealed partial class Download : Page
    {
        DispatcherTimer Timer;

        public Download()
        {
            this.InitializeComponent();

            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            Timer.Tick += Timer_Tick;
            Timer.Start();

            if ((Application.Current as App).WebLink == "about:blank")
            {
                EdgeWebView.VerticalAlignment = VerticalAlignment.Top;
            }
            EdgeWebView.Source = new Uri((Application.Current as App).WebLink);
        }
        private void CoreWebView2_NewWindowRequested(CoreWebView2 sender, CoreWebView2NewWindowRequestedEventArgs args)
        {
            EdgeWebView.CoreWebView2.Navigate(args.Uri.ToString());
            args.Handled = true;
        }

        private void EdgeWebView_CoreWebView2Initialized(WebView2 sender, CoreWebView2InitializedEventArgs args)
        {
            sender.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
        }

        private void Timer_Tick(object sender, object e)
        {
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
        }

        private void DownloadButton(object sender, RoutedEventArgs e)
        {
            bool IsDownloadPageOpening = false;
            try
            {
                IsDownloadPageOpening = EdgeWebView.CoreWebView2.IsDefaultDownloadDialogOpen;
            }
            catch
            {

            }

            if(!IsDownloadPageOpening)
            {
                EdgeWebView.CoreWebView2.OpenDefaultDownloadDialog();
            }
            else
            {
                EdgeWebView.CoreWebView2.CloseDefaultDownloadDialog();
            }
        }

        public void CloseWebView()
        {
            EdgeWebView.Close();
        }
    }
}
