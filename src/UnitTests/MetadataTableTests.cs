﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Tao.Core;
using Tao.Interfaces;
using Tao.Containers;

namespace Tao.UnitTests
{
    [TestFixture]
    public class MetadataTableTests : BaseStreamTests
    {
        [Test]
        public void ShouldBeAbleToEnumerateTablesThatAlreadyExist()
        {
            var stream = GetStream();
            var container = CreateContainer();

            var enumerator = container.GetInstance<IFunction<Stream, IEnumerable<TableId>>>("EnumerateExistingMetadataTables");
            Assert.IsNotNull(enumerator);

            var results = enumerator.Execute(stream);
            var tableIds = new List<TableId>(results);
            Assert.IsTrue(tableIds.Count > 0);

            Assert.IsTrue(tableIds.Contains(TableId.Assembly));
            Assert.IsTrue(tableIds.Contains(TableId.Module));
            Assert.IsTrue(tableIds.Contains(TableId.MethodDef));
            Assert.IsTrue(tableIds.Contains(TableId.TypeDef));
        }

        [Test]
        public void ShouldBeAbleToGetRowCountsForAllMetadataTables()
        {
            var stream = GetStream();
            var container = CreateContainer();
            var counter = container.GetInstance<IFunction<Stream, IDictionary<TableId, int>>>("ReadMetadataTableRowCounts");
            Assert.IsNotNull(counter);

            var result = counter.Execute(stream);
            Assert.IsNotNull(result);
            Assert.AreEqual(4, result.Count);

            Assert.AreEqual(1, result[TableId.Module]);
            Assert.AreEqual(1, result[TableId.TypeDef]);
            Assert.AreEqual(1, result[TableId.MethodDef]);
            Assert.AreEqual(1, result[TableId.Assembly]);
        }

        [Test]
        public void ShouldBeAbleToReadHeapSizesField()
        {
            var stream = GetStream();
            var container = CreateContainer();

            var reader = container.GetInstance<IFunction<Stream, byte?>>("ReadHeapSizesField");
            Assert.IsNotNull(reader);

            var result = reader.Execute(stream);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result);
        }

        [Test]
        public void ShouldBeAbleToReadHeapSizes()
        {
            var stream = GetStream();
            var container = CreateContainer();
            var reader = container.GetInstance<IFunction<Stream, ITuple<int, int, int>>>("ReadMetadataHeapIndexSizes");
            Assert.IsNotNull(reader);

            var results = reader.Execute(stream);
            Assert.IsNotNull(results);

            Assert.AreEqual(results.Item1, 2, "Incorrect #Strings heap index size");
            Assert.AreEqual(results.Item2, 2, "Incorrect #Blob heap index size");
            Assert.AreEqual(results.Item3, 2, "Incorrect #Blob heap index size");
        }
    }
}