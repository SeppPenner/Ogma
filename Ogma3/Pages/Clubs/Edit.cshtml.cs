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
using Ogma3.Infrastructure.Attributes;
using Ogma3.Infrastructure.Extensions;
using Ogma3.Services.FileUploader;
using Utils.Extensions;

namespace Ogma3.Pages.Clubs
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

        [BindProperty]
        public InputModel Input { get; set; }
        
        public class InputModel
        {
            public long Id { get; set; }
            
            [Required]
            [StringLength(
                CTConfig.CClub.MaxNameLength, 
                ErrorMessage = CTConfig.CClub.ValidateLengthMsg,
                MinimumLength = CTConfig.CClub.MinNameLength
            )]
            public string Name { get; set; }
                
            [Required]
            [StringLength(
                CTConfig.CClub.MaxHookLength, 
                ErrorMessage = CTConfig.CClub.ValidateLengthMsg,
                MinimumLength = CTConfig.CClub.MinHookLength
            )]
            public string Hook { get; set; }
                
            [StringLength(
                CTConfig.CClub.MaxDescriptionLength, 
                ErrorMessage = CTConfig.CClub.ValidateLengthMsg
            )]
            public string Description { get; set; }
                
            [DataType(DataType.Upload)]
            [MaxFileSize(CTConfig.CClub.CoverMaxWeight)]
            [AllowedExtensions(new []{".jpg", ".jpeg", ".png", ".webp"})]
            public IFormFile Icon { get; set; }
        }
        public async Task<IActionResult> OnGetAsync(long? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var club = await _context.Clubs
                .Where(c => c.Id == id)
                .Select(c => new
                {
                    c.Id,
                    c.Name,
                    c.Description,
                    c.Hook,
                    FounderId = c.ClubMembers.First(cm => cm.Role == EClubMemberRoles.Founder).MemberId
                })
                .AsNoTracking()
                .FirstOrDefaultAsync();
            
            Input = new InputModel
            {
                Id = club.Id,
                Name = club.Name,
                Description = club.Description,
                Hook = club.Hook
            };

            if (club.Name == null) return NotFound();

            if (!User.IsUserSameAsLoggedIn(club.FounderId)) return Unauthorized();
            
            return Page();
        }

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync(long? id)
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var uid = User.GetNumericId();
            if (uid == null) return Unauthorized();

            var club = await _context.Clubs
                .Where(c => c.Id == id)
                .Where(c => c.ClubMembers.Any(cm => cm.MemberId == uid && cm.Role == EClubMemberRoles.Founder))
                .FirstOrDefaultAsync();

            if (club == null) return NotFound();

            club.Name = Input.Name;
            club.Slug = Input.Name.Friendlify();
            club.Hook = Input.Hook;
            club.Description = Input.Description;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ClubExists(club.Id))
                {
                    return NotFound();
                }

                throw;
            }
            
            if (Input.Icon != null && Input.Icon.Length > 0)
            {
                var file = await _uploader.Upload(
                    Input.Icon, 
                    "club-icons", 
                    $"{club.Id}-{club.Name.Friendlify()}",
                    _config.ClubIconWidth,
                    _config.ClubIconHeight
                );
                club.IconId = file.FileId;
                club.Icon = file.Path;
                // Final save
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("/Club/Index", new { id = club.Id });
        }

        private bool ClubExists(long id)
        {
            return _context.Clubs.Any(e => e.Id == id);
        }
    }
}
