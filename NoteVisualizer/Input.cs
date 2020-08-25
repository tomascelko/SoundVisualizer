using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.CompilerServices;
using System.Windows.Forms.Design;
using System.Timers;

namespace NoteVisualizer
{
    interface ISoundReader
    {
        /// <summary>
        /// Reads input file header and creates Processed MetaData object with all inportant attributes for further processing
        /// </summary>
        /// <param name="waveFile"></param>
        /// <returns></returns>
        ProcessedMetaData ReadHeader(BinaryReader waveFile);
        /// <summary>
        /// Serves as initial read from the file into the buffer
        /// </summary>
        /// <param name="waveFile">source from where you read</param>
        /// <param name="byteCount">number of bytes to read</param>
        /// <returns>Metadata needed for further processing</returns>

        byte[] ReadDataBuffer(FileStream waveFile, int byteCount);
        /// <summary>
        /// "Moves" the data buffer by the distance parameter - buffer is only a window through which we look at the data stream (also reuses the same buffer object to save memory and time)
        /// </summary>
        /// <param name="waveFile"></param>
        /// <param name="distance">Number of bytes by which the buffer is "moved"</param>
        /// <param name="dataBuffer"></param>
        void MoveDataBuffer(FileStream waveFile, int distance, byte[] dataBuffer);
    }
    class WavSoundReader : ISoundReader
    {
        public ProcessedMetaData ReadHeader(BinaryReader reader)
        {

            //Read the wave file header from the buffer. 
            int headerSize = 0;
            int optionalSize = 0;
            int normalSize = 44;
            int chunkID = reader.ReadInt32();
            int fileSize = reader.ReadInt32();
            int riffType = reader.ReadInt32();
            int fmtID = reader.ReadInt32();
            int fmtSize = reader.ReadInt32();
            int fmtCode = reader.ReadInt16();
            int channels = reader.ReadInt16();
            int sampleRate = reader.ReadInt32();
            int fmtAvgBPS = reader.ReadInt32();
            int fmtBlockAlign = reader.ReadInt16();
            int bitDepth = reader.ReadInt16();
            if (fmtSize == 18)
            {
                // Read any extra values
                int fmtExtraSize = reader.ReadInt16();
                reader.ReadBytes(fmtExtraSize);
                optionalSize = fmtExtraSize + 2;
            }
            int dataID = reader.ReadInt32();
            int dataSize = reader.ReadInt32();
            headerSize = normalSize + optionalSize;
            ProcessedMetaData metaData = new ProcessedMetaData(channels, bitDepth, sampleRate, headerSize);
            return (metaData);
        }
        public byte[] ReadDataBuffer(FileStream stream, int byteCount)
        {

            byte[] buffer = new byte[byteCount];
            stream.Read(buffer, 0, byteCount);
            return (buffer);
        }
        public void MoveDataBuffer(FileStream stream, int distance, byte[] dataBuffer)
        {
            if (distance >= dataBuffer.Length)
            {
                throw new Exception("Invalid distance for buffer");
            }
            for (int i = 0; i < dataBuffer.Length - distance; i++)
            {
                dataBuffer[i] = dataBuffer[i + distance];
            }
            byte[] newBuffer = ReadDataBuffer(stream, distance);
            for (int i = dataBuffer.Length - distance; i < dataBuffer.Length; i++)
            {
                dataBuffer[i] = newBuffer[i - dataBuffer.Length + distance];
            }
        }

    }
    /// <summary>
    /// Holds all necessary data about the music sample for further processing
    /// </summary>
    struct ProcessedMetaData
    {
        public readonly int headerSize;
        public readonly int channelsCount;
        public readonly int bitDepth;
        public readonly int sampleFreq;
        public ProcessedMetaData(int channelsCount, int bitDepth, int sampleFreq, int headerSize)
        {
            this.headerSize = headerSize;
            this.channelsCount = channelsCount;
            this.bitDepth = bitDepth;
            this.sampleFreq = sampleFreq;
        }
    }
}
