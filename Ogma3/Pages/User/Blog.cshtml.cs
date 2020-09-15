using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ogma3.Data;
using Ogma3.Data.DTOs;
using Ogma3.Data.Models;
using Ogma3.Data.Repositories;
using Ogma3.Pages.Shared;

namespace Ogma3.Pages.User
{
    public class BlogModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserRepository _userRepo;
        public BlogModel(ApplicationDbContext context, UserRepository userRepo)
        {
            _context = context;
            _userRepo = userRepo;
        }
        
        public IList<Blogpost> Posts { get;set; }
        private const int PerPage = 25;
        public ProfileBar ProfileBar { get; set; }
        public bool IsCurrentUser { get; set; }
        public Pagination Pagination { get; set; }
        public async Task<ActionResult> OnGetAsync(string name, [FromQuery] int page = 1)
        {
            ProfileBar = await _userRepo.GetProfileBar(name.ToUpper());
            if (ProfileBar == null) return NotFound();

            IsCurrentUser = ProfileBar.Id.ToString() == User.FindFirstValue(ClaimTypes.NameIdentifier);

            var query = IsCurrentUser
                ? _context.Blogposts.Where(b => b.Author.Id == ProfileBar.Id)
                : _context.Blogposts.Where(b => b.Author.Id == ProfileBar.Id && b.IsPublished);

            var postsCount = await query.CountAsync();

            Posts = await query
                .Skip(Math.Max(0, page - 1) * PerPage)
                .Take(PerPage)
                .Include(b => b.Author)
                .AsNoTracking()
                .ToListAsync();
            
            // Prepare pagination
            Pagination = new Pagination
            {
                PerPage = PerPage,
                ItemCount = postsCount,
                CurrentPage = page
            };
            
            return Page();
        }

    }
}
