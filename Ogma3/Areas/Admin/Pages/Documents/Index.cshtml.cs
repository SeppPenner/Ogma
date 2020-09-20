using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Ogma3.Data;
using Ogma3.Data.Models;

namespace Ogma3.Areas.Admin.Pages.Documents
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Document> Docs { get; set; }
        public async Task OnGetAsync()
        {
            Docs = await _context.Documents
                .Where(d => !d.RevisionDate.HasValue)
                .OrderBy(d => d.Slug)
                .AsNoTracking()
                .ToListAsync();
        }
    }
}