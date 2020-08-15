using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ogma3.Data;
using Ogma3.Data.Models;
using Utils.Extensions;

namespace Ogma3.Pages.Blog
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly OgmaUserManager _userManager;

        public CreateModel(ApplicationDbContext context, OgmaUserManager userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public IActionResult OnGet()
        {
            Input = new InputModel();
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
            var user = await _userManager.GetUserAsync(User);
            
            // Find hashtags
            var rgx = new Regex(@"\#[\w\-]+");
            var tags = rgx
                .Matches(Input.Body)
                .Select(m => m.Value)
                .Distinct()
                .Take(CTConfig.CBlogpost.MaxTagsAmount)
                .ToList();

            await _context.Blogposts.AddAsync(new Blogpost
            {
                Title = Input.Title.Trim(),
                Slug = Input.Title.Trim().Friendlify(),
                Body = Input.Body.Trim(),
                Author = user,
                CommentsThread = new CommentsThread(),
                WordCount = Input.Body.Trim().Split(' ', '\t', '\n').Length,
                Hashtags = tags.ToArray()
            });
            await _context.SaveChangesAsync();

            return RedirectToPage("./Index", new { name = user.UserName });
        }
    }
}
