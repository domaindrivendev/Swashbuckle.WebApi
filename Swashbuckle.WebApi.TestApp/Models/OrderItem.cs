namespace Swashbuckle.WebApi.TestApp.Models
{
    public class OrderItem
    {
        public int LineNo { get; set; }

        public string Product { get; set; }

        public ProductCategory Category { get; set; }

        public int Quantity { get; set; }
    }

    public enum ProductCategory
    {
        Category1,
        Category2,
        Category3
    }
}