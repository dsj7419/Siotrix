using Discord;
using Discord.WebSocket;
using Discord.Commands;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Siotrix.Discord.Roslyn
{
    [Name("Developer")]  
    public class EvalModule : ModuleBase<SocketCommandContext>
    {
        private Stopwatch _timer = new Stopwatch();

        protected override void BeforeExecute(CommandInfo info)
        {
            _timer.Start();
         //   Console.WriteLine(info.Summary);
        }

        [Command("evaluate", RunMode = RunMode.Async)]
        [Alias("eval")]
        [Summary("Evaluate a c# expression.")]
        [Remarks("<code> - Any simple c# expressions and discord.net code.")]
        [MinPermissions(AccessLevel.BotOwner)]
        public async Task EvalAsync([Remainder]string code)
        {
            var options = ScriptOptions.Default
            .AddReferences(           
                typeof(string).GetTypeInfo().Assembly,
                typeof(Assembly).GetTypeInfo().Assembly,
                typeof(Task).GetTypeInfo().Assembly,
                typeof(Enumerable).GetTypeInfo().Assembly,
                typeof(List<>).GetTypeInfo().Assembly,
                typeof(IGuild).GetTypeInfo().Assembly,
                typeof(SocketGuild).GetTypeInfo().Assembly,
                typeof(Entity).GetTypeInfo().Assembly
                )
            .AddImports(
                "System",
                "System.Diagnostics",
                "System.Text",
                "System.Reflection",
                "System.Threading.Tasks",
                "System.Linq",
                "System.Collections.Generic",
                "System.Net",
                "Discord",
                "Discord.WebSocket",
                "Siotrix"
            );
            
            string cleancode = GetCode(code);
            string reply, type;

            try
            {
                var result = await CSharpScript.EvaluateAsync(cleancode, options, Context);
                type = result.GetType().Name;
                reply = result.ToString();
            } catch (Exception ex)
            {
                type = ex.GetType().Name;
                reply = ex.Message;
            }

            var embed = GetEmbed(cleancode, reply, type);
            await ReplyAsync("", embed: embed);
        }

        private string GetCode(string rawmsg)
        {
            string code = rawmsg;

            if (code.StartsWith("```"))
                code = code.Substring(3, code.Length - 6);
            if (code.StartsWith("cs"))
                code = code.Substring(2, code.Length - 2);

            code = code.Trim();
            code = code.Replace(";\n", ";");
            code = code.Replace("; ", ";");
            code = code.Replace(";", ";\n");

            return code;
        }

        private EmbedBuilder GetEmbed(string code, string result, string resultType)
        {

            _timer.Stop();
            
            var builder = new EmbedBuilder();
            builder.Color = new Color(25, 185, 0);
            builder.AddField(x =>
            {
                x.Name = "Code";
                x.Value = $"```cs\n{code}```";
            });
            builder.AddField(x =>
            {
                x.Name = $"Result<{resultType}>";
                x.Value = result;
            });
            builder.WithFooter(x =>
            {
                x.Text = $"In {_timer.ElapsedMilliseconds}ms";
            });

            return builder;
        }       
    }
}
