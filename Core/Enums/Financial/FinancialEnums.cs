namespace HUP.Core.Enums.Financial
{
    public enum FeeStatus
    {
        Pending = 0,
        PartiallyPaid = 1,
        Paid = 2,
        Overdue = 3,
        Waived = 4
    }

    public enum PaymentMethod
    {
        Cash = 0,
        Online = 1,
        BankTransfer = 2,
        Cheque = 3
    }

    public enum FeeType
    {
        Tuition = 0,
        Book = 1,
        Lab = 2,
        Bus = 3,
        Activity = 4,
        Other = 5
    }
}
