using Microsoft.VisualStudio.TestTools.UnitTesting;
using TimeArchiver.Contracts;
using FakeItEasy;
using TimeArchiver.Classes;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using System.Linq;

[assembly:InternalsVisibleTo("DynamicProxyGenAssembly2")]
namespace Test.TimeArchiver
{

    [TestClass]
    public partial class IndexTests
    {

        public TestContext TestContext { get; set; }

        private MockIndexInteraction _indexInteraction
        {
            get =>
            
                TestContext.Properties["ii"] as MockIndexInteraction;
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

        private IndexRecord index(long start, long end) => new IndexRecord { Start = start, End = end, StoresData = false};

        private IndexRecord data(long start, long end) => new IndexRecord { Start = start, End = end, StoresData = true, TestKey = _indexInteraction.newKey() };

        private int TestIndexCapacity => int.Parse(TestContext.Properties.ContainsKey("IndexCapacity") ? (string)TestContext.Properties["IndexCapacity"]:"2");

        private IndexRecord index(long start,long end,params IndexRecord[] children)
        {
            var ind = index(start, end);
            ind.MaxUnderlyingDepth = (short)(children.Max(k => k.MaxUnderlyingDepth) + 1);
            ind.TestKey = _indexInteraction.newKey();
            _indexInteraction.AddChildren(ind,children);
            return ind;
        }

        private void indexTree(long start, long end, params IndexRecord[] chilrden)
        {
            var root = index(start, end, chilrden);            
            _indexInteraction.SetRoot(root);
        }

        private void indexTreeDataOnly(long start, long end)
        {
            var root = data(start, end);            
            _indexInteraction.SetRoot(root);
        }

        private async Task addBlock(long start,long end)
        {
            var db = dataBlock(start, end);          
            recordAdded = db;         
            await _dataInteraction.AddBlock(db);

        }

        [TestInitialize]
        public void Initialize()
        {
            _indexInteraction = A.Fake<MockIndexInteraction>(c=>c.CallsBaseMethods());       
            
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
