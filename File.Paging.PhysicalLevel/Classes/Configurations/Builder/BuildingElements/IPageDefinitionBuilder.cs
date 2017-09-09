namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
{
    public interface IPageDefinitionBuilder
    {
        IPageRecordTypeBuilder<TRecordType> AsPageWithRecordType<TRecordType>() where TRecordType : struct;
    }
}