namespace LibiadaWeb.Math
{
    using System.Collections.Generic;
    using System.Numerics;

    /// <summary>
    /// The fast fourier transform.
    /// </summary>
    public static class FastFourierTransform
    {
        /// <summary>
        /// The fourier transform.
        /// </summary>
        /// <param name="characteristics">
        /// The characteristics.
        /// </param>
        public static void FourierTransform(List<List<double>> characteristics)
        {
            // переводим в комлексный вид
            // cycle through all characteristics
            for (int i = 0; i < characteristics.Count; i++)
            {
                var complex = new List<Complex>();
                int j;

                // cycle through all sequence fragments
                for (j = 0; j < characteristics[i].Count; j++)
                {
                    complex.Add(new Complex(characteristics[i][j], 0));
                }

                int m = 1;

                while (m < j)
                {
                    m *= 2;
                }

                for (; j < m; j++)
                {
                    complex.Add(new Complex(0, 0));
                }

                Complex[] data = FourierTransform(complex.ToArray()); 

                // converting array to double
                for (int g = 0; g < characteristics[i].Count; g++)
                {
                    characteristics[i][g] = data[g].Real;
                }
            }
        }

        /// <summary>
        /// Calculates spectrum of given signal.
        /// </summary>
        /// <param name="x">
        /// Array of signal values. array size should be power of 2.
        /// </param>
        /// <returns>
        /// Spectrum of the signal. 
        /// </returns>
        public static Complex[] FourierTransform(Complex[] x)
        {
            Complex[] result;
            int n = x.Length;
            if (n == 2)
            {
                result = new Complex[2];
                result[0] = x[0] + x[1];
                result[1] = x[0] - x[1];
            }
            else
            {
                var even = new Complex[n / 2];
                var odd = new Complex[n / 2];
                for (int i = 0; i < n / 2; i++)
                {
                    even[i] = x[2 * i];
                    odd[i] = x[(2 * i) + 1];
                }

                even = FourierTransform(even);
                odd = FourierTransform(odd);
                result = new Complex[n];
                for (int i = 0; i < n / 2; i++)
                {
                    result[i] = even[i] + (W(i, n) * odd[i]);
                    result[i + (n / 2)] = even[i] - (W(i, n) * odd[i]);
                }
            }

            return result;
        }

        /// <summary>
        /// Calculates turning mod(?) e^(-i*2*PI*k/N).
        /// </summary>
        /// <param name="k">
        /// The k.
        /// </param>
        /// <param name="n">
        /// The n.
        /// </param>
        /// <returns>
        /// The <see cref="Complex"/>.
        /// </returns>
        private static Complex W(int k, int n)
        {
            if ((k % n) == 0)
            {
                return 1;
            }

            double arg = -2 * System.Math.PI * k / n;
            return new Complex(System.Math.Cos(arg), System.Math.Sin(arg));
        }
    }
}
