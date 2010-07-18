using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseQueue.Benchmark
{
    /// <summary>
    /// Test entity used by the benchmarker and unit tests
    /// </summary>
    [Serializable]
    public class Entity
    {
        #region Factory Methods

        public static Entity Create()
        {
            return new Entity(true);
        }

        public static ICollection<Entity> CreateCollection()
        {
            return CreateCollection(10);
        }

        public static ICollection<Entity> CreateCollection(int count)
        {
            return Enumerable.Range(1, count).Select(i => Create()).ToList();
        }

        #endregion

        internal Entity() : this(false) { }

        public Entity(bool defaults)
        {
            if (!defaults)
                return;

            Text = RandomHelper.GetString(255);
            Positive = Int64.MaxValue;
            Negative = Int64.MinValue;
            Date = DateTime.UtcNow;
            Urls = Enumerable.Range(0, 10).Select(i => RandomHelper.GetString(255)).ToList();
        }

        public string Text { get; set; }

        public Int64 Positive { get; set; }

        public Int64 Negative { get; set; }

        public DateTime Date { get; set; }

        public List<string> Urls { get; set; }
        
        public override bool Equals(object obj)
        {
            var other = obj as Entity;

            if (other == null)
                return false;

            return Text == other.Text
                && Positive == other.Positive
                && Negative == other.Negative
                && Date.ToShortTimeString() == other.Date.ToShortTimeString()
                && Urls.SequenceEqual(other.Urls);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
