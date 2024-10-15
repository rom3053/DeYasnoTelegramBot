namespace DeYasnoTelegramBot.Application.Common.Constants.ApiActions;

public static class YasnoScrapperApiActions
{
    private const string Base = "api";

    private const string SessionController = $"{Base}/OutageScheduleSession";

    private const string OutageScheduleInputController = $"{Base}/OutageScheduleInput";

    private const string OutageScheduleController = $"{Base}/OutageSchedule";

    #region SessionController
    /// <summary>
    /// [Get]
    /// </summary>
    public const string InitSession = $"{SessionController}/init-session";

    /// <summary>
    /// [Get]
    /// </summary>
    public const string GetSession = $"{SessionController}/{{sessionId}}";
    #endregion

    #region OutageScheduleInputController
    public const string Step_1_SelectRegion = $"{OutageScheduleInputController}/{{sessionId}}/step-1-select-region";
    public const string Step_2_InputCity = $"{OutageScheduleInputController}/{{sessionId}}/step-2-input-city";
    public const string Step_4_InputStreet = $"{OutageScheduleInputController}/{{sessionId}}/step-4-input-street";
    public const string Step_6_InputHouseNumber = $"{OutageScheduleInputController}/{{sessionId}}/step-6-input-house-number";
    public const string Step_3_5_7_SelectOption = $"{OutageScheduleInputController}/{{sessionId}}/step-3-5-7-select-option";
    public const string AutomaticInput = $"{OutageScheduleInputController}/{{sessionId}}/automatic-input";
    #endregion

    #region OutageScheduleController
    public const string GetParsedTable = $"{OutageScheduleController}/{{sessionId}}/parsed-table";
    public const string GetScheduleScreenshot = $"{OutageScheduleController}/{{sessionId}}/screenshot";
    #endregion
}
