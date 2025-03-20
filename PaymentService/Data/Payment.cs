namespace PaymentService.Data
{
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public string Status { get; set; }
    }
}
