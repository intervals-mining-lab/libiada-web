namespace Libiada.Web.Math;

public class ArrayAligner : IArrayAligner
{
    /// <summary>
    /// The distance calculator factory method.
    /// For all functions, the greater the produced value, the better the match.
    /// For "Difference" and "NormalizedDifference" values are always negative,
    /// so closer they are to zero the better the match is.
    /// </summary>
    /// <param name="validationType">
    /// The validation type.
    /// </param>
    /// <returns>
    /// The <see cref="Func{double, double, double}"/>.
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Thrown if validation type is unknown.
    /// </exception>
    public Func<double, double, double> GetDistanceCalculator(string validationType)
    {
        return validationType switch
        {
            "Difference" => (first, second) => -System.Math.Abs(first - second),
            "NormalizedDifference" => (first, second) => first == second ? 0 : -System.Math.Abs((first - second)
                                    / (System.Math.Abs(first) + System.Math.Abs(second))),
            "QuadraticNormalizedDifference" => (first, second) => first == second ? 0 : -(first - second) * (first - second)
                                    / (System.Math.Abs(first) + System.Math.Abs(second)),
            "Similarity" => (first, second) => System.Math.Min(System.Math.Abs(first), System.Math.Abs(second)),
            "Equality" => (first, second) => System.Math.Abs(first - second) < (System.Math.Abs(first + second) / 20) ? 1 : 0,
            _ => throw new ArgumentException("unknown validation type", nameof(validationType)),
        };
    }

    /// <summary>
    /// Calculates measure for all possible cyclic rotations of the first array.
    /// </summary>
    /// <param name="first">
    /// The first values list.
    /// </param>
    /// <param name="second">
    /// The second values list.
    /// </param>
    /// <param name="measure">
    /// The distance calculation function.
    /// </param>
    /// <returns>
    /// The tuple of distances array for all rotations and optimal rotation index.
    /// </returns>
    public (double[] distances, int optimalRotation) CalculateMeasureForAllRotations(
        List<double> first,
        List<double> second,
        Func<double, double, double> measure)
    {
        double[] distances = new double[first.Count];
        int optimalRotation = 0;
        double maximumEquality = double.MinValue;
        for (int i = 0; i < first.Count; i++)
        {
            double distance = Measure(first, second, measure);
            if (maximumEquality < distance)
            {
                optimalRotation = i;
                maximumEquality = distance;
            }

            distances[i] = distance;
            Rotate(first);
        }

        return (distances, optimalRotation);
    }

    /// <summary>
    /// Rotates list cyclicly.
    /// </summary>
    /// <param name="list">
    /// The list to rotate.
    /// </param>
    private void Rotate(List<double> list)
    {
        double first = list[0];
        list.RemoveAt(0);
        list.Add(first);
    }

    /// <summary>
    /// Calculates measure for two lists with given metric.
    /// </summary>
    /// <param name="first">
    /// The first list.
    /// </param>
    /// <param name="second">
    /// The second list.
    /// </param>
    /// <param name="measure">
    /// The distance calculation function.
    /// </param>
    /// <returns>
    /// The calculated measure.
    /// </returns>
    private double Measure(List<double> first, List<double> second, Func<double, double, double> measure)
    {
        double result = 0;
        for (int i = 0; i < second.Count; i++)
        {
            result += measure(first[i], second[i]);
        }

        return result;
    }
}
