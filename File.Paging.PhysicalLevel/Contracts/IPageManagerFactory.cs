using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pager
{
    public interface IPageManagerFactory
    {
        IPageManager CreateManager(string fileName,PageManagerConfiguration configuration, bool createFileIfNotExists);
    }
}
