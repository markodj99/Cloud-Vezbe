namespace Common.Model
{
    public enum ReturnCode : int
    {
        Success = 0,
        ValidatorError = 1,
        TransactionCoordinatorError = 2,
        BookStoreError = 3,
        BankError = 4
    }
}
