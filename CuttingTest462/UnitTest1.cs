using CuttingV2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace CuttingTest462
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var material1 = "1";
            var materialBatch1 = new Feedstock("te1", material1, 2, 0, "plavka1");
            var materialBatch2 = new Feedstock("te2", material1, 4, 0, "plavka1");
            List<Feedstock> materialBatches = new List<Feedstock>() { materialBatch1, materialBatch2 };

            var product = new Product("det2", "path1", 2, 1, material1) { billet_len = 1 };
            var productBatch1 = new Batch("batch1", product, 6, new DateTime(2021, 12, 2)) { auto_start = 1 };
            //var productBatch2 = new Batch("batch2", product, 80, 10, 0, new DateTime(2021, 12, 1));

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1 };
            Alt[] alts = Array.Empty<Alt>();
            //Test(productBatches, materialBatches, alts);
            var reserves = Program.CalcWithAlt(productBatches, materialBatches, alts);
            Assert.IsTrue(productBatch1.quantity == 6);
            Assert.IsTrue(productBatch1.feasibleQuantity == 2);
        }
        [TestMethod]
        public void TestMethod2()
        {
            var material1 = "1";
            var materialBatch1 = new Feedstock("te1", material1, 822, 0, "plavka1");
            var materialBatch2 = new Feedstock("te2", material1, 1070, 0, "plavka1");
            var materialBatch3 = new Feedstock("te2", material1, 160, 0, "plavka1");

            var material2 = "2";
            var materialBatchAlt1 = new Feedstock("teAlt1", material2, 999999, 0, "plavka2");

            var materialBatches = new Feedstock[] { materialBatch1, materialBatch2, materialBatch3, materialBatchAlt1 };

            var sample = new Product("sample", "path1", 200, 1, material1) { billet_len = 198 };
            var sampleBatch = new Batch("batch1", sample, 2, new DateTime(2022, 03, 09));
            var product = new Product("det1", "path1", 61, 1, material1) { billet_len = 59 };
            var productBatch1 = new Batch("batch1", product, 28, new DateTime(2022, 03, 09)) { auto_start = 1, sampleBatch = sampleBatch };

            var product2 = new Product("det2", "path1", 1565, 1, material1) { billet_len = 1563 };
            var productBatch2 = new Batch("batch21", product2, 25, new DateTime(2022, 04, 20)) { auto_start = 1 };
            var productBatch21 = new Batch("batch22", product2, 7, new DateTime(2022, 06, 12)) { auto_start = 1 };

            var productBatch3 = new Batch("batch3", product, 36, new DateTime(2022, 03, 12)) { auto_start = 0 };

            var product4 = new Product("det4", "path1", 43, 1, material1) { billet_len = 41 };
            var productBatch4 = new Batch("batch4", product4, 153, new DateTime(2022, 04, 06)) { auto_start = 0 };

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1, productBatch2, productBatch21, productBatch3, productBatch4 };
            Alt[] alts = new Alt[] {
                new Alt{ productId = product2.id, originalMatarialId = product2.material, altMaterialId = material2  }
            };
            //Test(productBatches, materialBatches, alts);
            var reserves = Program.CalcWithAlt(productBatches, materialBatches, alts);
            Assert.IsTrue(productBatch1.IsProvided, "test failed");
        }
        [TestMethod]
        public void TestMethodSample1()
        {
            var material1 = "1";
            var materialBatch1 = new Feedstock("te1", material1, 92589, 0, "plavka1");
            //var materialBatch2 = new Feedstock("te2", material1, 1070, 0, "plavka1");

            //var material2 = "2";
            //var materialBatchAlt1 = new Feedstock("teAlt1", material2, 999999, 0, "plavka2");

            var materialBatches = new Feedstock[] { materialBatch1 };

            var productSample = new Product("sample", "pathSample1", 205, 1, material1) { billet_len = 198 };
            var sampleBatch = new Batch("batch1", productSample, 5, new DateTime(2022, 03, 09));
            var product = new Product("det1", "path1", 170, 2, material1) { billet_len = 168 };
            var productBatch1 = new Batch("batch1", product, 10, new DateTime(2022, 03, 09)) { auto_start = 1, sampleBatch = sampleBatch };

            //var product2 = new Product("det2", "path1", 1565, 1, material1) { billet_len = 1563 };
            //var productBatch2 = new Batch("batch21", product2, 25, new DateTime(2022, 04, 20)) { auto_start = 1 };
            //var productBatch21 = new Batch("batch22", product2, 7, new DateTime(2022, 06, 12)) { auto_start = 1 };

            //var productBatch3 = new Batch("batch3", product, 36, new DateTime(2022, 03, 12)) { auto_start = 0 };

            //var product4 = new Product("det4", "path1", 43, 1, material1) { billet_len = 41 };
            //var productBatch4 = new Batch("batch4", product4, 153, new DateTime(2022, 04, 06)) { auto_start = 0 };

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1 };
            Alt[] alts = new Alt[] {
                //new Alt{ productId = product2.id, originalMatarialId = product2.material, altMaterialId = material2  }
            };
            //Test(productBatches, materialBatches, alts);
            var reserves = Program.CalcWithAlt(productBatches, materialBatches, alts);
            Assert.IsTrue(productBatch1.IsProvided, "test failed");
        }
        [TestMethod]
        public void TestMethod3()
        {
            var material1 = "1";
            var materialBatch1 = new Feedstock("te7230", material1, 7230, 0, "plavka1") { availability = 4 };
            var materialBatch2 = new Feedstock("te695", material1, 695, 0, "plavka2") { availability = 4 };
            var materialBatch3 = new Feedstock("te347", material1, 347, 0, "plavka3") { availability = 2 };
            List<Feedstock> materialBatches = new List<Feedstock>() { materialBatch1, materialBatch2, materialBatch3 };

            var product = new Product("det2", "path1", 325, 1, material1);
            var productBatch1 = new Batch("batch1", product, 2, new DateTime(2022, 4, 18)) { auto_start = 1, check_melt = true };

            var product2 = new Product("det2", "path1", 46, 1, material1);
            var productBatch2 = new Batch("batch2", product2, 79, new DateTime(2022, 5, 8));

            var product3 = new Product("det2", "path1", 130, 1, material1);
            var productBatch3 = new Batch("batch2", product3, 60, new DateTime(2022, 5, 14));
            var productBatch4 = new Batch("batch2", product3, 60, new DateTime(2022, 5, 14));

            var product5 = new Product("det2", "path1", 190, 1, material1);
            var productBatch5 = new Batch("batch1", product, 9, new DateTime(2022, 6, 11)) { auto_start = 1, check_melt = true };

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1, productBatch2, productBatch3, productBatch4 };
            Alt[] alts = Array.Empty<Alt>();
            //Test(productBatches, materialBatches, alts);
            var reserves = Program.CalcWithAlt(productBatches, materialBatches, alts);
            var r1 = reserves.FirstOrDefault(x => x.productBatch == productBatch1);
            Assert.IsTrue(r1 != null);
            Assert.IsTrue(r1.materialBatch == materialBatch2);
        }
        [TestMethod]
        public void TestMethodAlts()
        {
            var materialAlt1 = "1";
            var materialBatch1 = new Feedstock("te1", materialAlt1, 4470, 0, "plavka1");
            var materialBatch2 = new Feedstock("te2", materialAlt1, 4470, 0, "plavka1");
            var materialBatch3 = new Feedstock("te2", materialAlt1, 5000, 0, "plavka1", 2);

            var materialBatches = new Feedstock[] { materialBatch1, materialBatch2, materialBatch3 };

            var materialDet = "2";
            var product1 = new Product("det1", "path1", 155, 1, materialDet);
            var productBatch1 = new Batch("batch1", product1, 30, new DateTime(2022, 03, 09)) { auto_start = 1 };

            var product2 = new Product("det2", "path1", 625, 1, materialDet);
            var productBatch2 = new Batch("batch21", product2, 5, new DateTime(2022, 04, 20)) { auto_start = 1 };

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1, productBatch2 };
            Alt[] alts = new Alt[] {
                new Alt{ productId = product1.id, originalMatarialId = product1.material, altMaterialId = materialAlt1  },
                new Alt{ productId = product2.id, originalMatarialId = product2.material, altMaterialId = materialAlt1  },
            };
            //Test(productBatches, materialBatches, alts);
            var reserves = Program.CalcWithAlt(productBatches, materialBatches, alts);
            Assert.IsTrue(reserves.Any(r => r.productBatch == productBatch1 && r.materialBatch == materialBatch1));
            Assert.IsTrue(productBatch2.IsProvided);
        }
        [TestMethod]
        public void TestReduceBatchQuantity()
        {
            var material1 = "1";
            var materialBatch1 = new Feedstock("te1", material1, 4, 0, "plavka1");
            var materialBatch2 = new Feedstock("te2", material1, 1, 0, "plavka1");
            var materialBatch3 = new Feedstock("te2", material1, 1, 0, "plavka1", 2);

            var materialBatches = new Feedstock[] { materialBatch1, materialBatch2, materialBatch3 };

            //var materialDet = material1;
            var product1 = new Product("det1", "path1", 2, 1, material1);
            var productBatch1 = new Batch("batch1", product1, 3, new DateTime(2000, 01, 01)) { auto_start = 1 };

            var product2 = new Product("det2", "path1", 1, 1, material1);
            var productBatch2 = new Batch("batch21", product2, 1, new DateTime(2001, 01, 01)) { auto_start = 1 };

            IEnumerable<Batch> productBatches = new Batch[] { productBatch1, productBatch2 };
            Alt[] alts = new Alt[] {
            //    new Alt{ productId = product1.id, originalMatarialId = product1.material, altMaterialId = materialAlt1  },
            //    new Alt{ productId = product2.id, originalMatarialId = product2.material, altMaterialId = materialAlt1  },
            };
            //Test(productBatches, materialBatches, alts);
            var reserves = Program.CalcWithAlt(productBatches, materialBatches, alts);
            Assert.IsTrue(reserves.Any(r => r.productBatch == productBatch1 && r.materialBatch == materialBatch1));
            Assert.IsTrue(productBatch1.quantity == 3);
            Assert.IsTrue(productBatch1.feasibleQuantity == 2);
            Assert.IsTrue(productBatch1.IsProvided);
        }
    }
}
