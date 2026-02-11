using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplicationRazorPages.Persistence;
using WebApplicationRazorPages.Persistence.Entities;

namespace WebApplicationRazorPages.Pages.Persons;

public class Create : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<Create> _logger;

    [BindProperty] public string FullName { get; set; }
    [BindProperty] [RegularExpression(".*\\.be")] public string? Email { get; set; }
    [BindProperty] public DateTime BirthDate { get; set; }
    [BindProperty] public bool IsAlive { get; set; }
    [BindProperty] public int JobId { get; set; }
    public IEnumerable<SelectListItem> SliJobs { get; set; }

    public Create(ApplicationDbContext dbContext, ILogger<Create> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public void OnGet()
    {
        var jobs = _dbContext.Set<Job>().ToList();
        SliJobs = jobs
            .Select(job => new SelectListItem() { Text = job.Name, Value = job.Id.ToString() })
            .OrderBy(x => x.Text)
            .ToList();
        // do things..
        // return form page
    }

    public IActionResult OnPost()
    {
        if(!ModelState.IsValid)
            return Page();
        
        // TODO: retrieve field values from form
        // TODO: save in database
        var person = new Person()
        {
            FullName = FullName,
            Email = Email,
            BirthDate = BirthDate,
            IsAlive = IsAlive,
            JobId = JobId
        };

        _dbContext.Add(person);
        _dbContext.SaveChanges();

        return RedirectToPage("List");
    }
}