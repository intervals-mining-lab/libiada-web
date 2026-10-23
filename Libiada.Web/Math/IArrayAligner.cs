namespace Libiada.Web.Math;

public interface IArrayAligner
{
    Func<double, double, double> GetDistanceCalculator(string validationType);
    public (double[] distances, int optimalRotation) CalculateMeasureForAllRotations(
           List<double> first,
           List<double> second,
           Func<double, double, double> measure);
}
