using System;
using System.Linq;
using System.Web.Mvc;
using Hangfire.Highlighter.Jobs;
using Hangfire.Highlighter.Models;

namespace Hangfire.Highlighter.Controllers
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
            if (ModelState.IsValid)
            {
                snippet.CreatedAt = DateTime.UtcNow;

                _db.CodeSnippets.Add(snippet);
                _db.SaveChanges();

                using (StackExchange.Profiling.MiniProfiler.StepStatic("Job enqueue"))
                {
                    // Enqueue a job
                    BackgroundJob.Enqueue<SnippetHighlighter>(x => x.Highlight(snippet.Id));
                }

                return RedirectToAction("Details", new { id = snippet.Id });
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
    }
}