using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Configuration.Builder.BuildingElements;
using System.Runtime.CompilerServices;

namespace System.IO.Paging.PhysicalLevel.Configuration
{


    public class PageManagerConfiguration
    {

        internal Dictionary<byte, PageContentConfiguration> PageMap = new Dictionary<byte, PageContentConfiguration>();
        internal Dictionary<byte, PageHeadersConfiguration> HeaderConfig = new Dictionary<byte, PageHeadersConfiguration>();

        public enum PageSize { Kb4 = 4*1024, Kb8 = 8 * 1024 }

        internal PageSize SizeOfPage { get; set; }
        internal int ExtentSize { get; set; }
        [MethodImpl(MethodImplOptions.Synchronized)]
        protected IPageDefinitionBuilder DefinePageType(byte num)
        {
            if (PageMap.ContainsKey(num))
                throw new InvalidOperationException("Page with such number where already registered");

            PageMap.Add(num,null);
            return new PageDefinitionBuilder(this,num);
        }

        internal PageManagerConfiguration()
        {           
        }

        protected PageManagerConfiguration(PageSize sizeOfPage) : this(sizeOfPage,64)
        {
            
        }

        protected PageManagerConfiguration(PageSize sizeOfPage,int extentSizeInKb)
        {
            ExtentSize = extentSizeInKb*1024;
            SizeOfPage = sizeOfPage;
        }


        internal void Verify()
        {
            foreach (var pageContentConfiguration in PageMap)
            {
              
                if (pageContentConfiguration.Value ==null)
                    throw new ArgumentException($"Page type configuration {pageContentConfiguration.Key} is incomplete");
                pageContentConfiguration.Value.Verify();
            }
        }
    }

   

   

   
  

  
}
