public class AppErr : Exception
{
    public int StatusCode {get; set;}

    public AppErr(int StatusCode, string Message) : base(Message)
    {
        this.StatusCode = StatusCode;
    }
}