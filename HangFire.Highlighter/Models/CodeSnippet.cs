using System;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;

namespace Hangfire.Highlighter.Models
{
    public class CodeSnippet
    {
        public int Id { get; set; }

        [Required, AllowHtml, Display(Name = "C# source")]
        public string SourceCode { get; set; }
        public string HighlightedCode { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? HighlightedAt { get; set; }
    }
}