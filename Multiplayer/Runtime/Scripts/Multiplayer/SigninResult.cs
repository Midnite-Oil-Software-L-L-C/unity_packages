namespace MidniteOilSoftware.Multiplayer
{
    public struct SigninResult
    {
        public bool Success;
        public string Message;


        public static SigninResult Successful => new SigninResult(true, string.Empty);

        public SigninResult(bool success, string message)
        {
            Success = success;
            Message = message;
        }
    }
}