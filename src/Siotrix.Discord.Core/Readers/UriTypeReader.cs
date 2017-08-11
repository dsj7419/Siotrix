using System;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;

namespace Siotrix.Discord.Readers
{
    public class UriTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            var cleanInput = input;

            var firstChar = input.First();
            if (firstChar == '<')
                cleanInput = input.Substring(1, input.Length - 1);

            if (Uri.TryCreate(cleanInput, UriKind.RelativeOrAbsolute, out Uri site))
                return Task.FromResult(TypeReaderResult.FromSuccess(site));
            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed,
                $"`{cleanInput}` is not a valid url"));
        }
    }
}