using System.Collections.Generic;
using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Utility
{
    [Name("Utility")]
    [MinPermissions(AccessLevel.User)]
    public class AboutModule : ModuleBase<SocketCommandContext>
    {
        private readonly Dictionary<ulong, string> _specialThanks = new Dictionary<ulong, string>();

        [Command("about")]
        [Summary("A little information and special thanks surrounding this bot, Siotrix.")]
        [Remarks(" - no additional arguments needed.")]
        [MinPermissions(AccessLevel.User)]
        private async Task ShowAbout()
        {
            if (_specialThanks.Count <= 0)
                _specialThanks.Add(158056840402436096,
                    "Someone who was a good inspiration to start my base code off of.");

            var gPrefix = await Context.GetGuildPrefixAsync();
            var message =
                $"A little bit about Siotrx! Siotrix is a discord bot written in C# by {Context.Client.GetUser(173905004661309441).Username}#{Context.Client.GetUser(173905004661309441).Discriminator}." +
                $"I was created because a lot of the larger bots seem to miss having a personal touch for each guild. The goal was to make a" +
                $"bot with all of the great features you love in discord bots, but give it the flexibility to be custom tailored just for your guild!\n\n" +
                $"Please use the command **{gPrefix.Prefix}help** to see the features I can offer!\n\n" +
                $"I'd love all the help I can get, and bugreports/sugestions are always welcome! you can DM {Context.Client.GetUser(173905004661309441).Username}#{Context.Client.GetUser(173905004661309441).Discriminator} anytime!\n\n" +
                $"Support server: <{SiotrixConstants.DiscordInv}>\n" +
                $"My patreon page: <{SiotrixConstants.BotDonate}>\n\n" +
                ":sparkles: **Special Thanks** :sparkles:\n";
            var count = 1;
            foreach (var thanks in _specialThanks)
            {
                var user = Context.Client.GetUser(thanks.Key);
                message = message + $"  {count++}. **{user?.Username}#{user?.Discriminator}** - {thanks.Value}\n";
            }
            message = message +
                      $"  {count++}. Everyone from Islands of Nyne for giving {Context.Client.GetUser(173905004661309441).Username}#{Context.Client.GetUser(173905004661309441).Discriminator} the initial idea\n";
            message = message +
                      "\nI also want to thank the people at the discord.net API discord server. Good group of guys that put up with a lot of really dumb" +
                      "questions, but ultimately sort through it and give each person the attention they need.";
            await Context.Channel.SendMessageSafeAsync(message);
        }
    }
}