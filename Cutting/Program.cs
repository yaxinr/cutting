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
            var product = new Product(1, 10, 20);
            var productBatch1 = new Batch(product, 1);
            var productBatch2 = new Batch(product, 4);
            var material = new Product(1, 1, 10);
            var materialBatch1 = new Batch(material, 30);
            var workcenter = new Workcenter();

            List<Batch> productBatches = new List<Batch>() { productBatch1, productBatch2 };
            List<Batch> materialBatches = new List<Batch>() { materialBatch1 };
            List<Reserve> reserves = new List<Reserve>();

            foreach (var productBatch in productBatches)
                if (productBatch.NotProvided > 0)
                    foreach (var materialBatch in materialBatches)
                        if (materialBatch.NotReserved >= productBatch.product.len)
                        {
                            var reserve = new Reserve(productBatch, materialBatch);
                            reserves.Add(reserve);
                        }
            //Console.WriteLine(reserves);
            foreach (var reserve in reserves)
                Console.WriteLine("productBatch.id={0} materialBatch.id={1} productQuantity={2}", reserve.productBatch.id, reserve.materialBatch.id, reserve.productQuantity);
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
        public Batch(Product product, int quantity)
        {
            id = count++;
            this.product = product;
            this.quantity = quantity;
            Reserved = 0;
            Provided = 0;
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

        public Product(int id, int len, int seconds)
        {
            this.id = id;
            this.len = len;
            this.seconds = seconds;
        }
    }
}
