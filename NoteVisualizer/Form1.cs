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

namespace NoteVisualizer
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            MainManager.ProcessSoundFile("clarinet2.wav");
        }

    }
    static class Extensions
    {
        public static double Sqr(this double value)
        {
            return (value * value);
        }
        public static Complex[] MakeFFT(this Complex[] input, int size, int startingIndex, int distance)
        {
            if (size > 1)
            {
                Complex[] even = MakeFFT(input, size / 2, startingIndex, distance * 2);
                Complex[] odd = MakeFFT(input, size / 2, startingIndex + distance, distance * 2);
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
        public static Complex[] MakeHPS(this Complex[] input, int iterationsCount)
        {
            Complex[] output = new Complex[input.Length];
            for (int i = 0; i < input.Length; i++)
            {
                output[i].realPart = /*Math.Abs(input[i].realPart);*/ Math.Pow(Math.Abs(input[i].realPart), 1.0/iterationsCount);
                output[i].imaginaryPart =/* Math.Abs(input[i].imaginaryPart);*/ Math.Pow(Math.Abs(input[i].imaginaryPart), 1.0/iterationsCount);
            }
            for (int  i = 1; i < iterationsCount; i++)
            {
                for (int j = 0; j < input.Length / (i + 1); j ++)
                {
                    output[j].realPart *= /*Math.Abs(input[i * j].realPart);*/ Math.Pow(Math.Abs(input[i * j].realPart), 1.0/iterationsCount);
                    output[j].imaginaryPart *= /*Math.Abs(input[i * j].imaginaryPart);*/Math.Pow(Math.Abs(input[i * j].imaginaryPart), 1.0/iterationsCount);
                }

            }
            return (output);
        }
    }
    interface ISoundReader
    {
        ProcessedMetaData ReadHeader(BinaryReader waveFile);
        byte[] ReadDataBuffer(FileStream waveFile, int byteCount);
        void MoveDataBuffer(FileStream waveFile, int distance, byte[] dataBuffer);
    }
    class WavSoundReader : ISoundReader
    {
        public int chunkSize = 8192 * 2  + 4; //used for reading bytes
        public ProcessedMetaData ReadHeader(BinaryReader reader)
        {
            //BinaryReader reader = new BinaryReader(waveFile);

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

            // Store the audio data of the wave file to a byte array. 
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
    class ProcessedMetaData
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
    interface IWindowFunction
    {
        int OverlapSize { get; }
        void Windowify(Complex[] samples);
        Complex Calculate(Complex sample, int sampleIndex, int sampleCount);
    }
    class HannWindowFunction : IWindowFunction
    {
        public int OverlapSize => 4096; //in bytes
        public void Windowify(Complex[] samples)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = Calculate(samples[i], i, samples.Length - 1);
            }
        }

        public Complex Calculate(Complex sample, int sampleIndex, int sampleCount)
        {
            sample.realPart *= (Math.Sin(Math.PI * sampleIndex / sampleCount)).Sqr();
            return (sample);

        }
    }
    class NuttalWindowFunction : IWindowFunction
    {
        public int OverlapSize => 1024; //in bytes
        const double a0 = 0.355768;
        const double a1 = 0.487396;
        const double a2 = 0.144232;
        const double a3 = 0.012604;
        public void Windowify(Complex[] samples)
        {
            for (int i = 0; i < samples.Length; i++)
            {
                samples[i] = Calculate(samples[i], i, samples.Length - 1);
            }
        }

        public Complex Calculate(Complex sample, int sampleIndex, int sampleCount)
        {
            sample.realPart *= a0 - a1 * Math.Cos(2 * Math.PI * sampleIndex / sampleCount) + a2 * Math.Cos(4 * Math.PI * sampleIndex / sampleCount) - a3 * Math.Cos(6 * Math.PI * sampleIndex / sampleCount);
            return (sample);

        }
    }
    class Tone
    {

    }
    class NoteDetector
    {
        const int range = 8;
        Note[] baseNotes = { new NoteC(1), new NoteCSharp(1), new NoteD(1), new NoteDSharp(1),
                         new NoteE(1), new NoteF(1), new NoteFSharp(1), new NoteG(1),
                         new NoteGSharp(1), new NoteA(1), new NoteASharp(1), new NoteB(1)};
        public Note GetClosestNote(double freq)
        {
            double minDeltaFreq = int.MaxValue;
            double deltaFreq;
            Note closestNote = null;
            for (int i = 1; i < range; i++)
            {
                for (int j = 0; j < baseNotes.Length; j++)
                {
                    deltaFreq = Math.Abs(freq - (baseNotes[j].baseFrequency * (2 << i - 1)));
                    if (minDeltaFreq > deltaFreq)
                    {
                        minDeltaFreq = deltaFreq;
                        closestNote = baseNotes[j];
                        closestNote.number = i;   
                    }
                }
            }
            return (closestNote);
        }
    }
    #region Notes
    abstract class Note
    {
        public int number;
        public readonly double baseFrequency;
        public int Length { get; set; }
        public Note ()
        {
        }
        public Note(int number, double baseFreq)
        {
            this.number = number;
            this.baseFrequency = baseFreq;
        }

    }
    class Pause : Note
    {
        public Pause()
        {
            this.Length = 1;
        }
    }
    class NoteC : Note
    {
        public NoteC(int number) : base(number, 32.70)
        {
            this.Length = 1;
        }
    }
    class NoteCSharp : Note
    {
        public NoteCSharp(int number) : base(number, 34.65)
        {
            this.Length = 1;
        }
    }
    class NoteD : Note
    {
        public NoteD(int number) : base(number, 36.71)
        {
            this.Length = 1;
        }
    }
    class NoteDSharp : Note
    {
        public NoteDSharp(int number) : base(number, 38.89)
        {
            this.Length = 1;
        }
    }
    class NoteE : Note
    {
        public NoteE(int number) : base(number, 41.20)
        {
            this.Length = 1;
        }
    }
    class NoteF : Note
    {
        public NoteF(int number) : base(number, 43.65)
        {
            this.Length = 1;
        }
    }
    class NoteFSharp : Note
    {
        public NoteFSharp(int number) : base(number, 46.25)
        {
            this.Length = 1;
        }
    }
    class NoteG : Note
    {
        public NoteG(int number) : base(number, 49.00)
        {
            this.Length = 1;
        }
    }
    class NoteGSharp : Note
    {
        public NoteGSharp(int number) : base(number, 51.91)
        {
            this.Length = 1;
        }
    }
    class NoteA : Note
    {
        public NoteA(int number) : base(number, 55.00)
        {
            this.Length = 1;
        }
    }
    class NoteASharp : Note
    {
        public NoteASharp(int number) : base(number, 58.27)
        {
            this.Length = 1;
        }
    }
    class NoteB : Note
    {
        public NoteB(int number) : base(number, 61.74)
        {
            this.Length = 1;
        }
    }
    interface INoiseDetector
    {
        bool IsNoise(Complex[] buffer);
    }
    class RMSNoiseDetector:INoiseDetector
    {
        readonly double amplitudeThreshold;
        public RMSNoiseDetector(double maxAmplitude)
        {
            amplitudeThreshold = maxAmplitude / 10;
        }
        public double CalculateRMSAmplitude(Complex[] buffer)
        {
            return Math.Sqrt(buffer.Sum(sample => sample.realPart * sample.realPart));
        }
        public bool IsNoise(Complex[] buffer)
        {
            if (CalculateRMSAmplitude(buffer) < amplitudeThreshold)
                return true;
            return false;
        }
    }
    class MusicSample
    {
        public readonly ProcessedMetaData metaData;
        public readonly string sourceName;
        public List<Note> Notes;
        public MusicSample(ProcessedMetaData metaData, string sourceName)
        {
            this.metaData = metaData;
            this.sourceName = sourceName;
        }

    }
    #endregion 
    struct MaxFreqBin
    {
        public int binNumber { get; private set; }
        public double value { get; private set;  }
        public void Set(int binNumber, double value)
        {
            this.binNumber = binNumber;
            this.value = value;
        }
        public MaxFreqBin(int binNumber, double value)
        {
            this.binNumber = binNumber;
            this.value = value;
        }
    }    
    static class MainManager
    {
        const int HPSIterationsCount = 5;
        static int chunkSize {get; set;} = 8192 * 2 ; //used for FFT, change so the chunksize is always constant in terms of number of real numbers, not bytes
                                                    //adjust chunksize accordint to metadata
        static int sampleArraySize { get; set; } 
        static private Complex[] GetComplexSamples(byte[] byteArray, int bitDepth, int channelsCount) //transforms bytes to doubles
        {
            Complex[] complexArray = new Complex[(chunkSize * 8 / (bitDepth * channelsCount)) + 1 ];
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
                        byteIndex  += 2;
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
        static private Complex[] ExtendSamples(Complex[] samples)
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

        static MaxFreqBin GetMaxFreqBin(Complex[] arrayAfterFFT)
        {
            MaxFreqBin max = new MaxFreqBin(0, (int.MinValue));
            for (int i = 0; i < arrayAfterFFT.Length / 2; i++)
            {
                if (Math.Abs(arrayAfterFFT[i].realPart / arrayAfterFFT.Length) > max.value)
                {
                    max.Set(i, Math.Sqrt(arrayAfterFFT[i].realPart.Sqr() + arrayAfterFFT[i].imaginaryPart.Sqr()) / arrayAfterFFT.Length);
                }
            }
            return (max);
        }
        static double CalculateMaxFreq(int maxAmplitudeIndex, ProcessedMetaData metaData, int samplesCount)
        {
            return (((double) maxAmplitudeIndex * metaData.sampleFreq /  samplesCount));
        }
        static private byte[] ExtractByteChannel(byte[] dualChannel, bool isEven)
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
        static internal void ProcessSoundFile(string inputFilePath) // ---> Main
        {

            #region processing header
            BinaryReader headerStream = new BinaryReader(File.Open(inputFilePath, FileMode.Open));
            ISoundReader soundReader = new WavSoundReader();
            ProcessedMetaData metaData = soundReader.ReadHeader(headerStream);
            sampleArraySize = (8 * chunkSize / (metaData.channelsCount * metaData.bitDepth)) +1 ;
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
            int count = 0;
            byte[] byteValues = soundReader.ReadDataBuffer(dataStream, chunkSize + (metaData.channelsCount * metaData.bitDepth / 8));
            StreamWriter writer = new StreamWriter("output.txt"); //temp
            while (dataStream.Position < dataStream.Length - chunkSize)
            {
                /*
                count++;
                if (count == 1000)
                {

                }*/
                complexSamples = GetComplexSamples(metaData.channelsCount == 1 ? byteValues : ExtractByteChannel(byteValues, true), metaData.bitDepth, metaData.channelsCount);
                INoiseDetector noiseDetector = new RMSNoiseDetector(2 << (metaData.bitDepth - 1));
                if (noiseDetector.IsNoise(complexSamples))
                    writer.WriteLine("Pause");
                else {
                    extendedComplexSamples = ExtendSamples(complexSamples);
                    //window.Windowify(extendedComplexSamples);
                    extendedComplexSamples = extendedComplexSamples.MakeFFT(extendedComplexSamples.Length, 0, 1);
                    //extendedComplexSamples = extendedComplexSamples.MakeHPS(HPSIterationsCount);
                    //StreamWriter writer = new StreamWriter("output.txt");
                    MaxFreqBin maxFreqBin = GetMaxFreqBin(extendedComplexSamples);
                    double maxFreq = CalculateMaxFreq(maxFreqBin.binNumber, metaData, sampleArraySize);
                    NoteDetector noteDetector = new NoteDetector();
                    Note detectedNote = noteDetector.GetClosestNote(maxFreq / 2);
                    writer.WriteLine("{0} ; {1}", detectedNote.GetType().Name, detectedNote.number);

                    /*for (int i = 0; i < extendedComplexSamples.Length; i++)
                    {
                        writer.Write("{0} ", i);
                        writer.Write("{0} ", extendedComplexSamples[i].realPart / extendedComplexSamples.Length);
                        writer.WriteLine("{0} ", extendedComplexSamples[i].imaginaryPart / extendedComplexSamples.Length);
                    }*/
                }
                writer.Flush();
                //writer.Close();
                
                soundReader.MoveDataBuffer(dataStream, window.OverlapSize, byteValues);
            }
            #endregion
        }
    }
}
