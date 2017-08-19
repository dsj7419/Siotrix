using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace Siotrix.Discord
{
    [Name("Moderator")]
    [Group("antilink")]
    [Summary("Set various antilinking properties in the guild")]
    public class AntilinkModule : ModuleBase<SocketCommandContext>
    {
        [Command("status")]
        [Summary("Check current antilink status for guild.")]
        [Remarks("- No other argument needed")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task AntilinkStatusAsync()
        {
            var gColor = await Context.GetGuildColorAsync();
            var description = "";
            var title = $"Antilink Information for {Context.Guild.Name}";

            var antilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);

            if (antilink == null)
            {
                await ReplyAsync("I am not able to find your guilds Antilink settings. Please try again later.");
                return;
            }

            var builder = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex));

            //  TableExtensions.TableBuilder tb = new TableExtensions.TableBuilder();
            // IEnumerable<Tuple<int, string, bool, bool>> channellist = new[] { Tuple.Create(1, "help", false, false)};
            string isActive = antilink.IsActive ? isActive = "On" : isActive = "Off";
            string isDmActive = antilink.IsDmMessage ? isDmActive = "On" : isDmActive = "Off";
            var channelcount = 0;
            description = $"Current Antilink status: **{isActive}**\n";
            description += $"Current Antilink DM status: **{isDmActive}**\n";
            description += $"DM Message: **{antilink.DmMessage}**\n\n";
            // description += String.Format("## | {0, 20} | {1, 28} | {2, 35}\n", "Channel Name", "Is Active", "Is Strict");
            //  tb.AddRow("Num", "Channel Name", "Active?", "Strict Mode");
            //   tb.AddRow("------", "----------------", "---------", "-------------");

            foreach (var channel in Context.Guild.Channels)
            foreach (var user in channel.Users)
                if (user.IsBot && user.Id == SiotrixConstants.BotId)
                {
                    var antilinkChannel =
                        await AntilinkExtensions.GetAntilinkChanneListAsync(Context.Guild.Id, channel);

                    if (antilinkChannel != null)
                    {
                        channelcount++;
                        builder.AddInlineField($"{channel.Name.FirstLetterToUpperCaseOrConvertNullToEmptyString()} ",
                            $"Active: {antilinkChannel.IsActive}  | Strict Mode: {antilinkChannel.IsStrict}");

                        //   tb.AddRow(channelcount, channel.Name, antilinkChannel.IsActive, antilinkChannel.IsStrict);
                        // description += String.Format("** {0,3}** | {1, 20} | {2,28} | {3,35}\n", channelcount, channel.Name, antilinkChannel.IsActive, antilinkChannel.IsStrict);
                    }
                }
            if (channelcount == 0)
                description += $"No channel settings have been found..";
            else if (channelcount == 1)
                description += $"Only {channelcount} channel has been found with settings: ";
            else
                description += $"These {channelcount} channels have been found with settings: ";

            builder.WithDescription(description);
            //  var embed = EmbedExtensions.MakeNewEmbed(title, description, g_color);
            await ReplyAsync("", embed: builder);
            //   description += tb.Output();
            // await ReplyAsync(description);
        }

        [Command("toggle")]
        [Summary("Toggle antilinking on or off.")]
        [Remarks("- No other argument needed")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task AntilinkToggleAsync()
        {
            var antilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);

            if (antilink == null)
            {
                await AntilinkExtensions.CreateAntilinkAsync(Context);
                var subAntilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);
                await AntilinkExtensions.SetIsActiveAsync(subAntilink, true);
                await ReplyAsync(
                    "I am not able to find your guilds Antilink settings. Creating default and activating Antilink");
                return;
            }

            var isActive = antilink.IsActive;

            if (isActive)
            {
                await AntilinkExtensions.SetIsActiveAsync(antilink, false);
                await ReplyAsync("Antilink has been disabled for the guild.");
            }
            else
            {
                await AntilinkExtensions.SetIsActiveAsync(antilink, true);
                await ReplyAsync("Antilink has been enabled for the guild.");
            }
        }

        [Command("toggledm")]
        [Summary("Toggle antilink DM's to users if they break the antilink rule on or off.")]
        [Remarks("- No other argument needed")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task AntilinkDmToggleAsync()
        {
            var antilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);

            if (antilink == null)
            {
                await AntilinkExtensions.CreateAntilinkAsync(Context);
                await ReplyAsync(
                    "I am not able to find your guilds Antilink settings. Creating default setup with DM's off");
                return;
            }

            var isOn = antilink.IsDmMessage;

            if (isOn)
            {
                await AntilinkExtensions.SetIsDmAsync(antilink, false);
                await ReplyAsync("Antilink DM's have been disabled for the guild.");
            }
            else
            {
                await AntilinkExtensions.SetIsDmAsync(antilink, true);
                await ReplyAsync("Antilink DM's have been enabled for the guild.");
            }
        }

        [Command("dmmessage")]
        [Summary(
            "Set the DM message that the bot will send to the user if they use a link in a channel they aren't supposed to.")]
        [Remarks("[message] - no argument will list the current message and reset will restore to default.")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task AntilinkDmMessageAsync([Remainder] string message = null)
        {
            var antilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);

            if (antilink == null && message != null)
            {
                await AntilinkExtensions.CreateAntilinkAsync(Context);
                var subAntilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);
                await AntilinkExtensions.SetDmMessageAsync(subAntilink, message);
                await ReplyAsync(
                    "I am not able to find your guilds Antilink settings. Creating default setup with your DM message.");
                return;
            }

            if (antilink == null && message == null)
            {
                await AntilinkExtensions.CreateAntilinkAsync(Context);
                await ReplyAsync(
                    "I am not able to find your guilds Antilink settings. Creating default setup with the default DM message.");
                return;
            }

            if (message == null)
            {
                await ReplyAsync($"Your current DM Message is: **{antilink.DmMessage}**");
                return;
            }

            if (message == "reset")
            {
                await AntilinkExtensions.SetDmMessageAsync(antilink,
                    $"{Context.Guild.Name.ToUpper()} does not allow that link in the channel.");
                await ReplyAsync($"Your current DM Message has been reset.");
                return;
            }

            await AntilinkExtensions.SetDmMessageAsync(antilink, message);
            await ReplyAsync($"Your DM Message has been changed to: **{message}**");
        }

        [Command("togglechannel")]
        [Summary("Toggle antilink in a specific channel on or off. Antilink itself must be on to work")]
        [Remarks("(channel name)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task AntilinkChannelToggleAsync(SocketChannel channel)
        {
            var antilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);

            if (antilink == null)
            {
                await AntilinkExtensions.CreateAntilinkAsync(Context);
                var subAntilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);
                await AntilinkExtensions.SetIsActiveAsync(subAntilink, true);
                await ReplyAsync(
                    "I am not able to find your guilds Antilink settings. Creating default and activating Antilink");
                return;
            }

            var antilinkChannel = await AntilinkExtensions.GetAntilinkChanneListAsync(Context.Guild.Id, channel);

            if (antilinkChannel == null)
            {
                await AntilinkExtensions.CreateAntilinkChannelAsync(Context, antilink, channel);
                var subAntilink = await AntilinkExtensions.GetAntilinkChanneListAsync(Context.Guild.Id, channel);
                await AntilinkExtensions.SetIsActiveChannelAsync(subAntilink, true);
                await ReplyAsync(
                    "I am not able to find this channels Antilink settings. Creating and activating for this channel.");
                return;
            }

            var isActive = antilinkChannel.IsActive;
            var channelname = channel as IChannel;

            if (isActive)
            {
                await AntilinkExtensions.SetIsActiveChannelAsync(antilinkChannel, false);
                await ReplyAsync($"Antilink has been disabled for {channelname.Name}.");
            }
            else
            {
                await AntilinkExtensions.SetIsActiveChannelAsync(antilinkChannel, true);
                await ReplyAsync($"Antilink has been enabled for {channelname.Name}");
            }
        }

        [Command("togglestrict")]
        [Summary("Toggle antilink strict rules on or off for a channel.")]
        [Remarks("(channel name)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task AntilinkStrictToggleAsync(SocketChannel channel)
        {
            var antilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);

            if (antilink == null)
            {
                await AntilinkExtensions.CreateAntilinkAsync(Context);
                var subAntilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);
                await AntilinkExtensions.SetIsActiveAsync(subAntilink, true);
                await ReplyAsync(
                    "I am not able to find your guilds Antilink settings. Creating default and activating Antilink");
                return;
            }

            var antilinkChannel = await AntilinkExtensions.GetAntilinkChanneListAsync(Context.Guild.Id, channel);

            if (antilinkChannel == null)
            {
                await AntilinkExtensions.CreateAntilinkChannelAsync(Context, antilink, channel);
                var subAntilink = await AntilinkExtensions.GetAntilinkChanneListAsync(Context.Guild.Id, channel);
                await AntilinkExtensions.SetIsActiveChannelAsync(subAntilink, true);
                await AntilinkExtensions.SetIsStrictChannelAsync(subAntilink, true);
                await ReplyAsync(
                    "I am not able to find this channels Antilink settings. Creating and activating strict rules for this channel.");
                return;
            }

            var isStrict = antilinkChannel.IsStrict;
            var channelname = channel as IChannel;

            if (isStrict)
            {
                await AntilinkExtensions.SetIsStrictChannelAsync(antilinkChannel, false);
                await ReplyAsync($"Antilink strict mode has been disabled for {channelname.Name}.");
            }
            else
            {
                await AntilinkExtensions.SetIsStrictChannelAsync(antilinkChannel, true);
                await ReplyAsync($"Antilink strict mode has been enabled for {channelname.Name}");
            }
        }

        [Command("toggleall")]
        [Summary("Toggle all channels bot is in on or off depending on what each is set to.")]
        [Remarks("- No additional argument needed.")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task AntilinkToggleAllAsync()
        {
            var activecount = 0;
            var nonactivecount = 0;
            var channelcount = 0;
            var antilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);

            if (antilink == null)
            {
                await AntilinkExtensions.CreateAntilinkAsync(Context);
                await ReplyAsync(
                    "I am not able to find your guilds Antilink settings. Creating default Antilink. Try command again.");
                return;
            }

            foreach (var channel in Context.Guild.Channels)
            foreach (var user in channel.Users)
                if (user.IsBot && user.Id == SiotrixConstants.BotId)
                {
                    channelcount++;
                    var antilinkChannel =
                        await AntilinkExtensions.GetAntilinkChanneListAsync(Context.Guild.Id, channel);

                    if (antilinkChannel == null)
                    {
                        await AntilinkExtensions.CreateAntilinkChannelAsync(Context, antilink, channel);
                        var subAntilink =
                            await AntilinkExtensions.GetAntilinkChanneListAsync(Context.Guild.Id, channel);
                        await AntilinkExtensions.SetIsActiveChannelAsync(subAntilink, true);
                        activecount++;
                    }
                    else
                    {
                        var isActive = antilinkChannel.IsActive;

                        if (isActive)
                        {
                            await AntilinkExtensions.SetIsActiveChannelAsync(antilinkChannel, false);
                            nonactivecount++;
                        }
                        else
                        {
                            await AntilinkExtensions.SetIsActiveChannelAsync(antilinkChannel, true);
                            activecount++;
                        }
                    }
                }
            await ReplyAsync(
                $"A total of {channelcount} channels have changed. {activecount} activated, and {nonactivecount} deactivated.");
        }

        [Command("authorize")]
        [Summary("Authorize a user to use a link either permenantly, or one time only")]
        [Remarks(
            "(user) (channel) [permenant] - must use the word permenant or it will be a one time only authorization.")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task AntilinkAuthorizeAsync(SocketGuildUser user, SocketChannel channel,
            [Remainder] string parameter = null)
        {
            var message = "";
            var loggingMessage = "";

            if (channel.GetUser(user.Id) == null)
            {
                await ReplyAsync($"You cant authorize {user.Username} because they are not in that channel!");
                return;
            }


            if (user.IsBot && user.Id == SiotrixConstants.BotId)
            {
                await ReplyAsync($"You cant authorize me because I control my own fate!");
                return;
            }

            if (user.GetPermissions(channel as IGuildChannel).ManageMessages)
            {
                await ReplyAsync($"No need to authorise {user.Username}. They bypass antilink with their permissions.");
                return;
            }

            var antilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);

            if (antilink == null)
            {
                await AntilinkExtensions.CreateAntilinkAsync(Context);
                var subAntilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);
                await AntilinkExtensions.SetIsActiveAsync(subAntilink, true);
                await ReplyAsync(
                    "I am not able to find your guilds Antilink settings. Creating default and activating Antilink. Try command again.");
                return;
            }

            var antilinkChannel = await AntilinkExtensions.GetAntilinkChanneListAsync(Context.Guild.Id, channel);

            if (antilinkChannel == null)
            {
                await AntilinkExtensions.CreateAntilinkChannelAsync(Context, antilink, channel);
                var subAntilink = await AntilinkExtensions.GetAntilinkChanneListAsync(Context.Guild.Id, channel);
                await AntilinkExtensions.SetIsActiveChannelAsync(subAntilink, true);
                await ReplyAsync(
                    "I am not able to find this channels Antilink settings. Creating and activating for this channel. Try command again.");
                return;
            }

            var isChannelActive = antilinkChannel.IsActive;
            var channelname = channel as IChannel;

            if (!isChannelActive)
            {
                await ReplyAsync(
                    "You have tried to authorize a person in a channel that is not actively monitored. Activate the channel first.");
                return;
            }

            var antilinkUser = await AntilinkExtensions.GetAntilinkUserListAsync(Context.Guild.Id, user.Id, channel.Id);            

            if (antilinkUser == null)
            {
                await AntilinkExtensions.CreateUserAntilinkAsync(Context, antilink, channel, user,
                    parameter == "permenant" ? false : true);
                var isPermenent = parameter == "permenant" ? "permenantly." : "for a single use.";
                message = $"You have authorized {user.Username} to use links in {channelname.Name} {isPermenent}.";
                loggingMessage =
                    $"{Context.User.Mention} has authorized {user.Mention} to use links in {channelname.Name} {isPermenent}.";
            }
            else if (parameter == "permenant")
            {
                await AntilinkExtensions.SetIsOneTimeAsync(antilinkUser, false);
                message = $"You have authorized {user.Username} to use links in {channelname.Name} permenantly.";
                loggingMessage =
                    $"{Context.User.Mention} has authorized {user.Mention} to use links in {channelname.Name} permenantly.";
            }
            else
            {
                await AntilinkExtensions.SetIsOneTimeAsync(antilinkUser, true);
                message = $"You have authorized {user.Username} to use a link in {channelname.Name} one time only.";
                loggingMessage =
                    $"{Context.User.Mention} has authorized {user.Mention} to use links in {channelname.Name} for a single use.";
            }

            await ReplyAsync(message);

            var channelToggle = await LogsToggleExtensions.GetLogToggleAsync(Context.Guild.Id, "antilink_assigned");
            var logToggled = await LogsToggleExtensions.GetLogChannelAsync(Context.Guild.Id);

            if (logToggled.IsActive && channelToggle != null)
            {
                var logChannel = Context.Guild.GetChannel(logToggled.ChannelId.ToUlong()) as ISocketMessageChannel;
                var builder = new EmbedBuilder()
                    .WithAuthor(new EmbedAuthorBuilder()
                        .WithIconUrl(user.GetAvatarUrl())
                        .WithName(user.Id + " " + loggingMessage))
                    .WithColor(new Color(100, 80, 0));
                await logChannel.SendMessageAsync(user.Mention, false, builder.Build());
            }


        }

        [Command("deauthorize")]
        [Summary("De-authorize a user from using links in a specified channel.")]
        [Remarks("(user) (channel)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task AntilinkDeAuthorizeAsync(SocketGuildUser user, SocketChannel channel)
        {
            var message = "";
            var loggingMessage = "";
            var antilinkUser = await AntilinkExtensions.GetAntilinkUserListAsync(Context.Guild.Id, user.Id, channel.Id);
            var channelname = channel as IChannel;

            if (antilinkUser == null)
            {
                message = $"{user.Username} was never authorized in {channelname.Name}.";
            }
            else
            {
                await AntilinkExtensions.DeleteAntilinkUserAsync(antilinkUser);
                message = $"You have de-authorized {user.Username} from using links in {channelname.Name}.";
                loggingMessage =
                    $"{Context.User.Mention} has de-authorized {user.Mention} from using links in {channelname.Name}";

                var channelToggle = await LogsToggleExtensions.GetLogToggleAsync(Context.Guild.Id, "antilink_removed");
                var logToggled = await LogsToggleExtensions.GetLogChannelAsync(Context.Guild.Id);

                if (logToggled.IsActive && channelToggle != null)
                {
                    var logChannel = Context.Guild.GetChannel(logToggled.ChannelId.ToUlong()) as ISocketMessageChannel;
                    var builder = new EmbedBuilder()
                        .WithAuthor(new EmbedAuthorBuilder()
                            .WithIconUrl(user.GetAvatarUrl())
                            .WithName(user.Id + " " + loggingMessage))
                        .WithColor(new Color(100, 80, 0));
                    await logChannel.SendMessageAsync(user.Mention, false, builder.Build());
                }
            }

            await ReplyAsync(message);
        }

        [Command("userstatus")]
        [Summary("Check current antilink authorization status for a user in the guild.")]
        [Remarks("(username)")]
        [MinPermissions(AccessLevel.GuildMod)]
        public async Task AntilinkStatusAsync(SocketGuildUser user)
        {
            var gColor = await Context.GetGuildColorAsync();
            var description = "";
            var title = $"Antilink Information for {user.Username} in {Context.Guild.Name}";

            var antilink = await AntilinkExtensions.GetAntilinkAsync(Context.Guild.Id);

            if (antilink == null)
            {
                await ReplyAsync("I am not able to find your guilds Antilink settings. Please try again later.");
                return;
            }

            var builder = new EmbedBuilder()
                .WithTitle(title)
                .WithColor(GuildEmbedColorExtensions.ConvertStringtoColorObject(gColor.ColorHex));

            string isActive = antilink.IsActive ? isActive = "On" : isActive = "Off";
            var channelcount = 0;
            description = $"Current Antilink status: **{isActive}**\n\n";

            foreach (var channel in Context.Guild.Channels)
            foreach (var users in channel.Users)
                if (users == user)
                {
                    if (user.GetPermissions(channel).ManageMessages)
                    {
                        await ReplyAsync($"{user.Mention} has manage channels ability so they are immune to antilink.");
                        return;
                    }
                    var antilinkUser =
                        await AntilinkExtensions.GetAntilinkUserListAsync(Context.Guild.Id, user.Id, channel.Id);
                    if (antilinkUser != null)
                    {
                        channelcount++;
                        string isOneTime = antilinkUser.IsOneTime
                            ? isOneTime = "one time usage"
                            : isOneTime = "unlimited usage";
                        builder.AddInlineField($"{channel.Name.FirstLetterToUpperCaseOrConvertNullToEmptyString()} ",
                            $"User has been authorized for {isOneTime} in this channel.");
                    }
                }

            if (channelcount == 0)
                description += $"This user is not authorized for any channel.";
            else if (channelcount == 1)
                description += $"Only {channelcount} channel has been found with authorization: ";
            else
                description += $"These {channelcount} channels have been found with authorization: ";

            builder.WithDescription(description);
            await ReplyAsync("", embed: builder);
        }
    }
}