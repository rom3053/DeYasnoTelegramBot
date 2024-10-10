namespace DeYasnoTelegramBot.Application.Common.Constants;

public static class BotCommands
{
    public const string Start = "/start";

    public const string GetScheduleScreenshot = "/schedule";

    public const string UpdateOwnSchedule = "/update";

    public const string OutageStatusNow = "/status_now";

    public const string ToggleNotification = "/toggle_notification";

    public static class CallbackCommands
    {
        public const string SelectedKiev = "/kiev";

        public const string SelectedDnipro = "/dnipro";
    }
}
