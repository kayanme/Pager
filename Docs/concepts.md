# Core principles

## Pages organization 

A page storage consists of equally-sized pages, which hold various types of records.
Every page, when an access is needed, is loaded into the memory and is unloaded when there is no need in it.
For an access speed optimization pages are loaded in extents, holding multiple consecutive pages. 
Each page type and occupation status is written in a Global Allocation Map (GAM).

## Records organization

Each page (if it is not processed like a blob) contains some amount of records depending on their and page size.
Pages can hold two different sets of records (only one set for page):
- fixed size records (the size is predetermined in page configuration)
- variable size records (the size is calculated during record processing)

Also page can have an applied order of records.

So there are four types of records organization, which have different methods of record allocation.
Allocation map is called Page Allocation Map (PAM) and is differs through all these types.
Actually PAM is a section at the beginning or at the end of a page.

### Fixed size, no order.
The simpliest, the fastest and the most space-preserving case.
Because of fixed size we definitly know the position of each record on page, so there is no need in more additional info (except occupation flag).
So PAM is at the start of page and takes fixed space, holding record occupation bits.
Actually,

PageSizeInBytes = (SizeOfRecordInBytes*TotalRecordCount) + TotalRecordCount/8 (one bit for each record)

### Fixed size, order applied.
As we can not manipulate order moving all records on page back and forth (it is too consuming), we should store the logical order separately.
That is - an array, which holds logical order number according to physical position of each record.
F.e., PAM, can look like that:

|Physical pos.|1|2|3 |4|5|6 |
|-------------|-|-|--|-|-|--|
|Logical order|3|1|-1|2|4|-1|

Which means, that the order of records (their physical position) should be:
2,4,1,5

The 3rd and 6th records are considered free.

Because of additional space needed for order:
PageSizeInBytes = (SizeOfRecordInBytes*TotalRecordCount) + TotalRecordCount

PAM is at the beginning either.

Both for all fixed-size types - we definitly know position and size of PAM, so it's processing is rather quick.

### Variable size, no order.

Here we should mention three more things.
1. We should store the size of every record.
2. We do not know exact position of each and the total count.
3. There may be a fragmentation because of adding and freeing records of different sizes.

The solution is to store PAM at the end of the page (allocating records from the start), fixing physical shift ans size in it, for each record.
Record added - one more slot is used, until there is no space at the page.

Of course, inserting is a more complex task, because we cannot simply insert at the first free position, but should check free size on it before.

### Variable size, order applied.

This case is not fully supported yet, but the concept is clear.
We are adding more space for storing logical index.

## Additional features for page

Page can have a fixed size header (at the beginning).
The one more additional feature is a locking system, which uses Addiction.System.Threading.HybridLocks to construct composition of shared and nonshared locks (go see Microsoft SQL Server to see how it may look like).

## Logical structure 

A logical pages can be built upon physical capabilities.
Currently, there are two logical page types.

### Continuous heap.

Looks like one infinite page to put records on it. It is a very good scenario for inserts with rare deletes.
Information about used physical pages is stored in additional service pages.

### Autosorting page.

SQL-index scenario. You define a key selector, which defines the logical order of records on page.
Because of natural support for such ordered pages is a binary search - you can do that searches rather quickly, without manual tossing these records.

