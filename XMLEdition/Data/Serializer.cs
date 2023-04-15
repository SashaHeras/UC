using System.IO;
using System.Xml.Serialization;

namespace XMLEdition.Data
{  
    public class Serializer
    {
        public static List<Product> products = new List<Product>();

        // Створення списку продуктів та файлу з ними
        public static void Create()
        {
            products = new List<Product>()
            {
                new Product()
                {
                    Id = 1,
                    Name = "Годинник Bitux",
                    Price = 200,
                    Vendor = "SDB",
                    Storage = "Додатковий",
                    Count = 100
                },
                new Product()
                {
                    Id = 2,
                    Name = "Мобільний телефон Samsung A9",
                    Price = 10000,
                    Vendor = "Samsung",
                    Storage = "Головний",
                    Count = 100
                }
            };

            SerializeToXml(products);
        }

        // Зчитування продуктів з файлу
        public static void Read()
        {
            DeserializeFromXml();
        }

        // Додавання продукту до файлу
        public static void Add(Product product)
        {
            products.Add(product);
            SerializeToXml(products);
        }

        // Редагування продукту в файлі
        public static void Update(Product product)
        {
            var prod = products.Where(p=>p.Id == product.Id).FirstOrDefault();
            products.Remove(prod);
            products.Add(product);
            SerializeToXml(products);
        }

        // Видалення продукту з файлу
        public static void Delete(Product product)
        {
            products.Remove(product);
            SerializeToXml(products);
        }

        // Запис списку продуктів в файл
        public static void SerializeToXml(List<Product> p)
        {
            var xmlSer = new XmlSerializer(typeof(List<Product>));
            using(var writter = new StreamWriter(@"C:\Users\acsel\source\repos\XMLEdition\products.xml"))
            {
                xmlSer.Serialize(writter, p);
            }
        }

        // Зчитування продуктів з файлу 
        public static void DeserializeFromXml()
        {
            var xmlSer = new XmlSerializer(typeof(List<Product>));
            using (var reader = new StreamReader(@"C:\Users\acsel\source\repos\XMLEdition\products.xml"))
            {
                products = (List<Product>)xmlSer.Deserialize(reader);
            }
        }
    }
}
