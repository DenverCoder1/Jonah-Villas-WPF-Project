namespace BE
{
    public enum OrderStatus
    {
        NotYetHandled = 0,
        SentEmail = 1,
        ClosedByNoCustomerResponse = 2,
        ClosedByCustomerResponse = 3
    }
}