using CuttingV2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

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
            Assert.IsTrue(productBatch1.quantity == 2);
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
            Assert.IsTrue(productBatch1.NotProvided == 0, "test failed");
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
            Assert.IsTrue(productBatch1.NotProvided == 0, "test failed");
        }
    }
}
