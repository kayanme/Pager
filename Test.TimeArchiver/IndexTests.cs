using FakeItEasy;
using FakeItEasy.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TimeArchiver.Contracts;

namespace Test.TimeArchiver
{
    public partial class IndexTests
    {
        private UnorderedCallAssertion LastCall { get; set; }
        private IOrderableCallAssertion LastCall2 { get; set; }

        private void MakeContinuation(UnorderedCallAssertion call)
        {
            if (LastCall2 != null)
                LastCall2 = LastCall2.Then(call);
            else if (LastCall != null)
            {
                LastCall2 = LastCall.Then(call);
            }
            else
            {
                LastCall = call;
            }
        }

        private void dataBlockCreated(IndexRecord root)
        {
            MakeContinuation(A.CallTo(() => _indexInteraction.CreateDataBlock(root, recordAdded)).MustHaveHappened());           
        }

        private void dataBlockCreated()
        {
            MakeContinuation(A.CallTo(() => _indexInteraction.CreateDataBlock(recordAdded)).MustHaveHappened());
        }

        private void indexDowned(IndexRecord record)
        {
            MakeContinuation(A.CallTo(() => _indexInteraction.CreateUnderlayingIndexRecord(record)).MustHaveHappened());
        }

        private void indexResized(IndexRecord record,long newStart,long newEnd)
        {
            MakeContinuation( A.CallTo(() => _indexInteraction.ResizeIndex(record, newStart, newEnd)).MustHaveHappened());
        }

        private void indexMoved(IndexRecord root, IndexRecord recordToMove)
        {
            MakeContinuation(A.CallTo(() => _indexInteraction.MoveIndex(root, recordToMove)).MustHaveHappened());
        }

        private void indexSwapped(IndexRecord record1, IndexRecord record2)
        {
            MakeContinuation(A.CallTo(() => _indexInteraction.SwapIndexes(record1, record2)).MustHaveHappened());
        }

        private DataRecord<double>[] recordAdded { get => TestContext.Properties["ra"] as DataRecord<double>[]; set { TestContext.Properties["ra"] = value; } }

        [TestMethod]
        public async Task InsertIntoEmptyRoot()
        {
            
            await addBlock(1, 3);
            dataBlockCreated();            

        }

        [TestMethod]
        public async Task InsertIntoNotEmptyRoot()
        {
            indexTreeDataOnly(1,2);
            await addBlock(1, 3);
            indexDowned(data(1, 2));
            dataBlockCreated(index(1, 2));
            indexResized(index(1, 2), 1, 3);

        }

        [TestMethod]
        public async Task InsertIntoFullRoot()
        {
            indexTree(1, 3, 
                       data(1, 2),
                       data(2, 3));
            await addBlock(3, 4);
            indexDowned(data(2, 3));            
            dataBlockCreated(index(2, 3));
            indexResized(index(2, 3), 2, 4);

        }

        [TestMethod]
        public async Task InsertIntoFullRootOverlappingEqual()
        {
            indexTree(1, 3,
                       data(1, 2),
                       data(2, 3));
            await addBlock(1, 3);
            indexDowned(data(1, 2));            
            dataBlockCreated(index(1, 2));
            indexResized(index(1, 2), 1, 3);
        }

        [TestMethod]
        public async Task InsertIntoFullRootOverlappingNotEqual()
        {
            indexTree(1, 5,
                       data(1, 4),
                       data(4, 5));
            await addBlock(2, 5);
            indexDowned(data(1, 4));            
            dataBlockCreated(index(1, 4));
            indexResized(index(1, 4), 1, 5);
        }

        [TestMethod]
        public async Task InsertIntoSubIndex()
        {
            indexTree(1, 5,
                       data(1, 3),
                       index(3, 5,
                           data(3,4),
                           data(4,5)
                       )
                     );
            await addBlock(5, 6);

            
            
            indexDowned(data(4, 5));            
            dataBlockCreated(index(4, 5));
            indexResized(index(4, 5), 4, 6);
            indexResized(index(3, 5), 3, 6);
            indexDowned(data(1, 3)); 
            indexMoved(index(1, 3), data(3, 4));
            indexResized(index(1,3),1,4);
            indexResized(index(1, 5), 1, 6);

            //indexTree(1, 6,
            //          index(1, 4,
            //            data(1, 3),
            //            data(3, 4)
            //            ),
            //          index(4, 6,
            //              data(4, 5),
            //              data(5, 6)
            //          )
            //        );
        }


        [TestMethod]
        public async Task InsertIntoSubIndexLargeBlock()
        {
            indexTree(1, 5,
                       data(1, 3),
                       index(3, 5,
                           data(3, 4),
                           data(4, 5)
                       )
                     );
            await addBlock(2, 6);

         
            indexDowned(index(3, 5));
            dataBlockCreated(index(3, 5));
            indexResized(index(3, 5), 2, 6);
            indexDowned(data(1, 3));
            
            indexMoved(index(1, 3), data(2, 6));
            indexResized(index(1, 3), 1, 6);            
            indexResized(index(1, 5), 1, 6);
            //indexTree(1, 5,
            //           index(1, 6,
            //              data(1, 3),
            //              data(2, 6)
            //           ),
            //           index(3, 5,
            //               data(3, 4),
            //               data(4, 5)
            //           )
            //         );



        }
    }
}
