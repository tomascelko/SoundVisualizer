using System;
using System.Collections.Generic;
using System.Text;

namespace NoteVisualizer
{
    /// <summary>
    /// object used to find the closest Note to the given frequency and Transposition chosen by the user
    /// </summary>
    class NoteDetector
    {
        const int range = 8;
        /// <summary>
        /// Used as implementation detail of finding the closest note
        /// </summary>
        Note[] baseNotes = { new NoteC(1), new NoteCis(1), new NoteD(1), new NoteDis(1),
                         new NoteE(1), new NoteF(1), new NoteFis(1), new NoteG(1),
                         new NoteGis(1), new NoteA(1), new NoteAis(1), new NoteB(1)};
        /// <summary>
        /// List ( not dictionary because we need to enumerate in order) that contains key and value pairs to find base frequency based on the note Type when creating new note of give type
        /// </summary>
        public static List<NoteBaseFrequencyPair> GetBaseFreq = new List<NoteBaseFrequencyPair>();
        public static int CurrentTranspo { get; set; }
        static NoteDetector()
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
            CurrentTranspo = 0;
        }
        /// <summary>
        /// Changes GetBaseFreq to reflect tuning of different instruments
        /// </summary>
        /// <param name="count">number of Half-tones to transpose</param>
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
            CurrentTranspo++;
            TransposeUp(count - 1);
        }
        /// <summary>
        /// Analogical to TransposeUp method
        /// </summary>
        public static void TransposeDown(int count)
        {
            if (count == 0)
                return;
            var lastFreq = GetBaseFreq[GetBaseFreq.Count - 1].BaseFreq;

            for (int i = GetBaseFreq.Count - 1; i > 0; i--)
            {
                GetBaseFreq[i].BaseFreq = GetBaseFreq[i - 1].BaseFreq;
            }
            GetBaseFreq[0].BaseFreq = lastFreq / 2;
            CurrentTranspo--;
            TransposeDown(count - 1);
        }
        /// <summary>
        /// List that determines the order of notes in a scale
        /// </summary>
        public List<Type> noteTypes { get; private set; }
        /// <summary>
        /// initializes order of notes in a scale
        /// </summary>
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
    class NoteBaseFrequencyPair
    {
        public string Note { get; set; }
        public double BaseFreq { get; set; }
        public NoteBaseFrequencyPair(string Note, double BaseFreq)
        {
            this.Note = Note;
            this.BaseFreq = BaseFreq;
        }

    }
}
