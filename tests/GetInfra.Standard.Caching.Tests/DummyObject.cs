namespace GetInfra.Caching.Tests
{
    public class DummyObject 
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public override bool Equals(object other)
        {
            var toCompareWith = other as DummyObject;
            if (toCompareWith == null)
                return false;
            return this.Id == toCompareWith.Id &&
                this.Name == toCompareWith.Name;
        }
    }
}
