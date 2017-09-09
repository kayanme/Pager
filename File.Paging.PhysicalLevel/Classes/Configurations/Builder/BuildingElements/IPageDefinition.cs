using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    internal class PageDefinitionBuilder: IPageDefinitionBuilder
    {
        protected readonly PageManagerConfiguration _config;
        internal PageManagerConfiguration Config => _config;
        protected readonly byte _pageNum;
        internal byte PageNum => _pageNum;
      
        public PageDefinitionBuilder(PageManagerConfiguration config, byte pageNum)
        {
            _config = config;
            _pageNum = pageNum;
        }

      

        public IPageRecordTypeBuilder<TRecordType> AsPageWithRecordType<TRecordType>() where TRecordType:struct
        {
            var t = new PageDefinitionBuilder<TRecordType>(_config, _pageNum);
            return t;
        }     
    }
}
