#nullable enable

using System;
using System.ComponentModel.DataAnnotations;

namespace Ogma3.Data.Models
{
    public class Chapter
    {
        [Key]
        [Required]
        public int Id { get; set; }

        [Required]
        public int Order { get; set; }

        public DateTime PublishDate { get; set; }

        [Required]
        [MinLength(CTConfig.Chapter.MinTitleLength)]
        [MaxLength(CTConfig.Chapter.MaxTitleLength)]
        public string Title { get; set; }

        [Required]
        [MinLength(CTConfig.Chapter.MinBodyLength)]
        [MaxLength(CTConfig.Chapter.MaxBodyLength)]
        public string Body { get; set; }

        [MaxLength(CTConfig.Chapter.MaxNotesLength)]
        public string? StartNotes { get; set; } = null;

        [MaxLength(CTConfig.Chapter.MaxNotesLength)]
        public string? EndNotes { get; set; } = null;
    }
}