using System;
using System.Collections.Generic;
using System.Text;

namespace NoteVisualizer
{
    interface IWindowFunction
    {
        /// <summary>
        /// number of bytes by which the windows should overlap
        /// </summary>
        int OverlapSize { get; }
        /// <summary>
        /// Applies Window function to the buffer - used to reduce "alligned error"
        /// </summary>
        /// <param name="samples"></param>
        void Windowify(Complex[] samples);
        /// <summary>
        /// Applies the window function to the specific sample in the buffer
        /// </summary>
        /// <param name="sample">sample to transform</param>
        /// <param name="sampleIndex"></param>
        /// <param name="sampleCount">count of all samples in a buffer</param>
        /// <returns></returns>
        Complex Calculate(Complex sample, int sampleIndex, int sampleCount);
    }
    /// <summary>
    /// Most frequently used window function to reduce "allign error"
    /// </summary>
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
    /// <summary>
    /// steeper window function - Reduces "allign error" more effectively but lowers frequency resolution
    /// </summary>
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
}
