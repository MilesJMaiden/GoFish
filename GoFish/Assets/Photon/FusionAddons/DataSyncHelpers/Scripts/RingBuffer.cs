using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.Rendering;

namespace Fusion.Addons.DataSyncHelpers
{
    [System.Serializable]
    public struct RingBuffer
    {
        public interface IRingBufferDataSource
        {
            int DataLength { get; }
            byte DataAt(int index);
            void SetDataAt(int index, byte value);
        }

        public int totalData;
        public short firstIndex;
        public short nextIndex;
        public short indexCount;
        public const int HEADER_LENGTH = sizeof(int) + sizeof(short) * 2;
        public short LastIndex => (short)(nextIndex == 0 ? indexCount - 1 : nextIndex - 1);
        int dataStorageSize;

        const int DEFAULT_CACHE_INCREASE_STEP = 300;

        public RingBuffer(int dataStorageSize)
        {
            this.dataStorageSize = dataStorageSize;
            short indexCount = (short)(dataStorageSize - HEADER_LENGTH);
            this.indexCount = indexCount;
            firstIndex = -1;
            nextIndex = 0;
            totalData = 0;
        }

        [System.Serializable]
        public struct ChangeChunk
        {
            public short startIndex;
            public short endIndex;
        }

        [System.Serializable]
        public struct LossRange
        {
            public int start;
            public int end;

            public int Count => (start == -1 || end == -1) ? 0 : end - start + 1;

            public static LossRange NoLoss => new LossRange { start = -1, end = -1 };

        }

        [System.Serializable]
        public struct Change
        {
            public int length;
            public ChangeChunk[] changeChunks;
            public LossRange lossRange;

            public Change(RingBuffer previousRingBuffer, int totalData, short firstIndex, short nextIndex)
            {
                lossRange = LossRange.NoLoss;
                short lastIndex = (short)((nextIndex - 1) > 0 ? nextIndex - 1 : previousRingBuffer.indexCount - 1);
                if (previousRingBuffer.totalData == totalData)
                {
                    // No change
                    changeChunks = new ChangeChunk[0];
                    length = 0;
                }
                else if (previousRingBuffer.nextIndex > lastIndex)
                {
                    // The change filled the buffer and continued at 0
                    //Debug.LogError($"The change filled the buffer and continued at 0 ({previousRingBuffer.nextIndex} > {lastIndex})");
                    changeChunks = new ChangeChunk[2];
                    changeChunks[0].startIndex = previousRingBuffer.nextIndex;
                    changeChunks[0].endIndex = (short)(previousRingBuffer.indexCount - 1);
                    length = previousRingBuffer.indexCount - previousRingBuffer.nextIndex;
                    changeChunks[1].startIndex = 0;
                    changeChunks[1].endIndex = lastIndex;
                    length += lastIndex + 1;
                }
                else
                {
                    // Change, but either less that was remaining to fill the buffer (or more than the full buffer)
                    if ((totalData - previousRingBuffer.totalData) >= previousRingBuffer.indexCount)
                    {
                        //TODO "=" might not be a data loss, double check
                        // Filled completely
                        //Debug.LogError($"Data lost: a whole buffer has been filled during last update {totalData - previousRingBuffer.totalData - previousRingBuffer.indexCount}");
                        lossRange.start = previousRingBuffer.totalData;
                        lossRange.end = totalData - 1 - previousRingBuffer.indexCount;
                        if (firstIndex == 0)
                        {
                            //Debug.LogError("1) Filled completly, starting at 0");
                            changeChunks = new ChangeChunk[1];
                            changeChunks[0].startIndex = 0;
                            changeChunks[0].endIndex = (short)(previousRingBuffer.indexCount - 1);
                            length = previousRingBuffer.indexCount;
                        }
                        else
                        {
                            //Debug.LogError("2) Filled completly, NOT starting at 0");
                            changeChunks = new ChangeChunk[2];
                            changeChunks[0].startIndex = firstIndex;
                            changeChunks[0].endIndex = (short)(previousRingBuffer.indexCount - 1);
                            length = previousRingBuffer.indexCount - firstIndex;
                            changeChunks[1].startIndex = 0;
                            changeChunks[1].endIndex = lastIndex;
                            length += lastIndex + 1;
                        }

                    }
                    else
                    {
                        //Debug.LogError("Filled less that was remaining to fill the buffer");
                        changeChunks = new ChangeChunk[1];
                        changeChunks[0].startIndex = previousRingBuffer.nextIndex;
                        changeChunks[0].endIndex = lastIndex;
                        length = lastIndex - previousRingBuffer.nextIndex + 1;
                    }
                }
            }
        }

