namespace Domain.Enums;

public enum ConfirmEmailResult
{
    Confirmed, 
    AlreadyConfirmed, 
    Invalid, 
    Expired, 
    TooManyAttempts
}