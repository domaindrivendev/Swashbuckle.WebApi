using System.ComponentModel.DataAnnotations;

namespace Swashbuckle.Dummy.Models
{
    public class Product
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public decimal Price { get; set; }

        [Required]
        public ProductType Type { get; set; }
    }

    public class Book : Product
    {
        public string Author { get; set; }
    }

    public class Album : Product
    {
        public string Artist { get; set; }
    }

    public abstract class Service : Product
    { }

    public class Shipping : Service
    { }

    public class Packaging : Service
    { }

    public enum ProductType
    {
        Book,
        Album,
        Shipping,
        Packaging
    }
}