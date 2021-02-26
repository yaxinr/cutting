using System;
using System.Collections.Generic;
using System.Linq;

namespace Cutting
{
    public class Program
    {
        static void Main()
        {
            var material1 = new Product("1");
            //var sourceBatch1 = new Batch("plavka1", material1, 80);
            var materialBatch1 = new Batch("te1", material1, 80, 10, 0, DateTime.Today, "plavka1");
            //var sourceBatch2 = new Batch("plavka2", material1, 80);
            var materialBatch2 = new Batch("te2", material1, 80, 2, 0, DateTime.Today, "plavka1");
            List<Batch> materialBatches = new List<Batch>() { materialBatch1, materialBatch2 };

            var product = new Product("2", 10, 20, material1.id);
            var productBatch1 = new Batch("det1", product, 80, 1);
            var productBatch2 = new Batch("det2", product, 80, 3);

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1, productBatch2 };
            PrintResult(Calc(productBatches, materialBatches));
            Console.Read();
        }

        public static Reserve[] Calc(IEnumerable<Batch> productBatches, IEnumerable<Batch> materialBatches)
        {
            List<Reserve> reserves = new List<Reserve>();
            var materialBatchesByMaterial = materialBatches.GroupBy(b => b.product.id)
                .ToLookup(x => x.Key, x => x.ToLookup(b => b.melt));
            foreach (var productBatch in productBatches.OrderBy(b => b.deadline).ThenByDescending(b => b.RequiredLen))
                foreach (var materialBatchesForProductBatch in materialBatchesByMaterial[productBatch.product.material])
                    foreach (var meltBatches in materialBatchesForProductBatch.OrderBy(m => m.Sum(b => b.NotReserved)).ToArray())
                        if (CheckMelt(meltBatches, productBatch.quantity, productBatch.product.len))
                        {
                            foreach (var materialBatch in meltBatches.OrderBy(b => b.NotReserved).ToArray())
                                if (materialBatch.NotReserved >= productBatch.product.len && productBatch.NotProvided > 0)
                                {
                                    var reserve = new Reserve(productBatch, materialBatch);
                                    reserves.Add(reserve);
                                }
                            break;
                        }
            return reserves.ToArray();
        }

        private static void PrintResult(IEnumerable<Reserve> reserves)
        {
            //Console.WriteLine(reserves);
            foreach (var reserve in reserves)
                Console.WriteLine("productBatch.id={0} materialBatch.id={1} productQuantity={2}", reserve.productBatch.id, reserve.materialBatch.id, reserve.productQuantity);
        }

        public static bool CheckMelt(IEnumerable<Batch> materialBatches, int required, int len)
        {
            foreach (var materialBatch in materialBatches)
            {
                int qnt = materialBatch.NotReserved / len;
                required -= qnt;
                if (required <= 0)
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
            if (productBatch.deadline < materialBatch.deadline)
                materialBatch.deadline = productBatch.deadline;
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
        public readonly int kg;
        public DateTime deadline = DateTime.MaxValue;
        public int place_id;
        public string location_id;
        private int reserved;
        private int provided;
        public string melt;
        public int NotReserved { get; private set; }
        public int NotProvided { get; private set; }
        public int RequiredLen
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
        public Batch(string id, Product product, int place_id, int quantity = 0, int kg = 0, DateTime? deadline = null, string melt = null, string location_id = null)
        {
            this.id = id;
            this.product = product;
            this.quantity = quantity;
            this.kg = kg;
            Reserved = 0;
            Provided = 0;
            NotReserved = quantity;
            NotProvided = quantity;
            this.melt = melt;
            this.deadline = deadline ?? DateTime.MaxValue;
            this.place_id = place_id;
            this.location_id = location_id;
        }

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
