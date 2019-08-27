using System;
using System.Collections.Generic;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml.Serialization;
using System.Security.Cryptography;

namespace WpfApp2
{
    /// <summary>
    /// Lógica de interacción para MainWindow.xaml
    /// </summary>
    /// 


    public partial class MainWindow : Window
    {
        private const string CONFIG_FILENAME = "TaggerConfiguration.xml";
        private static string BASE_DIR = string.Empty;
        private static string[] SUPPORTED_FILES = { ".mpg", ".avi", ".flv", ".mkv", ".mp4", ".mpeg", ".wmv", ".mov" };
        public static RootElement CONFIGURATION = new RootElement();
        private static object LISTBOX_INDEX = null;
        public const int MAX_TAGS = 1024;

        private List<TagStoreElementTag> _datagrid_items;

        public List<TagStoreElementTag> datagrid_Items
        {
            get { return _datagrid_items; }
            set { _datagrid_items = value; }
        }

        public MainWindow()
        {
            InitializeComponent();

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
            tag_grid.ItemsSource = CONFIGURATION.tagStore;
            
        }

        private void populate_listbox()
        {
            foreach (var item in CONFIGURATION.file)
            {
                listboxRoot.Items.Add(item.name);
            }
        }
        private bool exists_Configuration(string path)
        {
            if (File.Exists(System.IO.Path.Combine(path, CONFIG_FILENAME)))
            {
                return true;
            }
            else
            {
                return false;
            }
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

            generate_button.IsEnabled = false;
            populate_listbox();
            populate_tag_grid();
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
                    file_list_element.tagged = false;
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

            generate_button.IsEnabled = false;
        }

        private void Search_Dir_Button_Click(object sender, RoutedEventArgs e)
        {
            string file_path = string.Empty;

            file_path = choose_Folder();

            if (exists_Configuration(file_path))
            {
                BASE_DIR = file_path;
                Environment.CurrentDirectory = (BASE_DIR);
                load_Configuration(System.IO.Path.Combine(BASE_DIR, CONFIG_FILENAME));
            }
            else
            {
                BASE_DIR = file_path;
                generate_button.IsEnabled = true;
            }
        }

        private Boolean Save_Configuration()
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
                return true;
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
                return false;
            }
        }

        private void Generate_Button_Click(object sender, RoutedEventArgs e)
        {
            Create_Tagger_Configuration();
            Boolean result = Save_Configuration();
        }

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

        private void ListboxRoot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            LISTBOX_INDEX = listboxRoot.Items[listboxRoot.SelectedIndex];
        }

        private void ListboxRoot_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (listboxRoot.SelectedItem != null)
            {
                System.Diagnostics.Process.Start(LISTBOX_INDEX.ToString());
                //System.Windows.Forms.MessageBox.Show(LISTBOX_INDEX.ToString());
            }
        }

        private void Add_tag_button_Click(object sender, RoutedEventArgs e)
        {
            Window1 tagWindow = new Window1();
            tagWindow.Show();

        }

        private void Save_Configuration_Button_Click(object sender, RoutedEventArgs e)
        {
            Save_Configuration();
        }

    }
}
