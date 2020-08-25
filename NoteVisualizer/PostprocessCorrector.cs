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
    /// Reduces the "single note error"
    /// </summary>
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
            for (int i = 0; i < origSample.Notes.Count - 2; i += 3)
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

}
