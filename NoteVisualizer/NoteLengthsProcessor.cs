using System;
using System.Collections.Generic;
using System.Text;

namespace NoteVisualizer
{
    /// <summary>
    /// determines the final length of a note
    /// </summary>
    interface INoteLengthProcessor
    {
        void ProcessSample(MusicSample sample);
    }
    /// <summary>
    /// Valid lengths for a note
    /// </summary>
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
        /// <summary>
        /// Squeezes uninterupted section of shorter notes into one longer note
        /// </summary>
        /// <param name="notes"></param>
        private void SqueezeNotes(List<Note> notes)
        {
            for (int i = 0; i < notes.Count; i++)
            {
                AddNoteFraction(notes[i]);
            }
        }
        private void AddNoteFraction(Note note)
        {
            if (newNotes.Count == 0 || (!((newNotes[newNotes.Count - 1]) == note) && newNotes[newNotes.Count - 1].Length < maxLength))
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
}
