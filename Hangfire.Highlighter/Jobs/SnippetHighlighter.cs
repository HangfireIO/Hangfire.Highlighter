using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Transactions;
using Hangfire.Highlighter.Hubs;
using Hangfire.Highlighter.Models;
using Microsoft.AspNet.SignalR;

namespace Hangfire.Highlighter.Jobs
{
    public class SnippetHighlighter : IDisposable
    {
        private readonly IHubContext _hubContext;
        private readonly HighlighterDbContext _dbContext;
        private readonly IBackgroundJobClient _backgroundJobs;

        public SnippetHighlighter()
            : this(GlobalHost.ConnectionManager.GetHubContext<SnippetHub>(), new HighlighterDbContext(), new BackgroundJobClient())
        {
        }

        internal SnippetHighlighter(
            IHubContext hubContext,
            HighlighterDbContext dbContext,
            IBackgroundJobClient backgroundJobs)
        {
            if (hubContext == null) throw new ArgumentNullException(nameof(hubContext));
            if (dbContext == null) throw new ArgumentNullException(nameof(dbContext));
            if (backgroundJobs == null) throw new ArgumentNullException(nameof(backgroundJobs));

            _hubContext = hubContext;
            _dbContext = dbContext;
            _backgroundJobs = backgroundJobs;
        }

        public void CreateSnippet(CodeSnippet snippet)
        {
            using (var scope = new TransactionScope(
                TransactionScopeOption.RequiresNew,
                new TransactionOptions { IsolationLevel = IsolationLevel.ReadCommitted }))
            {
                _dbContext.CodeSnippets.Add(snippet);
                _dbContext.SaveChanges();

                var parentId = _backgroundJobs.Enqueue<SnippetHighlighter>(x => x.HighlightAsync(snippet.Id));
                _backgroundJobs.ContinueJobWith<SnippetHighlighter>(parentId, x => x.SendToSubscribers(snippet.Id));

                scope.Complete();
            }
        }

        public async Task HighlightAsync(int snippetId)
        {
            var snippet = await _dbContext.CodeSnippets.FindAsync(snippetId);
            if (snippet == null) return;

            snippet.HighlightedCode = await HighlightSourceAsync(snippet.SourceCode);
            snippet.HighlightedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
        }

        public void SendToSubscribers(int snippetId)
        {
            var snippet = _dbContext.CodeSnippets.Find(snippetId);
            if (snippet == null) return;

            _hubContext.Clients.Group(SnippetHub.GetGroup(snippet.Id))
                .highlight(snippet.Id, snippet.HighlightedCode, snippet.HighlightedIn?.TotalMilliseconds.ToString("N0"));
        }

        public async Task CleanUpAsync()
        {
            await _dbContext.Database.ExecuteSqlCommandAsync("DELETE FROM [CodeSnippets]");

            _backgroundJobs.Enqueue<SnippetHighlighter>(x => x.CreateSnippet(new CodeSnippet
            {
                SourceCode = "Console.WriteLine(\"Hello, world!\");",
                CreatedAt = DateTime.UtcNow
            }));
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
    }
}