﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ogma3.Data;
using Ogma3.Data.Enums;
using Ogma3.Data.Models;
using Ogma3.Infrastructure.Attributes;
using Ogma3.Infrastructure.Extensions;
using Ogma3.Services.FileUploader;
using Utils.Extensions;

namespace Ogma3.Pages.Stories
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ImageUploader _uploader;
        private readonly OgmaConfig _config;


        public EditModel(ApplicationDbContext context, ImageUploader uploader, OgmaConfig config)
        {
            _context = context;
            _uploader = uploader;
            _config = config;
        }

        public List<Rating> Ratings { get; set; }
        
        [BindProperty] 
        public InputModel Input { get; set; }

        public class InputModel
        {
            public long Id { get; set; }
            
            [Required]
            [StringLength(
                CTConfig.CStory.MaxTitleLength,
                ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                MinimumLength = CTConfig.CStory.MinTitleLength
            )]
            public string Title { get; set; }

            [Required]
            [StringLength(
                CTConfig.CStory.MaxDescriptionLength,
                ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                MinimumLength = CTConfig.CStory.MinDescriptionLength
            )]
            public string Description { get; set; }

            [Required]
            [StringLength(
                CTConfig.CStory.MaxHookLength,
                ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                MinimumLength = CTConfig.CStory.MinHookLength
            )]
            public string Hook { get; set; }

            [DataType(DataType.Upload)]
            [MaxFileSize(CTConfig.CStory.CoverMaxWeight)]
            [AllowedExtensions(new[] {".jpg", ".jpeg", ".png"})]
            public IFormFile Cover { get; set; }

            [Required] 
            public long Rating { get; set; }

            [Required]
            public EStoryStatus Status { get; set; }

            [Required] 
            public List<long> Tags { get; set; }

            [Required]
            public bool Published { get; set; }
        }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            // Get logged in user
            var uid = User.GetNumericId();
            if (uid == null) return Unauthorized();
            
            // Get story to edit and make sure author matches logged in user
            var story = await _context.Stories
                .Include(s => s.Tags)
                .Include(s => s.Rating)
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.Id == id && s.AuthorId == uid);

            if (story == null) return NotFound();
            
            // Fill InputModel
            Input = new InputModel
            {
                Id = story.Id,
                Title = story.Title,
                Description = story.Description,
                Hook = story.Hook,
                Rating = story.Rating.Id,
                Tags = story.Tags.Select(st => st.Id).ToList(),
                Status = story.Status,
                Published = story.IsPublished
            };
            
            // Fill Ratings dropdown
            Ratings = await _context.Ratings.ToListAsync();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(long? id)
        {
            Ratings = await _context.Ratings.ToListAsync();
            
            if (ModelState.IsValid)
            {
                var tags = await _context.Tags
                    .Where(t => Input.Tags.Contains(t.Id))
                    .ToListAsync();

                // Get logged in user
                var uid = User.GetNumericId();
                if (uid == null) return Unauthorized();
                
                // Get the story and make sure the logged-in user matches author
                var story = await _context.Stories
                    .Include(s => s.Tags)
                    .Include(s => s.Rating)
                    .FirstOrDefaultAsync(s => s.Id == id && s.AuthorId == uid);
                
                // 404 if none found
                if (story == null) return NotFound();
                
                // Check if it can be published
                if(!story.IsPublished && Input.Published && story.ChapterCount <= 0)
                {
                    ModelState.AddModelError("", "You cannot publish a story with no chapters");
                    return Page();
                }

                // Update story
                story.Title = Input.Title;
                story.Slug = Input.Title.Friendlify();
                story.Description = Input.Description;
                story.Hook = Input.Hook;
                story.Rating = await _context.Ratings.FindAsync(Input.Rating);
                story.Tags = tags;
                story.Status = Input.Status;
                story.IsPublished = Input.Published;
                
                _context.Update(story);
                await _context.SaveChangesAsync();
                
                // Handle cover upload
                if (Input.Cover != null && Input.Cover.Length > 0)
                {
                    // Upload cover
                    var file = await _uploader.Upload(
                        Input.Cover, 
                        "covers", 
                        $"{story.Id}-{story.Slug}",
                        _config.StoryCoverWidth,
                        _config.StoryCoverHeight
                    );
                    story.CoverId = file.FileId;
                    story.Cover = file.Path;
                    // Final save
                    await _context.SaveChangesAsync();
                }
                
                return RedirectToPage("../Story", new { id = story.Id, slug = story.Slug });
            }
            else
            {
                return Page();
            }
        }
    }
}
