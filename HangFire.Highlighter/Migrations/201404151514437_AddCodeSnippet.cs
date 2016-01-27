namespace Hangfire.Highlighter.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddCodeSnippet : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.CodeSnippets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        SourceCode = c.String(nullable: false),
                        HighlightedCode = c.String(),
                        CreatedAt = c.DateTime(nullable: false),
                        HighlightedAt = c.DateTime(),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.CodeSnippets");
        }
    }
}
