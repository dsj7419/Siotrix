using Discord;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading;

namespace Siotrix.Discord
{
    public static class SiotrixConstants
    {
        // bot constants
        public const string BOT_VERSION = "0.11.0";
        public const string BOT_PREFIX = "!";
        public const string BOT_NAME = "Siotrix";
        public const string ZERO_LENGTH_CHAR = "\u180E";
        public const string DISCORD_INV = "https://discord.gg/nXvBxDX";
        public const string BOT_URL = "https://discord.gg/nXvBxDX";
        public const string BOT_INVITE = "https://discordapp.com/oauth2/authorize?client_id=285812392930050048&scope=bot&permissions=2097176631";
        public const string BOT_DONATE = "https://www.patreon.com/siotrix";
        public const string FAKE_EVERYONE = "@" + ZERO_LENGTH_CHAR + "everyone";
        public const string BOT_SUCCESS = "👍";

        //bot embed constants
        public const string BOT_FOOTER_TEXT = "A global bot with a local feel.";
        public const string BOT_DESC = "Siotrix - Created by gamers for gamers.";

        // images and logos
        public const string BOT_LOGO = "https://s3.us-east-2.amazonaws.com/siotriximages/ShareX/2017/06/ApplicationFrameHost_2017-06-22_11-10-46.png";
        public const string BOT_AVATAR = "https://s3.us-east-2.amazonaws.com/siotriximages/ShareX/2017/06/ApplicationFrameHost_2017-06-22_11-15-13.png";
        public const string BOT_AUTHOR_ICON = "https://s3.us-east-2.amazonaws.com/siotriximages/ShareX/2017/06/chrome_2017-06-22_10-54-00.png";
        public const string BOT_FOOTER_ICON = "https://s3.us-east-2.amazonaws.com/siotriximages/ShareX/2017/06/chrome_2017-06-22_10-54-00.png";

        // 
        public const int MAX_MESSAGE_LENGTH_LONG = 1900;
        public const int MAX_MESSAGE_LENGTH_SHORT = 750;
        public const int MAX_NICKNAME_LENGTH = 32;
        public const int MIN_NICKNAME_LENGTH = 2;
        public const int MAX_GUILD_NAME_LENGTH = 100;
        public const int MIN_GUILD_NAME_LENGTH = 2;
        public const int MAX_CHANNEL_NAME_LENGTH = 100;
        public const int MIN_CHANNEL_NAME_LENGTH = 2;
        public const int MAX_ROLE_NAME_LENGTH = 100;
        public const int MIN_ROLE_NAME_LENGTH = 1;
        public const int MAX_TOPIC_LENGTH = 1024;
        public const int MAX_GAME_LENGTH = 128; //Yes, I know it CAN go past that, but it won't show for others.
        public const int MAX_EMBED_LENGTH_LONG = 2048;
        public const int MAX_EMBED_LENGTH_SHORT = 1024;
        public const int MAX_TITLE_LENGTH = 256;
        public const int MAX_FIELDS = 25;
        public const int MAX_DESCRIPTION_LINES = 20;
        public const int MAX_FIELD_LINES = 5;
        public const int MAX_LENGTH_FOR_FIELD_VALUE = 250000;

        public static ReadOnlyCollection<string> VALID_IMAGE_EXTENSIONS = new ReadOnlyCollection<string>(new List<string>
        {
            ".jpeg", ".jpg", ".png",
        });
        public static ReadOnlyCollection<string> VALID_GIF_EXTENTIONS = new ReadOnlyCollection<string>(new List<string>
        {
            ".gif", ".gifv",
        });
        public static ReadOnlyCollection<string> COMMANDS_UNABLE_TO_BE_TURNED_OFF = new ReadOnlyCollection<string>(new List<string>
        {
            "configurecommands", "help",
        });
    }
}
