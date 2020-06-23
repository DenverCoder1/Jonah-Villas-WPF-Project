namespace BE
{
    public enum OrderStatus
    {
        NotYetHandled,
        SentEmail,
        ClosedByNoCustomerResponse,
        ClosedByCustomerResponse,
        Rejected
    }
}