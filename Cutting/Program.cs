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
            var materialBatch1 = new Batch("te1", material1, 80, 100, 0, DateTime.Today, "plavka1");
            //var sourceBatch2 = new Batch("plavka2", material1, 80);
            var materialBatch2 = new Batch("te2", material1, 80, 20, 0, DateTime.Today, "plavka1");
            List<Batch> materialBatches = new List<Batch>() { materialBatch1, materialBatch2 };

            var product = new Product("det2", "path1", 10, 20, material1.id);
            var productBatch1 = new Batch("batch1", product, 80, 1);
            var productBatch2 = new Batch("batch2", product, 80, 3);
            var product3 = new Product("det3", "path3", 10, 20, "2");
            var productBatch3 = new Batch("batch3", product3, 80, 3);

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1, productBatch2, productBatch3 };
            Alt[] alts = new Alt[] {
                new Alt { originalMatarialId = "2", alterMatarialId = "1", productId = "det3" }            ,
                new Alt { originalMatarialId = "2", alterMatarialId = "1", productId = string.Empty },
            };
            PrintResult(CalcWithAlt(productBatches, materialBatches, alts));
            //var altres = AltReserve(productBatches.ToArray(), materialBatches, alts);
            //Console.WriteLine("alts:");
            //PrintResult(altres);
            Console.WriteLine("material batches:");
            foreach (var b in materialBatches)
                Console.WriteLine("id={0} matId={1} qnt={2} reserv={3}", b.id, b.product.id, b.quantity, b.Reserved);
            Console.WriteLine("product batches:");
            foreach (var b in productBatches)
                Console.WriteLine("id={0} matId={1} notProvided={2}", b.id, b.product.material, b.NotProvided);

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

        public static Reserve[] Calc2(IEnumerable<Batch> productBatches, IEnumerable<Batch> materialBatches)
        {
            List<Reserve> reserves = new List<Reserve>();
            var materials = materialBatches.ToLookup(x => x.product.id);
            foreach (var productBatch in productBatches.OrderBy(b => b.deadline).ThenByDescending(b => b.RequiredLen))
            {
                var material = materials[productBatch.product.material];
                var availabilities = material.ToLookup(m => m.availability);
                List<Batch> availBatches = new List<Batch>();
                foreach (var availability in availabilities.OrderBy(x => x.Key))
                {
                    availBatches.AddRange(availability);

                    ReserveCreate(reserves, productBatch, availBatches.ToArray());
                    if (productBatch.NotProvided <= 0) break;
                }
            }
            return reserves.ToArray();
        }

        public static Reserve[] CalcWithAlt(IEnumerable<Batch> productBatches, IEnumerable<Batch> materialBatches, Alt[] alts)
        {
            List<Reserve> reserves = new List<Reserve>();
            var materials = materialBatches.ToLookup(x => x.product.id);
            var altByOriginalAndProduct = alts.ToLookup(a => a.originalMatarialId + a.productId, a => a.alterMatarialId);
            foreach (var productBatch in productBatches.OrderBy(b => b.deadline).ThenByDescending(b => b.RequiredLen))
            {
                {
                    var material = materials[productBatch.product.material].Where(b => b.NotReserved > productBatch.product.len);
                    ReserveMaterial(reserves, productBatch, material);
                }
                if (productBatch.NotProvided > 0)
                {
                    List<string> altIds = altByOriginalAndProduct[productBatch.product.material + productBatch.product.id].ToList();
                    altIds.AddRange(altByOriginalAndProduct[productBatch.product.material]);
                    var material = altIds.SelectMany(altId => materials[altId].Where(b => b.NotReserved > productBatch.product.len));
                    ReserveMaterial(reserves, productBatch, material);
                }
            }
            return reserves.ToArray();
        }

        private static void ReserveMaterial(List<Reserve> reserves, Batch productBatch, IEnumerable<Batch> material)
        {
            var availabilities = material.ToLookup(m => m.availability);
            List<Batch> availBatches = new List<Batch>();
            foreach (var availability in availabilities.OrderBy(x => x.Key))
            {
                availBatches.AddRange(availability);
                ReserveCreate(reserves, productBatch, availBatches.ToArray());
                if (productBatch.NotProvided <= 0) break;
            }
        }

        private static void ReserveCreate(List<Reserve> reserves, Batch productBatch, Batch[] matBatches)
        {
            var melts = matBatches.ToLookup(x => x.melt);
            foreach (var meltBatches in melts.OrderBy(m => m.Sum(b => b.NotReserved)).ToArray())
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
        }

        public static Reserve[] AltReserve(Batch[] productBatches, IEnumerable<Batch> materialBatches, Alt[] alts)
        {
            var materialBatchesLookupByProductId = materialBatches.ToLookup(i => i.product.id);
            List<Reserve> reserves = new List<Reserve>();

            foreach (var productBatch in productBatches.OrderBy(b => b.deadline).ThenByDescending(b => b.RequiredLen))
            {
                var altMaterialIds = alts
                    .Where(a => a.originalMatarialId == productBatch.product.material && (a.productId == productBatch.product.id || String.IsNullOrEmpty(a.productId)))
                    .Select(a => a.alterMatarialId);
                Batch[] availableBatches = altMaterialIds.SelectMany(id => materialBatchesLookupByProductId[id]).ToArray();
                ReserveCreate(reserves, productBatch, availableBatches);
                //foreach (var meltBatches in availableBatches.GroupBy(b => b.melt).OrderBy(m => m.Sum(b => b.NotReserved)).ToArray())
                //    if (CheckMelt(meltBatches, productBatch.quantity, productBatch.product.len))
                //    {
                //        foreach (var materialBatch in meltBatches.OrderBy(b => b.NotReserved).ToArray())
                //            if (materialBatch.NotReserved >= productBatch.product.len && productBatch.NotProvided > 0)
                //            {
                //                var reserve = new Reserve(productBatch, materialBatch);
                //                reserves.Add(reserve);
                //            }
                //        break;
                //    }
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

    public class Alt
    {
        public string originalMatarialId;
        public string alterMatarialId;
        public string productId;
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
        public int availability;
        public string location_id;
        private int reserved;
        private int provided;
        public string melt;
        public int NotReserved { get; private set; }
        public int NotProvided { get; private set; }
        public int Seconds => product.seconds * quantity;
        public int RequiredLen => product.len * quantity;
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
        public Batch(string id, Product product, int place_id, int quantity = 0, int kg = 0, DateTime? deadline = null, string melt = null, string location_id = null, int availability = 0)
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
            this.availability = availability;
        }

        public List<Reserve> Reserves;
    }

    public class Product
    {
        public readonly string id;
        public readonly string path;
        public int diameter;
        public int len;
        public int pieces;
        public string material;
        public int seconds;

        public Product(string id, string path="", int len = 0, int pieces = 0, string material = null, int seconds = 0, int diameter = 0)
        {
            this.id = id;
            this.path = path;
            this.len = len > 0 ? len : 1;
            this.pieces = pieces;
            this.material = material;
            this.seconds = seconds;
            this.diameter = diameter;
        }
    }
}
