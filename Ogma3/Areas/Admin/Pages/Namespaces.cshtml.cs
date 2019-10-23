using System.ComponentModel.DataAnnotations;
using System.Drawing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Ogma3.Areas.Admin.Pages
{
    public class Namespaces : PageModel
    {
        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Required]
            public string Name { get; set; }

            public Color Color { get; set; }
        }
        
        public void OnGet()
        {
            
        }
    }
}