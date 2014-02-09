using System;

namespace LibiadaWeb.Models
{
    public class AutoCorrelation
    {
        public double[] Execute(double[] x)
        {
            double[] result = GetAutoCorrelationOfSeries(x);
            return result;
        }

        public double GetAverage(double[] data)
        {
            int len = data.Length;

            if (len == 0)
                throw new Exception("No data");

            double sum = 0;

            for (int i = 0; i < data.Length; i++)
                sum += data[i];

            return sum / len;
        }

        public double GetVariance(double[] data)
        {
            int len = data.Length;

            // Get average
            double avg = GetAverage(data);

            double sum = 0;

            for (int i = 0; i < data.Length; i++)
                sum += Math.Pow((data[i] - avg), 2);

            return sum / len;
        }
        public double GetStdev(double[] data)
        {
            return Math.Sqrt(GetVariance(data));
        }

        public double GetCorrelation(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new Exception("Length of sources is different");
            double avgX = GetAverage(x);
            double stdevX = GetStdev(x);
            double avgY = GetAverage(y);
            double stdevY = GetStdev(y);
            double covXY = 0;
            double pearson = 0;
            int len = x.Length;
            for (int i = 0; i < len; i++)
                covXY += (x[i] - avgX) * (y[i] - avgY);
            covXY /= len;
            pearson = covXY / (stdevX * stdevY);
            return pearson;
        }

        public double[] GetAutoCorrelationOfSeries(double[] x)
        {
            int half = x.Length / 2;
            double[] autoCorrelation = new double[half];
            double[] a = new double[half];
            double[] b = new double[half];
            for (int i = 0; i < half; i++)
            {
                a[i] = x[i];
                b[i] = x[i + i];
                autoCorrelation[i] = GetCorrelation(a, b);
                if (i % 1000 == 0)
                    Console.WriteLine(i);
            }
            return autoCorrelation;
        }
    }
}
