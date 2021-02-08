using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cutting
{
    public class Program
    {
        static void Main()
        {
            var material1 = new Product("1");
            var sourceBatch1 = new Batch("plavka1", material1);
            var materialBatch1 = new Batch("te1", material1, 35, sourceBatch1);
            var sourceBatch2 = new Batch("plavka2", material1);
            var materialBatch2 = new Batch("te2", material1, 30, sourceBatch2);
            List<Batch> materialBatches = new List<Batch>() { materialBatch1, materialBatch2 };

            var product = new Product("2", 10, 20, material1.id);
            var productBatch1 = new Batch("det1", product, 1);
            var productBatch2 = new Batch("det2", product, 3);

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1, productBatch2 };
            PrintResult(Calc(productBatches, materialBatches));
            Console.Read();
        }

        public static Reserve[] Calc(IEnumerable<Batch> productBatches, IEnumerable<Batch> materialBatches)
        {
            List<Reserve> reserves = new List<Reserve>();
            var sourcesDic = materialBatches.ToLookup(x => x.sourceBatch);
            var materialDic = sourcesDic.ToLookup(x => x.Key.product.id);
            foreach (var productBatch in productBatches.OrderByDescending(b => b.Required))
            {
                foreach (var meltBatches in materialDic[productBatch.product.material].OrderByDescending(m => m.Sum(b => b.NotReserved)))
                {
                    if (CheckMelt(meltBatches, productBatch))
                    {
                        foreach (var materialBatch in meltBatches.OrderBy(b => b.id))
                            if (materialBatch.NotReserved >= productBatch.product.len && productBatch.NotProvided >0)
                            {
                                var reserve = new Reserve(productBatch, materialBatch);
                                reserves.Add(reserve);
                            }
                        break;
                    }
                }
            }
            return reserves.ToArray();
        }

        private static void PrintResult(IEnumerable<Reserve> reserves)
        {
            //Console.WriteLine(reserves);
            foreach (var reserve in reserves)
                Console.WriteLine("productBatch.id={0} materialBatch.id={1} productQuantity={2}", reserve.productBatch.id, reserve.materialBatch.id, reserve.productQuantity);
        }

        private static bool CheckMelt(IEnumerable<Batch> materialBatches, Batch productBatch)
        {
            int req = productBatch.quantity;
            foreach (var batch in materialBatches)
            {
                int qnt = batch.NotReserved / productBatch.product.len;
                req -= qnt;
                if (req <= 0)
                    return true;
            }
            return false;
        }
    }

    public class Reserve
    {
        public Batch productBatch;
        public Batch materialBatch;
        public readonly int productQuantity;

        public Reserve(Batch productBatch, Batch materialBatch)
        {
            this.productBatch = productBatch;
            this.materialBatch = materialBatch;
            int availableQnt = materialBatch.NotReserved / productBatch.product.len;
            this.productQuantity = Math.Min(productBatch.NotProvided, availableQnt);

            materialBatch.Reserved += productQuantity * productBatch.product.len;
            productBatch.Provided += productQuantity;
        }
    }

    public class Workcenter
    {
        public List<Batch> batches = new List<Batch>();
        public Workcenter()
        {

        }
    }

    public class Batch
    {
        public string id;
        public Product product;
        public readonly int quantity;
        private int reserved;
        private int provided;
        public Batch sourceBatch;
        public int Required
        {
            get => product.len * quantity;
        }
        public int Reserved
        {
            get => reserved;
            set
            {
                reserved = value;
                NotReserved = quantity - reserved;
            }
        }
        public int Provided
        {
            get => provided;
            set
            {
                provided = value;
                NotProvided = quantity - provided;
            }
        }
        public Batch(string id, Product product, int quantity = 0, Batch sourceBatch = null)
        {
            this.id = id;
            this.product = product;
            this.quantity = quantity;
            Reserved = 0;
            Provided = 0;
            NotReserved = quantity;
            NotProvided = quantity;
            this.sourceBatch = sourceBatch;
        }

        public int NotReserved { get; private set; }
        public int NotProvided { get; private set; }

        public List<Reserve> Reserves;
    }

    public class Product
    {
        public readonly string id;
        public int len;
        public int pieces;
        public string material;

        public Product(string id, int len = 0, int pieces = 0, string material = null)
        {
            this.id = id;
            this.len = len;
            this.pieces = pieces;
            this.material = material;
        }
    }
}
