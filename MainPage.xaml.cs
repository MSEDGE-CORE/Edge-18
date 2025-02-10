using Microsoft.UI.Xaml.Controls;
using Microsoft.Web.WebView2.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x804 上介绍了“空白页”项模板

namespace App3
{
    public sealed partial class MainPage : Page
    {
        DispatcherTimer Timer;
        public bool IsSideWindowOpen = false;
        public bool IsTabListWindowOpen = false;
        int LayoutState = 0;

        public static MainPage Browser
        {
            get { return (Window.Current.Content as Frame)?.Content as MainPage; }
        }

        public MainPage()
        {
            this.InitializeComponent();

            SetTitleBar();
        }

        public void SetTitleBar()
        {
            var CoreTitleBar = CoreApplication.GetCurrentView().TitleBar;
            CoreTitleBar.ExtendViewIntoTitleBar = true;
            ApplicationViewTitleBar TitleBar = ApplicationView.GetForCurrentView().TitleBar;
            TitleBar.ButtonBackgroundColor = Colors.Transparent;
            TitleBar.ButtonInactiveBackgroundColor = Colors.Transparent;

            Window.Current.SetTitleBar(AppTitleBar);
        }

        public TabViewItem SelectedTab
        {
            get
            {
                TabViewItem selectedItem = (TabViewItem)MicrosoftEdge.SelectedItem;
                return selectedItem;
            }
        }

        private void Timer_Tick(object sender, object e)
        {
            Page_SizeChanged(null,null);
            if (SettingsFrame.Visibility == Visibility.Visible && SettingsFrame.Opacity == 0)
            {
                SettingsFrame.Visibility = Visibility.Collapsed;
            }

            SideGrid.Visibility = (SideGridTransform.X < SideWindow.Width && !IsSideWindowOpen || IsSideWindowOpen) ? Visibility.Visible : Visibility.Collapsed;
            if(SideGrid.Visibility == Visibility.Collapsed && SideWindow.Content != null)
            {
                if (SideWindow.Content.GetType() == typeof(Collection))
                    ((SideWindow.Content) as Collection).OnClosing();
                else if (SideWindow.Content.GetType() == typeof(History))
                    ((SideWindow.Content) as History).OnClosing();
                SideWindow.Content = null;
            }
            TabListGrid.Visibility = (TabListGridTransform.X < TabListWindow.Width && !IsTabListWindowOpen || IsTabListWindowOpen) ? Visibility.Visible : Visibility.Collapsed;
        
            for(int i = 0; i < ListView.Items.Count; i++)
            {
                if (ListView.Items.Count != MicrosoftEdge.TabItems.Count)
                {
                    TabsChanged();
                    break;
                }
                if ((Application.Current as App).TabList[i].Title != (MicrosoftEdge.TabItems[i] as TabViewItem).Header.ToString())
                {
                    TabsChanged();
                    break;
                }
            }
            
        }

        public void TabView_Loaded(object sender, RoutedEventArgs e)
        {
            TabViewItem NewTabPage = CreateNewTab();
            MicrosoftEdge.TabItems.Add(NewTabPage);
            MicrosoftEdge.SelectedItem = NewTabPage;

            Timer = new DispatcherTimer();
            Timer.Interval = new TimeSpan(0, 0, 0, 0, 100);
            Timer.Tick += Timer_Tick;
            Timer.Start();

            ListView.ItemsSource = (Application.Current as App).TabList;
            TabsChanged();
        }

        public void TabView_AddButtonClick(TabView sender, object args)
        {
            TabViewItem NewTabPage = CreateNewTab();
            MicrosoftEdge.TabItems.Add(NewTabPage);
            MicrosoftEdge.SelectedItem = NewTabPage;

            ShowSideWindow(0);
            ShowTabListWindow(0);
        }

        public void TabView_TabCloseRequested(TabView sender = null, TabViewTabCloseRequestedEventArgs args = null)
        {
            if((Application.Current as App).WebViewSelected == 0)
            {
                (((Frame)args.Tab.Content).Content as WebPage2).CloseWebView();
            }
            else if((Application.Current as App).WebViewSelected == 1)
            {
                (((Frame)args.Tab.Content).Content as WebPage).CloseWebView();
            }
            sender.TabItems.Remove(args.Tab);

            if (sender.TabItems.Count == 0)
            {
                CoreApplication.Exit();
            }

            ShowSideWindow(0);
            ShowTabListWindow(0);
        }

