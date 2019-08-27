using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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
using System.Windows.Shapes;




namespace WpfApp2
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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

        public Window1()
        {
            DataContext = this;
            InitializeComponent();
            Populate_Tag_Window();
        }

        private void Populate_Tag_Window()
        {
            TaglistBox.Items.Clear();

            if (MainWindow.CONFIGURATION.tagStore != null)
            {
                foreach (var item in MainWindow.CONFIGURATION.tagStore)
                {
                    if (item != null)
                    {
                        TaglistBox.Items.Add(item.name);
                    }
                }
            }
        }

        private void AddTagbutton_Click(object sender, RoutedEventArgs e)
        {
            int count = 0;
            bool finish = false;
            int tag_Store_Length = MainWindow.CONFIGURATION.tagStore.Length;

            TagStoreElementTag[] new_Store = new TagStoreElementTag[tag_Store_Length + 1];

            TagStoreElementTag element = new TagStoreElementTag();
            element.name = Tag_Name_Textbox.Text;
            //element.group = Tag_Name_Group.Text;
            element.image = Tag_Image_Textbox.Text;

            foreach (var item in MainWindow.CONFIGURATION.tagStore)
            {
                if (item.name == element.name)
                {
                    DialogResult dialogResult = System.Windows.Forms.MessageBox.Show("Tag " + element.name + " already exists", "Overwrite", MessageBoxButtons.YesNo);
                    if (dialogResult == System.Windows.Forms.DialogResult.Yes)
                    {
                        new_Store[count] = element;
                    }
                    finish = true;
                }
                else
                {
                    new_Store[count] = item;
                }
                count++;
            }

            if (!finish)
            {
                new_Store[count] = element;
            }
            MainWindow.CONFIGURATION.tagStore = new_Store;
            Populate_Tag_Window();
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
    }
}
