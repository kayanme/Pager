namespace System.IO.Paging.PhysicalLevel.Configuration.Builder.BuildingElements
{
    public interface IPageDefinitionBuilder
    {
        IPageRecordTypeBuilder<TRecordType> AsPageWithRecordType<TRecordType>() where TRecordType : struct;
    }
}