        public TabViewItem CreateNewTab()
        {
            TabViewItem newItem = new TabViewItem();
            Frame frame = new Frame();
            if((Application.Current as App).WebViewSelected == 0)
            {
                frame.Navigate(typeof(WebPage2));
            }
            else if((Application.Current as App).WebViewSelected == 1) 
            {
                frame.Navigate(typeof(WebPage));
            }
            newItem.Content = frame;
            //newItem.IconSource = new Microsoft.UI.Xaml.Controls.SymbolIconSource() { Symbol = Symbol.Stop };
            newItem.Header = "新标签页";
            return newItem;
        }

        public void Settings()
        {
            if(SettingsFrame.Visibility != Visibility.Visible)
            {
                SettingsFrame.Visibility = Visibility.Visible;
                SettingsFrame.Navigate(typeof(Settings), null, new SuppressNavigationTransitionInfo());
                SettingsBackControl.Visibility = Visibility.Visible;

                SettingsStoryBoardDoubleAnimation.From = 0.001;
                SettingsStoryBoardDoubleAnimation.To = 1;
                SettingsStoryBoard.Begin();
            }

            ShowSideWindow(0);
            ShowTabListWindow(0);
        }

        public void SettingsBack_Click(object sender, RoutedEventArgs e)
        {
            SettingsBackControl.Visibility = Visibility.Collapsed;

            SettingsStoryBoardDoubleAnimation.From = 1;
            SettingsStoryBoardDoubleAnimation.To = 0;
            SettingsStoryBoard.Begin();

            Browser.SetTitleBar();
            ShowSideWindow(0);
            ShowTabListWindow(0);
        }

        private void NewTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            MainPage.Browser.TabView_AddButtonClick(null, null);
            args.Handled = true;
        }

        private void CloseSelectedTabKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            var InvokedTabView = (args.Element as TabView);

            if (((TabViewItem)InvokedTabView.SelectedItem).IsClosable)
            {
                InvokedTabView.TabItems.Remove(InvokedTabView.SelectedItem);
            }
            if (MicrosoftEdge.TabItems.Count == 0)
            {
                CoreApplication.Exit();
            }

