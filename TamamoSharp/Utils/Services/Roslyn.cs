using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace TamamoSharp.Services
{
    public class RoslynService
    {
        private readonly IServiceProvider _svc;
        private ScriptOptions _opts;

        public RoslynService(IServiceProvider svc)
        {
            _svc = svc;

            _opts = ScriptOptions.Default
                .AddReferences(
                    typeof(string).GetTypeInfo().Assembly,
                    typeof(Assembly).GetTypeInfo().Assembly,
                    typeof(Task).GetTypeInfo().Assembly,
                    typeof(Enumerable).GetTypeInfo().Assembly,
                    typeof(List<>).GetTypeInfo().Assembly,
                    typeof(IGuild).GetTypeInfo().Assembly,
                    typeof(SocketGuild).GetTypeInfo().Assembly
                )
                .AddImports(
                    "System",
                    "System.Reflection",
                    "System.Threading.Tasks",
                    "System.Linq",
                    "System.Collections.Generic",
                    "Discord",
                    "Discord.WebSocket"
                );
        }

        public async Task<Embed> ExecuteAsync(SocketCommandContext ctx, string content)
        {
            Stopwatch timer = new Stopwatch();

            object result;

            timer.Start();
            try
            {
                result = await CSharpScript.EvaluateAsync(content, _opts, new RoslynGlobals(_svc, ctx));
            }
            catch (Exception e)
            {
                result = e;
            }
            timer.Stop();

            Color color = (result is Exception)
                ? new Color(137, 0, 1)
                : new Color(0, 128, 0);

            EmbedBuilder builder = new EmbedBuilder()
                .WithColor(color)
                .WithFooter(x => { x.Text = $"Execution Time: {timer.ElapsedMilliseconds} ms"; });
            builder.AddField(x =>
            {
                x.Name = $"Result<{result?.GetType().FullName ?? "null"}>";
                x.Value = (result is Exception e)
                    ? e.Message
                    : result ?? "null";
            });

            return builder.Build();
        }
    }

    public class RoslynGlobals
    {
        private readonly SocketCommandContext Context;
        private readonly IServiceProvider _svc;

        public RoslynGlobals(IServiceProvider svc, SocketCommandContext ctx)
        {
            _svc = svc;
            Context = ctx;
        }
    }
}
