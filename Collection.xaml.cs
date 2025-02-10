using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Navigation;

// https://go.microsoft.com/fwlink/?LinkId=234238 上介绍了“空白页”项模板

namespace App3
{
    public sealed partial class Collection : Page
    {
        public static MainPage Browser
        {
            get { return (Window.Current.Content as Frame)?.Content as MainPage; }
        }

        private bool isEditing = false;
        int CollectionCount = 0;

        public Collection()
        {
            this.InitializeComponent();

            Add_Collection_Flyout.ShouldConstrainToRootBounds = false;
            PageLoaded();

            ListView.ItemsSource = (Application.Current as App).CollectionList;
        }

        private async void PageLoaded()
        {
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
        }

        private void Add_Collection_Click(object sender, RoutedEventArgs e)
        {
            Collection_Title.Text = Browser.SelectedTab.Header.ToString();
            Collection_Uri.Text = (Application.Current as App).WebLink;
            //Collection_Uri.IsReadOnly = true;
        }

        private async void Add_Collection_Complete_Click(object sender, RoutedEventArgs e)
        {
            Add_Collection_Flyout.Hide();
            if (Collection_Uri.Text == "about:blank")
            {
                return;
            }

            (Application.Current as App).CollectionList.Add(new Collection_List() { CollectionTitle = Collection_Title.Text, CollectionUri = Collection_Uri.Text });

            Windows.Storage.StorageFolder StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
            CollectionCount += 1;
            string CollectionTitle = Collection_Title.Text + "\n" + Collection_Uri.Text + "\n";
            string FileCollectionTitle = "Collection\\CollectionTitle" + CollectionCount.ToString();
            Windows.Storage.StorageFile Count = await StorageFolder.CreateFileAsync("Collection\\CollectionCount", Windows.Storage.CreationCollisionOption.OpenIfExists);
            Windows.Storage.StorageFile Title = await StorageFolder.CreateFileAsync(FileCollectionTitle, Windows.Storage.CreationCollisionOption.OpenIfExists);
            await Windows.Storage.FileIO.WriteTextAsync(Count, CollectionCount.ToString());
            await Windows.Storage.FileIO.WriteTextAsync(Title, CollectionTitle);
        }

        private void Edit_Collection_Click(object sender, RoutedEventArgs e)
        {
            ListView.SelectedIndex = -1;
            isEditing = true;
            DefaultToolBar.Visibility = Visibility.Collapsed;
            EditToolBar.Visibility = Visibility.Visible;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!isEditing)
            {
                if (ListView.SelectedItems.Count > 0 && ListView.SelectedItem != null)
                {
                    (Application.Current as App).TabStartLink = (Application.Current as App).CollectionList[ListView.SelectedIndex].CollectionUri.ToString();
                    MainPage.Browser.TabView_AddButtonClick(null, null);
                }
            }
        }

