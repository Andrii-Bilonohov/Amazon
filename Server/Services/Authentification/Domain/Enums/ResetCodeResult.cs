namespace Domain.Enums;

public enum ResetCodeResult
{
    Valid, 
    Invalid, 
    Expired, 
    TooManyAttempts
}