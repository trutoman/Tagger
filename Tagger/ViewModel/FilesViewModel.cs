using Tagger.Model;

namespace Tagger.ViewModel
{
    public class FilesViewModel : IGeneric
    {
        private FileViewCollection _fileList = new FileViewCollection();
        public FileViewCollection fileList
        {
            get { return _fileList; }
            set { _fileList = value; }
        }

        private FileView _fileSelected = new FileView();
        public FileView fileSelected
        {
            get { return _fileSelected; }
            set { _fileSelected = value; }
        }

        public FilesViewModel()
        {

        }
    }
}