        public int IndexToDataSourcePosition(int index)
        {
            return index + HEADER_LENGTH;
        }

        public int DataSourcePositionToIndex(int dataSourcePosition)
        {
            return dataSourcePosition - HEADER_LENGTH;
        }

        // Update the buffer. Return the position to update in the source data array
        public int[] AddData(int dataLength)
        {
            //Debug.LogError($"Add data: firstIndex: {firstIndex} / dataLength: {dataLength}");
            int[] dataSourcePositionsToWrite = new int[dataLength];
            if (dataLength == 0) return dataSourcePositionsToWrite;
            int positionSet = 0;
            int cursor = nextIndex;
            while (positionSet < dataLength)
            {
                dataSourcePositionsToWrite[positionSet] = IndexToDataSourcePosition(cursor);
                cursor = (cursor + 1) % indexCount;
                positionSet++;
            }

            // 2 initial cases: buffer never filled once (firstIndex == 0), and buffer filled once (firstIndex == nextIndex)
            bool bufferFilledOnce = firstIndex == nextIndex;
            //if (bufferFilledOnce) Debug.LogError($"Buffer was already filled once {firstIndex} == {nextIndex}");
            if (dataLength > indexCount)
            {
                Debug.LogWarning("Too much data: some data will be losed on storage");
                bufferFilledOnce = true;
            }
            if ((dataLength + nextIndex) >= indexCount)
            {
                //Debug.LogError($"Data will go other the max index (({dataLength} + {nextIndex}) >= {indexCount})");
                bufferFilledOnce = true;
            }
            if (firstIndex == -1)
            {
                //Debug.LogError("First write: setting firstIndex to 0");
                firstIndex = 0;
            }

            // Counters update
            totalData += (byte)dataLength;
            if (bufferFilledOnce)
            {
                // The buffer has already been filled once
                //Debug.LogError("The buffer has already been filled once");
                firstIndex = (short)((nextIndex + dataLength) % indexCount);
            }
            nextIndex = (short)((nextIndex + dataLength) % indexCount);
            return dataSourcePositionsToWrite;
        }

        // Update the buffer and stress which index ranges where changed
        public Change DataUpdate(int totalData, short firstIndex, short nextIndex)
        {
            Change change = new Change(this, totalData, firstIndex, nextIndex);
            this.totalData = totalData;
            this.firstIndex = firstIndex;
            this.nextIndex = nextIndex;
            return change;
        }

        #region IRingBufferDataSource
        public RingBuffer(IRingBufferDataSource dataSource) : this(dataSource.DataLength) { }

        public void SaveBufferHeaderToDatasource(IRingBufferDataSource dataSource)
        {
            var headerIndex = 0;
            var totalDataBytes = System.BitConverter.GetBytes(totalData);
            var firstIndexBytes = System.BitConverter.GetBytes(firstIndex);
            var nextIndexBytes = System.BitConverter.GetBytes(nextIndex);
            foreach (var totalDataByte in totalDataBytes)
            {
                dataSource.SetDataAt(headerIndex, totalDataByte);
                headerIndex++;
            }
            foreach (var firstIndexByte in firstIndexBytes)
            {
                dataSource.SetDataAt(headerIndex, firstIndexByte);
                headerIndex++;
            }
            foreach (var nextIndexByte in nextIndexBytes)
            {
                dataSource.SetDataAt(headerIndex, nextIndexByte);
                headerIndex++;
            }
        }

        public Change DataUpdate(IRingBufferDataSource dataSource)
        {
            int cursor = 0;
            var totalDataBytes = new byte[sizeof(int)];
            for (int i = 0; i < totalDataBytes.Length; i++) totalDataBytes[i] = dataSource.DataAt(i + cursor);
            var totalData = System.BitConverter.ToInt32(totalDataBytes);
            cursor += totalDataBytes.Length;
            var firstIndexBytes = new byte[sizeof(short)];
            for (int i = 0; i < firstIndexBytes.Length; i++) firstIndexBytes[i] = dataSource.DataAt(i + cursor);
            cursor += firstIndexBytes.Length;
            var firstIndex = System.BitConverter.ToInt16(firstIndexBytes);
            var nextIndexBytes = new byte[sizeof(short)];
            for (int i = 0; i < nextIndexBytes.Length; i++) nextIndexBytes[i] = dataSource.DataAt(i + cursor);
            cursor += nextIndexBytes.Length;
            var nextIndex = System.BitConverter.ToInt16(nextIndexBytes);

            if (totalData == 0)
            {
                // Check with an empty unitialized data source
                firstIndex = -1;
                nextIndex = 0;
            }
            return DataUpdate(totalData, firstIndex, nextIndex);
        }

