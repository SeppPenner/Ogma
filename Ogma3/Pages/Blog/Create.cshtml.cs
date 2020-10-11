using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ogma3.Data;
using Ogma3.Data.Models;
using Ogma3.Data.Repositories;
using Ogma3.Pages.Shared;
using Utils.Extensions;

namespace Ogma3.Pages.Blog
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly StoriesRepository _storiesRepo;
        private readonly ChaptersRepository _chaptersRepo;

        public CreateModel(ApplicationDbContext context, StoriesRepository storiesRepo, ChaptersRepository chaptersRepo)
        {
            _context = context;
            _storiesRepo = storiesRepo;
            _chaptersRepo = chaptersRepo;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task<IActionResult> OnGet([FromQuery] long? story, [FromQuery] long? chapter)
        {
            Input = new InputModel();

            if (story.HasValue)
            {
                Input.StoryMinimal = await _storiesRepo.GetMinimal((long) story);
                Input.IsUnavailable = Input.StoryMinimal is null;
            }
            else if (chapter.HasValue)
            {
                Input.ChapterMinimal = await _chaptersRepo.GetMinimal((long) chapter);
                Input.IsUnavailable = Input.ChapterMinimal is null;
            }
            
            return Page();
        }

        public class InputModel
        {
            [Required]
            [StringLength(CTConfig.CBlogpost.MaxTitleLength,
                ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                MinimumLength = CTConfig.CBlogpost.MinTitleLength)]
            public string Title { get; set; }
            
            [Required]
            [StringLength(CTConfig.CBlogpost.MaxBodyLength,
                ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.",
                MinimumLength = CTConfig.CBlogpost.MinBodyLength)]
            public string Body { get; set; }
            
            public string Tags { get; set; }

            public ChapterMinimal? ChapterMinimal { get; set; }
            public long? ChapterMinimalId { get; set; }
            public StoryMinimal? StoryMinimal { get; set; }
            public long? StoryMinimalId { get; set; }
            public bool IsUnavailable { get; set; }
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }
            
            // Get logged in user
            var uid = User.GetNumericId();
            var uname = User.GetUsername();
            
            // Return if not logged in
            if (uid == null || uname == null) return Unauthorized();
            
            // Create array of hashtags
            var tags = Input.Tags?
                .Split(',')
                .Where(t => !string.IsNullOrWhiteSpace(t))
                .ToList()
                .Select(t => '#' + t.Trim(' ', '#', ',').Friendlify())
                .Distinct()
                .ToArray();

            var post = new Blogpost
            {
                Title = Input.Title.Trim(),
                Slug = Input.Title.Trim().Friendlify(),
                Body = Input.Body.Trim(),
                AuthorId = (long) uid,
                CommentsThread = new CommentsThread(),
                WordCount = Input.Body.Trim().Split(' ', '\t', '\n').Length,
                Hashtags = tags ?? Array.Empty<string>()
            };

            if (Input.StoryMinimalId.HasValue)
            {
                post.AttachedStoryId = Input.StoryMinimalId;
            } 
            else if (Input.ChapterMinimalId.HasValue)
            {
                post.AttachedChapterId = Input.ChapterMinimalId;
            }

            await _context.Blogposts.AddAsync(post);
            await _context.SaveChangesAsync();

            return RedirectToPage("/User/Blog", new { name = uname });
        }
    }
}
