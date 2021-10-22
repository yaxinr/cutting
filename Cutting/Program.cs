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
            var material1 = new Product("1");
            //var sourceBatch1 = new Batch("plavka1", material1, 80);
            var materialBatch1 = new Batch("te1", material1, 80, 100, 0, DateTime.Today, "plavka1");
            //var sourceBatch2 = new Batch("plavka2", material1, 80);
            var materialBatch2 = new Batch("te2", material1, 80, 20, 0, DateTime.Today, "plavka1");
            List<Batch> materialBatches = new List<Batch>() { materialBatch1, materialBatch2 };

            var product = new Product("det2", "path1", 10, 20, material1.id);
            var productBatchSample = new Batch("batchSample", product, 80, 2);
            var productBatch2 = new Batch("batch2", product, 80, 3) { sampleBatch = productBatchSample };
            var product3 = new Product("det3", "path3", 10, 20, "2");
            var productBatch3 = new Batch("batch3", product3, 80, 3);

            IEnumerable<Batch> productBatches = new Batch[] { productBatchSample, productBatch2, productBatch3 };
            Alt[] alts = new Alt[] {
                new Alt { originalMatarialId = "2", altMaterialId = "1", productId = "det3" },
                new Alt { originalMatarialId = "2", altMaterialId = "1", productId = string.Empty },
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
                        if (CheckMelt(meltBatches, productBatch.quantity, productBatch.product.len, productBatch.sampleBatch))
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
            string[] altMaterialIds = alts.Select(x => x.altMaterialId).Distinct().ToArray();
            var materialMinLen = productBatches.GroupBy(b => b.product.material).ToDictionary(m => m.Key, m => m.Min(b => b.product.len));
            if (reserveList == null) reserveList = new Reserve[] { };
            ConcurrentBag<Reserve> reservesBag = new ConcurrentBag<Reserve>(reserveList);
            var materials = materialBatches.ToLookup(x => x.product.id);
            var meltsLookup = materialBatches.ToLookup(x => x.melt);

            if (directions != null)
                foreach (var direction in directions)
                {
                    foreach (var productBatch in productBatches.Where(x => x.product.path.ToUpper().StartsWith(direction.pathTemplate.ToUpper()))
                        .OrderBy(b => b.deadline).ThenByDescending(b => b.RequiredLen))
                    {
                        var material = materials[direction.materialId].Where(b => b.NotReserved > productBatch.product.len).ToArray();
                        ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
                    }
                }

            var productMaterialIds = productBatches.ToLookup(pb => pb.product.material);
            Parallel.ForEach(materials.Select(x => x.Key).Except(altMaterialIds), materialId =>
            {
                var prodBatches = productMaterialIds[materialId];
                foreach (var productBatch in prodBatches.OrderBy(b => b.deadline)
                .ThenByDescending(b => b.batchId > 0).ThenBy(b => b.batchId)
                .ThenByDescending(b => b.RequiredLen))
                    if (productBatch.NotProvided > 0)
                    {
                        var material = materials[productBatch.product.material].Where(b => b.NotReserved > productBatch.product.len).ToArray();
                        ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
                    }
            });

            var altByOriginalAndProduct = alts.ToLookup(a => a.originalMatarialId + a.productId, a => a.altMaterialId);
            {
                var prodBatches = productBatches.Where(pb => pb.NotProvided > 0).ToArray();
                foreach (var productBatch in prodBatches.OrderBy(b => b.deadline)
                    .ThenByDescending(b => b.batchId > 0).ThenBy(b => b.batchId)
                    .ThenByDescending(b => b.RequiredLen))
                {
                    {
                        var material = materials[productBatch.product.material].Where(b => b.NotReserved > productBatch.product.len).ToArray();
                        ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
                    }
                    if (productBatch.NotProvided > 0)
                    {
                        List<string> altIds = altByOriginalAndProduct[productBatch.product.material + productBatch.product.id].ToList();
                        altIds.AddRange(altByOriginalAndProduct[productBatch.product.material]);
                        var material = altIds.SelectMany(altId => materials[altId].Where(b => b.NotReserved > productBatch.product.len))
                                             .ToArray();
                        ReserveMaterial(reservesBag, productBatch, material, materialMinLen);
                    }
                }
            }
            return reservesBag.ToArray();
        }

        private static void ReserveMaterial(ConcurrentBag<Reserve> reserves, Batch productBatch, Batch[] material, Dictionary<string, int> materialMinLen, ILookup<string, Batch> meltsLookup = null)
        {
            var availabilities = material.ToLookup(m => 10 * m.availability + m.sub_level).ToArray();
            List<Batch> availBatches = new List<Batch>();
            foreach (var availability in availabilities.OrderBy(x => x.Key))
                if (productBatch.NotProvided > 0)
                {
                    availBatches.AddRange(availability);
                    ReserveCreate(reserves, productBatch, availBatches.ToArray(), materialMinLen, meltsLookup);
                }
                else break;
        }

        private static void ReserveCreate(ConcurrentBag<Reserve> reserves, Batch productBatch, Batch[] matBatches, Dictionary<string, int> materialMinLen, ILookup<string, Batch> meltsLookup)
        {
            var melts = matBatches.ToLookup(x => x.melt).ToArray();
            foreach (var meltBatches in melts
                .OrderByDescending(m => CountNoWasteMelt(m, productBatch.quantity, productBatch.product.len, MinLen(materialMinLen, m.First().product.id)))
                .ThenBy(m => meltsLookup == null ? 0 : meltsLookup[m.Key].Sum(x => x.NotReserved))
                .ToArray())
                if (CheckMelt(meltBatches, productBatch.quantity, productBatch.product.len, productBatch.sampleBatch))
                {
                    int minLen = MinLen(materialMinLen, meltBatches.First().product.id);
                    ProdBatchReserve(reserves, productBatch,
                        meltBatches
                        .Where(b => b.NotReserved <= (productBatch.NotProvided + minLen) && b.NotReserved % productBatch.product.len < minLen)
                        .OrderByDescending(b => b.NotReserved).ToArray());
                    if (productBatch.NotProvided <= 0) break;
                    ProdBatchReserve(reserves, productBatch, meltBatches.OrderBy(b => b.NotReserved).ToArray());
                    ProdBatchReserve(reserves, productBatch.sampleBatch, meltBatches.OrderBy(b => b.NotReserved).ToArray());
                    break;
                }
        }

        private static void ProdBatchReserve(ConcurrentBag<Reserve> reserves, Batch productBatch, Batch[] notReserveredMeltBatches)
        {
            if (productBatch != null)
                foreach (var materialBatch in notReserveredMeltBatches)
                    if (materialBatch.NotReserved >= productBatch.product.len && productBatch.NotProvided > 0)
                    {
                        var reserve = new Reserve(productBatch, materialBatch);
                        reserves.Add(reserve);
                        if (productBatch.NotProvided <= 0) break;
                    }
        }

        private static int MinLen(Dictionary<string, int> materialMinLen, string material)
        {
            return materialMinLen.TryGetValue(material, out int minLen) ? minLen : 10;
        }

        //public static Reserve[] AltReserve(Batch[] productBatches, IEnumerable<Batch> materialBatches, Alt[] alts)
        //{
        //    var materialBatchesLookupByProductId = materialBatches.ToLookup(i => i.product.id);
        //    List<Reserve> reserves = new List<Reserve>();

        //    foreach (var productBatch in productBatches.OrderBy(b => b.deadline).ThenByDescending(b => b.RequiredLen))
        //    {
        //        var altMaterialIds = alts
        //            .Where(a => a.originalMatarialId == productBatch.product.material && (a.productId == productBatch.product.id || String.IsNullOrEmpty(a.productId)))
        //            .Select(a => a.alterMatarialId);
        //        Batch[] availableBatches = altMaterialIds.SelectMany(id => materialBatchesLookupByProductId[id]).ToArray();
        //        ReserveCreate(reserves, productBatch, availableBatches);
        //        //foreach (var meltBatches in availableBatches.GroupBy(b => b.melt).OrderBy(m => m.Sum(b => b.NotReserved)).ToArray())
        //        //    if (CheckMelt(meltBatches, productBatch.quantity, productBatch.product.len))
        //        //    {
        //        //        foreach (var materialBatch in meltBatches.OrderBy(b => b.NotReserved).ToArray())
        //        //            if (materialBatch.NotReserved >= productBatch.product.len && productBatch.NotProvided > 0)
        //        //            {
        //        //                var reserve = new Reserve(productBatch, materialBatch);
        //        //                reserves.Add(reserve);
        //        //            }
        //        //        break;
        //        //    }
        //    }
        //    return reserves.ToArray();
        //}

        private static void PrintResult(IEnumerable<Reserve> reserves)
        {
            //Console.WriteLine(reserves);
            foreach (var reserve in reserves)
                Console.WriteLine("productBatch.id={0} materialBatch.id={1} productQuantity={2}", reserve.productBatch.id, reserve.materialBatch.id, reserve.productQuantity);
        }

        public static bool CheckMelt(IEnumerable<Batch> materialBatches, int required, int len, Batch sampleBatch)
        {
            var notReservedArr = materialBatches.Select(x => x.NotReserved).ToArray();
            int sampleRequired = sampleBatch == null ? 0 : sampleBatch.quantity;
            if (!CheckArr(notReservedArr, required, len)) return false;
            if (sampleBatch != null && !CheckArr(notReservedArr, sampleBatch.quantity, sampleBatch.product.len)) return false;
            return true;
        }

        public static bool CheckArr(int[] notReservedArr, int required, int len)
        {
            for (int i = 0; i < notReservedArr.Length; i++)
            {
                int qnt = notReservedArr[i] / len;
                notReservedArr[i] -= qnt * len;
                required -= qnt;
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
        public string melt;
        public int NotReserved { get; private set; }
        public int NotProvided { get; private set; }
        public int Seconds => product.seconds * product.pieces * quantity;
        public int RequiredLen => product.len * quantity;
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
