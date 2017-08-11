using Discord;

namespace Siotrix.Discord
{
    public static class ActionResult
    {
        public static bool IsSuccess { get; set; } = false;

        public static string CommandName { get; set; } = null;

        public static int TimeLength { get; set; } = 0;

        public static IUserMessage Instance { get; set; } = null;

        public static long CaseId { get; set; } = 0;

        public static long UserId { get; set; } = 0;

        public static bool IsFoundedCaseNumber { get; set; } = false;
    }
}