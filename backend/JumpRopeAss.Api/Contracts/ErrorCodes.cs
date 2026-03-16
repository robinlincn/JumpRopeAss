namespace JumpRopeAss.Api.Contracts;

public static class ErrorCodes
{
    public const int InvalidParam = 40000;
    public const int Unauthorized = 40100;
    public const int Forbidden = 40300;
    public const int NotFound = 40400;
    public const int Conflict = 40900;
    public const int ServerError = 50000;

    public const int EntryCoachNotFirstCoach = 40310;
    public const int EntryStatusInvalid = 40910;
    public const int IdnPendingExists = 40930;
}

