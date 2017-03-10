using Discord;
using Discord.WebSocket;
using Doggo.Commands;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Doggo.Discord.Roslyn
{
    [RequireOwner]
    [Group("evaluate"), Alias("eval", "e")]
    public class EvalModule : ModuleBase<SocketCommandContext>
    {
        private Stopwatch _timer = new Stopwatch();

        protected override void BeforeExecute()
        {
            _timer.Start();
        }

        [Command(RunMode = RunMode.Async)]
        public async Task EvalAsync([Remainder]string code)
        {
            var options = ScriptOptions.Default
            .AddReferences(new Assembly[]
            {
                typeof(string).GetTypeInfo().Assembly,
                typeof(Assembly).GetTypeInfo().Assembly,
                typeof(Task).GetTypeInfo().Assembly,
                typeof(Enumerable).GetTypeInfo().Assembly,
                typeof(List<>).GetTypeInfo().Assembly,
                typeof(IGuild).GetTypeInfo().Assembly,
                typeof(SocketGuild).GetTypeInfo().Assembly
            })
            .AddImports(new string[]
            {
                "System",
                "System.Reflection",
                "System.Threading.Tasks",
                "System.Linq",
                "System.Collections.Generic",
                "Discord",
                "Discord.WebSocket"
            });
            
            string cleancode = GetCode(code);
            string reply;
            string type;

            try
            {
                var result = await CSharpScript.EvaluateAsync(cleancode, options, Context, typeof(SocketCommandContext));
                type = result.GetType().Name;
                reply = result.ToString();
            } catch (Exception ex)
            {
                type = ex.GetType().Name;
                reply = ex.Message;
            }
            _timer.Stop();
            
            var builder = new EmbedBuilder();
            builder.Color = new Color(197, 187, 62);
            builder.AddField(x =>
            {
                x.Name = "Code";
                x.Value = $"```cs\n{cleancode}```";
            });
            builder.AddField(x =>
            {
                x.Name = $"Result<{type}>";
                x.Value = $"```{reply}```";
            });
            builder.WithFooter(x =>
            {
                x.Text = $"In {_timer.ElapsedMilliseconds}ms";
            });

            await Context.ReplyAsync("", embed: builder);
        }

        private string GetCode(string rawmsg)
        {
            string code = rawmsg;
            
            if (code.StartsWith("```"))
                code = code.Substring(3, code.Length - 6);
            if (code.StartsWith("cs"))
                code = code.Substring(2, code.Length - 2);

            code = code.Trim();
            code = code.Replace(";", ";\n");

            return code;
        }
    }
}