        private async void Delete_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ListView.SelectedItems.Count > 0 && ListView.SelectedItem != null)
            {
                (Application.Current as App).CollectionList.RemoveAt(ListView.SelectedIndex);
                CollectionCount--;

                Windows.Storage.StorageFolder StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile Count = await StorageFolder.CreateFileAsync("Collection\\CollectionCount", Windows.Storage.CreationCollisionOption.OpenIfExists);
                await Windows.Storage.FileIO.WriteTextAsync(Count, CollectionCount.ToString());
                for (int i = 0; i < CollectionCount; i++)
                {
                    string CollectionTitle = (Application.Current as App).CollectionList[i].CollectionTitle + "\n" + (Application.Current as App).CollectionList[i].CollectionUri + "\n";
                    string FileCollectionTitle = "Collection\\CollectionTitle" + (i + 1).ToString();
                    Windows.Storage.StorageFile Title = await StorageFolder.CreateFileAsync(FileCollectionTitle, Windows.Storage.CreationCollisionOption.OpenIfExists);
                    await Windows.Storage.FileIO.WriteTextAsync(Title, CollectionTitle);
                }
            }
        }

        private void Complete_Button_Click(object sender, RoutedEventArgs e)
        {
            ListView.SelectedIndex = -1;
            isEditing = false;
            DefaultToolBar.Visibility = Visibility.Visible;
            EditToolBar.Visibility = Visibility.Collapsed;
        }

        private async void Up_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ListView.SelectedItems.Count > 0 && ListView.SelectedItem != null && ListView.SelectedIndex != 0)
            {
                string WTitle = (Application.Current as App).CollectionList[ListView.SelectedIndex].CollectionTitle;
                string WUri = (Application.Current as App).CollectionList[ListView.SelectedIndex].CollectionUri;
                (Application.Current as App).CollectionList[ListView.SelectedIndex].CollectionTitle = (Application.Current as App).CollectionList[ListView.SelectedIndex - 1].CollectionTitle;
                (Application.Current as App).CollectionList[ListView.SelectedIndex].CollectionUri = (Application.Current as App).CollectionList[ListView.SelectedIndex - 1].CollectionUri;
                (Application.Current as App).CollectionList[ListView.SelectedIndex - 1].CollectionTitle = WTitle;
                (Application.Current as App).CollectionList[ListView.SelectedIndex - 1].CollectionUri = WUri;
                int SIndex = ListView.SelectedIndex;
                ListView.ItemsSource = null;
                ListView.ItemsSource = (Application.Current as App).CollectionList;
                ListView.SelectedIndex = SIndex - 1;

                Windows.Storage.StorageFolder StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile Count = await StorageFolder.CreateFileAsync("Collection\\CollectionCount", Windows.Storage.CreationCollisionOption.OpenIfExists);
                await Windows.Storage.FileIO.WriteTextAsync(Count, CollectionCount.ToString());
                for (int i = 0; i < CollectionCount; i++)
                {
                    string CollectionTitle = (Application.Current as App).CollectionList[i].CollectionTitle + "\n" + (Application.Current as App).CollectionList[i].CollectionUri + "\n";
                    string FileCollectionTitle = "Collection\\CollectionTitle" + (i + 1).ToString();
                    Windows.Storage.StorageFile Title = await StorageFolder.CreateFileAsync(FileCollectionTitle, Windows.Storage.CreationCollisionOption.OpenIfExists);
                    await Windows.Storage.FileIO.WriteTextAsync(Title, CollectionTitle);
                }
            }
        }

        private async void Down_Button_Click(object sender, RoutedEventArgs e)
        {
            if (ListView.SelectedItems.Count > 0 && ListView.SelectedItem != null && ListView.SelectedIndex != CollectionCount - 1)
            {
                string WTitle = (Application.Current as App).CollectionList[ListView.SelectedIndex].CollectionTitle;
                string WUri = (Application.Current as App).CollectionList[ListView.SelectedIndex].CollectionUri;
                (Application.Current as App).CollectionList[ListView.SelectedIndex].CollectionTitle = (Application.Current as App).CollectionList[ListView.SelectedIndex + 1].CollectionTitle;
                (Application.Current as App).CollectionList[ListView.SelectedIndex].CollectionUri = (Application.Current as App).CollectionList[ListView.SelectedIndex + 1].CollectionUri;
                (Application.Current as App).CollectionList[ListView.SelectedIndex + 1].CollectionTitle = WTitle;
                (Application.Current as App).CollectionList[ListView.SelectedIndex + 1].CollectionUri = WUri;
                int SIndex = ListView.SelectedIndex;
                ListView.ItemsSource = null;
                ListView.ItemsSource = (Application.Current as App).CollectionList;
                ListView.SelectedIndex = SIndex + 1;

                Windows.Storage.StorageFolder StorageFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                Windows.Storage.StorageFile Count = await StorageFolder.CreateFileAsync("Collection\\CollectionCount", Windows.Storage.CreationCollisionOption.OpenIfExists);
                await Windows.Storage.FileIO.WriteTextAsync(Count, CollectionCount.ToString());
                for (int i = 0; i < CollectionCount; i++)
                {
                    string CollectionTitle = (Application.Current as App).CollectionList[i].CollectionTitle + "\n" + (Application.Current as App).CollectionList[i].CollectionUri + "\n";
                    string FileCollectionTitle = "Collection\\CollectionTitle" + (i + 1).ToString();
                    Windows.Storage.StorageFile Title = await StorageFolder.CreateFileAsync(FileCollectionTitle, Windows.Storage.CreationCollisionOption.OpenIfExists);
                    await Windows.Storage.FileIO.WriteTextAsync(Title, CollectionTitle);
                }
            }
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            Browser.SideWindowBackground_Click();
        }
        public void OnClosing()
        {
            ListView.ItemsSource = null;
        }
    }
}
