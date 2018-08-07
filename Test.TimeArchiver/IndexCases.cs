using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeArchiver.Contracts;
using FakeItEasy;
using System.Linq;
using TimeArchiver.Classes;
using System.Threading.Tasks;

namespace Test.TimeArchiver
{
    [TestClass]
    public partial class IndexTests
    {

        public TestContext TestContext { get; set; }

        private IIndexInteraction<double> _indexInteraction
        {
            get =>
            
                TestContext.Properties["ii"] as IIndexInteraction<double>;
            set
            {
                TestContext.Properties["ii"] = value;
            }
        }

        private IDataInteraction<double> _dataInteraction
        {
            get =>

                TestContext.Properties["di"] as IDataInteraction<double>;
            set
            {
                TestContext.Properties["di"] = value;
            }
        }

        private DataRecord<double>[] dataBlock(long start, long end) => new[] {
            new DataRecord<double> {Data = start,Stamp = start },
            new DataRecord<double> {Data = end,Stamp = end }};

        private void CheckResult(params long[] data) { }

        private IndexRecord index(long start, long end,int depth) => new IndexRecord { Start = start, End = end, StoresData = false,MaxUnderlyingDepth = depth };

        private IndexRecord data(long start, long end) => new IndexRecord { Start = start, End = end, StoresData = true };

        private int TestIndexCapacity => int.Parse(TestContext.Properties.ContainsKey("IndexCapacity") ? (string)TestContext.Properties["IndexCapacity"]:"2");

        private IndexRecord index(long start,long end,params IndexRecord[] chilrden)
        {
            var ind = index(start, end);
            ind.MaxUnderlyingDepth =chilrden.Any()? chilrden.Max(k => k.MaxUnderlyingDepth) + 1:0;
            A.CallTo(() => _indexInteraction.GetChildren(ind)).Returns(chilrden);
            A.CallTo(() => _indexInteraction.IsChildrenCapacityFull(ind)).Returns(chilrden.Length == TestIndexCapacity);
            return ind;
        }

        private void indexTree(long start, long end, params IndexRecord[] chilrden)
        {
            var root = index(start, end, chilrden);
            A.CallTo(() => _indexInteraction.GetRoot()).Returns(root);
        }

        private void indexTreeDataOnly(long start, long end)
        {
            var root = data(start, end);
            A.CallTo(() => _indexInteraction.GetRoot()).Returns(root);
        }

        private async Task addBlock(long start,long end)
        {
            var db = dataBlock(start, end);
            recordAdded = db;
            A.CallTo(() => _indexInteraction.CreateDataBlock(A<IndexRecord>.Ignored, recordAdded)).Returns(index(start,end));
            await _dataInteraction.AddBlock(db);

        }

        [TestInitialize]
        public void Initialize()
        {
            _indexInteraction = A.Fake<IIndexInteraction<double>>();
            A.CallTo(() => _indexInteraction.CreateUnderlayingIndexRecord(A<IndexRecord>.Ignored))  
                .Invokes((IndexRecord ind)=>A.CallTo(()=>_indexInteraction.GetChildren(index(ind.Start, ind.End,ind.MaxUnderlyingDepth+1))).Returns(new[] { ind }))
                .ReturnsLazily((IndexRecord ind) => index(ind.Start, ind.End,ind.MaxUnderlyingDepth+1));

            A.CallTo(() => _indexInteraction.ResizeIndex(A<IndexRecord>.Ignored, A<long>.Ignored, A<long>.Ignored))
                .Invokes((IndexRecord ind, long l, long b) => A.CallTo(() => _indexInteraction.GetChildren(index(l,b))).Returns(_indexInteraction.GetChildren(ind)))
                .ReturnsLazily((IndexRecord i,long l,long b) => index(l, b,i.MaxUnderlyingDepth));

            A.CallTo(() => _indexInteraction.MoveIndex(A<IndexRecord>.Ignored, A<IndexRecord>.Ignored))
                .Invokes((IndexRecord root, IndexRecord moved) =>
                {
                   A.CallTo(() => _indexInteraction.GetChildren(root)).Returns( _indexInteraction.GetChildren(root).Concat( new[] { moved }).ToArray());
                    A.CallTo(() => _indexInteraction.GetChildren(root)).Returns(_indexInteraction.GetChildren(root).Concat(new[] { moved }).ToArray());
                })
                .ReturnsLazily((IndexRecord root,IndexRecord moved) => index(moved.Start, moved.End));

            

            _dataInteraction = new DataInteraction<double>(_indexInteraction);
      
        }

