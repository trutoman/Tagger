using Tagger.Model;

namespace Tagger.ViewModel
{
    class TagsViewModel : IGeneric
    {
        private TagViewCollection _tagsList = new TagViewCollection();
        public TagViewCollection tagsList
        {
            get { return _tagsList; }
            set { _tagsList = value; }
        }

        private TagView _tagSelected = new TagView();
        public TagView tagSelected
        {
            get { return _tagSelected; }
            set { _tagSelected = value; }
        }

        public TagsViewModel()
        {

        }
    }

}