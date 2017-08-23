using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Siotrix.Discord
{
    public static class SiotrixConstants
    {
        // bot constants
        public const string BotVersion = "0.11.0";

        public const string BotPrefix = "!";
        public const string BotName = "Siotrix";
        public const string ZeroLengthChar = "\u180E";
        public const long BotId = 344714431265243136;
        public const string DiscordInv = "https://discord.gg/qRDPXJF";
        public const string BotUrl = "https://discord.gg/qRDPXJF";
        public const string BotColor = "0x010101";

        public const string BotInvite =
            "https://discordapp.com/oauth2/authorize?client_id=344714431265243136&scope=bot&permissions=2097176631";

        public const string BotDonate = "https://www.patreon.com/siotrix";
        public const string FakeEveryone = "@" + ZeroLengthChar + "everyone";
        public const string FakeDiscordLink = "discord" + ZeroLengthChar + ".gg";
        public const string BotSuccess = "👍";
        public const int WaitTime = 3;

        // Other Constants
        public const long BugChannel = 336881482712743937;
        public const long SuggestionChannel = 336882503006814210;

        //bot embed constants
        public const string BotFooterText = "A global bot with a local feel.";

        public const string BotDesc = "Siotrix - Created by gamers for gamers.";

        // images and logos
        public const string BotLogo =
                "https://s3.us-east-2.amazonaws.com/siotriximages/ShareX/2017/06/ApplicationFrameHost_2017-06-22_11-10-46.png"
            ;

        public const string BotAvatar =
                "https://s3.us-east-2.amazonaws.com/siotriximages/ShareX/2017/06/ApplicationFrameHost_2017-06-22_11-15-13.png"
            ;

        public const string BotAuthorIcon =
            "https://s3.us-east-2.amazonaws.com/siotriximages/ShareX/2017/06/chrome_2017-06-22_10-54-00.png";

        public const string BotFooterIcon =
            "https://s3.us-east-2.amazonaws.com/siotriximages/ShareX/2017/06/chrome_2017-06-22_10-54-00.png";

        // embeds
        public const int MaxMessageLengthLong = 1900;

        public const int MaxMessageLengthShort = 750;
        public const int MaxNicknameLength = 32;
        public const int MinNicknameLength = 2;
        public const int MaxGuildNameLength = 100;
        public const int MinGuildNameLength = 2;
        public const int MaxChannelNameLength = 100;
        public const int MinChannelNameLength = 2;
        public const int MaxRoleNameLength = 100;
        public const int MinRoleNameLength = 1;
        public const int MaxTopicLength = 1024;
        public const int MaxGameLength = 128; //Yes, I know it CAN go past that, but it won't show for others.
        public const int MaxEmbedLengthLong = 2048;
        public const int MaxEmbedLengthShort = 1024;
        public const int MaxTitleLength = 256;
        public const int MaxFields = 25;
        public const int MaxDescriptionLines = 20;
        public const int MaxFieldLines = 5;
        public const int MaxLengthForFieldValue = 250000;

        public static readonly string[] BadWords = { "shit", "fuck", "nigger", "rape", "sex", "coon" };

        public static ReadOnlyCollection<string> ValidImageExtensions =
            new ReadOnlyCollection<string>(new List<string>
            {
                ".jpeg",
                ".jpg",
                ".png"
            });

        public static ReadOnlyCollection<string> ValidGifExtentions = new ReadOnlyCollection<string>(new List<string>
        {
            ".gif",
            ".gifv"
        });

        public static ReadOnlyCollection<string> CommandsUnableToBeTurnedOff = new ReadOnlyCollection<string>(
            new List<string>
            {
                "cset",
                "help"
            });

        public static ReadOnlyCollection<string> LogNamesCommandList = new ReadOnlyCollection<string>(
            new List<string>
            {
                "user_join",
                "user_leave",
                "message_edit",
                "message_deleted",
                "user_muted",
                "user_unmuted",
                "user_banned",
                "user_unbanned",
                "role_created",
                "role_deleted",
                "role_modified",
                "role_assigned",
                "role_removed",
                "username",
                "nickname",
                "blacklist",
                "deblacklist",
                "antilink_assigned",
                "antilink_removed",
                "antilink_violation",
                "filter_violation"
            });
    }
}