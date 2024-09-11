namespace DeYasnoTelegramBot.Application.Common.Helpers;

public static class NotificationMessages
{
    public const string Message_About_5min_PowerOff = "Закругляйся дурачек, через 5 хвилин світла не буде.";
    public const string Message_About_15min_PowerOff = "Через 15 хвилин вимкнуть світло.";
    public const string Message_About_30min_PowerOff = "Через 30 хвилин вимкнуть світло.";

    public const string Message_About_5min_PossiblePowerOn = "Через 5 хвилин починається сіра зона.";
    public const string Message_About_15min_PossiblePowerOn = "Через 15 хвилин починається сіра зона.";
    public const string Message_About_30min_PossiblePowerOn = "Через 30 хвилин починається сіра зона.";

    public const string Message_About_5min_PowerOn = "Через 5 хвилин світло буде.";
    public const string Message_About_15min_PowerOn = "Через 15 хвилин світло буде.";
    public const string Message_About_30min_PowerOn = "Через 30 хвилин світло буде.";

    public const string Message_About_Step_0 = "Тепер ви будете отримувати нотифікації про відключення світла.\nЗадопомогою команд:\n/schedule можете отримати зображення розкладу.";
    public const string Message_About_Step_2 = "Зараз введіть назву <b>міста</b>";
    public const string Message_About_Step_3 = "Відправте номер з вашим варіантом міста";
    public const string Message_About_Step_4 = "Зараз введіть назву <b>вулиці</b>";
    public const string Message_About_Step_5 = "Відправте номер з вашим варіантом вулиці";
    public const string Message_About_Step_6 = "Зараз введіть <b>номер будинку</b>";
    public const string Message_About_Step_7 = "Відправте номер з вашим варіантом номеру будинку";

    public const string Message_About_Incorrect_Input = "Відправте номер пункту.\nНаприклад:\n1. Дніпро\n2. Київ\nЯкщо хочете обрати Дніпро, то відправте <b>1</b>";

    public static class CommandMessages
    {
        public const string GetScheduleScreenshotCommand = "Ваш розклад.";

        public static class StartCommand 
        {
            public const string Kiev = "Київ";
            public const string Dnipro = "Дніпро";
            public const string CommandText = "Оберіть Ваш регіон.";
        }

        public static class GetOutageStatusCommand
        {
            public const string PowerOn = "Зараз світло є.";
            public const string PowerOff = "Зараз світло нема.";
            public const string PowerPossibleOn = "Зараз сіра зона.";
        }

        public static class UpdateOwnScheduleCommand
        {
            public const string CommandSuccessText = "Ваш графік оновлено в системі.";
        }
    }
}
