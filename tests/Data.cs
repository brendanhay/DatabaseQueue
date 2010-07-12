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

            char ch;

            for (var i = 0; i < size; i++)
            {
                ch = Convert.ToChar(Convert.ToInt32(Math.Floor(26 * _random.NextDouble() + 65)));

                builder.Append(ch);
            }

            return builder.ToString();
        }

    }
}
