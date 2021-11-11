using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cutting
{
    public class Program
    {
        static void Main()
        {
            TestZeroWaste();
            Console.Read();
        }

        static void Test(IEnumerable<Batch> productBatches, IEnumerable<Batch> materialBatches, Alt[] alts, Direction[] directions = null, Reserve[] reserveList = null)
        {
            PrintResult(CalcWithAlt(productBatches, materialBatches, alts));
            Console.WriteLine("material batches:");
            foreach (var b in materialBatches)
                Console.WriteLine("id={0} matId={1} qnt={2} reserv={3}", b.id, b.product.id, b.quantity, b.Reserved);
            Console.WriteLine("product batches:");
            foreach (var b in productBatches)
                Console.WriteLine("id={0} matId={1} notProvided={2}", b.id, b.product.material, b.NotProvided);

            Console.Read();
        }

        static void Test1()
        {
            var material1 = new Product("1");
            //var sourceBatch1 = new Batch("plavka1", material1, 80);
            var materialBatch1 = new Batch("te1", material1, 80, 105, 0, DateTime.Today, "plavka1");
            //var sourceBatch2 = new Batch("plavka2", material1, 80);
            var materialBatch2 = new Batch("te2", material1, 80, 35, 0, DateTime.Today, "plavka1");
            List<Batch> materialBatches = new List<Batch>() { materialBatch1, materialBatch2 };

            var product = new Product("det2", "path1", 20, 1, material1.id);
            var productBatchSample = new Batch("batchSample", product, 80, 5);
            var productBatch2 = new Batch("batch2", product, 80, 3) { sampleBatch = productBatchSample };
            var product3 = new Product("det3", "path3", 10, 20, "2");
            var productBatch3 = new Batch("batch3", product3, 80, 3);

            IEnumerable<Batch> productBatches = new Batch[] { productBatchSample };
            Alt[] alts = new Alt[] {
                new Alt { originalMatarialId = "2", altMaterialId = "1", productId = "det3" },
                new Alt { originalMatarialId = "2", altMaterialId = "1", productId = string.Empty },
            };
            Test(productBatches, materialBatches, alts);
        }

        static void TestBilletLen()
        {
            var material1 = new Product("1");
            //var sourceBatch1 = new Batch("plavka1", material1, 80);
            var materialBatch1 = new Batch("te1", material1, 80, 1500, 0, DateTime.Today, "plavka1");
            //var sourceBatch2 = new Batch("plavka2", material1, 80);
            //var materialBatch2 = new Batch("te2", material1, 80, 35, 0, DateTime.Today, "plavka1");
            List<Batch> materialBatches = new List<Batch>() { materialBatch1 };

            var product = new Product("det2", "path1", 1502, 1, material1.id) { billet_len = 1500 };
            var productBatch1 = new Batch("batch1", product, 80, 1);
            //var productBatch2 = new Batch("batch2", product, 80, 3) { sampleBatch = productBatchSample };
            //var product3 = new Product("det3", "path3", 10, 20, "2");
            //var productBatch3 = new Batch("batch3", product3, 80, 3);

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1 };
            Alt[] alts = new Alt[] { };
            Test(productBatches, materialBatches, alts);
        }

        static void TestZeroWaste()
        {
            var material1 = new Product("1");
            var materialBatch1 = new Batch("te1", material1, 80, 510, 0, DateTime.Today, "plavka1");
            var materialBatch2 = new Batch("te2", material1, 80, 3000, 0, DateTime.Today, "plavka1");
            List<Batch> materialBatches = new List<Batch>() { materialBatch1, materialBatch2 };

            var product = new Product("det2", "path1", 255, 1, "2") { billet_len = 253 };
            var productBatch1 = new Batch("batch1", product, 80, 6, 0, new DateTime(2021, 12, 2));
            var productBatch2 = new Batch("batch2", product, 80, 10, 0, new DateTime(2021, 12, 1));

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1, productBatch2 };
            Alt[] alts = new Alt[] { new Alt { originalMatarialId = "2", altMaterialId = "1", productId = "det2" }, };
            Test(productBatches, materialBatches, alts);
        }

        //public static Reserve[] Calc(IEnumerable<Batch> productBatches, IEnumerable<Batch> materialBatches)
        //{
        //    List<Reserve> reserves = new List<Reserve>();
        //    var materialBatchesByMaterial = materialBatches.GroupBy(b => b.product.id)
        //        .ToLookup(x => x.Key, x => x.ToLookup(b => b.melt));
        //    foreach (var productBatch in productBatches.OrderBy(b => b.deadline).ThenByDescending(b => b.RequiredLen))
        //        foreach (var materialBatchesForProductBatch in materialBatchesByMaterial[productBatch.product.material])
        //            foreach (var meltBatches in materialBatchesForProductBatch.OrderBy(m => m.Sum(b => b.NotReserved)).ToArray())
        //                if (CheckMelt(meltBatches, productBatch.quantity, productBatch.product, productBatch.sampleBatch))
        //                {
        //                    foreach (var materialBatch in meltBatches.OrderBy(b => b.NotReserved).ToArray())
        //                        if (materialBatch.NotReserved >= productBatch.product.len && productBatch.NotProvided > 0)
        //                        {
        //                            var reserve = new Reserve(productBatch, materialBatch);
        //                            reserves.Add(reserve);
        //                        }
        //                    break;
        //                }
        //    return reserves.ToArray();
        //}

        //public static Reserve[] Calc2(IEnumerable<Batch> productBatches, IEnumerable<Batch> materialBatches)
        //{
        //    List<Reserve> reserves = new List<Reserve>();
        //    var materials = materialBatches.ToLookup(x => x.product.id);
        //    foreach (var productBatch in productBatches.OrderBy(b => b.deadline).ThenByDescending(b => b.RequiredLen))
        //    {
        //        var material = materials[productBatch.product.material];
        //        var availabilities = material.ToLookup(m => m.availability);
        //        List<Batch> availBatches = new List<Batch>();
        //        foreach (var availability in availabilities.OrderBy(x => x.Key))
        //        {
        //            availBatches.AddRange(availability);

        //            ReserveCreate(reserves, productBatch, availBatches.ToArray());
        //            if (productBatch.NotProvided <= 0) break;
        //        }
        //    }
        //    return reserves.ToArray();
        //}

        //public static Reserve[] CalcWithAlt(IEnumerable<Batch> productBatches, IEnumerable<Batch> materialBatches, Alt[] alts, Reserve[] reserveList = null)
        //{
        //    var productBatchesArr = productBatches.Except(productBatches.Where(b => b.sampleBatch != null).Select(x => x.sampleBatch)).ToArray();
        //    string[] altMaterialIds = alts.Select(x => x.alterMatarialId).Distinct().ToArray();
        //    var materialMinLen = productBatchesArr.GroupBy(b => b.product.material).ToDictionary(m => m.Key, m => m.Min(b => b.product.len));
        //    if (reserveList == null) reserveList = new Reserve[] { };
        //    ConcurrentBag<Reserve> reservesBag = new ConcurrentBag<Reserve>(reserveList);
        //    var materials = materialBatches.ToLookup(x => x.product.id);

        //    var productMaterialIds = productBatchesArr.ToLookup(pb => pb.product.material);
        //    Parallel.ForEach(materials.Select(x => x.Key).Except(altMaterialIds), materialId =>
        //    {
        //        var prodBatches = productMaterialIds[materialId];
        //        foreach (var productBatch in prodBatches.OrderBy(b => b.deadline)
        //        .ThenByDescending(b => b.batchId > 0).ThenBy(b => b.batchId)
        //        .ThenByDescending(b => b.RequiredLen))
        //        {
        //            var material = materials[productBatch.product.material].Where(b => b.NotReserved > productBatch.product.len).ToArray();
        //            ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
        //        }
        //    });

        //    var altByOriginalAndProduct = alts.ToLookup(a => a.originalMatarialId + a.productId, a => a.alterMatarialId);
        //    {
        //        var prodBatches = productBatchesArr.Where(pb => pb.NotProvided > 0).ToArray();
        //        foreach (var productBatch in prodBatches.OrderBy(b => b.deadline)
        //            .ThenByDescending(b => b.batchId > 0).ThenBy(b => b.batchId)
        //            .ThenByDescending(b => b.RequiredLen))
        //        {
        //            {
        //                var material = materials[productBatch.product.material].Where(b => b.NotReserved > productBatch.product.len).ToArray();
        //                ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
        //            }
        //            if (productBatch.NotProvided > 0)
        //            {
        //                List<string> altIds = altByOriginalAndProduct[productBatch.product.material + productBatch.product.id].ToList();
        //                altIds.AddRange(altByOriginalAndProduct[productBatch.product.material]);
        //                var material = altIds.SelectMany(altId => materials[altId].Where(b => b.NotReserved > productBatch.product.len))
        //                                     .ToArray();
        //                ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
        //            }
        //        }
        //    }
        //    return reservesBag.ToArray();
        //}

        public static Reserve[] CalcWithAlt(IEnumerable<Batch> productBatches, IEnumerable<Batch> materialBatches, Alt[] alts, Direction[] directions = null, Reserve[] reserveList = null)
        {
            var altMaterialIds = alts.Select(x => x.altMaterialId).Distinct();
            var materialMinLen = productBatches.GroupBy(b => b.product.material).ToDictionary(m => m.Key, m => m.Min(b => b.product.len));
            if (reserveList == null) reserveList = new Reserve[] { };
            ConcurrentBag<Reserve> reservesBag = new ConcurrentBag<Reserve>(reserveList);
            var materials = materialBatches.ToLookup(x => x.product.id);

            bool gteProductLen(Batch b, Batch productBatch) => b.NotReserved > productBatch.product.len || b.NotReserved == productBatch.product.billet_len;
            if (directions != null)
                foreach (var direction in directions)
                    foreach (var productBatch in productBatches.Where(x => x.product.path.ToUpper().StartsWith(direction.pathTemplate.ToUpper()))
                        .OrderBy(b => b.deadline).ThenByDescending(b => b.RequiredLen))
                    {
                        var material = materials[direction.materialId].Where(b => gteProductLen(b, productBatch));
                        ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
                    }

            Parallel.Invoke(
                () =>
                {
                    var productMaterialIds = productBatches.ToLookup(pb => pb.product.material);
                    Parallel.ForEach(materials.Select(x => x.Key).Except(altMaterialIds), materialId =>
                    {
                        var prodBatches = productMaterialIds[materialId];
                        foreach (var productBatch in prodBatches.OrderBy(b => b.deadline)
                        .ThenByDescending(b => b.batchId > 0).ThenBy(b => b.batchId)
                        .ThenByDescending(b => b.RequiredLen))
                            if (productBatch.NotProvided > 0)
                            {
                                var material = materials[productBatch.product.material].Where(b => gteProductLen(b, productBatch));
                                ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
                            }
                    });
                },
                () =>
                {
                    var altByOriginalAndProduct = alts.ToLookup(a => a.originalMatarialId + a.productId, a => a.altMaterialId);
                    var prodBatches = productBatches.Where(pb => pb.NotProvided > 0);
                    foreach (var productBatch in prodBatches.OrderBy(b => b.deadline)
                        .ThenByDescending(b => b.batchId > 0).ThenBy(b => b.batchId)
                        .ThenByDescending(b => b.RequiredLen))
                    {
                        {
                            var material = materials[productBatch.product.material].Where(b => gteProductLen(b, productBatch));
                            ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
                        }
                        if (productBatch.NotProvided > 0)
                        {
                            var altIds = altByOriginalAndProduct[productBatch.product.material + productBatch.product.id]
                                .Concat(altByOriginalAndProduct[productBatch.product.material]);
                            var material = altIds.SelectMany(altId => materials[altId].Where(b => gteProductLen(b, productBatch)));
                            ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
                        }
                    }
                }
            );
            return reservesBag.ToArray();
        }

        private static void ReserveMaterial(ConcurrentBag<Reserve> reserves, Batch productBatch, IEnumerable<Batch> material, Dictionary<string, int> materialMinLen)
        {
            var availabilities = material.ToLookup(m => 10 * m.availability + m.sub_level);
            List<Batch> availBatches = new List<Batch>();
            foreach (var availability in availabilities.OrderBy(x => x.Key))
                if (productBatch.NotProvided > 0)
                {
                    availBatches.AddRange(availability);
                    ReserveCreate(reserves, productBatch, availBatches, materialMinLen);
                }
                else break;
        }

        private static void ReserveCreate(ConcurrentBag<Reserve> reserves, Batch productBatch, IEnumerable<Batch> matBatches, Dictionary<string, int> materialMinLen)
        {
            var melts = matBatches.ToLookup(x => productBatch.check_melt ? x.melt : string.Empty);
            int batchOrder(Batch b) => BatchWaste(b);
            foreach (var meltBatches in melts
                .Where(meltBtcs => CheckMelt(meltBtcs, productBatch.quantity, productBatch.product, productBatch.sampleBatch))
                .OrderBy(meltBtcs => WasteSum(meltBtcs, productBatch.quantity, productBatch.product.len))
                )
            {
                int minLen = MinLen(materialMinLen, meltBatches.First().product.id);
                var orderedByWaste = meltBatches
                    .Where(materialBatch => materialBatch.NotReserved >= productBatch.product.billet_len)
                    .OrderBy(batchOrder);
                ProdBatchReserve(reserves, productBatch, orderedByWaste);
                ProdBatchReserve(reserves, productBatch.sampleBatch, meltBatches.OrderBy(b => b.NotReserved));
                break;
            }

            int WasteSum(IEnumerable<Batch> m, int required, int len)
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

            int BatchWaste(Batch materialBatch)
            {
                int qnt = Math.Min(productBatch.NotProvided, materialBatch.NotReserved / productBatch.product.len);
                return materialBatch.NotReserved >= productBatch.NotProvidedLen
                    ? materialBatch.NotReserved - productBatch.NotProvidedLen
                    : materialBatch.NotReserved - qnt * productBatch.product.len;
            }
        }

        private static void ProdBatchReserve(ConcurrentBag<Reserve> reserves, Batch productBatch, IEnumerable<Batch> notReservedMeltBatches)
        {
            if (productBatch != null)
                foreach (var materialBatch in notReservedMeltBatches)
                {
                    if (productBatch.NotProvided <= 0) break;
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

        public static bool CheckMelt(IEnumerable<Batch> materialBatches, int required, Product product, Batch sampleBatch)
        {
            var notReservedArr = materialBatches.Select(x => x.NotReserved).ToArray();
            int sampleRequired = sampleBatch == null ? 0 : sampleBatch.quantity;
            if (!CheckArr(notReservedArr, required, product.len, product.billet_len)) return false;
            if (sampleBatch != null && !CheckArr(notReservedArr, sampleBatch.quantity, sampleBatch.product.len, sampleBatch.product.billet_len)) return false;
            return true;
        }

        public static bool CheckArr(int[] notReservedArr, int required, int len, int billetLen)
        {
            for (int i = 0; i < notReservedArr.Length; i++)
            {
                if (billetLen > 0 && billetLen == notReservedArr[i])
                {
                    notReservedArr[i] = 0;
                    required--;
                }
                else
                {
                    int qnt = notReservedArr[i] / len;
                    notReservedArr[i] -= qnt * len;
                    required -= qnt;
                }
                if (required <= 0)
                    return true;
            }
            return false;
        }

        ///<summary>
        /// Count no-waste material batches
        ///</summary>
        ///<param name="materialBatches"> Batches</param>
        ///<param name="required">required quantity by product batch</param>
        ///<param name="len">product len</param>
        ///<param name="minLen">material batch min len</param>
        ///<returns>count</returns>
        public static int CountNoWasteMelt(IEnumerable<Batch> materialBatches, int required, int len, int minLen)
        {
            return materialBatches
                .Where(b => b.NotReserved <= (required + minLen) && b.NotReserved % len < minLen)
                .Count();
        }
    }

    public class Alt
    {
        public string originalMatarialId;
        public string altMaterialId;
        public string productId;
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
        public Batch materialBatch;
        public readonly int productQuantity;

        public Reserve(Batch productBatch, Batch materialBatch, int productQnt = 0)
        {
            this.productBatch = productBatch;
            this.materialBatch = materialBatch;
            if (productQnt <= 0)
            {
                int availableQnt = materialBatch.NotReserved / productBatch.product.len;
                this.productQuantity = Math.Min(productBatch.NotProvided, availableQnt);
            }
            else
            {
                this.productQuantity = productQnt;
            }
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
        public readonly int quantity;
        public readonly int kg;
        public DateTime deadline = DateTime.MaxValue;
        public DateTime created_at = DateTime.MinValue;
        public int place_id;
        public int availability;
        public int sub_level;
        public string location_id;
        private int reserved;
        private int provided;
        public bool check_melt = true;
        public string melt;
        public int NotReserved { get; private set; }
        public int NotProvided { get; private set; }
        public int Seconds => product.seconds * product.pieces * quantity;
        public int RequiredLen => product.len * quantity;
        public int NotProvidedLen => product.len * NotProvided;
        public Batch sampleBatch;
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
