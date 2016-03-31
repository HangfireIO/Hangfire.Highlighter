using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Hangfire.Highlighter.Hubs;
using Hangfire.Highlighter.Models;
using Microsoft.AspNet.SignalR;

namespace Hangfire.Highlighter.Jobs
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

        public async Task HighlightAsync(int snippetId)
        {
            var snippet = await _dbContext.CodeSnippets.FindAsync(snippetId);
            if (snippet == null) return;

            snippet.HighlightedCode = await HighlightSourceAsync(snippet.SourceCode);
            snippet.HighlightedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();

            _hubContext.Clients.Group(SnippetHub.GetGroup(snippet.Id))
                .highlight(snippet.Id, snippet.HighlightedCode);
        }

        public void CleanUp()
        {
            _dbContext.Database.ExecuteSqlCommand("TRUNCATE TABLE [CodeSnippets]");
        }

        public void Dispose()
        {
            _dbContext.Dispose();
        }

        [Obsolete]
        public void Highlight(int snippetId)
        {
            throw new NotSupportedException("Please use HighlightAsync method instead.");
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
    }
}