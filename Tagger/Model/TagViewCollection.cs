using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tagger.Model
{
    public class TagViewCollection : ObservableCollection<TagView>
    {

    }

    public class TagView
    {
        private string _id;
        public string id
        {
            get { return _id; }
            set { _id = value; }
        }

        private string _imagePath;
        public string imagePath
        {
            get { return _imagePath; }
            set { _imagePath = value; }
        }

        private string _group;
        public string group
        {
            get { return _group; }
            set { _group = value; }
        }

        public TagView()
        {

        }

        public TagView(string id, string imagePath, string group)
        {
            this.id = id;
            this.imagePath = imagePath;
            this.group = group;
        }
    }
}
