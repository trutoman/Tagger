using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tagger.Model
{
    public class FileViewCollection : ObservableCollection<FileView>
    {

    }

    public class FileView
    {
        private string _path;
        public string path
        {
            get { return _path; }
            set { _path = value; }
        }

        private string _name;
        public string name
        {
            get { return _name; }
            set { _name = value; }
        }

        private string _size;
        public string size
        {
            get { return _size; }
            set { _size = value; }
        }

        private string _dir;
        public string dir
        {
            get { return _dir; }
            set { _dir = value; }
        }

        private bool _tagged;
        public bool tagged
        {
            get { return _tagged; }
            set { _tagged = value; }
        }

        private List<string> _fileTags;
        public List<string> fileTags
        {
            get { return _fileTags; }
            set { _fileTags = value; }
        }

        public FileView()
        {

        }

        public FileView(string path, string name, string size, string dir, bool tagged, List<string> fileTags)
        {
            this.path = path;
            this.name = name;
            this.size = size;
            this.dir = dir;
            this.tagged = tagged;
            this.fileTags = fileTags;
        }
    }
}
