﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CuttingV2
{
    public class Program
    {
        static void Main()
        {
            TestZeroWaste();
            Console.Read();
        }

        static void Test(IEnumerable<Batch> productBatches, IEnumerable<Feedstock> materialBatches, Alt[] alts, Direction[] directions = null, Reserve[] reserveList = null)
        {
            PrintResult(CalcWithAlt(productBatches, materialBatches, alts));
            Console.WriteLine("material batches:");
            foreach (var b in materialBatches)
                Console.WriteLine("id={0} matId={1} qnt={2} reserv={3}", b.id, b.materialId, b.quantity, b.Reserved);
            Console.WriteLine("product batches:");
            foreach (var b in productBatches)
                Console.WriteLine("id={0} matId={1} notProvided={2}", b.id, b.product.material, b.NotProvided);
        }

        static void Test1()
        {
            var materialId1 = "1";
            //var sourceBatch1 = new Batch("plavka1", material1, 80);
            var materialBatch1 = new Feedstock("te1", materialId1, 105, 0, "plavka1");
            //var sourceBatch2 = new Batch("plavka2", material1, 80);
            var materialBatch2 = new Feedstock("te2", materialId1, 35, 0, "plavka1");
            var materialBatches = new List<Feedstock>() { materialBatch1, materialBatch2 };

            var product = new Product("det2", "path1", 20, 1, materialId1);
            var productBatchSample = new Batch("batchSample", product, 5);
            var productBatch2 = new Batch("batch2", product, 3) { SampleBatches = new Batch[] { productBatchSample } };
            var product3 = new Product("det3", "path3", 10, 20, "2");
            var productBatch3 = new Batch("batch3", product3, 3);

            IEnumerable<Batch> productBatches = new Batch[] { productBatchSample };
            Alt[] alts = new Alt[] {
                new Alt { originalMatarialId = "2", altMaterialId = "1", productId = "det3" },
                new Alt { originalMatarialId = "2", altMaterialId = "1", productId = string.Empty },
            };
            Test(productBatches, materialBatches, alts);
        }

        static void TestBilletLen()
        {
            var materialId1 = "1";
            //var sourceBatch1 = new Batch("plavka1", material1, 80);
            var materialBatch1 = new Feedstock("te1", materialId1, 1500, 0, "plavka1");
            //var sourceBatch2 = new Batch("plavka2", material1, 80);
            //var materialBatch2 = new Batch("te2", material1,  35, 0, DateTime.Today, "plavka1");
            var materialBatches = new List<Feedstock>() { materialBatch1 };

            var product = new Product("det2", "path1", 1502, 1, materialId1) { billet_len = 1500 };
            var productBatch1 = new Batch("batch1", product, 1);
            //var productBatch2 = new Batch("batch2", product, 80, 3) { sampleBatch = productBatchSample };
            //var product3 = new Product("det3", "path3", 10, 20, "2");
            //var productBatch3 = new Batch("batch3", product3, 80, 3);

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1 };
            Alt[] alts = new Alt[] { };
            Test(productBatches, materialBatches, alts);
        }

        static void TestZeroWaste()
        {
            var materialId1 = "1";
            var materialBatch1 = new Feedstock("te1", materialId1, 510, 0, "plavka1");
            var materialBatch2 = new Feedstock("te2", materialId1, 3000, 0, "plavka1");
            var materialBatches = new List<Feedstock>() { materialBatch1, materialBatch2 };

            var product = new Product("det2", "path1", 255, 1, "2") { billet_len = 253 };
            var productBatch1 = new Batch("batch1", product, 6, new DateTime(2021, 12, 2));
            var productBatch2 = new Batch("batch2", product, 10, new DateTime(2021, 12, 1));

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1, productBatch2 };
            Alt[] alts = new Alt[] { new Alt { originalMatarialId = "2", altMaterialId = "1", productId = "det2" }, };
            Test(productBatches, materialBatches, alts);
        }

        public static Reserve[] CalcWithAlt(IEnumerable<Batch> productBatches, IEnumerable<Feedstock> materialBatches, Alt[] alts = null, Direction[] directions = null, IEnumerable<AltPath> altPaths = null)
        {
            if (alts == null) alts = new Alt[0];
            var altMaterialIds = alts.Select(x => x.altMaterialId).Distinct();
            var materialMinLen = productBatches.GroupBy(b => b.product.material).ToDictionary(m => m.Key, m => m.Min(b => b.product.len));
            //if (reserveList == null) reserveList = new Reserve[] { };
            ConcurrentBag<Reserve> reservesBag = new ConcurrentBag<Reserve>();
            var materials = materialBatches.ToLookup(x => x.materialId);

            if (directions != null)
            {
                foreach (var productBatch in productBatches.OrderBy(b => b.deadline).ThenByDescending(b => b.RequiredLen))
                    foreach (var direction in directions.Where(direction => productBatch.product.path.ToUpper().StartsWith(direction.pathTemplate.ToUpper())))
                    {
                        var material = materials[direction.materialId].Where(b => b.CanReserve(productBatch));
                        ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
                    }
            }
            var productMaterialIds = productBatches.ToLookup(pb => pb.product.material);
            Parallel.ForEach(materials.Select(x => x.Key).Except(altMaterialIds), materialId =>
            {
                var prodBatches = productMaterialIds[materialId];
                foreach (var productBatch in prodBatches
                .OrderByDescending(b => b.auto_start)
                .ThenBy(b => b.deadline)
                .ThenByDescending(b => b.batchId > 0).ThenBy(b => b.batchId)
                .ThenByDescending(b => b.RequiredLen))
                    if (!productBatch.IsProvided)
                    {
                        var feedstocks = materials[productBatch.product.material].Where(b => b.CanReserve(productBatch));
                        ReserveMaterial(reservesBag, productBatch, feedstocks, materialMinLen);
                    }
            });
            var altByOriginalAndProduct = alts.ToLookup(a => a.originalMatarialId + a.productId);
            foreach (var productBatch in productBatches.Where(pb => !pb.IsProvided)
                .OrderByDescending(b => b.auto_start)
                .ThenByDescending(b => b.batchId > 0)
                .ThenBy(b => b.deadline)
                .ThenByDescending(b => b.RequiredLen)
                .ThenBy(b => b.batchId)
                )
            {
                {
                    var material = materials[productBatch.product.material].Where(b => b.CanReserve(productBatch));
                    ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
                }
                if (!productBatch.IsProvided)
                {
                    var altIds = altByOriginalAndProduct[productBatch.product.material + productBatch.product.id]
                        .Concat(altByOriginalAndProduct[productBatch.product.material]);
                    var feedstocks = altIds.Select(a => a.altMaterialId).Distinct()
                        .SelectMany(altMaterialId => materials[altMaterialId].Where(b => b.CanReserve(productBatch)));
                    ReserveMaterial(reservesBag, productBatch, feedstocks, materialMinLen);
                }
            }
            //if (altPaths != null)
            //{
            //    var altPathsByProduct = altPaths.ToLookup(x => x.productId);
            //    var deficitBatches = productBatches.Where(pb => pb.AutoStart && !pb.IsProvided)
            //        .OrderBy(b => b.deadline).ThenByDescending(b => b.RequiredLen);
            //    foreach (var productBatch in deficitBatches)
            //    {
            //        var productAltPaths = altPathsByProduct[productBatch.product.id];
            //        foreach (var productAltPath in productAltPaths.OrderByDescending(x => x.gravity))
            //        {
            //            var feedstocks = materials[productAltPath.altMaterialId].Where(feedstock => feedstock.CanReserve(productBatch));
            //            ReserveMaterial(reservesBag, productBatch, feedstocks, materialMinLen);
            //            if (productBatch.IsProvided)
            //            {
            //                productBatch.pathId = productAltPath.altPath;
            //                break;
            //            }
            //        }
            //    }
            //}
            return reservesBag.ToArray();
        }
        private static void ReserveMaterial(ConcurrentBag<Reserve> reserves, Batch productBatch, IEnumerable<Feedstock> feedstocks, Dictionary<string, int> materialMinLen)
        {
            var availabilities = feedstocks.ToLookup(m => 10 * m.availability + m.sub_level);
            List<Feedstock> availFeedstocks = new List<Feedstock>();
            foreach (var availability in availabilities.OrderBy(x => x.Key))
                if (!productBatch.IsProvided)
                {
                    availFeedstocks.AddRange(availability);
                    ReserveCreate(reserves, productBatch, availFeedstocks, materialMinLen);
                }
                else break;
            if (!productBatch.IsProvided && productBatch.AutoStart)
            {
                if (productBatch.check_melt)
                    foreach (var melt in feedstocks.GroupBy(mb => mb.melt).OrderByDescending(melt => melt.Sum(x => x.NotReserved)))
                    {
                        ReduceProductBatchQuantity(melt);
                        break;
                    }
                else
                    ReduceProductBatchQuantity(feedstocks.OrderByDescending(b => b.NotReserved));
            }
            void ReduceProductBatchQuantity(IEnumerable<Feedstock> availableFeedstocks)
            {
                var feedstocksArray = availableFeedstocks.ToArray();
                if (feedstocksArray.Length > 0)
                {
                    int residualQuantity = productBatch.ResidualQuantityFromFeedstocks(feedstocksArray);
                    if (residualQuantity > 0)
                    {
                        productBatch.feasibleQuantity -= residualQuantity;
                        ReserveMaterial(reserves, productBatch, feedstocksArray, materialMinLen);
                    }
                }
            }
        }

        private static void ReserveCreate(ConcurrentBag<Reserve> reserves, Batch productBatch, IEnumerable<Feedstock> feedstocks, Dictionary<string, int> materialMinLen)
        {
            var melts = feedstocks.ToLookup(x => productBatch.check_melt ? x.melt : string.Empty);
            int batchOrder(Feedstock b) => productBatch.Waste(b.NotReserved);
            foreach (var meltFeedstocks in melts
                .Where(meltFeedstocks => productBatch.ResidualQuantityFromFeedstocks(meltFeedstocks) == 0)
                .OrderBy(meltFeedstocks => WasteSum(meltFeedstocks, productBatch.feasibleQuantity, productBatch.product.len))
                )
            {
                int minLen = MinLen(materialMinLen, meltFeedstocks.First().materialId);
                var orderedByWaste = meltFeedstocks
                    .Where(materialBatch => materialBatch.NotReserved >= productBatch.product.billet_len)
                    .OrderBy(batchOrder);
                ProdBatchReserve(reserves, productBatch, orderedByWaste);
                if (productBatch.SampleBatches != null)
                    foreach (var sampleBatch in productBatch.SampleBatches)
                        ProdBatchReserve(reserves, sampleBatch, meltFeedstocks.OrderBy(b => b.NotReserved));
                break;
            }

            int WasteSum(IEnumerable<Feedstock> m, int required, int len)
            {
                int[] notReservedArr = m
                    .OrderBy(batchOrder)
                    .Select(b => b.NotReserved).ToArray();

                for (int i = 0; i < notReservedArr.Length; i++)
                {
                    int qnt = Math.Min(required, notReservedArr[i] / len);
                    notReservedArr[i] -= qnt * len;
                    required -= qnt;
                    if (required <= 0)
                        return notReservedArr.Sum();
                }
                return int.MaxValue;
            }
        }

        private static void ProdBatchReserve(ConcurrentBag<Reserve> reserves, Batch productBatch, IEnumerable<Feedstock> notReservedMeltBatches)
        {
            if (productBatch != null)
                foreach (var materialBatch in notReservedMeltBatches)
                {
                    if (productBatch.IsProvided) break;
                    if (productBatch.product.billet_len > 0 && materialBatch.NotReserved == productBatch.product.billet_len)
                    {
                        var reserve = new Reserve(productBatch, materialBatch, 1);
                        reserves.Add(reserve);
                    }
                    else if (materialBatch.NotReserved >= productBatch.product.len)
                    {
                        var reserve = new Reserve(productBatch, materialBatch);
                        reserves.Add(reserve);
                    }
                }
        }

        private static int MinLen(Dictionary<string, int> materialMinLen, string material)
        {
            return materialMinLen.TryGetValue(material, out int minLen) ? minLen : 10;
        }

        private static void PrintResult(IEnumerable<Reserve> reserves)
        {
            //Console.WriteLine(reserves);
            foreach (var reserve in reserves)
                Console.WriteLine("productBatch.id={0} materialBatch.id={1} productQuantity={2}"
                    , reserve.productBatch.id, reserve.materialBatch.id, reserve.productQuantity);
        }

        public static int BatchResidualQuantity(IEnumerable<Feedstock> feedstocks, int required, Product product, Batch sampleBatch)
        {
            var notReservedArr = feedstocks.Select(x => x.NotReserved).ToArray();
            if (sampleBatch != null && GetResidual(notReservedArr, sampleBatch.quantity, sampleBatch.product.len, sampleBatch.product.billet_len) > 0) return required;
            int qnt = GetResidual(notReservedArr, required, product.len, product.billet_len);
            if (qnt > 0) return qnt;
            return 0;
        }

        public static int GetResidual(int[] notReservedArr, int required, int len, int billetLen)
        {
            for (int i = 0; i < notReservedArr.Length; i++)
            {
                int notReserved = notReservedArr[i];
                if (billetLen > 0 && billetLen == notReserved)
                {
                    notReservedArr[i] = 0;
                    required--;
                }
                else
                {
                    int qnt = Math.Min(notReserved / len, required);
                    notReservedArr[i] -= qnt * len;
                    required -= qnt;
                }
                if (required <= 0)
                    return 0;
            }
            return required;
        }
    }

    public class Alt
    {
        public string originalMatarialId;
        public string altMaterialId;
        public string productId;
    }
    public class AltPath
    {
        public string originalMatarialId;
        public string altMaterialId;
        public string productId;
        public string altPath;
        public int gravity;
    }

    // Material direction to product
    public class Direction
    {
        public string materialId;
        public string pathTemplate;
    }

    public class Reserve
    {
        public Batch productBatch;
        public Feedstock materialBatch;
        public readonly int productQuantity;
        public Reserve(Batch productBatch, Feedstock materialBatch, int productQnt = 0)
        {
            this.productBatch = productBatch;
            this.materialBatch = materialBatch;
            if (productQnt <= 0)
            {
                int availableQnt = materialBatch.NotReserved / productBatch.product.len;
                this.productQuantity = Math.Min(productBatch.NotProvided, availableQnt);
            }
            else
                this.productQuantity = productQnt;
            materialBatch.Reserved += productQuantity * productBatch.product.len;
            productBatch.Provided += productQuantity;
            if (productBatch.deadline < materialBatch.deadline)
                materialBatch.deadline = productBatch.deadline;
        }
    }

    public class Batch
    {
        public string id;
        public int batchId;
        public string batchUid;
        public Product product;
        public int quantity;
        public int feasibleQuantity;
        public DateTime deadline = DateTime.MaxValue;
        public int auto_start;
        public int tare_kind;
        public bool AutoStart => auto_start == 1 && batchId == 0;
        private int provided;
        public bool check_melt = true;
        public int NotProvided => feasibleQuantity - provided;
        public bool IsProvided => feasibleQuantity <= provided;
        public int Seconds => product.seconds * product.pieces * feasibleQuantity;
        public int RequiredLen => product.len * feasibleQuantity;
        public int NotProvidedLen => product.len * NotProvided;
        public Batch sampleBatch;
        public Batch[] SampleBatches;
        public string pathId;
        public int Provided
        {
            get => provided;
            set => provided = value;
        }
        public Batch(string id, Product product, int quantity, DateTime? deadline = null)
        {
            this.id = id;
            this.product = product;
            this.quantity = quantity;
            feasibleQuantity = quantity;
            Provided = 0;
            this.deadline = deadline ?? DateTime.MaxValue;
        }
        public int ResidualQuantityFromFeedstocks(IEnumerable<Feedstock> feedstocks)
        {
            int required = feasibleQuantity;
            var notReservedArr = feedstocks.Select(x => x.NotReserved).ToArray();
            if (SampleBatches != null)
                foreach (var sampleBatch in SampleBatches)
                    if (GetResidual(sampleBatch.quantity, sampleBatch.product.len, sampleBatch.product.billet_len) > 0)
                        return required;
            int residual = GetResidual(required, product.len, product.billet_len);
            if (residual > 0) return residual;
            return 0;
            int GetResidual(int req, int len, int billetLen)
            {
                for (int i = 0; i < notReservedArr.Length; i++)
                {
                    int notReserved = notReservedArr[i];
                    if (billetLen > 0 && billetLen == notReserved)
                    {
                        notReservedArr[i] = 0;
                        req--;
                    }
                    else
                    {
                        int qnt = Math.Min(notReserved / len, req);
                        notReservedArr[i] -= qnt * len;
                        req -= qnt;
                    }
                    if (req <= 0)
                        return 0;
                }
                return req;
            }
        }
        public int Waste(int free) => free
                - (free >= NotProvidedLen
                ? NotProvidedLen
                : Math.Min(NotProvided, free / product.len) * product.len);
    }
    public class Feedstock
    {
        public string id;
        public string materialId;
        public int quantity;
        public readonly int kg;
        public DateTime deadline = DateTime.MaxValue;
        public DateTime created_at = DateTime.MinValue;
        public int place_id;
        public int availability;
        public int sub_level;
        public string location_id;
        private int reserved;
        public string melt;
        public int NotReserved { get; private set; }
        public int Reserved
        {
            get => reserved;
            set
            {
                reserved = value;
                NotReserved = quantity - reserved;
            }
        }
        public Feedstock(string id, string materialId, int quantity, int kg, string melt, int availability = 0, int place_id = 0, string location_id = null)
        {
            this.id = id;
            this.materialId = materialId;
            this.quantity = quantity;
            this.kg = kg;
            Reserved = 0;
            NotReserved = quantity;
            this.melt = melt;
            this.place_id = place_id;
            this.location_id = location_id;
            this.availability = availability;
        }
        public bool CanReserve(Batch productBatch) => NotReserved > productBatch.product.len || NotReserved == productBatch.product.billet_len;
    }

    public class Product
    {
        public readonly string id;
        public readonly string path;
        public int diameter;
        public int len;
        public int billet_len;
        public int pieces;
        public string material;
        public int seconds;

        public Product(string id, string path = "", int len = 0, int pieces = 1, string material = null, int seconds = 0, int diameter = 0)
        {
            this.id = id;
            this.path = path;
            this.len = len > 0 ? len : 1;
            this.pieces = pieces > 0 ? pieces : 1;
            this.material = material;
            this.seconds = seconds;
            this.diameter = diameter;
        }
    }
}