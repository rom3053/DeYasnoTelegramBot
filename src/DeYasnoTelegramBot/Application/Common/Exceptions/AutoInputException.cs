namespace DeYasnoTelegramBot.Application.Common.Exceptions;

public class AutoInputException : DeYasnoException
{
    public string TelegramErrorMessage = "Виникла помилка. Можливо збережені дані не корректні.";
}
