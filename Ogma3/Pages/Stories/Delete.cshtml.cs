﻿using System.Linq;
using System.Threading.Tasks;
using B2Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Ogma3.Data;
using Ogma3.Data.Models;

namespace Ogma3.Pages.Stories
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _config;
        private readonly IB2Client _b2Client;
        private readonly OgmaConfig _ogmaConfig;
        
        public DeleteModel(ApplicationDbContext context, IB2Client b2Client, IConfiguration config, OgmaConfig ogmaConfig)
        {
            _context = context;
            _b2Client = b2Client;
            _config = config;
            _ogmaConfig = ogmaConfig;
        }

        [BindProperty] public Story Story { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            // Get the story and make sure the logged-in user matches author
            Story = await _context.Stories
                .Include(s => s.Author)
                .Include(s => s.Votes)
                .Include(s => s.Rating)
                .Include(s => s.Chapters)
                    .ThenInclude(c => c.CommentsThread)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id);

            if (Story == null) return NotFound();
            if (!Story.Author.IsLoggedIn(User)) return RedirectToPage("Index");

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null) return NotFound();

            // Get the story and make sure the logged-in user matches author
            Story = await _context.Stories
                .Include(s => s.Author)
                .FirstOrDefaultAsync(s => s.Id == id);
            if (Story == null) return NotFound();
            if (!Story.Author.IsLoggedIn(User)) return Forbid();

            // Remove tag associations
            await _context.StoryTags
                .Where(st => st.StoryId == Story.Id)
                .ForEachAsync(st => _context.StoryTags.Remove(st));

            // Remove story
            _context.Stories.Remove(Story);
            
            // Delete cover
            if (Story.CoverId != null && Story.Cover != null) 
                await _b2Client.Files.Delete(Story.CoverId, Story.Cover.Replace(_ogmaConfig.Cdn, ""));

            // Save
            await _context.SaveChangesAsync();

            return RedirectToPage("/User/Stories", new { Name = Story.Author.UserName });
        }
    }
}