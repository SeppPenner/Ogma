using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ogma3.Data.Repositories;
using Ogma3.Pages.Shared;
using Ogma3.Pages.Shared.Bars;
using Ogma3.Pages.Shared.Details;
using Utils.Extensions;

namespace Ogma3.Pages.Blog
{
    public class DetailsModel : PageModel
    {
        private readonly UserRepository _userRepo;
        private readonly BlogpostsRepository _blogpostsRepo;
        
        public DetailsModel(UserRepository userRepo, BlogpostsRepository blogpostsRepo)
        {
            _userRepo = userRepo;
            _blogpostsRepo = blogpostsRepo;
        }

        public BlogpostDetails Blogpost { get; set; }
        public ProfileBar ProfileBar { get; set; }

        public async Task<IActionResult> OnGetAsync(long id, string? slug)
        {
            Blogpost = await _blogpostsRepo.GetDetails(id, false);

            if (Blogpost == null) return NotFound();
            if (!Blogpost.IsPublished && !User.IsUserSameAsLoggedIn(Blogpost.AuthorId)) return NotFound();

            if (Blogpost.AttachedChapter is not null && !Blogpost.AttachedChapter.IsPublished)
            {
                Blogpost.IsUnavailable = true;
            }
            else if (Blogpost.AttachedStory is not null && !Blogpost.AttachedStory.IsPublished)
            {
                Blogpost.IsUnavailable = true;
            }
            
            ProfileBar = await _userRepo.GetProfileBar(Blogpost.AuthorId);
            
            return Page();
        }

    }
}
