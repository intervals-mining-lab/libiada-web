// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AutoCorelation.cs" company="">
//   
// </copyright>
// <summary>
//   The auto correlation.
// </summary>
// --------------------------------------------------------------------------------------------------------------------



using System;

namespace LibiadaWeb.Models
{
    /// <summary>
    /// The auto correlation.
    /// </summary>
    public class AutoCorrelation
    {
        /// <summary>
        /// The execute.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <returns>
        /// The <see cref="double[]"/>.
        /// </returns>
        public double[] Execute(double[] x)
        {
            double[] result = this.GetAutoCorrelationOfSeries(x);
            return result;
        }

        /// <summary>
        /// The get average.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
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

        /// <summary>
        /// The get variance.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        public double GetVariance(double[] data)
        {
            int len = data.Length;

            // Get average
            double avg = this.GetAverage(data);

            double sum = 0;

            for (int i = 0; i < data.Length; i++)
                sum += Math.Pow(data[i] - avg, 2);

            return sum / len;
        }

        /// <summary>
        /// The get stdev.
        /// </summary>
        /// <param name="data">
        /// The data.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        public double GetStdev(double[] data)
        {
            return Math.Sqrt(this.GetVariance(data));
        }

        /// <summary>
        /// The get correlation.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <param name="y">
        /// The y.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        /// <exception cref="Exception">
        /// </exception>
        public double GetCorrelation(double[] x, double[] y)
        {
            if (x.Length != y.Length)
                throw new Exception("Length of sources is different");
            double avgX = this.GetAverage(x);
            double stdevX = this.GetStdev(x);
            double avgY = this.GetAverage(y);
            double stdevY = this.GetStdev(y);
            double covXY = 0;
            double pearson = 0;
            int len = x.Length;
            for (int i = 0; i < len; i++)
                covXY += (x[i] - avgX) * (y[i] - avgY);
            covXY /= len;
            pearson = covXY / (stdevX * stdevY);
            return pearson;
        }

        /// <summary>
        /// The get auto correlation of series.
        /// </summary>
        /// <param name="x">
        /// The x.
        /// </param>
        /// <returns>
        /// The <see cref="double[]"/>.
        /// </returns>
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
                autoCorrelation[i] = this.GetCorrelation(a, b);
                if (i % 1000 == 0)
                    Console.WriteLine(i);
            }

            return autoCorrelation;
        }
    }
}
