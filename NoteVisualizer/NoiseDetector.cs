using System;
using System.Collections.Generic;
using System.Text;

namespace NoteVisualizer
{
    interface INoiseDetector
    {
        /// <summary>
        /// determines whether the buffer contains valid sound or noise
        /// </summary>
        /// <param name="buffer"></param>
        /// <returns></returns>
        bool IsNoise(Complex[] buffer);
        double CurrentAmplitude(Complex[] buffer);
    }
    /// <summary>
    /// Noise detector based on root median square
    /// </summary>
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
}
