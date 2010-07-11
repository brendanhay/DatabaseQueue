using System;
using System.Collections.Generic;
using System.Linq;

namespace DatabaseQueue.Tests
{
    [Serializable]
    public class DummyEntity
    {
        #region Factory Methods

        public static DummyEntity Create()
        {
            return new DummyEntity(true);
        }

        public static ICollection<DummyEntity> CreateCollection()
        {
            return CreateCollection(10);
        }

        public static ICollection<DummyEntity> CreateCollection(int count)
        {
            return Enumerable.Range(1, count).Select(i => Create()).ToList();
        }

        #endregion

        internal DummyEntity() : this(false) { }

        public DummyEntity(bool defaults)
        {
            if (!defaults)
                return;

            Text = "Taumatawhakatangihangakoauauotamateapokaiwhenuakitanatahu";
            Number = int.MaxValue;
            Date = DateTime.UtcNow;
            Urls = new List<Uri> {
               new Uri("http://google.com"),
               new Uri("http://jobview.monster.com"),
               new Uri("http://stuff.co.nz"),
               new Uri("http://microsoft.com"),
               new Uri("http://seznam.cz"),
            };

            Nested = new List<DummyEntity>();
        }

        public string Text { get; set; }

        public int Number { get; set; }

        public DateTime Date { get; set; }

        public IList<Uri> Urls { get; set; }
        
        public ICollection<DummyEntity> Nested { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as DummyEntity;

            if (other == null)
                return false;

            return Text == other.Text
                && Number == other.Number
                && Date.ToShortTimeString() == other.Date.ToShortTimeString()
                && Urls.SequenceEqual(other.Urls);
        }
    }
}
