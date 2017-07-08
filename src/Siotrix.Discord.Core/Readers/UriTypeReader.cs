using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Siotrix.Discord.Readers
{
    public class UriTypeReader : TypeReader
    {
        public override Task<TypeReaderResult> Read(ICommandContext context, string input, IServiceProvider services)
        {
            string cleanInput = input;

            var firstChar = input.First();
            if (firstChar == '<')
                cleanInput = input.Substring(1, input.Length - 1);

            if (Uri.TryCreate(cleanInput, UriKind.RelativeOrAbsolute, out Uri site))
                return Task.FromResult(TypeReaderResult.FromSuccess(site));
            else
                return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, $"`{cleanInput}` is not a valid url"));
        }
    }
}