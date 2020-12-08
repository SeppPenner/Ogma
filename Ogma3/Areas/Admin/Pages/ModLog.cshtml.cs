using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ogma3.Data;
using Ogma3.Data.Models;
using Ogma3.Pages.Shared;

namespace Ogma3.Areas.Admin.Pages
{
    public class ModLog : PageModel
    {
        private readonly ApplicationDbContext _context;
        private const int PerPage = 50;

        public ModLog(ApplicationDbContext context)
        {
            _context = context;
        }
        
        public ICollection<ModeratorAction> Actions { get; set; }
        public Pagination Pagination { get; set; }

        public async Task OnGet([FromQuery] int page = 1)
        {
            Actions = await _context.ModeratorActions
                .OrderByDescending(ma => ma.DateTime)
                .Paginate(page, PerPage)
                .ToListAsync();
            var count = await _context.ModeratorActions.CountAsync();

            Pagination = new Pagination
            {
                PerPage = PerPage,
                CurrentPage = page,
                ItemCount = count
            };
        }
    }
}