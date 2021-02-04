using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cutting
{
    class Program
    {
        static void Main(string[] args)
        {
            Seed();
            //cut();
            Console.Read();
        }

        private static void Seed()
        {
            var material1 = new Product(1);
            var sourceBatch1 = new Batch(material1);
            var materialBatch1 = new Batch(material1, 45, sourceBatch1);
            var sourceBatch2 = new Batch(material1);
            var materialBatch2 = new Batch(material1, 30, sourceBatch2);

            var product = new Product(2, 10, 20, material1);
            var productBatch1 = new Batch(product, 1);
            var productBatch2 = new Batch(product, 4);
            var workcenter = new Workcenter();

            List<Batch> productBatches = new List<Batch>() { productBatch1, productBatch2 };
            List<Batch> materialBatches = new List<Batch>() { materialBatch1, materialBatch2 };
            var sourcesDic = materialBatches.ToLookup(x => x.sourceBatch);
            var materialDic = sourcesDic.ToLookup(x => x.Key.product);


            List<Reserve> reserves = new List<Reserve>();

            foreach (var productBatch in productBatches)
            {
                foreach (var melt in materialDic[productBatch.product.material].OrderBy(b => b.Key.id))
                {
                    if (CheckMelt(melt, productBatch))
                    {
                        foreach (var materialBatch in melt.OrderBy(b => b.id))
                        {
                            var reserve = new Reserve(productBatch, materialBatch);
                            reserves.Add(reserve);
                        }
                        break;
                    }
                }
                //if (productBatch.NotProvided > 0)
                //    foreach (var materialBatch in materialBatches)
                //        if (materialBatch.NotReserved >= productBatch.product.len)
                //        {
                //            var reserve = new Reserve(productBatch, materialBatch);
                //            reserves.Add(reserve);
                //        }
            }
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

    internal class Reserve
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

    internal class Workcenter
    {
        public List<Batch> batches = new List<Batch>();
        public Workcenter()
        {

        }
    }

    internal class Batch
    {
        static int count;
        public int id;
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
        public Batch(Product product, int quantity = 0, Batch sourceBatch = null)
        {
            id = count++;
            this.product = product;
            this.quantity = quantity;
            Reserved = 0;
            Provided = 0;
            this.sourceBatch = sourceBatch;
        }

        public int NotReserved { get; private set; }
        public int NotProvided { get; private set; }

        public List<Reserve> Reserves;
    }

    internal class Product
    {
        readonly int id;
        public int len;
        int seconds;
        public Product material;

        public Product(int id, int len = 0, int seconds = 0, Product material = null)
        {
            this.id = id;
            this.len = len;
            this.seconds = seconds;
            this.material = material;
        }
    }
}
