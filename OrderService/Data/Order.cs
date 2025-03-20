namespace OrderService.Data
{
    public class Order
    {
        public Guid Id {  get; set; } = Guid.NewGuid();
        public string Product { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }
}
