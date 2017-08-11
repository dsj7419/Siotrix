using System;
using Discord;

namespace Siotrix.Discord
{
    public static class EmbedExtensions
    {
        public static EmbedBuilder MakeNewEmbed(string title = null, string description = null, Color? color = null,
            string imageURL = null, string URL = null, string thumbnailURL = null)
        {
            //Make the embed builder
            var embed = new EmbedBuilder().WithColor(1, 1, 1);

            //Validate the URLs
            imageURL = ValidateURL(imageURL) ? imageURL : null;
            URL = ValidateURL(URL) ? URL : null;
            thumbnailURL = ValidateURL(thumbnailURL) ? thumbnailURL : null;

            //Add in the properties
            if (title != null)
                embed.WithTitle(title.Substring(0, Math.Min(SiotrixConstants.MAX_TITLE_LENGTH, title.Length)));
            if (description != null)
                embed.WithDescription(description);
            if (color != null)
                embed.WithColor(color.Value);
            if (imageURL != null)
                embed.WithImageUrl(imageURL);
            if (URL != null)
                embed.WithUrl(URL);
            if (thumbnailURL != null)
                embed.WithThumbnailUrl(thumbnailURL);

            return embed;
        }

        public static bool ValidateURL(string input)
        {
            if (input == null)
                return false;

            return Uri.TryCreate(input, UriKind.Absolute, out Uri uriResult);
        }

        /*public static EmbedBuilder WithUrl(this EmbedBuilder builder, string url)
            => Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri) ? builder.WithUrl(uri) : builder;

        public static EmbedBuilder withImageUrl(this EmbedBuilder builder, string url)
            => Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri) ? builder.WithImageUrl(uri) : builder;

        public static EmbedBuilder WithThumbnailUrl(this EmbedBuilder builder, string url)
            => Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri) ? builder.WithThumbnailUrl(uri) : builder;

        public static EmbedAuthorBuilder WithIconUrl(this EmbedAuthorBuilder builder, string url)
            => Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri) ? builder.WithIconUrl(uri) : builder;

        public static EmbedFooterBuilder WithIconUrl(this EmbedFooterBuilder builder, string url)
            => Uri.TryCreate(url, UriKind.RelativeOrAbsolute, out var uri) ? builder.WithIconUrl(uri) : builder; */
    }
}