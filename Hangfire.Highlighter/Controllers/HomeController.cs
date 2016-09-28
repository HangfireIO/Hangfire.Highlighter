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
        private readonly BackgroundJobClient _jobs = new BackgroundJobClient();

        public ActionResult Index()
        {
            return View(_db.CodeSnippets.OrderByDescending(x => x.Id).ToList());
        }

        public ActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public ActionResult Create([Bind(Include = "SourceCode")] CodeSnippet snippet, string email)
        {
            if (!String.IsNullOrEmpty(email))
            {
                // Simplest SPAM prevention trick
                return RedirectToAction("Index");
            }

            if (ModelState.IsValid)
            {
                snippet.CreatedAt = DateTime.UtcNow;

                _db.CodeSnippets.Add(snippet);
                _db.SaveChanges();

                var parentId = _jobs.Enqueue<SnippetHighlighter>(x => x.HighlightAsync(snippet.Id));
                _jobs.ContinueWith<SnippetHighlighter>(parentId, x => x.SendToSubscribers(snippet.Id));

                return RedirectToAction("Index");
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