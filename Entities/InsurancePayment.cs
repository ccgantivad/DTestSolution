namespace Entities
{
    public class InsurancePayment
    {
        public int PaymentId { get; set; }
        public DateTime PaymentDatetime { get; set; }
        public string Franchise { get; set; }
        public string Currency { get; set; }
        public int Amount { get; set; }
    }
}
