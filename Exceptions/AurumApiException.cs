namespace AurumFinance.Exceptions
{
    /// <summary>
    /// Thrown by IAurumApiClient whenever the backend returns a non-success
    /// status code. Message is the backend's own ErrorResponse.Error text
    /// (e.g. "Invalid email or password."), so controllers can surface it
    /// directly via ModelState without re-interpreting status codes.
    /// </summary>
    public class AurumApiException : Exception
    {
        public int? StatusCode { get; }

        public AurumApiException(string message, int? statusCode = null) : base(message)
        {
            StatusCode = statusCode;
        }
    }
}
