using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Runtime.CompilerServices;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.Collections.Specialized;

namespace WpfApp2
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : INotifyPropertyChanged
    {
        private const string CONFIG_FILENAME = "TaggerConfiguration.xml";
        private static string BASE_DIR = string.Empty;
        private static string[] SUPPORTED_FILES = { ".mpg", ".avi", ".flv", ".mkv", ".mp4", ".mpeg", ".wmv", ".mov" };
        public static RootElement CONFIGURATION = new RootElement();
        //private static FileListElement[] ACTUAL_TAGS = new FileListElementTags[] { };
        public const int MAX_TAGS = 1024;

        public class FileView
        {
            public string path { get; set; }
            public string name { get; set; }
            public string size { get; set; }
            public bool tagged { get; set; }
            public ObservableCollection<TagStoreElementTag> filetags { get; set; }
        }

        ///////////////////////////////////////////////////////////////////////////////////////////

        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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


        //private ObservableCollection<FileListElementTags> _actualTags = new ObservableCollection<FileListElementTags>();
        //public ObservableCollection<FileListElementTags> actualTags
        //{
        //    get
        //    {
        //        if (selectedFile.tagged)
        //        {

        //            foreach (var item in tagList)
        //            {

        //            }
        //        }
        //        return _actualTags;
        //    }

        //    set
        //    {
        //        if (_actualTags != value)
        //        {
        //            _actualTags = value;
        //            OnPropertyChanged();
        //        }
        //    }
        //}


        private void CollectionChangeHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
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


        //private void OnListBoxItemContainerFocused(object sender, RoutedEventArgs e)
        //{
        //    (sender as ListBoxItem).IsSelected = true;
        //}

        //private List<TagStoreElementTag> _datagrid_items;

        //public List<TagStoreElementTag> datagrid_Items
        //{
        //    get { return _datagrid_items; }
        //    set { _datagrid_items = value; }
        //}


        ///////////////////////////////////////////////////////////////////////////////////////////


        public MainWindow()
        {
            DataContext = this;
            this.tagList.CollectionChanged += this.CollectionChangeHandler;
            InitializeComponent();

        }

        private void PopulateTagGrid()
        {
            //List<TagStoreElementTag> lsts = new List<TagStoreElementTag>();

            //foreach (var item in CONFIGURATION.tagStore)
            //{
            //    lsts.Add(item);
            //}

            tagImageList.ItemsSource = tagList;
        }

        public string choose_Folder()
        {
            FolderBrowserDialog fbd = new FolderBrowserDialog();

            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                labelrootpath.Content = "";
                labelrootpath.Content = fbd.SelectedPath;

                return fbd.SelectedPath;
            }
            return string.Empty;
        }

        private void populate_tag_grid()
        {
            //tag_grid.ItemsSource = tagList;
        }



        private FileView ComposeFileView(string file)
        {
            FileView element = new FileView();

            element.path = file;
            element.name = System.IO.Path.GetFileName(file);
            element.size = File_Size(file);
            element.tagged = false;

            for (int i = 0; i < CONFIGURATION.file.Length; i++)
            {
                if (CONFIGURATION.file[i].name == file)
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
                        TagStoreElementTag auxTag = new TagStoreElementTag() {name = auxName, image = auxImage};
                        element.filetags.Add(auxTag);
                    }
                }
            }
            return element;
        }

        private void populate_listbox()
        {
            {
                // Enumerar ficheros
                //try
               // {
                    string[] allFiles = Directory.GetFiles(BASE_DIR, "*.*")
                        .Where(f => SUPPORTED_FILES.Contains(System.IO.Path.GetExtension(f).ToLower())).ToArray();
                    int total_elements = allFiles.GetLength(0);
                    foreach (string currentFile in allFiles)
                    {
                        //ComposeFileView(currentFile);
                        fileList.Add(ComposeFileView(currentFile));
                    }
               // }
                //catch (Exception e)
                //{
                //    System.Windows.Forms.MessageBox.Show(e.Message, "Error populating File listbox", MessageBoxButtons.OK, MessageBoxIcon.Error);
                //}

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
            Array.Sort<TagStoreElementTag>(MainWindow.CONFIGURATION.tagStore, (x, y) => String.Compare(x.name, y.name,
                         StringComparison.CurrentCultureIgnoreCase));
            SaveConfigurationButton.IsEnabled = false;
            PopulateTagGrid();
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
                populate_listbox();
                populate_tag_grid();
            }
            else
            {
                BASE_DIR = file_path;
                SaveConfigurationButton.IsEnabled = true;
            }
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
                System.Diagnostics.Process.Start(selectedFile.path);
                //System.Windows.Forms.MessageBox.Show(LISTBOX_INDEX.ToString());
            }
        }

        //private void Add_tag_button_Click(object sender, RoutedEventArgs e)
        //{
        //    Window1 tagWindow = new Window1();
        //    tagWindow.Show();
        //    SaveConfigurationButton.IsEnabled = true;

        //}

        private void ListboxRoot_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            for (int i = 0; i < CONFIGURATION.file.Length; i++)
            {
                if (selectedFile.path == CONFIGURATION.file[i].name)
                {
                    // File exist in CONFIGURATION so it is tagged                    
                    // ACTUAL_TAGS = CONFIGURATION.file[i].tags;
                    //actualTagsGrid.ItemsSource = selectedFile.filetags;                    
                }
            }
        }

        private void SaveConfigurationButtonClick(object sender, RoutedEventArgs e)
        {
            try
            {
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
            finish = true;
        }

        private void Tag_Name_Textbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!String.IsNullOrEmpty(Tag_Name_Textbox.Text))
            {
                addTagbutton.IsEnabled = true;
            }
            else
            {
                addTagbutton.IsEnabled = false;
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ActualTagsGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void InsertTagOnFile (TagStoreElementTag tagName, FileView file)
        {
            bool found = false;
            System.Windows.Forms.MessageBox.Show("");
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
            }
        }

        private void TagStoreButton_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //string content = (sender as System.Windows.Controls.Button).Content.ToString();
            //string second = e.ToString();

            //System.Windows.Controls.Button source_button = (System.Windows.Controls.Button)sender;
            string name = ((TagStoreElementTag)source_button.DataContext).name;
            string image = ((TagStoreElementTag)source_button.DataContext).image;
            string group = ((TagStoreElementTag)source_button.DataContext).group;

            TagStoreElementTag clickedTag = new TagStoreElementTag { name = name, image = image, group = group };

            if (selectedFile != null)
            {
                InsertTagOnFile(clickedTag, selectedFile);
            }
            else
            {
                System.Windows.Forms.MessageBox.Show(" No file selected" );
            }

        }
    }
}

