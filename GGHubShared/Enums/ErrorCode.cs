namespace GGHubShared.Enums
{
    public enum ErrorCode
    {
        None = 0,
        InvalidCredentials = 1,
        UserNotFound = 2,
        Unauthorized = 3,
        ValidationFailed = 4,
        NotFound = 5,
        PaymentError = 6,
        PaymentSuccess = 66,
        InsufficientBalance = 7,
        DuplicateEmail = 8,
        DuplicateUsername = 9,
        ServerError = 10
    }
}
