using System;
using System.Text;

namespace DatabaseQueue.Benchmark
{
    public static class RandomHelper
    {
        private static readonly Random _random = new Random((int)DateTime.Now.Ticks);

        public static string GetString(int size)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65)));

                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static int GetInt32(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}
