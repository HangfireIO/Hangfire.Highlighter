using System.Data.Entity;

namespace Hangfire.Highlighter.Models
{
    public class HighlighterDbContext : DbContext
    {
        public HighlighterDbContext()
            : base("HighlighterDb")
        {
        }

        public DbSet<CodeSnippet> CodeSnippets { get; set; }
    }
}