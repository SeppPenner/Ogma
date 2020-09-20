using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ogma3.Data.Repositories;
using Ogma3.Pages.Shared;

namespace Ogma3.Pages.Club.Forums
{
    public class IndexModel : PageModel
    {
        private readonly ClubRepository _clubRepo;
        private readonly ThreadRepository _threadRepo;

        public IndexModel(ClubRepository clubRepo, ThreadRepository threadRepo)
        {
            _clubRepo = clubRepo;
            _threadRepo = threadRepo;
        }

        public ClubBar ClubBar { get; set; }
        public IList<ThreadCard> ThreadCards { get;set; }

        public async Task<IActionResult> OnGetAsync(long id)
        {
            long.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out var uid);
            ClubBar = await _clubRepo.GetClubBar(id, uid);
            
            if (ClubBar == null) return NotFound();

            ThreadCards = await _threadRepo.GetThreadCards(id, 3);

            return Page();
        }

    }
}