            ShowSideWindow(0);
            ShowTabListWindow(0);
            args.Handled = true;
        }

        private void TabsChanged()
        {
            (Application.Current as App).TabList.Clear();
            for (int i = 0; i < MicrosoftEdge.TabItems.Count; i++)
            {
                (Application.Current as App).TabList.Add(new Tab_List() { Title = (MicrosoftEdge.TabItems[i] as TabViewItem).Header.ToString() });
            }
        }

        public void SideWindowBackground_Click(object sender = null, RoutedEventArgs e = null)
        {
            ShowSideWindow(0);
            ShowTabListWindow(0);
        }

        public void ShowSideWindow(int ToOpen = 0)
        {
            if (SideWindow.Content != null && ((ToOpen == 1 && IsSideWindowOpen && SideWindow.Content.GetType() == typeof(Collection)) || (ToOpen == 2 && IsSideWindowOpen && SideWindow.Content.GetType() == typeof(History))))
            {
                ToOpen = 0;
            }
            else if (SideWindow.Content != null && ((ToOpen == 2 && IsSideWindowOpen && SideWindow.Content.GetType() == typeof(Collection)) || (ToOpen == 1 && IsSideWindowOpen && SideWindow.Content.GetType() == typeof(History))))
            {
                SideGridTransform.X = SideWindow.Width;
                SideGrid.Opacity = 0;

                if (SideWindow.Content.GetType() == typeof(Collection))
                    ((SideWindow.Content) as Collection).OnClosing();
                else if (SideWindow.Content.GetType() == typeof(History))
                    ((SideWindow.Content) as History).OnClosing();
            }

            if (ToOpen == 1)
            {
                SideWindow.Navigate(typeof(Collection), null, new SuppressNavigationTransitionInfo());
            }
            else if (ToOpen == 2)
            {
                SideWindow.Navigate(typeof(History), null, new SuppressNavigationTransitionInfo());
            }

            IsSideWindowOpen = ToOpen != 0 ? true : false;
            SideFlowIn.From = ToOpen != 0 ? SideGridTransform.X : SideGridTransform.X;
            if (SideWindow.Width > 0)
                SideFlowIn.To = ToOpen != 0 ? 0 : SideWindow.Width;
            SideOpacity.From = ToOpen != 0 ? SideGrid.Opacity : SideGrid.Opacity;
            SideOpacity.To = ToOpen != 0 ? 1 : 0;
            SideStoryBoard.Begin();
            SideWindowBackground.Opacity = 0;
            SideWindowBackground.Visibility = ToOpen != 0 ? Visibility.Visible : Visibility.Collapsed;
            SideWindowBackground.Opacity = 0;
            SideGrid.Visibility = Visibility.Visible;
            SideSeparateBar.Visibility = ToOpen != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void MicrosoftEdge_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(SideWindow.Width > 0)
            {
                ShowSideWindow(0);
                ShowTabListWindow(0);
            }
        }

        private void Page_SizeChanged(object sender, SizeChangedEventArgs e)
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

            if(LayoutState == 1)
            {
                SideWindow.Width = 360;
                SideGrid.Margin = new Thickness(0, 90, 0, 0);
                TabListButton.Visibility = Visibility.Visible;
                TabListWindow.Width = 240;
                TabListGrid.Margin = new Thickness(0, 40, 0, 0);
                TabListBackground.Margin = new Thickness(0, 40, 0, 0);
            }
            else if(LayoutState == 2)
            {
                SideWindow.Width = ActualWidth;
                SideGrid.Margin = new Thickness(0, 40, 0, 0);
                TabListButton.Visibility = Visibility.Collapsed;
                TabListWindow.Width = ActualWidth;
                TabListGrid.Margin = new Thickness(0, 40, 0, 100);
                TabListBackground.Margin = new Thickness(0, 40, 0, 100);
            }
        }

        private void TabListButton_Click(object sender, RoutedEventArgs e)
        {
            ShowTabListWindow(1);
        }

        public void ShowTabListWindow(int ToOpen = 0)
        {
            if (IsTabListWindowOpen && ToOpen == 1)
                ToOpen = 0;
            if (ToOpen == 1)
                ListView.SelectedIndex = MicrosoftEdge.SelectedIndex;
            else
                ListView.SelectedIndex = -1;
            IsTabListWindowOpen = ToOpen != 0 ? true : false;
            TabListFlowIn.From = ToOpen != 0 ? TabListGridTransform.X : TabListGridTransform.X;
            if (TabListWindow.Width > 0)
                TabListFlowIn.To = ToOpen != 0 ? 0 : -TabListWindow.Width;
            TabListOpacity.From = ToOpen != 0 ? TabListGrid.Opacity : TabListGrid.Opacity;
            TabListOpacity.To = ToOpen != 0 ? 1 : 0;
            TabListStoryBoard.Begin();
            TabListBackground.Opacity = 0;
            TabListBackground.Visibility = ToOpen != 0 ? Visibility.Visible : Visibility.Collapsed;
            TabListBackground.Opacity = 0;
            TabListGrid.Visibility = Visibility.Visible;
            TabListSeparateBar.Visibility = ToOpen != 0 ? Visibility.Visible : Visibility.Collapsed;
        }

        private void TabListBackground_Click(object sender, RoutedEventArgs e)
        {
            ShowTabListWindow(0);
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        { 
            if (ListView.SelectedIndex >= 0 && ListView.SelectedIndex != MicrosoftEdge.SelectedIndex)
            {
                MicrosoftEdge.SelectedIndex = ListView.SelectedIndex;
                ShowTabListWindow(0);
                ListView.SelectedIndex = -1;
            }
        }

        private void EscKeyboardAccelerator_Invoked(KeyboardAccelerator sender, KeyboardAcceleratorInvokedEventArgs args)
        {
            ShowTabListWindow(0);
        }
    }
}
