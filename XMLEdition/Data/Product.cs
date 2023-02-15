using System.Xml.Serialization;

namespace XMLEdition.Data
{
    [Serializable]
    [XmlRoot("Products")]
    public class Product
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Price { get; set; }

        public string Vendor { get; set; }

        public string Storage { get; set; }

        public int Count { get; set; }
    }
}