        [TestMethod]        
        public void Case1()
        {
            indexTree(1,3,
                        data(1,3)
                );
            _dataInteraction.AddBlock(
                   dataBlock(4,5)
                );


            indexTree(1, 5,
                        data(1, 3),
                        data(4, 5)
                );
        }

        [TestMethod]
        public void Case2()
        {
            indexTree(1, 3,
                        data(1, 3)
                );
            _dataInteraction.AddBlock(
                   dataBlock(1, 3)
                );


            indexTree(1, 3,
                        data(1, 3),
                        data(1, 3)
                );
        }


        [TestMethod]        
        public void Case3()
        {
            indexTree(1, 5,
                        index(1, 3,
                           data(1, 2),
                           data(2, 3)
                        ),
                        index(3, 5,
                           data(3, 4),
                           data(4, 5)
                        )
                );
            _dataInteraction.AddBlock(
                   dataBlock(2, 4)
                );


            indexTree(1, 5,
                        index(1, 3,
                           data(1, 2),
                           data(2, 3),
                           data(2, 4)
                        ),
                        index(3, 5,
                           data(2, 4), //<-тот же, что в другом индексе
                           data(3, 4),
                           data(4, 5)
                        )
                );
        }

        [TestMethod]
        [TestProperty("IndexCapacity", "4")]
        public void Case4()
        {
            indexTree(1, 5,
                        data(1, 2),
                        data(2, 3),
                        data(3, 4),
                        data(4, 5)
                     );
            _dataInteraction.AddBlock(
                   dataBlock(2, 4)
                );


            indexTree(1, 5,
                        index(1, 2,
                           data(1, 2)
                        ),
                        index(2, 4,
                           data(2, 3),
                           data(3, 4),
                           data(2, 4)
                        ),                       
                        index(4, 5,
                           data(4, 5)
                        )
                );
        }


        [TestMethod]
        [TestProperty("IndexCapacity", "4")]
        public void Case5()
        {
            indexTree(1, 5,
                        data(1, 2),
                        data(2, 3),
                        data(3, 4),
                        data(4, 5)
                     );
            _dataInteraction.AddBlock(
                   dataBlock(1, 5)
                );


            indexTree(1, 5,
                        index(1, 5,
                           data(1, 2),
                           data(2, 3),
                           data(1, 5)

                        ),
                        index(1, 5,
                           data(1, 5),//<-- тот же блок, что и в пред. индексе
                           data(3, 4),
                           data(4, 5)
                        )
                );
        }

        [TestMethod]
        [TestProperty("IndexCapacity", "4")]
        public void Case6()
        {
            indexTree(1, 5,
                        data(1, 2),
                        data(2, 3),
                        data(3, 4),
                        data(4, 5)
                     );
            _dataInteraction.AddBlock(
                   dataBlock(5, 6)
                );


            indexTree(1, 5,
                        data(1, 2),
                        data(2, 3),
                        data(3, 4),
                        index(4, 6,
                          data(4, 5),
                          data(5, 6)
                        )
                );
        }

        [TestMethod]        
        public void Case7()
        {
            indexTree(1, 5,
                        index(1, 3,
                           data(1,2),
                           data(2,3)
                           ),
                        index(3, 5,
                           data(3, 4),
                           data(4, 5)
                        )
                     );
            _dataInteraction.AddBlock(
                   dataBlock(5, 6)
                );


            indexTree(1, 5,
                        index(1, 3,
                           data(1, 2),                           
                           data(2, 3)                            
                        ),
                        index(3, 6,
                           data(3, 4),
                           index(4,6,
                             data(4, 5), 
                             data(5, 6)
                           )
                        )
                     );
        }


        [TestMethod]
        [TestProperty("IndexCapacity", "2")]
        public void Case8()
        {
            indexTree(1, 5,
                        index(1, 3,
                           data(1, 2),
                           data(2, 3)
                           ),
                        index(3, 6,
                           data(3, 4),
                           data(4, 6)
                        )
                     );
            _dataInteraction.AddBlock(
                   dataBlock(5, 8)
                );


            indexTree(1, 5,
                        index(1, 4,
                           data(1, 2),
                           index(2, 4,
                             data(2, 3),
                             data(3, 4)
                           )
                        ),                        
                        index(4, 8,
                             data(4, 6),
                             data(5, 8)
                        )                          
                     );
        }

    }
}
