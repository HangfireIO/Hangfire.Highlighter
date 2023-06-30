using System.Data.Entity;
using System.Data.Entity.SqlServer;

namespace Hangfire.Highlighter.Models
{
    // TODO: Switch to EF 6.5 and uninstall ErikEJ.EntityFramework.SqlServer when released
    [DbConfigurationType(typeof(AppServiceMicrosoftSqlDbConfiguration))]
    public class HighlighterDbContext : DbContext
    {
        public HighlighterDbContext()
            : base("HighlighterDb")
        {
        }

        public DbSet<CodeSnippet> CodeSnippets { get; set; }
    }
}