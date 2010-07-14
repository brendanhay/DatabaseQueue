using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseQueue.Tests
{
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

            Text = Data.RandomString(250);
            Number = int.MaxValue;
            Date = DateTime.UtcNow;
            Urls = Enumerable.Range(0, 5).Select(i => Data.RandomString(20)).ToList();
            Empty = new List<Entity>();
        }

        public string Text { get; set; }

        public int Number { get; set; }

        public DateTime Date { get; set; }

        public List<string> Urls { get; set; }
        
        public List<Entity> Empty { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as Entity;

            if (other == null)
                return false;

            return Text == other.Text
                && Number == other.Number
                && Date.ToShortTimeString() == other.Date.ToShortTimeString()
                && Urls.SequenceEqual(other.Urls)
                && Empty.SequenceEqual(other.Empty);
        }
    }
}
