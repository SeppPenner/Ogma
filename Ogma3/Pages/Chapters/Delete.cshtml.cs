﻿using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ogma3.Data;
using Ogma3.Data.Models;

namespace Ogma3.Pages.Chapters
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Chapter Chapter { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Chapter = await _context.Chapters
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.Id == id);

            if (Chapter == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            // Get chapter
            Chapter = await _context.Chapters
                .Where(c => c.Id == id)
                .Include(c => c.CommentsThread)
                .FirstOrDefaultAsync();

            if (Chapter == null) return NotFound();
            
            // Get story for the chapter
            var story = await _context.Stories
                .Where(s => s.Id == Chapter.StoryId)
                .Include(s => s.Author)
                .Include(s => s.Chapters)
                .FirstOrDefaultAsync();

            if (story == null) return NotFound();

            if (!story.Author.IsLoggedIn(User)) return NotFound();

            // Recalculate words and chapters in the story
            story.WordCount = story.Chapters.Sum(c => c.WordCount) - Chapter.WordCount;
            story.ChapterCount = story.Chapters.Count - 1;

            _context.Chapters.Remove(Chapter);
            
            await _context.SaveChangesAsync();

            return RedirectToPage("../Story", new { id = Chapter.StoryId });
        }
    }
}
