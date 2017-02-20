﻿namespace GynBot.Modules.Admin
{
    #region Using

    using System.Threading.Tasks;
    using Discord;
    using Discord.Commands;

    #endregion

    [Name("Admin"), RequireUserPermission(GuildPermission.ManageGuild), RequireContext(ContextType.Guild)]
    public class AdminModule : ModuleBase
    {
        #region Public Methods

        [Command("setprefix"), Alias("prefix"), Summary("Changes the command prefix for the bot.")]
        public async Task ChangePrefixAsync([Summary("Prefix to change to.")]string prefix = "")
        {
            if (string.IsNullOrEmpty(prefix))
            {
                await
                    ReplyAsync
                        ($"This guild's comand prefix is currently set to: `{Globals.ServerConfigs[Context.Guild.Id].CommandPrefix}`");
                return;
            }
            var newConfig = Globals.ServerConfigs[Context.Guild.Id];
            newConfig.CommandPrefix = prefix;
            Globals.ServerConfigs[Context.Guild.Id] = newConfig;
            await ConfigHandler.SaveAsync(Globals.SERVER_CONFIG_PATH, Globals.ServerConfigs).ConfigureAwait(false);
            await
                ReplyAsync($"Prefix changed to {Globals.ServerConfigs[Context.Guild.Id].CommandPrefix}").
                    ConfigureAwait(false);
        }

        #endregion Public Methods
    }
}