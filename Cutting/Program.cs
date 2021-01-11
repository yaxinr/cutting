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
            seed();
        }

        private static void seed()
        {
            var product = new Product(1, 10, 20);
            var productBatch1 = new Batch(product, 3);
            var material = new Product(1, 1, 10);
            var materialBatch1 = new Batch(product, 30);
            var workcenter = new Workcenter();

            List<Batch> productBatches = new List<Batch>() { productBatch1 };
            List<Batch> materialBatches = new List<Batch>() { materialBatch1 };

            foreach (var productBatch in productBatches)
                foreach (var materialBatch in materialBatches)
                {
                    var reserve = new Reserve(productBatch, materialBatch, 2);
                }
        }
    }

    internal class Reserve
    {
        private Batch productBatch;
        private Batch materialBatch;
        private int quantity;

        public Reserve(Batch productBatch, Batch materialBatch, int quantity)
        {
            this.productBatch = productBatch;
            this.materialBatch = materialBatch;
            this.quantity = quantity;
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
        Product product;
        int quantity;
        public Batch(Product product, int quantity)
        {
            this.product = product;
            this.quantity = quantity;
        }
    }

    internal class Product
    {
        int id;
        int len;
        int seconds;

        public Product(int id, int len, int seconds)
        {
            this.id = id;
            this.len = len;
            this.seconds = seconds;
        }
    }
}
