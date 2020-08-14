using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections;
using System.Collections.Generic;
using NoteVisualizer;
using System.Linq;
using System.Configuration;
using System.Drawing;

namespace UnitTestSoundVisualizer
{
    static class ComplexComparer
    {
        const double Epsilon = 0.00001;
        public static bool AreEqual(Complex[] first, Complex[] second)
        {
            if (first.Length != second.Length)
                return false;
            for (int i = 0; i < first.Length; i++)
            {
                if (!(IsApproxEqual(first[i].realPart, second[i].realPart) && IsApproxEqual(first[i].imaginaryPart, second[i].imaginaryPart)))
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsApproxEqual(double first, double second)
        {
                return first - Epsilon <= second && second <= first + Epsilon;
        }
    }

    [TestClass]
    public class FFTTest
    {

        [TestMethod]
        public void TestAllOnes()
        {
            //Arrange
            const int size = 1024;
            Complex[] array = new Complex[size];
            Complex[] expected = new Complex[size];
            for (int i = 0; i < array.Length; i++)
            {
                expected[i] = new Complex() { realPart = 0, imaginaryPart = 0 };
                array[i] = new Complex() { realPart = 1, imaginaryPart = 0 };
            }
            expected[0].realPart = size;

            //Act
            var result = Extensions.MakeFFT(array, array.Length, 0, 1);

            //Assert
            Assert.IsTrue(ComplexComparer.AreEqual(result, expected));
        }
        [TestMethod]
        public void TestAllZeros()
        {
            //Arrange
            const int size = 1024;
            Complex[] array = new Complex[size];
            Complex[] expected = new Complex[size];
            for (int i = 0; i < array.Length; i++)
            {
                expected[i] = new Complex() { realPart = 0, imaginaryPart = 0 };
                array[i] = new Complex() { realPart = 0, imaginaryPart = 0 };
            }

            //Act
            var result = Extensions.MakeFFT(array, array.Length, 0, 1);

            //Assert
            Assert.IsTrue(ComplexComparer.AreEqual(result, expected));
        }
        [TestMethod]
        public void TestOnesAndZeros()
        {
            //Arrange
            const int size = 1024;
            Complex[] array = new Complex[size];
            Complex[] expected = new Complex[size];
            for (int i = 0; i < array.Length; i++)
            {
                expected[i] = new Complex() { realPart = 0, imaginaryPart = 0 };
                if (i % 2 == 0)
                {
                    array[i] = new Complex() { realPart = 1, imaginaryPart = 0 };
                }
                else 
                {
                    array[i] = new Complex() { realPart = 0, imaginaryPart = 0 };
                }
            }
            expected[0].realPart = size / 2;
            expected[size / 2].realPart = size / 2;

            //Act
            var result = Extensions.MakeFFT(array, array.Length, 0, 1);

            //Assert
            Assert.IsTrue(ComplexComparer.AreEqual(result, expected));
        }
        [TestMethod]
        public void TestSequence()
        {
            //Arrange
            const int size = 4;
            Complex[] array = new Complex[size];
            Complex[] expected =
            {
                new Complex() { realPart = 10, imaginaryPart = 10 },
                new Complex() { realPart = 0, imaginaryPart = -4 },
                new Complex() { realPart = -2, imaginaryPart = -2 },
                new Complex() { realPart = -4, imaginaryPart = 0 }
            };
            for (int i = 0; i < array.Length; i++)
            {
                array[i] = new Complex() { realPart = i + 1, imaginaryPart = i + 1 }; 
            }

            //Act
            var result = Extensions.MakeFFT(array, array.Length, 0, 1);

            //Assert
            Assert.IsTrue(ComplexComparer.AreEqual(result, expected));
        }
    }
}
