using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tagger.Model
{
    public class TaggerInstance
    {
        private string _baseDir;
        public string baseDir
        {
            get { return _baseDir; }
            set { _baseDir = value; }
        }

        private List<String> _extensionList;
        public List<string> extensionList
        {
            get { return _extensionList; }
            set { _extensionList = value; }
        }

        private FileViewCollection _filesView;
        public FileViewCollection filesView
        {
            get { return _filesView; }
            set { _filesView = value; }
        }

        private TagViewCollection _tagsView;
        public TagViewCollection tagsView
        {
            get { return _tagsView; }
            set { _tagsView = value; }
        }

        private TaggerInstanceInfo _info;
        public TaggerInstanceInfo info
        {
            get { return _info; }
            set { _info = value; }
        }

        public TaggerInstance()
        {

        }

        public TaggerInstance(string baseDir, List<string> extensionList)
        {

        }
    }

    public class TaggerInstanceInfo
    {
        private bool _unsaved;
        public bool unsaved
        {
            get { return _unsaved; }
            set { _unsaved = value; }
        }

        private int _totalFiles;
        public int totalFiles
        {
            get { return _totalFiles; }
            set { _totalFiles = value; }
        }

        private int _totalTaggedFiles;
        public int totalTaggedFiles
        {
            get { return _totalTaggedFiles; }
            set { _totalTaggedFiles = value; }
        }

        private int _totalTags;
        public int totalTags
        {
            get { return _totalTags; }
            set { _totalTags = value; }
        }
    }


}
