namespace Libiada.Web.Tests.Math;

using Libiada.Web.Math;
using System.Numerics;


[TestFixture]
public class FastFourierTransformTests
{
    /// <summary>
    /// Calculates the fast fourier transform array test.
    /// </summary>
    [Test]
    public void CalculateFastFourierTransformArrayTest()
    {
        double[][] input = new double[][]
        {
            new[] { 1.0, 2.0 },
            new[] { 3.0, 4.0 }
        };

        double[][] result = FastFourierTransform.CalculateFastFourierTransform(input);

        // First row sum: [1.0, 2.0] + [3.0, 4.0] = [4.0, 6.0]
        // First row difference: [1.0, 2.0] - [3.0, 4.0] = [-2.0, -2.0]
        Assert.Multiple(() =>
        {
            Assert.That(result[0][0], Is.EqualTo(4.0).Within(0.0001));
            Assert.That(result[0][1], Is.EqualTo(6.0).Within(0.0001));
            Assert.That(result[1][0], Is.EqualTo(-2.0).Within(0.0001));
            Assert.That(result[1][1], Is.EqualTo(-2.0).Within(0.0001));
        });
    }

    /// <summary>
    /// Calculates the fast fourier transform complex test.
    /// </summary>
    [Test]
    public void CalculateFastFourierTransformComplexTest()
    {
        Complex[] input = new Complex[]
        {
            new Complex(1, 0),
            new Complex(2, 0)
        };

        Complex[] result = FastFourierTransform.CalculateFastFourierTransform(input);

        // Sum: 1 + 2 = 3
        // Difference: 1 - 2 = -1
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Real, Is.EqualTo(3.0).Within(0.0001));
            Assert.That(result[0].Imaginary, Is.EqualTo(0.0).Within(0.0001));
            Assert.That(result[1].Real, Is.EqualTo(-1.0).Within(0.0001));
            Assert.That(result[1].Imaginary, Is.EqualTo(0.0).Within(0.0001));
        });
    }

    /// <summary>
    /// Calculates the fast fourier transform larger array test.
    /// </summary>
    [Test]
    public void CalculateFastFourierTransformLargerArrayTest()
    {
        Complex[] input = new Complex[]
        {
            new Complex(1, 0),
            new Complex(2, 0),
            new Complex(3, 0),
            new Complex(4, 0)
        };

        Complex[] result = FastFourierTransform.CalculateFastFourierTransform(input);

        // Sum: 1 + 2 + 3 + 4 = 10
        // First harmonic: (1 - 3) + i(2 - 4) = -2 + 2i
        // Second harmonic: 1 - 2 + 3 - 4 = -2
        // Third harmonic: (1 - 3) - i(2 - 4) = -2 - 2i
        Assert.Multiple(() =>
        {
            Assert.That(result[0].Real, Is.EqualTo(10.0).Within(0.0001));
            Assert.That(result[0].Imaginary, Is.EqualTo(0.0).Within(0.0001));
            Assert.That(result[1].Real, Is.EqualTo(-2.0).Within(0.0001));
            Assert.That(result[1].Imaginary, Is.EqualTo(2.0).Within(0.0001));
            Assert.That(result[2].Real, Is.EqualTo(-2.0).Within(0.0001));
            Assert.That(result[2].Imaginary, Is.EqualTo(0.0).Within(0.0001));
            Assert.That(result[3].Real, Is.EqualTo(-2.0).Within(0.0001));
            Assert.That(result[3].Imaginary, Is.EqualTo(-2.0).Within(0.0001));
        });
    }

    /// <summary>
    /// Calculates the fast fourier transform padding test.
    /// </summary>
    [Test]
    public void CalculateFastFourierTransformPaddingTest()
    {
        double[][] input = new double[][]
        {
            new[] { 1.0, 2.0 },
            new[] { 3.0, 4.0 },
            new[] { 5.0, 6.0 },
            new[] { 7.0, 8.0 },
            new[] { 9.0, 10.0 },
            new[] { 11.0, 12.0 },
            new[] { 13.0, 14.0 },
            new[] { 15.0, 16.0 },
            new[] { 17.0, 18.0 },
            new[] { 19.0, 20.0 },
            new[] { 21.0, 22.0 },
            new[] { 23.0, 24.0 },
            new[] { 25.0, 26.0 }
        };

        double[][] result = FastFourierTransform.CalculateFastFourierTransform(input);

        // Testing first few harmonics as example
        Assert.Multiple(() =>
        {
            Assert.That(result[0][0], Is.EqualTo(169.0).Within(0.0001)); 
            Assert.That(result[0][1], Is.EqualTo(182.0).Within(0.0001)); 
            Assert.That(result[1][0], Is.EqualTo(-75.4788).Within(0.0001));
            Assert.That(result[1][1], Is.EqualTo(-77.4925).Within(0.0001));
            Assert.That(result[2][0], Is.EqualTo(-18.8284).Within(0.0001));
            Assert.That(result[2][1], Is.EqualTo(-18.8284).Within(0.0001));
            Assert.That(result[3][0], Is.EqualTo(17.5876).Within(0.0001));
            Assert.That(result[3][1], Is.EqualTo(18.8359).Within(0.0001));
        });
    }
}
