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
    
    /// <summary>
    ///  Main sample processing algorithm 
    /// </summary>
    class MainProcessor
    {
        const int HPSIterationsCount = 5;
        public static double Progress { get; private set; }
        public int ChunkSize { get; private set; }
        private int sampleSize;
        private int SampleSize //size of one sample in bytes (according to metadata)
        {
            get
            {
                return sampleSize;
            }
            set
            {
                sampleSize = value;
                ChunkSize = SampleSize * 8196;
            }
        }

        private int sampleArraySize { get; set; }
        #region buffer methods
        private Complex[] ToComplexSamples(byte[] byteArray, int bitDepth, int channelsCount) //transforms bytes to doubles
        {
            Complex[] complexArray = new Complex[(ChunkSize * 8 / (bitDepth * channelsCount)) + 1];
            int byteIndex = 0;
            switch (bitDepth)
            {
                case 8:
                    for (int i = 0; i < complexArray.Length; i++)
                    {
                        complexArray[i].realPart = byteArray[byteIndex];
                        complexArray[i].imaginaryPart = 0;
                    }
                    break;
                case 16:
                    for (int i = 0; i < complexArray.Length; i++)
                    {
                        complexArray[i].realPart = BitConverter.ToInt16(byteArray, byteIndex);
                        complexArray[i].imaginaryPart = 0;
                        byteIndex += 2;
                    }
                    break;
                case 24:
                    for (int i = 0; i < complexArray.Length; i++)
                    {
                        complexArray[i].realPart = BitConverter.ToInt32(byteArray, byteIndex) & 0x0FFF; //check endianness
                        complexArray[i].imaginaryPart = 0;
                        byteIndex += 3;
                    }
                    break;
                case 32:
                    for (int i = 0; i < complexArray.Length; i++)
                    {
                        complexArray[i].realPart = BitConverter.Int32BitsToSingle(BitConverter.ToInt32(byteArray, byteIndex));
                        complexArray[i].imaginaryPart = 0;
                        byteIndex += 4;
                    }
                    break;
                default:
                    IOException ex = new IOException("Sorry, File sampling bitDepth is not supported or the file is corrupted");
                    throw (ex);
            }
            return (complexArray);
        }
        private Complex[] ExtendSamples(Complex[] samples)
        {
            Complex[] complexValues = new Complex[2 * samples.Length - 2];
            for (int i = 0; i < samples.Length; i++)
            {
                Complex complex = new Complex(samples[i].realPart, 0);
                complexValues[i] = complex;
                if (i > 0)
                {
                    complexValues[complexValues.Length - i] = complex;
                }
            }
            return (complexValues);
        }
        private MaxFreqBin GetMaxFreqBin(Complex[] arrayAfterFFT)
        {
            MaxFreqBin max = new MaxFreqBin(0, (int.MinValue));
            for (int i = 0; i < arrayAfterFFT.Length / 2; i++)
            {
                if (Math.Abs(arrayAfterFFT[i].realPart / arrayAfterFFT.Length) > max.Value)
                {
                    max.Set(i, Math.Sqrt(arrayAfterFFT[i].realPart.Sqr() + arrayAfterFFT[i].imaginaryPart.Sqr()) / arrayAfterFFT.Length);
                }
            }
            return (max);
        }
        /// <summary>
        /// Extracts one channel from the dual channel sample
        /// </summary>
        /// <param name="dualChannel"></param>
        /// <param name="isEven"></param>
        /// <returns>transformed buffer</returns>
        private byte[] ExtractByteChannel(byte[] dualChannel, bool isEven)
        {
            byte[] singleChannel = new byte[dualChannel.Length / 2];
            if (isEven)
            {
                for (int i = 0; i < dualChannel.Length; i++)
                {
                    if (i % 4 == 0)
                    {
                        singleChannel[i / 2] = dualChannel[i];
                    }
                    if (i % 4 == 1)
                    {
                        singleChannel[(i / 2) + 1] = dualChannel[i];
                    }

                }
            }
            else
            {
                for (int i = 0; i < dualChannel.Length; i++)
                {
                    if (i % 4 == 2)
                    {
                        singleChannel[(i / 2) - 1] = dualChannel[i];
                    }
                    if (i % 4 == 3)
                    {
                        singleChannel[(i / 2)] = dualChannel[i];
                    }

                }
            }
            return (singleChannel);
        }
        #endregion
        private double CalculateMaxFreq(int maxAmplitudeIndex, ProcessedMetaData metaData, int samplesCount)
        {
            return (((double)maxAmplitudeIndex * metaData.sampleFreq / samplesCount));
        }

        /// <summary>
        /// Manages threads taking files from the queue to process
        /// </summary>
        /// <param name="form"></param>
        public void ProcessSoundFiles(SoundVisualizer form)
        {
            Monitor.Enter(form.filesToProcess); //while condition check and .dequeue must be done 'atomically' - otherwise risk of race condition
            while (form.filesToProcess.Count != 0)
            {
                var fileToProcess = form.filesToProcess.Peek();
                form.filesToProcess.Dequeue();
                ProcessSoundFile(fileToProcess, fileToProcess.Substring(0, fileToProcess.Length - 4) + "_notes.ly", form); //cutting away .wav and adding notes.ly
                Monitor.Enter(form.filesToProcess);
            }
            Monitor.Exit(form.filesToProcess);

        }
        /// <summary>
        /// Main method for processing the whole music sample
        /// </summary>
        /// <param name="inputFilePath"></param>
        /// <param name="outputFileName"></param>
        /// <param name="form"></param>
        public void ProcessSoundFile(string inputFilePath, string outputFileName, SoundVisualizer form)
        {
            if (form.filesToProcess != null)
                Monitor.Exit(form.filesToProcess);
            #region processing header
            BinaryReader headerStream = new BinaryReader(File.Open(inputFilePath, FileMode.Open));
            ISoundReader soundReader = new WavSoundReader();
            ProcessedMetaData metaData = soundReader.ReadHeader(headerStream);
            SampleSize = metaData.channelsCount * metaData.bitDepth / 8;
            sampleArraySize = (ChunkSize / SampleSize) + 1;
            headerStream.Close();
            #endregion

            #region skipping header
            FileStream dataStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read);
            dataStream.Seek(metaData.headerSize, SeekOrigin.Begin);
            #endregion

            #region processing data            
            Complex[] complexSamples = new Complex[sampleArraySize];
            Complex[] extendedComplexSamples = new Complex[2 * sampleArraySize - 2];

            IWindowFunction window = new HannWindowFunction();
            byte[] byteValues = soundReader.ReadDataBuffer(dataStream, ChunkSize + SampleSize);
            MusicSample musicSample = new MusicSample(metaData, inputFilePath);
            musicSample.Notes = new List<Note>();

            while (dataStream.Position < dataStream.Length - ChunkSize)
            {

                complexSamples = ToComplexSamples(metaData.channelsCount == 1 ? byteValues : ExtractByteChannel(byteValues, isEven: true), metaData.bitDepth, metaData.channelsCount);
                INoiseDetector noiseDetector = new RMSNoiseDetector(2 << (metaData.bitDepth - 1));
                if (noiseDetector.IsNoise(complexSamples))
                {
                    Note detectedNote = new Pause();
                    musicSample.Notes.Add(detectedNote);
                }
                else
                {
                    extendedComplexSamples = ExtendSamples(complexSamples);
                    window.Windowify(extendedComplexSamples);
                    extendedComplexSamples = extendedComplexSamples.MakeFFT().MakeHPS(HPSIterationsCount);
                    MaxFreqBin maxFreqBin = GetMaxFreqBin(extendedComplexSamples);
                    double maxFreq = CalculateMaxFreq(maxFreqBin.BinNumber, metaData, sampleArraySize);
                    NoteDetector noteDetector = new NoteDetector();
                    Note detectedNote = noteDetector.GetClosestNote(maxFreq / 2);
                    musicSample.Notes.Add(detectedNote);
                }
                soundReader.MoveDataBuffer(dataStream, window.OverlapSize, byteValues);
            }
            IErrorCorrector corrector = new OverlapWindowCorrector();
            corrector.Correct(musicSample);

            INoteLengthProcessor noteLengthProcessor = new DefaultNoteLengthProcessor();
            noteLengthProcessor.ProcessSample(musicSample);

            StreamWriter writer2 = new StreamWriter(outputFileName);
            INoteWriter noteWriter = new LillyPondNoteWriter();
            noteWriter.WriteAll(musicSample, writer2);
            #endregion
        }
    }
    static class Extensions
    {
        public static double Sqr(this double value)
        {
            return (value * value);
        }
        /// <returns>Closest value in the array to the given value</returns>
        public static double GetNearest(this double value, double[] array)
        {
            double[] tempArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                tempArray[i] = (value - array[i]).Sqr();
            }
            return array[Array.IndexOf(tempArray, tempArray.Min())];
        }
        /// <summary>
        /// Paralelized Fast fourier transform given data
        /// </summary>
        /// <param name="input">data to transform</param>
        /// <param name="size">data count</param>
        /// <param name="startingIndex">index where to start FFT - called from outside with 0</param>
        /// <param name="distance">determines how many indices we need to skip in original array to get to next valid element (in this call)</param>
        /// <returns>array of complex values after FFT</returns>
        public static Complex[] MakeFFT(this Complex[] input, int size, int startingIndex, int distance)
        {
            const int minSize = 20000;
            if (size > 1)
            {
                Complex[] even = new Complex[size / 2];
                Complex[] odd = new Complex[size / 2];
                if (size > minSize)
                {

                    Task task = new Task(() => { even = MakeFFT(input, size / 2, startingIndex, distance * 2); });
                    task.Start();
                    odd = MakeFFT(input, size / 2, startingIndex + distance, distance * 2);
                    task.Wait();
                }
                else
                {
                    even = MakeFFT(input, size / 2, startingIndex, distance * 2);
                    odd = MakeFFT(input, size / 2, startingIndex + distance, distance * 2);
                }
                Complex[] output = new Complex[even.Length + odd.Length];
                double angleOfOmega = 2 * Math.PI / size;
                for (int i = 0; i < even.Length; i++)
                {

                    output[i].realPart = even[i].realPart + Math.Cos(angleOfOmega * i) * odd[i].realPart -
                        Math.Sin(angleOfOmega * i) * odd[i].imaginaryPart;
                    output[even.Length + i].realPart = even[i].realPart - (Math.Cos(angleOfOmega * i) * odd[i].realPart) +
                        (Math.Sin(angleOfOmega * i) * odd[i].imaginaryPart);
                    output[i].imaginaryPart = even[i].imaginaryPart + Math.Cos(angleOfOmega * i) * odd[i].imaginaryPart +
                        Math.Sin(angleOfOmega * i) * odd[i].realPart;
                    output[even.Length + i].imaginaryPart = even[i].imaginaryPart - Math.Cos(angleOfOmega * i) * odd[i].imaginaryPart -
                        Math.Sin(angleOfOmega * i) * odd[i].realPart;
                }
                return (output);
            }
            else
            {
                Complex[] output = new Complex[1];
                output[0] = input[startingIndex];
                return (output);
            }
        }
        /// <summary>
        /// Wrapping method to hide needed recursion parameters and set their initial value
        /// </summary>
        /// <param name="input">array to transform</param>
        /// <returns>transformed buffer</returns>
        public static Complex[] MakeFFT(this Complex[] input) => input.MakeFFT(input.Length, 0, 1);
        /// <summary>
        /// Calculates Harmonic product spectrum of buffer on input to reduce chance of "octava - error"
        /// </summary>
        /// <param name="input"></param>
        /// <param name="iterationsCount">HPS is usually computed iteratively to reduce "octava - error" even more</param>
        /// <returns>transformed buffer</returns>

        public static Complex[] MakeHPS(this Complex[] input, int iterationsCount)
        {
            Complex[] output = new Complex[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i].realPart = Math.Pow(Math.Abs(input[i].realPart), 1.0 / iterationsCount);
                output[i].imaginaryPart = Math.Pow(Math.Abs(input[i].imaginaryPart), 1.0 / iterationsCount);
            }
            for (int i = 1; i < iterationsCount; i++)
            {
                for (int j = 0; j < input.Length / (i + 1); j++)
                {
                    output[j].realPart *= Math.Pow(Math.Abs(input[i * j].realPart), 1.0 / iterationsCount);
                    output[j].imaginaryPart *= Math.Pow(Math.Abs(input[i * j].imaginaryPart), 1.0 / iterationsCount);
                }

            }
            return (output);
        }
    }
    /// <summary>
    /// A simple struct which represents complex number with its properties
    /// </summary>
    struct Complex
    {
        public double realPart { get; set; }
        public double imaginaryPart { get; set; }
        public Complex(double realPart, double imaginaryPart)
        {
            this.realPart = realPart;
            this.imaginaryPart = imaginaryPart;
        }
    }
    struct MaxFreqBin
    {
        public int BinNumber { get; private set; }
        public double Value { get; private set; }
        public void Set(int binNumber, double value)
        {
            this.BinNumber = binNumber;
            this.Value = value;
        }
        public MaxFreqBin(int binNumber, double value)
        {
            this.BinNumber = binNumber;
            this.Value = value;
        }
    }

}
