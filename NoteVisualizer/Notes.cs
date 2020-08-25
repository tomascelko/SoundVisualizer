using System;
using System.Collections.Generic;
using System.Text;

namespace NoteVisualizer
{
    #region Notes
    /// <summary>
    /// class with default implementation of methods for all notes
    /// </summary>
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
        public Pause() : base(0, 0)
        {
            this.Length = 1;
        }
    }
    class NoteC : Note
    {
        public NoteC(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteC).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteCis : Note
    {
        public NoteCis(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteCis).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteD : Note
    {
        public NoteD(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteD).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteDis : Note
    {
        public NoteDis(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteDis).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteE : Note
    {
        public NoteE(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteE).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteF : Note
    {
        public NoteF(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteF).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteFis : Note
    {
        public NoteFis(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteFis).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteG : Note
    {
        public NoteG(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteG).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteGis : Note
    {
        public NoteGis(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteGis).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteA : Note
    {
        public NoteA(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteA).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteAis : Note
    {
        public NoteAis(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteAis).Name).BaseFreq)
        {
            this.Length = 1;
        }
    }
    class NoteB : Note
    {
        public NoteB(int number) : base(number, NoteDetector.GetBaseFreq.Find(x => x.Note == typeof(NoteB).Name).BaseFreq)
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
}
