namespace System.IO.Paging.PhysicalLevel.Configuration.Builder.BuildingElements
{
   
    public interface IPageRecordTypeBuilder<TRecordType> where TRecordType : struct
    {
        IFixedPageBuilder<TRecordType> UsingRecordDefinition(IFixedSizeRecordDefinition<TRecordType> recordDefinition);
        IFixedPageBuilder<TRecordType> UsingRecordDefinition(Getter<TRecordType> fillBytes,
            Setter<TRecordType> fillFromBytes, int size);

      
        IVariablePageBuilder<TRecordType> UsingRecordDefinition(IVariableSizeRecordDefinition<TRecordType> recordDefinition);
        IVariablePageBuilder<TRecordType> UsingRecordDefinition(Getter<TRecordType> fillBytes,
            Setter<TRecordType> fillFromBytes, Func<TRecordType, int> size);
    }
}