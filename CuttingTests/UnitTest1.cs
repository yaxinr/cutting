using CuttingV2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace CuttingTests
{
    [TestClass]
    public class CalcTests
    {
        //[TestMethod]
        //public void TestCalc1()
        //{
        //    var material1 = new Product("1");
        //    var materialBatch1 = new Batch("te1", material1, 80, 510, 0, DateTime.Today, "plavka1");
        //    var materialBatch2 = new Batch("te2", material1, 80, 3000, 0, DateTime.Today, "plavka1");
        //    List<Batch> materialBatches = new List<Batch>() { materialBatch1, materialBatch2 };

        //    var product = new Product("det2", "path1", 255, 1, "2") { billet_len = 253 };
        //    var productBatch1 = new Batch("batch1", product, 80, 6, 0, new DateTime(2021, 12, 2));
        //    var productBatch2 = new Batch("batch2", product, 80, 10, 0, new DateTime(2021, 12, 1));

        //    IEnumerable<Batch> productBatches = new Batch[] { productBatch1, productBatch2 };
        //    Alt[] alts = new Alt[] { new Alt { originalMatarialId = "2", altMaterialId = "1", productId = "det2" }, };
        //    //Test(productBatches, materialBatches, alts);
        //    var reserves = Program.CalcWithAlt(productBatches, materialBatches, alts);
        //    Assert.IsTrue(reserves.Length > 0);
        //}
        [TestMethod]
        public void TestCalcChangeQuantity()
        {
            var material1 = "1";
            var materialBatch1 = new Feedstock("te1", material1, 2, 0, "plavka1");
            var materialBatch2 = new Feedstock("te2", material1, 4, 0, "plavka1");
            List<Feedstock> materialBatches = new() { materialBatch1, materialBatch2 };

            var product = new Product("det2", "path1", 2, 1, material1) { billet_len = 1 };
            var productBatch1 = new Batch("batch1", product, 6, new DateTime(2021, 12, 2)) { auto_start = 1 };
            //var productBatch2 = new Batch("batch2", product, 80, 10, 0, new DateTime(2021, 12, 1));

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1 };
            Alt[] alts = Array.Empty<Alt>();
            //Test(productBatches, materialBatches, alts);
            var reserves = Program.CalcWithAlt(productBatches, materialBatches, alts);
            Assert.IsTrue(productBatch1.quantity == 2);
        }
    }
}
