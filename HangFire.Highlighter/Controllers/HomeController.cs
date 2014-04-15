using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Mvc;
using HangFire.Highlighter.Models;

namespace HangFire.Highlighter.Controllers
{
    public class HomeController : Controller
    {
        private readonly HighlighterDbContext _db = new HighlighterDbContext();

        public ActionResult Index()
        {
            return View(_db.CodeSnippets.ToList());
        }

        public ActionResult Details(int id)
        {
            var snippet = _db.CodeSnippets.Find(id);
            return View(snippet);
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "SourceCode")] CodeSnippet snippet)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    snippet.CreatedAt = DateTime.UtcNow;

                    using (StackExchange.Profiling.MiniProfiler.StepStatic("Service call"))
                    {
                        snippet.HighlightedCode = HighlightSource(snippet.SourceCode);
                        snippet.HighlightedAt = DateTime.UtcNow;
                    }

                    _db.CodeSnippets.Add(snippet);
                    _db.SaveChanges();

                    return RedirectToAction("Details", new { id = snippet.Id });
                }
            }
            catch (HttpRequestException)
            {
                ModelState.AddModelError("", "Highlighting service returned error. Try again later.");
            }

            return View(snippet);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _db.Dispose();
            }
            base.Dispose(disposing);
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