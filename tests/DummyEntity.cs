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
            return new DummyEntity();
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

        public DummyEntity() : this(true) { }

        private DummyEntity(bool nested)
        {
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
            
            //if (!nested)
            //    return;

            Nested = new List<DummyEntity>(); //Enumerable.Range(1, 10).Select(i => new DummyEntity(false)).ToList();
        }

        public string Text { get; private set; }

        public int Number { get; private set; }

        public DateTime Date { get; private set; }

        public IList<Uri> Urls { get; private set; }
        
        public ICollection<DummyEntity> Nested { get; private set; }

        public override bool Equals(object obj)
        {
            return false;

            var other = obj as DummyEntity;

            if (other == null)
                return false;

            return Text == other.Text
                && Number == other.Number
                && Date == other.Date
                && Urls.SequenceEqual(other.Urls);
        }
    }
}
