# About System.IO.Paging
This is a low-level framework allowing to build up an I/O  system based on file pages. 
The concept is basically the same, as the most databases use, so it can help you to build your own.

# Usage sample
Describe your pages first:

'''C#

internal class PageConfiguration:PageManagerConfiguration
    {
        public PageConfiguration() : base(PageSize.Kb8)
        {            
            DefinePageType(1)
                .AsPageWithRecordType<TestRecord>()
                .UsingRecordDefinition(new TestRecordGetter())
				.WithHeader(new TestHeaderGetter())
                .ApplyLogicalSortIndex();

            DefinePageType(2)
                .AsPageWithRecordType<TestRecord>()
                .UsingRecordDefinition(new TestRecordGetter())
                .ApplyLockScheme(new ReaderWriterLockRuleset());
        }
    }

'''

Than create a manager, which will use that configuration to work exclusevly with it's own file:

'''C#

 using (var factory = new PageManagerFactory())
 {
       _pageManager = factory.CreateManagerWithAutoFileCreation("fileWithPages", new PageConfiguration());
 }

 '''

 After that you can use this manager and accessors it provides to communicate with pages and their records:

 '''C#

 //here we create a page first to work with it
    var pageRef = _pageManager.CreatePage(2);
//let us change it's header
	var headerPage = _pageManager.GetHeaderAccessor<TestHeader>(pageRef);
	var header = headerPage.GetHeader();
	//...changing your header...
	headerPage.ModifyHeader(header);

//now we are gonna add and play with some previously created record.
	using(var page = _pageManager.GetRecordAccessor<TestRecord>(pageRef))
	{
		var recordWithReference = page.AddRecord(newRecord);
		//here we will have (on success) our record with it's persistent page reference.
		//if we can't insert a record due to page fulness - it will be null, so take care

		//change record somehow...
		page.StoreRecord(recordWithReference);

		//let us see if our record is still on place?
		var reacquiredRecord = page.GetRecord(recordWithReference.Reference);

		//we are tired, we are going to remove it
		page.FreeRecord(recordWithReference);
	}

'''

At last you can destroy the page itself.

'''C#

	_pageManager.DeletePage(pageRef);

'''

# Current features 
- 4Kb and 8Kb pages support.
- Definition of multiple page types in one file.
- Fixed (predefined) and variable record size.
- Applying logical sort to maintain some record order.
- Defining page headers.
- Managing simple and more complex lock schemes on pages and records.
- Ability to work with a page as a raw block (f.e. for some dumps).

In addition, some logical features are available.
- "Infinite" virtual page type.
- Autosorting (based on some record property) page type.

# Perfomance
Access speed varies due to the scenario, the latest bechmarks are published in docs section.

[![License: MIT](https://img.shields.io/badge/License-MIT-blue.svg)](LICENSE)
[![Build](https://img.shields.io/gitlab/pipeline/kayanme/System.IO.Paging.svg?style=flat)]
[![Test coverage](https://img.shields.io/coveralls/github/kayanme/System.IO.Paging.svg?style=flat)]
[![NuGet](https://img.shields.io/nuget/v/Addiction.System.IO.Paging.PhysicalLevel.svg)](https://www.nuget.org/packages/Addiction.System.IO.Paging.PhysicalLevel)
[![Nuget downloads](https://img.shields.io/nuget/dt/Addiction.System.IO.Paging.PhysicalLevel.svg?style=flat)]
[![Nuget downloads](https://img.shields.io/nuget/dt/Addiction.System.IO.Paging.LogicalLevel.svg?style=flat)]