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

        public static List<DummyEntity> CreateCollection()
        {
            return CreateCollection(10);
        }

        public static List<DummyEntity> CreateCollection(int count)
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
            Urls = new List<string> {
               "http://google.com",
               "http://jobview.monster.com",
               "http://stuff.co.nz",
               "http://microsoft.com",
               "http://seznam.cz",
            };

            Nested = new List<DummyEntity>();
        }

        public string Text { get; set; }

        public int Number { get; set; }

        public DateTime Date { get; set; }

        public List<string> Urls { get; set; }
        
        public List<DummyEntity> Nested { get; set; }

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
