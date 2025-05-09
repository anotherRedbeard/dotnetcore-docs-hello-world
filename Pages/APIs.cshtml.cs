using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System;

namespace dotnetcoresample.Pages
{
    public class APIsModel : PageModel
    {
        private readonly ILogger<APIsModel> _logger;

        public APIsModel(ILogger<APIsModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {
            _logger.LogInformation("APIs documentation page visited at {Time}", DateTime.UtcNow);
        }
    }
}