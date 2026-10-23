namespace Libiada.Web.Tests.Math;

using Libiada.Web.Math;

[TestFixture(TestOf = typeof(ArrayAligner))]
public class ArrayAlignerTests
{
    [TestCase("Difference", 2, 5, -3)]
    [TestCase("Difference", 2, -5, -7)]
    [TestCase("Difference", -2, -5, -3)]
    [TestCase("Difference", 100, 100, 0)]
    [TestCase("NormalizedDifference", 2, 6, -0.5)]
    [TestCase("NormalizedDifference", -3, 6, -1)]
    [TestCase("NormalizedDifference", -10, -6, -0.25)]
    [TestCase("NormalizedDifference", 0, 0, 0)]
    [TestCase("NormalizedDifference", -2, 2, -1)]
    [TestCase("QuadraticNormalizedDifference", 2, 6, -2)]
    [TestCase("QuadraticNormalizedDifference", -3, 6, -9)]
    [TestCase("QuadraticNormalizedDifference", -10, -6, -1)]
    [TestCase("QuadraticNormalizedDifference", 0, 0, 0)]
    [TestCase("QuadraticNormalizedDifference", -2, 2, -4)]
    [TestCase("Similarity", 2, 5, 2)]
    [TestCase("Similarity", -2, -5, 2)]
    [TestCase("Similarity", -2, 5, 2)]
    [TestCase("Similarity", 100, 100, 100)]
    [TestCase("Equality", 5, 5.1, 1)]
    [TestCase("Equality", -10, -8, 0)]
    [TestCase("Equality", -10, -10, 1)]
    [TestCase("Equality", -1, 1, 0)]
    public void GetDistanceCalculatorTest(string validationType, double first, double second, double expected)
    {
        ArrayAligner arrayAligner = new();
        Func<double, double, double> calculator = arrayAligner.GetDistanceCalculator(validationType);

        Assert.That(calculator(first, second), Is.EqualTo(expected).Within(0.0001));
    }

    [Test]
    public void GetDistanceCalculatorTestUnknownValidationType()
    {
        ArrayAligner arrayAligner = new();
        ArgumentException exception = Assert.Throws<ArgumentException>(() => arrayAligner.GetDistanceCalculator("unknown"));

        Assert.That(exception.ParamName, Is.EqualTo("validationType"));
    }

    [Test]
    public void CalculateMeasureForAllRotationsTest()
    {
        ArrayAligner arrayAligner = new();
        List<double> first = [1, 2, 3];
        List<double> firstCopy = new(first);
        List<double> second = [3, 1, 2];

        (double[] distances, int optimalRotation) = arrayAligner.CalculateMeasureForAllRotations(
            first,
            second,
            (firstValue, secondValue) => firstValue == secondValue ? 1 : 0);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(distances, Is.EqualTo([0, 0, 3]));
            Assert.That(optimalRotation, Is.EqualTo(2));

            //check tat first array is the same after all rotations
            Assert.That(first, Is.EqualTo(firstCopy));
        }

        first = [2, -3, 0];
        firstCopy = new(first);
        second = [2, 3];

        (distances, optimalRotation) = arrayAligner.CalculateMeasureForAllRotations(
            first,
            second,
            (firstValue, secondValue) => firstValue - secondValue);

        using (Assert.EnterMultipleScope())
        {
            Assert.That(distances, Is.EqualTo([-6, -8, -3]));
            Assert.That(optimalRotation, Is.EqualTo(2));

            //check tat first array is the same after all rotations
            Assert.That(first, Is.EqualTo(firstCopy));
        }
    }
}
