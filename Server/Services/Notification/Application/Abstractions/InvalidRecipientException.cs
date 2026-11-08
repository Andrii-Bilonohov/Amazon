namespace Application.Abstractions;

public sealed class InvalidRecipientException
(
        string message, 
        Exception? inner = null
) : Exception(message, inner);