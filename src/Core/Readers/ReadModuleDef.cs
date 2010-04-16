﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Tao.Interfaces;
using Tao.Model;

namespace Tao.Readers
{
    /// <summary>
    /// Represents a class that reads a <see cref="ModuleDef"/> from the given metadata stream.
    /// </summary>
    public class ReadModuleDef : IFunction<Stream, ModuleDef>
    {
        private readonly IFunction<Stream, IDictionary<TableId, ITuple<int, Stream>>> _readAllMetadataTables;
        private readonly IFunction<Stream, ITuple<int, int, int>> _readMetadataHeapIndexSizes;
        private readonly IFunction<ITuple<uint, Stream>, string> _readStringFromStringsHeap;
        private readonly IFunction<ITuple<int, BinaryReader>, uint> _readHeapIndexValue;
        private readonly IFunction<ITuple<uint, Stream>, Guid> _readGuidFromGuidHeap;

        /// <summary>
        /// Initializes a new instance of the <see cref="ReadModuleDef"/> class.
        /// </summary>
        public ReadModuleDef(IFunction<Stream, IDictionary<TableId, ITuple<int, Stream>>> readAllMetadataTables, IFunction<Stream, ITuple<int, int, int>> readMetadataHeapIndexSizes, IFunction<ITuple<uint, Stream>, string> readStringFromStringsHeap, IFunction<ITuple<int, BinaryReader>, uint> readHeapIndexValue, IFunction<ITuple<uint, Stream>, Guid> readGuidFromGuidHeap)
        {
            _readAllMetadataTables = readAllMetadataTables;
            _readMetadataHeapIndexSizes = readMetadataHeapIndexSizes;
            _readStringFromStringsHeap = readStringFromStringsHeap;
            _readHeapIndexValue = readHeapIndexValue;
            _readGuidFromGuidHeap = readGuidFromGuidHeap;
        }

        /// <summary>
        /// Reads a <see cref="ModuleDef"/> from the given metadata stream.
        /// </summary>
        /// <param name="input">The input stream.</param>
        /// <returns>The <see cref="ModuleDef"/> embedded in the metadata stream.</returns>
        public ModuleDef Execute(Stream input)
        {
            var heapSizes = _readMetadataHeapIndexSizes.Execute(input);
            var tables = _readAllMetadataTables.Execute(input);

            var moduleTable = tables[TableId.Module];
            var tableStream = moduleTable.Item2;

            var stringIndexSize = heapSizes.Item1;
            var guidIndexSize = heapSizes.Item3;

            var reader = new BinaryReader(tableStream);
            tableStream.Seek(0, SeekOrigin.Begin);

            // Read the generation
            var generation = reader.ReadUInt16();

            var nameIndex = _readHeapIndexValue.Execute(stringIndexSize, reader);
            var guidIndex = _readHeapIndexValue.Execute(guidIndexSize, reader);

            var result = new ModuleDef();

            result.Name = _readStringFromStringsHeap.Execute(nameIndex, input);
            result.Mvid = _readGuidFromGuidHeap.Execute(guidIndex, input);

            return result;
        }
    }
}