        public Change DataUpdate(IRingBufferDataSource dataSource, out byte[] newBytes)
        {
            var change = DataUpdate(dataSource);
            //if(change.length > 0) Debug.LogError($"Change: {change.length} / {change.changeChunks.Length}");
            newBytes = new byte[change.length];
            int cursor = 0;
            foreach (var changeChunk in change.changeChunks)
            {
                if (changeChunk.startIndex > changeChunk.endIndex)
                {
                    throw new System.Exception("Error in change info");
                }
                //Debug.LogError($" chunk {changeChunk.startIndex}-{changeChunk.endIndex}");
                for (int i = changeChunk.startIndex; i <= changeChunk.endIndex; i++)
                {
                    //Debug.LogError($"=> change");
                    newBytes[cursor] = dataSource.DataAt(IndexToDataSourcePosition(i));
                    cursor++;
                }
            }
            return change;
        }

        public Change DataUpdate(IRingBufferDataSource dataSource, out byte[] newBytes, ref byte[] completeCache, int cacheIncreaseStep = DEFAULT_CACHE_INCREASE_STEP)
        {
            var change = DataUpdate(dataSource, out newBytes);

            // If provided, complete a local global cache of all data
            if (completeCache != null)
            {
                if (completeCache.Length < totalData)
                {
                    byte[] newCache = new byte[totalData + cacheIncreaseStep];

                    System.Buffer.BlockCopy(completeCache, 0, newCache, 0, completeCache.Length);
                    completeCache = newCache;
                }
                System.Buffer.BlockCopy(newBytes, 0, completeCache, totalData - newBytes.Length, newBytes.Length);
            }
            return change;
        }

        public void AddData(IRingBufferDataSource dataSource, byte[] newData)
        {
            var dataSourcePositionsToWrite = AddData(newData.Length);
            int newDataIndex = 0;
            if (dataSourcePositionsToWrite.Length != newData.Length)
            {
                throw new System.Exception("Error in AddData lengths");
            }
            foreach (var dataSourcePositionToWrite in dataSourcePositionsToWrite)
            {
                dataSource.SetDataAt(dataSourcePositionToWrite, newData[newDataIndex]);
                newDataIndex++;
            }
            // Edit data source header bytes
            SaveBufferHeaderToDatasource(dataSource);
        }
        public void AddData(IRingBufferDataSource dataSource, byte[] newData, ref byte[] completeCache, int cacheIncreaseStep = DEFAULT_CACHE_INCREASE_STEP)
        {
            var initialtotalData = totalData;
            AddData(dataSource, newData);

            // If provided, complete a local global cache of all data
            if (completeCache != null)
            {
                if (completeCache.Length < (initialtotalData + newData.Length))
                {
                    byte[] newCache = new byte[initialtotalData + newData.Length + cacheIncreaseStep];

                    System.Buffer.BlockCopy(completeCache, 0, newCache, 0, completeCache.Length);
                    completeCache = newCache;
                }
                System.Buffer.BlockCopy(newData, 0, completeCache, initialtotalData, newData.Length);
            }
        }
        #endregion

        #region IRingBufferEntry
        public interface IRingBufferEntry : IByteArraySerializable {}

        unsafe public Change DataUpdate<T>(IRingBufferDataSource dataSource, out byte[] newPaddingStartBytes, out T[] newEntries, ref byte[] completeCache, int cacheIncreaseStep = DEFAULT_CACHE_INCREASE_STEP) where T : unmanaged, IRingBufferEntry
        {
            var change = DataUpdate(dataSource, out var newBytes, ref completeCache, cacheIncreaseStep);
            // The start of the changes might be the end of data we missed: only the latest size(T) entries will be taken
            Split(newBytes, out newPaddingStartBytes, out newEntries);
            return change;
        }

        unsafe public void Split<T>(byte[] newBytes, out byte[] newPaddingStartBytes, out T[] newEntries) where T : unmanaged, IRingBufferEntry
        {
            ByteArrayTools.Split(newBytes, out newPaddingStartBytes, out newEntries);
        }

        unsafe public void AddEntry<T>(IRingBufferDataSource dataSource, T newEntry, ref byte[] completeCache, int cacheIncreaseStep = DEFAULT_CACHE_INCREASE_STEP) where T : unmanaged, IRingBufferEntry
        {
            AddData(dataSource, newEntry.AsByteArray, ref completeCache, cacheIncreaseStep);
        }

        unsafe public void AddEntry<T>(IRingBufferDataSource dataSource, T newEntry) where T : unmanaged, IRingBufferEntry
        {
            AddData(dataSource, newEntry.AsByteArray);
        }

        #endregion
    }

}
