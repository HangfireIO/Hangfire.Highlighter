using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using HangFire.Highlighter.Hubs;
using HangFire.Highlighter.Models;
using Microsoft.AspNet.SignalR;

namespace HangFire.Highlighter.Jobs
{
    public class SnippetHighlighter : IDisposable
    {
        private readonly IHubContext _hubContext;
        private readonly HighlighterDbContext _dbContext;

        public SnippetHighlighter()
            : this(GlobalHost.ConnectionManager.GetHubContext<SnippetHub>(), new HighlighterDbContext())
        {
        }

        internal SnippetHighlighter(IHubContext hubContext, HighlighterDbContext dbContext)
        {
            if (hubContext == null) throw new ArgumentNullException("hubContext");
            if (dbContext == null) throw new ArgumentNullException("dbContext");

            _hubContext = hubContext;
            _dbContext = dbContext;
        }

        public void Highlight(int snippetId)
        {
            var snippet = _dbContext.CodeSnippets.Find(snippetId);
            if (snippet == null) return;

            snippet.HighlightedCode = HighlightSource(snippet.SourceCode);
            snippet.HighlightedAt = DateTime.UtcNow;

            _dbContext.SaveChanges();

            _hubContext.Clients.Group(SnippetHub.GetGroup(snippet.Id))
                .highlight(snippet.HighlightedCode);
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        private static async Task<string> HighlightSourceAsync(string source)
        {
            using (var client = new HttpClient())
            {
                var response = await client.PostAsync(
                    @"http://hilite.me/api",
                    new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    { "lexer", "c#" },
                    { "style", "vs" },
                    { "code", source }
                }));

                response.EnsureSuccessStatusCode();

                return await response.Content.ReadAsStringAsync();
            }
        }

        private static string HighlightSource(string source)
        {
            // Microsoft.Net.Http does not provide synchronous API,
            // so we are using wrapper to perform a sync call.
            return RunSync(() => HighlightSourceAsync(source));
        }

        private static TResult RunSync<TResult>(Func<Task<TResult>> func)
        {
            return Task.Run<Task<TResult>>(func).Unwrap().GetAwaiter().GetResult();
        }
    }
}