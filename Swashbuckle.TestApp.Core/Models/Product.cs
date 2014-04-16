using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.TestApp.Models
{
    public abstract class Product
    {
        public int Id { get; set; }

        public decimal Price { get; set; }

        [Required]
        public string Type { get; set; }
    }

    public class Book : Product
    {
        public string Title { get; set; }

        public string Author { get; set; }
    }

    public class Album : Product
    {
        public string Name { get; set; }

        public string Artist { get; set; }
    }

    public abstract class Service : Product
    {}

    public class Shipping : Service
    {}

    public class Packaging : Service
    {}
}