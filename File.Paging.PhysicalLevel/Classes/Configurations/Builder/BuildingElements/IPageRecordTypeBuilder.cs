using System;

namespace File.Paging.PhysicalLevel.Classes.Configurations.Builder
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