using System;
using System.Text;

namespace DatabaseQueue.Tests
{
    public static class Data
    {
        private static readonly Random _random = new Random((int)DateTime.Now.Ticks);

        public static string RandomString(int size)
        {
            var builder = new StringBuilder();

            for (var i = 0; i < size; i++)
            {
                var ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65)));

                builder.Append(ch);
            }

            return builder.ToString();
        }

        public static int Random(int min, int max)
        {
            return _random.Next(min, max);
        }
    }
}
