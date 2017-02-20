namespace GynBot.Modules.Public.Services
{
    using System.Threading.Tasks;
    using Discord.Commands;

    public static class TagService
    {
        public static async Task ReadTagAsync(CommandContext ctx, string tagName)
        {
            using (ctx.Channel.EnterTypingState())
            {
                string tagValue;
                var tagExists = Globals.ServerConfigs[ctx.Guild.Id].Tags.TryGetValue(tagName, out tagValue);

                if (tagExists)
                {
                    await ctx.Channel.SendMessageAsync(tagValue);
                }
                else
                {
                    await ctx.Channel.SendMessageAsync("Tag not found.");
                }
            }
        }

        public static async Task CreateTagAsync(CommandContext ctx, string tagName, string tagValue)
        {
            Globals.ServerConfigs[ctx.Guild.Id].Tags.Add(tagName, tagValue);
        }
    }
}