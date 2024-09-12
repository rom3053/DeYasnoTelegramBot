namespace DeYasnoTelegramBot.Application.Common.Exceptions;

public class DeYasnoException : Exception
{
    public long ChatId { get; set; }

    public DeYasnoException()
    {
    }

    public DeYasnoException(long chatId)
    {
        ChatId = chatId;
    }

    public DeYasnoException(string message, Exception innerException, long chatId)
        : base(message, innerException)
    {
        ChatId = chatId;
    }
}
