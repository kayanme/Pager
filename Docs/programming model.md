#Page configuration

You should describe the configuration for a file, which will store your pages, by deriving from a PageManagerConfiguration.
Choose the desired page size (the same for each page type), and extent size (a page count in each).
Then describe in a fluent way each page type and the record it wil contain:

```C#
using System.IO.Paging.PhysicalLevel.Configuration;
using System.IO.Paging.PhysicalLevel.Configuration.Builder;

internal class SamplePageConfiguration:PageManagerConfiguration
    {
        public SamplePageConfiguration() : base(PageSize.Kb8,8)
        {
            DefinePageType(1)
                .AsPageWithRecordType<SampleRecord>()
                .UsingRecordDefinition(new SampleRecordGetter());

            DefinePageType(2)
                .AsPageWithRecordType<SampleRecord2>()
                .UsingRecordDefinition(new SampleRecordGetter2())
                .WithHeader(new SampleHeaderGetter());

            DefinePageType(3)
                .AsPageWithRecordType<SampleRecord>()
                .UsingRecordDefinition(new SampleRecordGetter())
                .ApplyLogicalSortIndex();


            DefinePageType(4)
                .AsPageWithRecordType<SampleRecord>()
                .UsingRecordDefinition(new SampleRecordGetter())
                .ApplyLockScheme(new ReaderWriterLockRuleset());

	        DefinePageType(5)
                .AsPageWithRecordType<PageImage>()
                .AsPlainImage(new PageImageProvider());
        }
    }
```

Here we see four page types, which can be arranged in one file.
There records can be the same (such as on page 1 and 3) or different types.
Each one should declare a provider, which should define how the record is read and stored from and to byte array.

## Record providers

There are different types of provider declaration:

### Fixed size

Declare realization of IFixedSizeRecordDefinition<>.


```C#
using System.IO.Paging.PhysicalLevel.Configuration.Builder;
public class SampleRecordGetter : IFixedSizeRecordDefinition<SampleRecord>
    {
        public  void FillBytes(ref SampleRecord record,byte[] targetArray)
        {
           RecordUtils.ToBytes(ref record.Value, targetArray,0);

        }

        public  void FillFromBytes(byte[] sourceArray,ref SampleRecord record)
        {

            RecordUtils.FromBytes(sourceArray, 0, ref record.Value);

        }

        public int Size => sizeof(long);
    }
```

Here you should describe the serialization of your record class, deserialization - and declare the size of each record.
Byte arrays, that will come to your methods, will be strictly the size you defined in Size property.
Avoid resizing that array by yourself. You will be punished for that.

You can use lambdas right in configuration description instead of an interface realisation, if you want.

### Variable size

Declare realization of IVariableSizeRecordDefinition<>.


```C#
using System.IO.Paging.PhysicalLevel.Configuration.Builder;
public class SampleRecordGetter : IVariableSizeRecordDefinition<SampleRecord>
    {
        public  void FillBytes(ref SampleRecord record,byte[] targetArray)
        {
           RecordUtils.ToBytes(ref record.Text, targetArray,0);

        }

        public  void FillFromBytes(byte[] sourceArray,ref SampleRecord record)
        {

            RecordUtils.FromBytes(sourceArray, 0, ref record.Text);

        }

        public int Size(SampleRecord record) =>Encoding.UTF8.GetByteCount(record.Text);
    }

Serialization and deserialization is pretty the same, but the size of record (and so target array in FillBytes) is calculating each time.
The size for a FillFromBytes is extracted from record itself, so just be sure, that you will decode that bytes correctly.

As with fixed record, you can use lambdas in a configuration description.

### Plain image.

You can use this option to access a page like a whole an array, managing all content layout by yourself.
You will store and retrieve the whole page through (de-)serializing an array of a page size.
This scenario can be good enough for large preformed pages (such as archives), which you will read and extract by special algorithms.
For a random access consider using a common per-record technique - it is much more convinient and pretty fast.

#### A word about providers

The complete read-write algorithm will be described below, but keep in mind, that these operations a crucial for performance.
Avoid of fill and read of a target arrays byte-per-byte, try to access your record as a whole memory block, if you can.
You can use RecordUtils wrapper upon unsafe operations, or use more modern Span's and Memory's.
Also you can make internal decisions about record type (which can lead to a serialization to a derived types).

The same is for size calculating. Every record storing will calculate this size, so avoid heavy operations there.

## Additional page features.

After record declaration - the main thing, that you came for into a page interaction - you can define addional options.

### Page header.

It is just a fixed size record at the beginning of the page, so it requires a common fixed size record provider.
You can not add or remove it, it will always be and occupy it's place.
If you want a variable size header - consider use of a variable sized record page with a fixed size header referring to the target record.

### Logical sort index

With this feature you will gain ability to put an explicit order for records on your page and thus made a binary search operation upon it.
The other way the search will be based just upon physical order of records on your page (which you can control only be swapping content of records manually).
You will lost some potential space with this ability due to need of storing order (see concepts).

### Locks

You can apply some lock scheme to the page. It is not automatic (you should gain and release required locks by yourself), but is a rather descriptive and can hold concurrency scenarios without additional infrastructure.
The lock scheme can be simple exlusive lock or a complex, such as [T-SQL](https://docs.microsoft.com/ru-ru/sql/relational-databases/media/lockconflicttable.png?view=sql-server-2017)

# Page manager

The entry point is page manager creation, with a PageManagerFactory.
