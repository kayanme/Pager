﻿namespace TimeArchiver.Contracts
{
    internal interface IDataPageInteractor<T> where T:struct
    {
        DataRecord<T>[] FindRange(DataPageRef dataPage, long start, long end);
        DataRecord<T> FindClosestLeft(DataPageRef dataPage, long stamp);
        DataPageRef CreateDataBlock(DataRecord<T>[] records);
    }
}
