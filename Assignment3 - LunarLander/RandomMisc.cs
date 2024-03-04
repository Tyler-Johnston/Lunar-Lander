
using Microsoft.Xna.Framework;
using System;

namespace Random
{
    public class RandomMisc
    {
        private RandomMT m_random = new RandomMT();

        public RandomMisc()
        {
            DateTime currentTime = DateTime.Now;
            long milliseconds = currentTime.Ticks / TimeSpan.TicksPerMillisecond;
            m_random.seed((ulong)milliseconds);
        }

        public ulong next()
        {
            return m_random.next();
        }

        public double nextDouble()
        {
            return m_random.nextDouble();
        }

        public int nextRange(int low, int high)
        {
            int range = high - low + 1;
            return (int)Math.Floor(m_random.nextDouble() * range) + low;
        }

        public System.Numerics.Vector2 nextCircleVector()
        {
            float angle = (float)(m_random.nextDouble() * 2 * Math.PI);
            return new System.Numerics.Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));
        }

        public double nextGaussian(double mean, double stdDev)
        {
            if (usePrevious)
            {
                usePrevious = false;
                return mean + y2 * stdDev;
            }

            usePrevious = true;

            double x1 = 0;
            double x2 = 0;
            double y1 = 0;
            double z = 0;

            do
            {
                x1 = 2 * nextDouble() - 1;
                x2 = 2 * nextDouble() - 1;
                z = (x1 * x1) + (x2 * x2);
            } while (z >= 1);

            z = Math.Sqrt((-2 * Math.Log(z)) / z);
            y1 = x1 * z;
            y2 = x2 * z;

            return mean + y1 * stdDev;
        }
        private bool usePrevious = false;
        double y2;
    }
}
