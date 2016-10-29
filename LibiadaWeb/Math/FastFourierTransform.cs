namespace LibiadaWeb.Math
{
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
        /// <returns>
        /// Spectrum of the signal as <see cref="T:double[][]"/>.
        /// </returns>
        public static double[][] CalculateFastFourierTransform(double[][] characteristics)
        {
            var powerOfTwo = PowerOfTwoCeiling(characteristics.Length);
            var result = new double[powerOfTwo][];
            for (int j = 0; j < powerOfTwo; j++)
            {
                result[j] = new double[characteristics[0].Length];
            }

            // transforming into complex representation
            // cycle through all characteristics
            for (int i = 0; i < characteristics[0].Length; i++)
            {
                var complex = new Complex[powerOfTwo];

                // cycle through all sequence fragments
                for (int j = 0; j < powerOfTwo; j++)
                {
                    complex[j] = j < characteristics.Length ? new Complex(characteristics[j][i], 0) : new Complex(0, 0);
                }

                Complex[] complexResult = CalculateFastFourierTransform(complex);

                // converting array to double
                for (int g = 0; g < powerOfTwo; g++)
                {
                    result[g][i] = complexResult[g].Real;
                }
            }

            return result;
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
        public static Complex[] CalculateFastFourierTransform(Complex[] x)
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

                even = CalculateFastFourierTransform(even);
                odd = CalculateFastFourierTransform(odd);
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
        /// Calculates nearest greater power of 2 for the given number.
        /// </summary>
        /// <param name="number">
        /// The number.
        /// </param>
        /// <returns>
        /// Prower of two as <see cref="int"/>.
        /// </returns>
        private static int PowerOfTwoCeiling(int number)
        {
            int powerOfTwo = 1;

            while (powerOfTwo < number)
            {
                powerOfTwo *= 2;
            }

            return powerOfTwo;
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
