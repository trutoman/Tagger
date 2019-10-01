using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.Collections.Specialized;

namespace WpfApp2
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    /// 

    public class SortAdorner : Adorner
    {
        private static Geometry ascGeometry =
            Geometry.Parse("M 0 4 L 3.5 0 L 7 4 Z");

        private static Geometry descGeometry =
            Geometry.Parse("M 0 0 L 3.5 4 L 7 0 Z");

        public ListSortDirection Direction { get; private set; }

        public SortAdorner(UIElement element, ListSortDirection dir)
            : base(element)
        {
            this.Direction = dir;
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            base.OnRender(drawingContext);

            if (AdornedElement.RenderSize.Width < 20)
                return;

            TranslateTransform transform = new TranslateTransform
                (
                    AdornedElement.RenderSize.Width - 15,
                    (AdornedElement.RenderSize.Height - 5) / 2
                );
            drawingContext.PushTransform(transform);

            Geometry geometry = ascGeometry;
            if (this.Direction == ListSortDirection.Descending)
                geometry = descGeometry;
            drawingContext.DrawGeometry(Brushes.Black, null, geometry);

            drawingContext.Pop();
        }
    }

    public partial class MainWindow : INotifyPropertyChanged
    {
        private const string CONFIG_FILENAME = "TaggerConfiguration.xml";
        private static string BASE_DIR = string.Empty;
        private static string[] SUPPORTED_FILES = { ".mpg", ".avi", ".flv", ".mkv", ".mp4", ".mpeg", ".wmv", ".mov" };
        public static RootElement CONFIGURATION = new RootElement();
        public const int MAX_TAGS = 1024;

        private GridViewColumnHeader listViewSortCol = null;
        private SortAdorner listViewSortAdorner = null;

        public class FileView
        {
            public string path { get; set; }
            public string name { get; set; }
            public string size { get; set; }
            public string dir { get; set; }
            public bool tagged { get; set; }
            public ObservableCollection<TagStoreElementTag> filetags { get; set; }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<TagStoreElementTag> _filterTags = new ObservableCollection<TagStoreElementTag>();
        public ObservableCollection<TagStoreElementTag> filterTags
        {
            get
            {
                return _filterTags;
            }

            set
            {
                if (_filterTags != value)
                {
                    _filterTags = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<FileView> _fileList = new ObservableCollection<FileView>();
        public ObservableCollection<FileView> fileList
        {
            get
            {
                return _fileList;
            }
            set
            {
                if (_fileList != value)
                {
                    System.Windows.Forms.MessageBox.Show("--Filelist set -----------------");
                    _fileList = value;
                    OnPropertyChanged();
                }
            }
        }

        private ObservableCollection<TagStoreElementTag> _tagList = new ObservableCollection<TagStoreElementTag>();
        public ObservableCollection<TagStoreElementTag> tagList
        {
            get
            {
                if (_tagList.Count <= 0)
                {
                    if (MainWindow.CONFIGURATION.tagStore != null)
                    {
                        //System.Windows.Forms.MessageBox.Show("Estoy en el get");
                        foreach (var item in MainWindow.CONFIGURATION.tagStore)
                        {
                            _tagList.Add(item);
                        }
                    }
                }
                return _tagList;
            }

            set
            {
                if (_tagList != value)
                {
                    _tagList = value;
                    OnPropertyChanged();
                }
            }
        }

        private void CollectionChangeHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            //System.Windows.Forms.MessageBox.Show("CollectionChangeHandler" + sender.ToString() + " -> " + e.Action);
            MainWindow.CONFIGURATION.tagStore = _tagList.ToArray();
        }

        private TagStoreElementTag _selectedTag;
        public TagStoreElementTag selectedTag
        {
            get { return _selectedTag; }
            set
            {
                if (_selectedTag != value)
                {
                    _selectedTag = value;
                    OnPropertyChanged();
                }
            }
        }

        private FileView _selectedFile;
        public FileView selectedFile
        {
            get { return _selectedFile; }
            set
            {
                if (_selectedFile != value)
                {
                    _selectedFile = value;
                    OnPropertyChanged();
                }
            }
        }

        public MainWindow()
        {
            DataContext = this;
            this.tagList.CollectionChanged += this.CollectionChangeHandler;
            InitializeComponent();

            ICollectionView view = CollectionViewSource.GetDefaultView(tagList);
            view.GroupDescriptions.Add(new PropertyGroupDescription("group"));
            view.SortDescriptions.Add(new SortDescription("group", ListSortDirection.Ascending));
            view.SortDescriptions.Add(new SortDescription("name", ListSortDirection.Ascending));
            tagImageList.ItemsSource = view;

            //CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(listboxRoot.ItemsSource);            
            //view.SortDescriptions.Add(new SortDescription("name", ListSortDirection.Ascending));
            //view.SortDescriptions.Add(new SortDescription("size", ListSortDirection.Ascending));
        }

        private void PopulateTagGrid()
        {
            tagImageList.ItemsSource = tagList;
        }

        public string choose_Folder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return fbd.SelectedPath;
            }
            return string.Empty;
        }

        private FileView ComposeFileView(string file)
        {
            FileView element = new FileView();

            element.path = file;
            element.name = System.IO.Path.GetFileName(file);
            element.size = File_Size(file);

            string path1 = System.IO.Path.GetDirectoryName(file);
            string result = path1.Substring(BASE_DIR.Length).TrimStart(System.IO.Path.DirectorySeparatorChar);
            element.dir = result;

            element.tagged = false;

            if (CONFIGURATION.file != null)
            {
                for (int i = 0; i < CONFIGURATION.file.Length; i++)
                {
                    if (CONFIGURATION.file[i].path == file)
                    {
                        element.filetags = new ObservableCollection<TagStoreElementTag>();
                        element.tagged = true;
                        for (int j = 0; j < CONFIGURATION.file[i].tags.Length; j++)
                        {
                            string auxName = CONFIGURATION.file[i].tags[j];
                            string auxImage = "";

                            for (int k = 0; k < CONFIGURATION.tagStore.Length; k++)
                            {
                                if (CONFIGURATION.tagStore[k].name == CONFIGURATION.file[i].tags[j])
                                {
                                    auxImage = CONFIGURATION.tagStore[k].image;
                                }
                            }
                            TagStoreElementTag auxTag = new TagStoreElementTag() { name = auxName, image = auxImage };
                            element.filetags.Add(auxTag);
                        }
                    }
                }
            }
            return element;
        }

        private void populate_listbox()
        {
            {
                // Enumerar ficheros
                try
                {
                    string[] allFiles = Directory.GetFiles(BASE_DIR, "*.*", SearchOption.AllDirectories)
                        .Where(f => SUPPORTED_FILES.Contains(System.IO.Path.GetExtension(f).ToLower())).ToArray();
                    int total_elements = allFiles.GetLength(0);
                    foreach (string currentFile in allFiles)
                    {
                        fileList.Add(ComposeFileView(currentFile));
                    }
                }
                catch (Exception e)
                {
                    System.Windows.Forms.MessageBox.Show(e.Message, "Error populating File listbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CheckFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            // check if focus is moving from a ListBoxItem, to a ListBoxItem
            if (e.OldFocus.GetType().Name == "ListBoxItem" && e.NewFocus.GetType().Name == "ListBoxItem")
            {
                // if so, cause the original ListBoxItem to loose focus
                (e.OldFocus as ListBoxItem).IsSelected = false;
            }
        }

        private bool exists_Configuration(string path)
        {
            return (File.Exists(System.IO.Path.Combine(path, CONFIG_FILENAME)));
        }

        private void load_Configuration(string config_file)
        {
            try
            {
                // Create an instance of the XmlSerializer specifying type.
                XmlSerializer serializer = new XmlSerializer(typeof(RootElement));

                // Create a TextReader to read the file. 
                FileStream fs = new FileStream(config_file, FileMode.Open);
                TextReader reader = new StreamReader(fs);

                CONFIGURATION = (RootElement)serializer.Deserialize(reader);
            }

            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }

            if (CONFIGURATION.tagStore != null)
            {
                Array.Sort<TagStoreElementTag>(MainWindow.CONFIGURATION.tagStore, (x, y) => String.Compare(x.name, y.name,
                             StringComparison.CurrentCultureIgnoreCase));
            }
            SaveConfigurationButton.IsEnabled = false;
        }

        private string File_Size(string filename)
        {
            string[] sizes = { "BY", "KB", "MB", "GB", "TB" };
            double len = new FileInfo(filename).Length;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }

            // Adjust the format string to your preferences. For example "{0:0.#}{1}" would
            // show a single decimal place, and no space.
            string result = String.Format("{0:0.#} {1}", len, sizes[order]);
            return result;
        }

        private void Create_Tagger_Configuration()
        {
            int counter = 0;
            // Enumerar ficheros
            try
            {
                string[] allFiles = Directory.GetFiles(BASE_DIR, "*.*")
                    .Where(f => SUPPORTED_FILES.Contains(System.IO.Path.GetExtension(f).ToLower())).ToArray();

                int total_elements = allFiles.GetLength(0);
                CONFIGURATION.file = new FileListElement[total_elements];
                CONFIGURATION.tagStore = new TagStoreElementTag[0];

                foreach (string currentFile in allFiles)
                {
                    FileListElement file_list_element = new FileListElement();
                    file_list_element.name = currentFile;
                    //file_list_element.checksum = CalculateMD5(currentFile);
                    file_list_element.size = File_Size(currentFile);
                    CONFIGURATION.file[counter] = file_list_element;
                    listboxRoot.Items.Add(currentFile);
                    counter++;
                }

                CONFIGURATION.rootDir = BASE_DIR;
                CONFIGURATION.numFiles = listboxRoot.Items.Count.ToString();
            }
            catch (Exception e)
            {
                System.Windows.Forms.MessageBox.Show(e.Message);
            }

            SaveConfigurationButton.IsEnabled = false;
        }

        private void Search_Dir_Button_Click(object sender, RoutedEventArgs e)
        {
            string file_path = string.Empty;
            file_path = choose_Folder();

            if (exists_Configuration(file_path) && file_path != string.Empty)
            {
                BASE_DIR = file_path;
                Environment.CurrentDirectory = (BASE_DIR);
                load_Configuration(System.IO.Path.Combine(BASE_DIR, CONFIG_FILENAME));

                if (CONFIGURATION.tagStore != null)
                {
                    PopulateTagGrid();
                }

                populate_listbox();
                RefreshInfo();
            }
            else
            {
                BASE_DIR = file_path;
                SaveConfigurationButton.IsEnabled = true;
            }
        }

        private void RefreshInfo()
        {
            labelrootpath.Content = "";
            labelrootpath.Content = "Tagger at " + BASE_DIR + " : ";
            labelrootpath.Content += " Total files: " + listboxRoot.Items.Count.ToString();
            labelrootpath.Content += " Tagged files: " + CONFIGURATION.file.Length.ToString();
            labelrootpath.Content += " Total tags: " + CONFIGURATION.tagStore.Length.ToString();
        }

        //private void Generate_Button_Click(object sender, RoutedEventArgs e)
        //{
        //    Create_Tagger_Configuration();
        //    Boolean result = Save_Configuration();
        //}

        static string CalculateMD5(string filename)
        {
            using (var md5 = MD5.Create())
            {
                using (var stream = File.OpenRead(filename))
                {
                    var hash = md5.ComputeHash(stream);
                    return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
                }
            }
        }

        private void ListboxRoot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listboxRoot.SelectedItem != null)
            {
                if (File.Exists(selectedFile.path))
                {
                    System.Diagnostics.Process.Start(selectedFile.path);
                    //System.Windows.Forms.MessageBox.Show(LISTBOX_INDEX.ToString());
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("El fichero no existe", "File not exists", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }
        }

        private void RemoveTaggedElement (FileView file)
        {
            if (CONFIGURATION.file != null)
            {
                try
                {
                    ObtainFilesChanges();
                    CONFIGURATION.numFiles = CONFIGURATION.file.Length.ToString();
                    string pathFileSerialized = System.IO.Path.Combine(BASE_DIR, CONFIG_FILENAME);
                    XmlSerializer out_xmlSerializer = new XmlSerializer(typeof(RootElement));

                    using (var sw = new StreamWriter(pathFileSerialized))
                    {
                        RootElement conf1 = CONFIGURATION;
                        out_xmlSerializer.Serialize(sw, conf1);
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Forms.MessageBox.Show(ex.Message);
                }
                SaveConfigurationButton.IsEnabled = false;
                RefreshInfo();
            }
        }

        private void ObtainFilesChanges()
        {
            List<string> filetaglist = new List<string>();
            List<FileListElement> allfiles = new List<FileListElement>();

            foreach (var item in fileList)
            {
                if (item.tagged)
                {
                    //for (int i = 0; i < CONFIGURATION.file.Length; i++)
                    //{
                    //if (CONFIGURATION.file[i].name == item.name)
                    // {
                    FileListElement element = new FileListElement();
                    element.name = item.name;
                    element.path = item.path;
                    element.size = item.size;
                    element.tagged = item.tagged;

                    foreach (var tag in item.filetags)
                    {
                        filetaglist.Add(tag.name);
                    }
                    element.tags = filetaglist.ToArray();
                    allfiles.Add(element);
                    filetaglist.Clear();
                }

            }

            CONFIGURATION.file = allfiles.ToArray();
        }


        private void SaveConfigurationButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Taglist changes are alredy done because of Changeevent over tagList.
                CONFIGURATION.rootDir = BASE_DIR;
                ObtainFilesChanges();
                CONFIGURATION.numFiles = CONFIGURATION.file.Length.ToString();
                string pathFileSerialized = System.IO.Path.Combine(BASE_DIR, CONFIG_FILENAME);
                XmlSerializer out_xmlSerializer = new XmlSerializer(typeof(RootElement));

                using (var sw = new StreamWriter(pathFileSerialized))
                {
                    RootElement conf1 = CONFIGURATION;
                    out_xmlSerializer.Serialize(sw, conf1);
                }
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
            SaveConfigurationButton.IsEnabled = false;
            RefreshInfo();
        }

        private void AddTagbutton_Click(object sender, RoutedEventArgs e)
        {
            bool finish = false;

            TagStoreElementTag element = new TagStoreElementTag();
            element.name = Tag_Name_Textbox.Text;
            element.group = Tag_Name_Group.Text;
            element.image = Tag_Image_Textbox.Text;

            for (int count = 0; count < tagList.Count; count++)
            {
                if (tagList[count].name == element.name)
                {
                    DialogResult dialogResult =
                        System.Windows.Forms.MessageBox.Show("Tag " + element.name + " already exists \nDo you want to overwrite it?", "Tag already exists",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Information);

                    if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                    {
                        tagList[count] = element;
                    }
                    finish = true;
                }
            }

            if (!finish)
            {
                tagList.Add(element);
                tagList.OrderBy(n => n.name);
                MainWindow.CONFIGURATION.tagStore = tagList.ToArray();
            }
            SaveConfigurationButton.IsEnabled = true;
        }

        private void RemoveTagbutton_Click(object sender, RoutedEventArgs e)
        {
            tagList.Remove(selectedTag);
            tagList.OrderBy(n => n.name);
            MainWindow.CONFIGURATION.tagStore = tagList.ToArray();
            SaveConfigurationButton.IsEnabled = true;
            RefreshInfo();
        }

        private void Tag_Name_Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(Tag_Name_Textbox.Text))
            {
                okTagButton.IsEnabled = true;
            }
            else
            {
                okTagButton.IsEnabled = false;
            }
        }

        private void Search_Image_Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fbd = new OpenFileDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string file_path = fbd.FileName;
                Tag_Image_Textbox.Text = file_path;
            }
        }

        private void InsertTagOnFile(TagStoreElementTag tagName, FileView file)
        {
            if (!file.tagged)
            {

                var modifiable = fileList.FirstOrDefault(i => i.path == file.path);
                modifiable.name = file.name;
                modifiable.path = file.path;
                modifiable.size = file.size;
                modifiable.tagged = true;
                modifiable.filetags = new ObservableCollection<TagStoreElementTag>();
                modifiable.filetags.Add(tagName);
                SaveConfigurationButton.IsEnabled = true;
                selectedFile = null;
                selectedFile = modifiable;
                listboxRoot.Items.Refresh();
                actualTagsGrid.Items.Refresh();
            }
            else
            {
                bool found = false;
                //System.Windows.Forms.MessageBox.Show("");
                foreach (var item in file.filetags)
                {
                    if (item.name == tagName.name)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    file.filetags.Add(tagName);
                    SaveConfigurationButton.IsEnabled = true;
                }
            }
            RefreshInfo();
        }

        private void TagStoreButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Button source_button = (System.Windows.Controls.Button)sender;
            string name = ((TagStoreElementTag)source_button.DataContext).name;
            string image = ((TagStoreElementTag)source_button.DataContext).image;
            string group = ((TagStoreElementTag)source_button.DataContext).group;

            TagStoreElementTag clickedTag = new TagStoreElementTag { name = name, image = image, group = group };

            TabItem ti = Tabs1.SelectedItem as TabItem;
            if (ti.Name == "file")
            {

                if (selectedFile != null)
                {
                    if (listboxRoot.SelectedItems.Count > 0)
                    {
                        List<FileView> sliceFiles = new List<FileView>();
                        foreach (var item in listboxRoot.SelectedItems)
                        {
                            sliceFiles.Add((FileView)item);
                        }
                        foreach (FileView file in sliceFiles)
                        {
                            InsertTagOnFile(clickedTag, file);
                        }
                    }
                }
                else
                {
                    System.Windows.Forms.MessageBox.Show("No file", "No file selected", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
            }

            else if (ti.Name == "filter")
            {
                bool found = false;
                //System.Windows.Forms.MessageBox.Show("");
                foreach (var item in filterTags)
                {
                    if (item.name == clickedTag.name)
                    {
                        found = true;
                    }
                }
                if (!found)
                {
                    filterTags.Add(clickedTag);
                }
                //FilterTags.Items.Refresh();
            }
        }

        private void RemoveTagOnFile(string name)
        {
            foreach (var item in selectedFile.filetags)
            {
                if (item.name == name)
                {
                    selectedFile.filetags.Remove(item);
                    SaveConfigurationButton.IsEnabled = true;
                    break;
                }
            }
            RefreshInfo();
        }

        private void Button_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            System.Windows.Controls.Button source_button = (System.Windows.Controls.Button)sender;
            string name = ((TagStoreElementTag)source_button.DataContext).name;
            RemoveTagOnFile(name);
        }

        private void lvUsersColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            GridViewColumnHeader column = (sender as GridViewColumnHeader);
            string sortBy = column.Tag.ToString();
            if (listViewSortCol != null)
            {
                AdornerLayer.GetAdornerLayer(listViewSortCol).Remove(listViewSortAdorner);
                listboxRoot.Items.SortDescriptions.Clear();
            }

            ListSortDirection newDir = ListSortDirection.Ascending;
            if (listViewSortCol == column && listViewSortAdorner.Direction == newDir)
                newDir = ListSortDirection.Descending;

            listViewSortCol = column;
            listViewSortAdorner = new SortAdorner(listViewSortCol, newDir);
            AdornerLayer.GetAdornerLayer(listViewSortCol).Add(listViewSortAdorner);
            listboxRoot.Items.SortDescriptions.Add(new SortDescription(sortBy, newDir));
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        private void Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            IEnumerable<System.Windows.Controls.Button> elements = FindVisualChildren<System.Windows.Controls.Button>(this).Where(x => x.Tag != null && x.Tag.ToString() == "tagtag");
            foreach (System.Windows.Controls.Button button in elements)
            {
                button.Width = slider.Value;
                button.Height = slider.Value;
            }
        }

        private void DeleteFile()
        {

            if (listboxRoot.SelectedItems.Count > 0)
            {
                List<FileView> sliceFiles = new List<FileView>();
                foreach (var item in listboxRoot.SelectedItems)
                {
                    sliceFiles.Add((FileView)item);
                }
                foreach (FileView file in sliceFiles)
                {
                    if (File.Exists(file.path))
                    {
                        if (System.Windows.MessageBox.Show("Remove " + file.name, 
                            "Remove file", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            try
                            {
                                File.Delete(file.path);
                                fileList.Remove(file);
                                if (file.tagged)
                                {
                                    RemoveTaggedElement(file);
                                }
                                //ICollectionView view = CollectionViewSource.GetDefaultView(listboxRoot.ItemsSource);
                                //view.Refresh();
                            }
                            catch (FileNotFoundException e)
                            {
                                System.Windows.MessageBox.Show($"The file was not found: '{e}'");
                            }
                            catch (DirectoryNotFoundException e)
                            {
                                System.Windows.MessageBox.Show($"The directory was not found: '{e}'");
                            }
                            catch (IOException e)
                            {
                                System.Windows.MessageBox.Show($"The file could not be deleted: '{e}'");
                            }
                        }
                    }
                }
            }
        }

        // A way to get a command from interface
        public class ActionCommand : ICommand
        {
            private readonly Action _action;

            public ActionCommand(Action action)
            {
                _action = action;
            }

            public void Execute(object parameter)
            {
                _action();
            }

            public bool CanExecute(object parameter)
            {
                return true;
            }

            public event EventHandler CanExecuteChanged;
        }

        private ICommand someCommand;
        public ICommand SomeCommand
        {
            get
            {
                return someCommand
                    ?? (someCommand = new ActionCommand(() =>
                    {
                        DeleteFile();
                    }));
            }
        }

        //////////////////////////////////////////

        private void AndButton_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Controls.Button source_button = (System.Windows.Controls.Button)sender;
            string name = ((TagStoreElementTag)source_button.DataContext).name;
            string image = "";
            string group = "";

            TagStoreElementTag clickedTag = new TagStoreElementTag { name = name, image = image, group = group };
        }
    }
}