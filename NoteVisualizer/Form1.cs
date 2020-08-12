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

namespace NoteVisualizer
{
    //unify chunksize
    public partial class SoundVisualizer : Form
    {
        public SoundVisualizer()
        {
            InitializeComponent();
        }
        public void SetProgressBar()
        {
            progressBar.Value = (int)Math.Round(MainManager.Progress * 100);
        }
        private void browseButtonClick(object sender, EventArgs e)
        {
            var FD = new System.Windows.Forms.OpenFileDialog();
            if (FD.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                inputSampleTextBox.Text = FD.FileName;
            }
        }

        public void Done()
        {
            MessageBox.Show("You can now view processed file by clicking \"View\"", "Successfully Done");
        }
        
        private void processButtonClicked(object sender, EventArgs e)
        {
            progressBar.Maximum = 100;

            BaseFrequencies.TransposeDown(2);
            MainManager.ProcessSoundFile(inputSampleTextBox.Text, outputTextBox.Text, this);
        }
        private void viewResultClicked(object sender, EventArgs e)
        { 
        
        }
        private void label1_Click(object sender, EventArgs e)
        {
        }
        private void textBox1_TextChanged(object sender, EventArgs e)
        {
        }
    }
    static class Extensions
    {
        public static double Sqr(this double value)
        {
            return (value * value);
        }
        public static double GetNearest(this double value, double[] array)
        {
            double[] tempArray = new double[array.Length];
            for (int i = 0; i < array.Length; i++)
            {
                tempArray[i] = (value - array[i]).Sqr();
            }
            return array[Array.IndexOf(tempArray, tempArray.Min())];
        }
        public static Complex[] MakeFFT(this Complex[] input, int size, int startingIndex, int distance) //parallelized
        {
            const int minSize = 256;
            if (size > 1)
            {
                Complex[] even = new Complex[size / 2];
                Complex[] odd = new Complex[size / 2];
                if (size > minSize)
                {

                    Task task = new Task(() => { even = MakeFFT(input, size / 2, startingIndex, distance * 2); });
                    task.Start();

                    //Complex[] even = MakeFFT(input, size / 2, startingIndex, distance * 2);
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
        public int chunkSize = 8192 *2  + 4; //used for reading bytes
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
    class NoteDetector
    {
        const int range = 8;
        Note[] baseNotes = { new NoteC(1), new NoteCis(1), new NoteD(1), new NoteDis(1),
                         new NoteE(1), new NoteF(1), new NoteFis(1), new NoteG(1),
                         new NoteGis(1), new NoteA(1), new NoteAis(1), new NoteB(1)};
        public List<Type> noteTypes { get; set; }
        public NoteDetector()
        {
            noteTypes = new List<Type>();
            foreach (Note note in baseNotes)
            {
                noteTypes.Add(note.GetType());
            }
        }
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
    interface INoteWriter
    {
        void StartWrite(TextWriter writer /*possible arguments - predznamenanie*/);
        void EndWrite();
        void WriteNote(Note note);
        public void WriteAll(MusicSample sample, TextWriter writer);
    }
    class LillyPondNoteWriter : INoteWriter
    {
        const int MaxNoteLength = 64;
        const int MinNoteLength = 4;
        int NotesOnLine { get; set; }
        const int MaxNotesOnLine = 30; 
        private TextWriter writer;
        public void StartWrite(TextWriter writer /*possible arguments - predznamenanie*/)
        {
            writer.WriteLine("{");
            this.writer = writer;
        }
        public void EndWrite()
        {
            writer.WriteLine();
            writer.Write("}");
            writer.Close();
        }
        public void WriteNote(Note note)
        {
            writer.Write(" {0}", NoteToString(note));
            NotesOnLine++;
        }
        public void WriteBreak()
        {
            writer.Write(" \\bar \"\" \\break");
        }
        public void WriteAll(MusicSample sample, TextWriter writer)
        {
            StartWrite(writer);
            for (int i = 0; i < sample.Notes.Count; i++)
            {
                if (NotesOnLine == 30)
                {
                    WriteBreak();
                    NotesOnLine = 0;
                }

                WriteNote(sample.Notes[i]);
            }
            EndWrite();
        }
        private string NoteToString(Note note)
        {

            if (note.GetType() == typeof(Pause))
                return "r" + LengthToString(note);
            string str = note.GetType().Name.ToLower().Substring(4);
            int dashCount = note.number - 2;
            for (int i = 0; i < dashCount; i++)
            {
                str += "'";
            }
            for (int i = 0; i < -dashCount; i++)
            {
                str += ",";
            }
            return str + LengthToString(note);
        }
        private string LengthToString(Note note)
        {
            var position = Array.IndexOf(UniformNoteLengths.value, note.Length);
            var lengthString = ((int)Math.Round(MaxNoteLength / UniformNoteLengths.value[position - (position % 2)])).ToString(); //getting the base length of a note
            if (position % 2 == 1) //has odd position in noteLengths
            {
                lengthString += '.';
            }
            return lengthString;


        }
    }
    static class BaseFrequencies
    {
        public static List<NoteBaseFrequencyPair> GetBaseFreq = new List<NoteBaseFrequencyPair>();
        static BaseFrequencies()
        {
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteC).Name, 32.70));
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteCis).Name, 34.65));
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteD).Name, 36.71));
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteDis).Name, 38.89));
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteE).Name, 41.20));
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteF).Name, 43.65));
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteFis).Name, 46.25));
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteG).Name, 49.00));
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteGis).Name, 51.91));
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteA).Name, 55.00));
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteAis).Name, 58.27));
            GetBaseFreq.Add(new NoteBaseFrequencyPair(typeof(NoteB).Name, 61.74));
        }
        public static void TransposeUp(int count)
        {
            if (count == 0)
                return;
            var firstFreq = GetBaseFreq[0].BaseFreq;
            for (int i = 0; i < GetBaseFreq.Count - 1; i++)
            {
                GetBaseFreq[i].BaseFreq = GetBaseFreq[i + 1].BaseFreq;
            }
            GetBaseFreq[GetBaseFreq.Count - 1].BaseFreq = 2 * firstFreq;
            TransposeUp(count - 1);              
        }
        public static void TransposeDown(int count) //TOCheck
        {
            if (count == 0)
                return;
            var lastFreq = GetBaseFreq[GetBaseFreq.Count - 1].BaseFreq;

            for (int i = GetBaseFreq.Count - 1; i > 0; i--)
            {
                GetBaseFreq[i].BaseFreq = GetBaseFreq[i - 1].BaseFreq;
            }
            GetBaseFreq[0].BaseFreq = lastFreq / 2;
            TransposeDown(count - 1);
        }

    }
    class NoteBaseFrequencyPair
    {
        public string Note { get; set; }
        public double BaseFreq;
        public NoteBaseFrequencyPair(string Note, double BaseFreq)
        {
            this.Note = Note;
            this.BaseFreq = BaseFreq;
        }
       
    }
    #region Notes

    abstract class Note : IComparable<Note>
    {
        public int number { get; set; }
        public readonly double baseFrequency;
        public double Length { get; set; }
        public Note()
        { }
        public Note(int number, double baseFreq)
        {
            this.number = number;
            this.baseFrequency = baseFreq;
        }
        public override int GetHashCode()
        {
            var noteDet = new NoteDetector();
            if (this is Pause)
                return 0;
            return noteDet.noteTypes.IndexOf(this.GetType()) * 100 + number + 1;
        }
        public override bool Equals(object obj)
        {
            return ((obj.GetType() == this.GetType() && ((Note)obj).number == this.number));
        }
        public static bool operator ==(Note note1, Note note2)
        {
            return note1.Equals(note2);
        }
        public static bool operator !=(Note note1, Note note2)
        {
            return !note1.Equals(note2);
        }
        public static bool operator <=(Note note1, Note note2)
        {
            var noteDet = new NoteDetector();
            if (note1.GetType() == typeof(Pause) && note2.GetType() == typeof(Pause))
                return true;
            return (note2.number > note1.number || ((note2.number == note1.number) && noteDet.noteTypes.IndexOf(note2.GetType()) >= noteDet.noteTypes.IndexOf(note1.GetType())));
        }
        public static bool operator >=(Note note1, Note note2)
        {
            var noteDet = new NoteDetector();
            return note1 == note2 || !(note1 <= note2);
        }
        public static bool operator <(Note note1, Note note2)
        {
            var noteDet = new NoteDetector();
            return note1 <= note2 && note1 != note2;
        }
        public static bool operator >(Note note1, Note note2)
        {
            var noteDet = new NoteDetector();
            return note1 >= note2 && note1 != note2;
        }
        public int CompareTo(Note note)
        {
            if (this == note)
                return 0;
            var noteDet = new NoteDetector();
            if (this.number > note.number || ((this.number == note.number) && noteDet.noteTypes.IndexOf(this.GetType()) > noteDet.noteTypes.IndexOf(note.GetType())))
                return 1;
            return -1;
        }
    }
 
    class Pause : Note
    {
        public Pause() : base (0, 0)
        {
            this.Length = 1;
        }
    }
    class NoteC : Note
    {
        public NoteC(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteC).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteCis : Note
    {
        public NoteCis(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteCis).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteD : Note
    {
        public NoteD(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteD).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteDis : Note
    {
        public NoteDis(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteDis).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteE : Note
    {
        public NoteE(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteE).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteF : Note
    {
        public NoteF(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteF).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteFis : Note
    {
        public NoteFis(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteFis).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteG : Note
    {
        public NoteG(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteG).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteGis : Note
    {
        public NoteGis(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteGis).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteA : Note
    {
        public NoteA(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteA).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteAis : Note
    {
        public NoteAis(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteAis).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteB : Note
    {
        public NoteB(int number) : base(number, BaseFrequencies.GetBaseFreq.Find(x => x.Note == typeof(NoteB).Name).BaseFreq)
        {
            this.Length = 1;
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
    interface INoiseDetector
    {
        bool IsNoise(Complex[] buffer);
        double CurrentAmplitude(Complex[] buffer);
    }
    class RMSNoiseDetector : INoiseDetector
    {
        readonly double amplitudeThreshold;
        public RMSNoiseDetector(double maxAmplitude)
        {
            amplitudeThreshold = maxAmplitude / 8;
        }
        public double CurrentAmplitude(Complex[] buffer) => CalculateRMSAmplitude(buffer);
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
    interface IErrorCorrector
    {
        void Correct(MusicSample sample);
    }
    class OverlapWindowCorrector : IErrorCorrector 
    {
        List<Note> newNotes = new List<Note>();
        public void Correct(MusicSample origSample)
        {
            for (int i = 0; i < origSample.Notes.Count - 2; i++)
            {
                newNotes.Add(MedianOfThree(origSample.Notes[i], origSample.Notes[i + 1], origSample.Notes[i + 2]));
            }
            origSample.Notes = newNotes;
        }
        private Note MedianOfThree(Note note1, Note note2, Note note3)
        {
            if (note1 > note2)
            {
                if (note2 >= note3)
                    return note2;
                else if (note1 < note3)
                    return note1;
            }
            else
            {
                if (note2 < note3)
                    return note2;
                else if (note1 >= note3)
                    return note1;
            }
            return note3;
        }
    }
    class NoOverlapWindowCorrector : IErrorCorrector
    {
        List<Note> newNotes = new List<Note>();
        public void Correct(MusicSample origSample)
        {
            for (int i = 0; i < origSample.Notes.Count - 2; i+=3)
            {
                newNotes.Add(MedianOfThree(origSample.Notes[i], origSample.Notes[i + 1], origSample.Notes[i + 2]));
            }
            origSample.Notes = newNotes;
        }
        private Note MedianOfThree(Note note1, Note note2, Note note3)
        {
            if (note1 > note2)
            {
                if (note2 >= note3)
                    return note2;
                else if (note1 < note3)
                    return note1;
            }
            else
            {
                if (note2 < note3)
                    return note2;
                else if (note1 >= note3)
                    return note1;
            }
            return note3;
        }
    }
    interface INoteLengthProcessor
    {
        void ProcessSample(MusicSample sample);
    }
    internal static class UniformNoteLengths
    { 
        internal static double[] value = { 4, 6, 8, 12, 16, 24, 32, 48, 64 };
    }
    class DefaultNoteLengthProcessor : INoteLengthProcessor
    {
        const int maxLength = 64;
        const double frameSize = 1; //shortest measurable time frame
        List<Note> newNotes = new List<Note>();
        public DefaultNoteLengthProcessor()
        {
            for (int i = 0; i < UniformNoteLengths.value.Length; i++)
            {
                UniformNoteLengths.value[i] *= frameSize;
            }
        }
        public void ProcessSample(MusicSample sample)
        {
            SqueezeNotes(sample.Notes); //returns result to newNotes variable
            AllignToNearestLength(); //returns result to newNotes variable -- reusing of existing List
            sample.Notes = newNotes;
        }
        private void SqueezeNotes(List<Note> notes)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                AddNoteFraction(notes[i]);
            }
        }
        private void AddNoteFraction(Note note)
        {
            if (newNotes.Count == 0 || (!((newNotes[newNotes.Count - 1]) == note)  && newNotes[newNotes.Count - 1].Length < maxLength))
            {
                newNotes.Add(note);
            }
            else
            {
                newNotes[newNotes.Count - 1].Length += note.Length;
            }
        }
        private void AllignToNearestLength()
        {
            for (int i = 0; i < newNotes.Count; i++)
            {
                if (newNotes[i].Length > 1) //skipping notes of length 1 as they are probably errors
                    newNotes[i].Length = newNotes[i].Length.GetNearest(UniformNoteLengths.value);
                else
                    newNotes.RemoveAt(i);
            }
        }
    }
    static class MainManager
    {
        const int HPSIterationsCount = 5;
        public static double Progress { get; private set; }
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
        static internal void ProcessSoundFile(string inputFilePath, string outputFileName, SoundVisualizer form) // ---> Main
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

            byte[] byteValues = soundReader.ReadDataBuffer(dataStream, chunkSize + (metaData.channelsCount * metaData.bitDepth / 8));

            MusicSample musicSample = new MusicSample(metaData, inputFilePath);
            musicSample.Notes = new List<Note>();

            StreamWriter writer = new StreamWriter("output.txt"); //temp

            //noteWriter.StartWrite(writer2);

            while (dataStream.Position < dataStream.Length - chunkSize)
            {

                complexSamples = GetComplexSamples(metaData.channelsCount == 1 ? byteValues : ExtractByteChannel(byteValues, true), metaData.bitDepth, metaData.channelsCount);
                INoiseDetector noiseDetector = new RMSNoiseDetector(2 << (metaData.bitDepth - 1));
                if (noiseDetector.IsNoise(complexSamples))
                {
                    writer.WriteLine("Pause");
                    Note detectedNote = new Pause();
                    //noteWriter.WriteNote(detectedNote);

                    musicSample.Notes.Add(detectedNote);
                }
                else
                {
                    extendedComplexSamples = ExtendSamples(complexSamples);
                    window.Windowify(extendedComplexSamples);
                    extendedComplexSamples = extendedComplexSamples.MakeFFT(extendedComplexSamples.Length, 0, 1);
                    extendedComplexSamples = extendedComplexSamples.MakeHPS(HPSIterationsCount);
                    //StreamWriter writer = new StreamWriter("output.txt");
                    MaxFreqBin maxFreqBin = GetMaxFreqBin(extendedComplexSamples);
                    double maxFreq = CalculateMaxFreq(maxFreqBin.binNumber, metaData, sampleArraySize);
                    NoteDetector noteDetector = new NoteDetector();
                    Note detectedNote = noteDetector.GetClosestNote(maxFreq / 2);

                    writer.WriteLine("{0} ; {1}", detectedNote.GetType().Name, detectedNote.number);

                    //noteWriter.WriteNote(detectedNote);

                    musicSample.Notes.Add(detectedNote);
                    /*for (int i = 0; i < extendedComplexSamples.Length; i++)
                    {
                        writer3.Write("{0} ", i);
                        writer3.Write("{0} ", extendedComplexSamples[i].realPart / extendedComplexSamples.Length);
                        writer3.WriteLine("{0} ", extendedComplexSamples[i].imaginaryPart / extendedComplexSamples.Length);
                    }*/
                }
                writer.Flush();
                //writer2.Flush();
                //writer.Close();

                soundReader.MoveDataBuffer(dataStream, window.OverlapSize, byteValues);
                Progress = Math.Min(((double)dataStream.Position) / (dataStream.Length - chunkSize), 1);
                form.SetProgressBar();
                
            }
            IErrorCorrector corrector = new OverlapWindowCorrector();
            corrector.Correct(musicSample);

            INoteLengthProcessor noteLengthProcessor = new DefaultNoteLengthProcessor();
            noteLengthProcessor.ProcessSample(musicSample);

            StreamWriter writer2 = new StreamWriter(outputFileName + ".ly");
            INoteWriter noteWriter = new LillyPondNoteWriter();
            noteWriter.WriteAll(musicSample, writer2);
            form.Done();
            #endregion
        }
    }
}
