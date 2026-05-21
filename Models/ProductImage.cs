public class ProductImage
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }

    public string Url { get; set; }
    public bool IsMain { get; set; }

    public Product Product { get; set; }
}