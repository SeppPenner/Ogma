using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Ogma3.Data;
using Ogma3.Data.Enums;
using Ogma3.Data.Models;
using Ogma3.Infrastructure.Attributes;
using Ogma3.Infrastructure.Extensions;
using Ogma3.Services.FileUploader;
using Utils.Extensions;

namespace Ogma3.Pages.Clubs
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly ImageUploader _uploader;
        private readonly OgmaConfig _config;

        public CreateModel(ApplicationDbContext context, ImageUploader uploader, OgmaConfig config)
        {
            _context = context;
            _uploader = uploader;
            _config = config;
        }

        public IActionResult OnGet()
        {
            return Page();
        }

        [BindProperty]
        public InputModel Input { get; set; }
        
        public class InputModel
        {
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

        // To protect from overposting attacks, please enable the specific properties you want to bind to, for
        // more details see https://aka.ms/RazorPagesCRUD.
        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();
            var uid = User.GetNumericId();
            if (uid is null) return Unauthorized();

            var club = new Data.Models.Club
            {
                Name = Input.Name,
                Slug = Input.Name.Friendlify(),
                Hook = Input.Hook,
                Description = Input.Description,
            };
            await _context.Clubs.AddAsync(club);

            var member = new ClubMember
            {
                MemberId = (long) uid,
                Role = EClubMemberRoles.Founder
            };
            club.ClubMembers.Add(member);
            
            await _context.SaveChangesAsync();
            
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

            return RedirectToPage("./Index");
        }
    }
}
