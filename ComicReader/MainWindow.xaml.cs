using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using Microsoft.WindowsAPICodePack.Dialogs;
using MaterialDesignThemes.Wpf;
using System.Collections;

namespace ComicReader
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        [DllImport("User32.dll")]
        private static extern bool SetCursorPos(int X, int Y);


        private string selectionImage = "";
        public List<Image> Images { get; set; }
        public string CurrentImagesViewPath = "";
        public int firstDisplayImageNumber = 5;
        public MainWindow()
        {
            InitializeComponent();
            Images = new List<Image>();
            DataContext = this;
        }
        void SetViewerToFirst()
        {
            srvViewer.ScrollToVerticalOffset(0);
            srvViewer.ScrollToHorizontalOffset(0);
        }
        void LoadImagesToViewerFromPath(string path)
        {
            tbViewPath.Text = path;
            Image image;
            string[] files = Directory.GetFiles(path);
            Array.Sort(files, new AlphanumComparatorFast());
            if(files.Count() == 0)
            {
                MessageBox.Show("Not any image in this path");
                return;
            }
            Images.Clear();
            int count = 0;
            foreach (string file in files)
            {
                image = new Image();
                image.MaxWidth = mainWindow.ActualWidth/1.3;
                if(count < firstDisplayImageNumber)
                {
                    count++;
                }
                else
                {
                    image.Visibility = Visibility.Collapsed;
                }
                FileInfo fileInfo = new FileInfo(file);
                try
                {
                    image.Source = new BitmapImage(new Uri(file));
                }
                catch 
                { 
                    image.Source = new BitmapImage(new Uri("https://cdn.icon-icons.com/icons2/1380/PNG/512/vcsconflicting_93497.png"));
                    image.MaxWidth = mainWindow.ActualWidth/1.9;
                }
                image.Tag = file;
                image.MouseRightButtonDown += Image_MouseRightButtonDown;
                Images.Add(image);
            }
            icImages.Items.Refresh();
            CurrentImagesViewPath = path;
            btnNextChapter.IsEnabled = true;
            btnPreviousChapter.IsEnabled = true;
            btnSizeUp.IsEnabled = true;
            btnSizeDown.IsEnabled = true;
        }
        private void Image_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            selectionImage = (sender as Image).Tag.ToString();
        }
        void CopyFile(FileInfo file, string desFileName)
        {
            string newFileName = desFileName;
            int count = 1;
            while(true)
            {
                if(!File.Exists(newFileName))
                {
                    file.CopyTo(newFileName, false);
                    break;
                }
                newFileName = Regex.Replace(desFileName, file.Extension + "$", "") + " (" + count + ")" + file.Extension;
                count++;
            }
        }
        private void MenuItemSave_Click(object sender, RoutedEventArgs e)
        {
            FileInfo file = new FileInfo(selectionImage);
            DirectoryInfo directoryStorage = file.Directory.Parent;
            directoryStorage = directoryStorage.CreateSubdirectory("Saved");
            string fileName = directoryStorage.FullName + "\\" + file.Name;
            CopyFile(file, fileName);
        }

        private void srvViewer_KeyDown(object sender, KeyEventArgs e)
        {
            ScrollViewer viewer = srvViewer;
            int scrollSpeed = 100;
            int speedUp = 100;
            if(e.Key == Key.J)
            {
                viewer.ScrollToVerticalOffset(viewer.ContentVerticalOffset + scrollSpeed);
            }
            else if(e.Key == Key.K)
            {
                viewer.ScrollToVerticalOffset(viewer.ContentVerticalOffset + scrollSpeed*(-1));
            }
            else if(e.Key == Key.L)
            {
                viewer.ScrollToHorizontalOffset(viewer.ContentHorizontalOffset + scrollSpeed);
            }
            else if(e.Key == Key.H)
            {
                viewer.ScrollToHorizontalOffset(viewer.ContentHorizontalOffset + scrollSpeed*(-1));
            }
            else if(e.Key == Key.D)
            {
                viewer.ScrollToVerticalOffset(viewer.ContentVerticalOffset + scrollSpeed + speedUp);
            }
            else if(e.Key == Key.U)
            {
                viewer.ScrollToVerticalOffset(viewer.ContentVerticalOffset + (scrollSpeed + speedUp)*(-1));
            }
            else if (e.Key == Key.O)
            {
                btnSizeUp_Click(null, null);
            }
            else if (e.Key == Key.I)
            {
                btnSizeDown_Click(null, null);
            }
            else if (e.Key == Key.N)
            {
                btnNextChapter_Click(null, null);
            }
            else if (e.Key == Key.P)
            {
                btnPreviousChapter_Click(null, null);
            }
        }
        string SelectFolder()
        {
            var folderSelectorDialog = new CommonOpenFileDialog();
            folderSelectorDialog.EnsureReadOnly = true;
            folderSelectorDialog.IsFolderPicker = true;
            folderSelectorDialog.AllowNonFileSystemItems = false;
            folderSelectorDialog.Multiselect = false;
            folderSelectorDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            folderSelectorDialog.Title = "Select Folder";
            if(folderSelectorDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                return folderSelectorDialog.FileName;
            }
            return null;
            
        }
        void OpenMenu()
        {
            piMenu.Kind = PackIconKind.Close;
            gdMenu.Visibility = Visibility.Visible;
        }
        void HideMenu()
        {
            piMenu.Kind = PackIconKind.Menu;
            gdMenu.Visibility = Visibility.Hidden;

        }
        void MenuToggle()
        {
            if (piMenu.Kind == PackIconKind.Menu)
            {
                OpenMenu();
            }
            else
            {
                HideMenu();
            }
        }
        private void ListDirectory(TreeView treeView, string path)
        {
            treeView.Items.Clear();
            var rootDirectoryInfo = new DirectoryInfo(path);
            treeView.Items.Add(CreateDirectoryNode(rootDirectoryInfo));
        }
        private static TreeViewItem CreateDirectoryNode(DirectoryInfo directoryInfo)
        {
            TreeViewItem item = new TreeViewItem();
            item.Header = directoryInfo.Name;
            item.Tag = directoryInfo;
            item.Items.Add("Loading...");
            return item;
            //var directoryNode = new TreeViewItem { Header = directoryInfo.Name, DataContext = directoryInfo.FullName };
            //var directories = directoryInfo.GetDirectories();
            //Array.Sort(directories, new AlphanumComparatorFast());
            //foreach (var directory in directories)
            //    directoryNode.Items.Add(CreateDirectoryNode(directory));

            ////foreach (var file in directoryInfo.GetFiles())
            ////    directoryNode.Items.Add(new TreeViewItem { Header = file.Name });

            //return directoryNode;

        }
        private void tvDirectory_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;
            item.Items.Clear();
            DirectoryInfo expandedDir = item.Tag as DirectoryInfo;
            DirectoryInfo[] subDirs = expandedDir.GetDirectories();
            Array.Sort(subDirs, new AlphanumComparatorFast());
            foreach(DirectoryInfo subDir in subDirs)
            {
                item.Items.Add(CreateDirectoryNode(subDir));
            }
        }
        private void tvDirectory_Selected(object sender, RoutedEventArgs e)
        {
            TreeViewItem item = e.Source as TreeViewItem;
            string path= "";
            if(item.Tag is DirectoryInfo)
            {
                path = (item.Tag as DirectoryInfo).FullName;
            }
            LoadImagesToViewerFromPath(path);
            SetViewerToFirst();
        }
        private void srvViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            ScrollViewer viewer = sender as ScrollViewer;
            if (viewer.VerticalOffset >= viewer.ScrollableHeight * 3 / 4)
            {
                int length = Images.Count();
                foreach (Image img in Images)
                {
                    if (img.Visibility == Visibility.Collapsed)
                    {
                        img.Visibility = Visibility.Visible;
                        break;
                    }
                }
            }
        }
        private void btnBrowserPath_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string folderName = SelectFolder();
                if (folderName is null) return;
                tbPath.Text = folderName;
                ListDirectory(tvDirectory, folderName);
            }
            catch { }
        }


        private void btnBrowserView_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string folderName = SelectFolder();
                if (folderName is null) return;
                tbViewPath.Text = folderName;
                LoadImagesToViewerFromPath(folderName);
            }
            catch { }
        }

        private void btnPreviousChapter_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo currentPath = new DirectoryInfo(CurrentImagesViewPath);
            string currentPathName = currentPath.Name;
            DirectoryInfo comicPath = currentPath.Parent;
            DirectoryInfo[] chapterPaths = comicPath.GetDirectories();
            Array.Sort(chapterPaths, new AlphanumComparatorFast());
            int length = chapterPaths.Count();
            for (int i = 0; i < length; i++)
            {
                if (chapterPaths[i].Name == currentPathName)
                {
                    if ((i - 1) < 0) return;
                    LoadImagesToViewerFromPath(chapterPaths[i - 1].FullName);
                    SetViewerToFirst();
                    break;
                }
            }
        }

        private void btnNextChapter_Click(object sender, RoutedEventArgs e)
        {
            DirectoryInfo currentPath = new DirectoryInfo(CurrentImagesViewPath);
            string currentPathName = currentPath.Name;
            DirectoryInfo comicPath = currentPath.Parent;
            DirectoryInfo[] chapterPaths = comicPath.GetDirectories();
            Array.Sort(chapterPaths, new AlphanumComparatorFast());
            int length = chapterPaths.Count();
            for (int i = 0; i < length; i++)
            {
                if (chapterPaths[i].Name == currentPathName)
                {
                    if ((i + 1) >= length) return;
                    LoadImagesToViewerFromPath(chapterPaths[i + 1].FullName);
                    SetViewerToFirst();
                    break;
                }
            }
        }

        private void btnSizeUp_Click(object sender, RoutedEventArgs e)
        {
            int length = Images.Count();
            for(int i = 0; i< length; i++)
            {
                Images[i].MaxWidth += 50;
            }
        }

        private void btnSizeDown_Click(object sender, RoutedEventArgs e)
        {
            int length = Images.Count();
            for(int i = 0; i< length; i++)
            {
                Images[i].MaxWidth -= 50;
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Header_MouseDown(object sender, MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void btnMinimizeAndMaximize_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Button).Background = Brushes.Gray;
        }

        private void btnMinimizeAndMaximize_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Button).Background = Brushes.Transparent;
        }

        private void btnClose_MouseEnter(object sender, MouseEventArgs e)
        {
            (sender as Button).Background = Brushes.Red;
        }

        private void btnClose_MouseLeave(object sender, MouseEventArgs e)
        {
            (sender as Button).Background = Brushes.Transparent;
        }

        private void btnBrowser_Click(object sender, RoutedEventArgs e)
        {
            Button btn = sender as Button;
            if(btn.Background == Brushes.Transparent && bdBrowser.Visibility == Visibility.Hidden)
            {
                btn.Background = Brushes.Gray;
                bdBrowser.Visibility = Visibility.Visible;
            }
            else
            {
                btn.Background = Brushes.Transparent;
                bdBrowser.Visibility = Visibility.Hidden;
            }
        }
        private void piMenu_MouseDown(object sender, InputEventArgs e)
        {
            MenuToggle();
        }

        private void btnMinimize_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void btnMaximize_Click(object sender, RoutedEventArgs e)
        {
            if(WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
                return;
            }
            WindowState = WindowState.Maximized;
        }

        private void icImages_MouseDown(object sender, MouseButtonEventArgs e)
        {
            HideMenu();
        }

    }
    public class AlphanumComparatorFast : IComparer
    {
        public int Compare(object x, object y)
        {
            if(x is DirectoryInfo)
            {
                x = (x as DirectoryInfo).FullName;
            }
            if(y is DirectoryInfo)
            {
                y = (y as DirectoryInfo).FullName;
            }
            string s1 = x as string;
            if (s1 == null)
            {
                return 0;
            }
            string s2 = y as string;
            if (s2 == null)
            {
                return 0;
            }

            int len1 = s1.Length;
            int len2 = s2.Length;
            int marker1 = 0;
            int marker2 = 0;

            // Walk through two the strings with two markers.
            while (marker1 < len1 && marker2 < len2)
            {
                char ch1 = s1[marker1];
                char ch2 = s2[marker2];

                // Some buffers we can build up characters in for each chunk.
                char[] space1 = new char[len1];
                int loc1 = 0;
                char[] space2 = new char[len2];
                int loc2 = 0;

                // Walk through all following characters that are digits or
                // characters in BOTH strings starting at the appropriate marker.
                // Collect char arrays.
                do
                {
                    space1[loc1++] = ch1;
                    marker1++;

                    if (marker1 < len1)
                    {
                        ch1 = s1[marker1];
                    }
                    else
                    {
                        break;
                    }
                } while (char.IsDigit(ch1) == char.IsDigit(space1[0]));

                do
                {
                    space2[loc2++] = ch2;
                    marker2++;

                    if (marker2 < len2)
                    {
                        ch2 = s2[marker2];
                    }
                    else
                    {
                        break;
                    }
                } while (char.IsDigit(ch2) == char.IsDigit(space2[0]));

                // If we have collected numbers, compare them numerically.
                // Otherwise, if we have strings, compare them alphabetically.
                string str1 = new string(space1);
                string str2 = new string(space2);

                int result;

                if (char.IsDigit(space1[0]) && char.IsDigit(space2[0]))
                {
                    int thisNumericChunk = int.Parse(str1);
                    int thatNumericChunk = int.Parse(str2);
                    result = thisNumericChunk.CompareTo(thatNumericChunk);
                }
                else
                {
                    result = str1.CompareTo(str2);
                }

                if (result != 0)
                {
                    return result;
                }
            }
            return len1 - len2;
        }
    }
}
