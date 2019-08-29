using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

        private ObservableCollection<TagStoreElementTag> _tagList = new ObservableCollection<TagStoreElementTag>();
        public ObservableCollection<TagStoreElementTag> tagList
        {
            get
            {
                if (_tagList.Count <= 0)
                {
                    foreach (var item in MainWindow.CONFIGURATION.tagStore)
                    {
                        _tagList.Add(item);
                    }
                }
                return _tagList;
            }
            //set
            //{
            //    _tagList = value;
            //    OnPropertyChanged("tagList");
            //}

            set
            {
                if (_tagList != value)
                {
                    _tagList = value;
                    OnPropertyChanged();
                }
            }
        }


        public Window1()
        {
            DataContext = this;

            Array.Sort<TagStoreElementTag>(MainWindow.CONFIGURATION.tagStore, (x, y) => String.Compare(x.name, y.name,
                                     StringComparison.CurrentCultureIgnoreCase));
            //Array.Sort(MainWindow.CONFIGURATION.tagStore, (x, y) => String.Compare(x.name, y.name));
            InitializeComponent();

            // Populate_Tag_Window();
        }

        //private void Populate_Tag_Window()
        //{
        //    //TaglistBox.Items.Clear();

        //    if (MainWindow.CONFIGURATION.tagStore != null)
        //    {
        //        foreach (var item in MainWindow.CONFIGURATION.tagStore)
        //        {
        //            if (item != null)
        //            {
        //                TaglistBox.Items.Add(item.name);
        //            }
        //        }
        //    }
        //}

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
                count++;
            }

            if (!finish)
            {
                tagList.Add(element);
                tagList.OrderBy(n => n.name);
                MainWindow.CONFIGURATION.tagStore = tagList.ToArray();
            }
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
