using System.Collections.Generic;
using System.IO.Paging.PhysicalLevel.Classes.References;

namespace System.IO.Paging.PhysicalLevel.Classes.Pages.Contracts
{
    public interface IPage<TRecordType> :IDisposable where TRecordType : struct
    {

        /// <summary>
        /// Adding new record to the page.
        /// </summary>
        /// <param name="newRecord"></param>
        /// <remarks>
        /// This action does not guarantee the disk persistence (in case of a process fail f.e.), 
        /// only its' availability to other readers.
        /// Use <see cref="IPhysicalRecordManipulation.Flush"/> to guarantee storing. 
        /// </remarks>
        /// <returns>Newly added record. Null, if the new added record exceeds the page capacity.</returns>
        TypedRecord<TRecordType> AddRecord(TRecordType newRecord);   
        /// <summary>
        /// Removes record from page. If it were already deleted - does nothing.
        /// </summary>
        /// <remarks>
        /// There are two issues to mention:
        /// - this operation swaps the reference of argument to reference to nothing, so you never get it back. However, other record copies will store previous reference.
        /// - there is no concurrency check there, unless for now (it is a special task).
        /// </remarks>
        /// <param name="recordToFree">Record to free.</param>
        void FreeRecord(TypedRecord<TRecordType> recordToFree);
        /// <summary>
        /// Retrieve record by it's reference. 
        /// </summary>
        /// <param name="recordReference">Reference to the record</param>
        /// <returns>Record or null, if it was deleted.</returns>
        /// <remarks>
        /// Notice, since a reference could be obtained only from an existing record 
        /// - the only way to have a null is to get a record after its' deletetion.
        /// </remarks>
        TypedRecord<TRecordType> GetRecord(PageRecordReference recordReference);
        /// <summary>
        /// Saves the record to file
        /// </summary>
        /// <param name="recordToStore">Record to replace.</param>
        /// <remarks>
        /// As with <see cref="AddRecord(TRecordType)"/> - no disk persistence guarantied. 
        /// Use <see cref="IPhysicalRecordManipulation.Flush"/> to guarantee writing.
        /// </remarks>
        void StoreRecord(TypedRecord<TRecordType> recordToStore);
        /// <summary>
        /// Returns a range of existing records from the first reference to the second inclusevly. 
        /// The range is acquired by a straight record scan through the page order.
        /// </summary>
        /// <param name="start">Record, from which start retrieving.</param>
        /// <param name="end">Record, on which stop retreiving.</param>
        /// <returns>Records between.</returns>
        /// <remarks>
        /// The method scans the page, and start returning records, when it finds the first one.
        /// Stops, when it finds the second.
        /// So there is a need of the first one been earlier, than the second one, 
        /// which is a matter of explicit order presence on page.
        /// If there is no applied - the order is just by it's physical layout.
        /// </remarks>
        IEnumerable<TypedRecord<TRecordType>> GetRecordRange(PageRecordReference start,PageRecordReference end); 
        /// <summary>
        /// Cycles through all records in page applied order.
        /// </summary>
        /// <returns>All existing records.</returns>
        /// <remarks>
        /// If logical order is applied - records are returning in that order. If not - by page layout.
        /// </remarks>
        IEnumerable<TypedRecord<TRecordType>> IterateRecords();
        

        void Flush();
    }
}