using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SocialViralAI.Views.Home
{
    public class ShortModel : PageModel
    {
        private readonly ILogger<ShortModel> _logger;

        public ShortModel(ILogger<ShortModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
