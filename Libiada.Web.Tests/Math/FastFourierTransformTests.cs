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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result[0].Length, Is.EqualTo(2));
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.EqualTo(2));
        Assert.That(result[0].Real, Is.EqualTo(3.0).Within(0.0001));
        Assert.That(result[1].Real, Is.EqualTo(-1.0).Within(0.0001));
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

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.EqualTo(4));
        Assert.That(result[0].Real, Is.EqualTo(10.0).Within(0.0001));
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
            new[] { 5.0, 6.0 }
        };

        double[][] result = FastFourierTransform.CalculateFastFourierTransform(input);

        Assert.That(result, Is.Not.Null);
        Assert.That(result.Length, Is.EqualTo(4));
        Assert.That(result[0].Length, Is.EqualTo(2));
    }
}
