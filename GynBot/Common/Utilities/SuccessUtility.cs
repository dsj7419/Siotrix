namespace GynBot.Common.Utilities
{
    public static class SuccessUtility
    {
        public static string Success(string followup = null)
        {
            var response = ":thumbsup:";
            if (!string.IsNullOrEmpty(followup))
                response += ": " + followup;
            return response;
        }
    }
}
