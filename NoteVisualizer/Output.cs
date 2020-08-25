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
    /// Writes notes to the given file base on the output format
    /// </summary>
    interface INoteWriter
    {
        void StartWrite(TextWriter writer);
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
        public void StartWrite(TextWriter writer)
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
                if (NotesOnLine == MaxNotesOnLine)
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
            if (position % 2 == 1) //has odd position in noteLengths means dot is needed
            {
                lengthString += '.';
            }
            return lengthString;


        }
    }
}
