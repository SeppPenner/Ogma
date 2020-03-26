using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ogma3.Data;
using Ogma3.Data.Models;

namespace Ogma3.Pages.Blog
{
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DeleteModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public Blogpost Blogpost { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Blogpost = await _context.Blogposts.FirstOrDefaultAsync(m => m.Id == id);

            if (Blogpost == null)
            {
                return NotFound();
            }
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            Blogpost = await _context.Blogposts.FindAsync(id);

            if (Blogpost != null)
            {
                _context.Blogposts.Remove(Blogpost);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("./Index", new { name = Blogpost.Author.UserName });
        }
    }
}
