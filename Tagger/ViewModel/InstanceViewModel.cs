using Tagger.Model;

namespace Tagger.ViewModel
{
    public class InstanceViewModel : IGeneric
    {
        private TaggerInstanceInfo _info;
        public TaggerInstanceInfo info
        {
            get { return _info; }
            set { _info = value; }
        }
    }
}
