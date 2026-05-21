public class ProductCreateDTO
{
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public Guid CategoryId { get; set; }

    public List<string> Images { get; set; }
}