using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using WebApplicationRazorPages.Persistence;
using WebApplicationRazorPages.Persistence.Entities;

namespace WebApplicationRazorPages.Pages.Persons;

public class CreateOrUpdate : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<CreateOrUpdate> _logger;

    [BindProperty] public int Id { get; set; }
    
    [BindProperty] public string FullName { get; set; }

    [BindProperty]
    [RegularExpression(".*\\.be")]
    public string? Email { get; set; }

    [BindProperty] public DateTime BirthDate { get; set; }
    [BindProperty] public bool IsAlive { get; set; }
    [BindProperty] public int JobId { get; set; }
    public IEnumerable<SelectListItem> SliJobs { get; set; }

    public CreateOrUpdate(ApplicationDbContext dbContext, ILogger<CreateOrUpdate> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public void OnGet(int id)
    {
        var jobs = _dbContext.Set<Job>().ToList();
        SliJobs = jobs
            .Select(job => new SelectListItem() { Text = job.Name, Value = job.Id.ToString() })
            .OrderBy(x => x.Text)
            .ToList();

        var person = _dbContext.Set<Person>().Find(id);

        if (person != null)
        {
            Id = person.Id;
            FullName = person.FullName;
            Email = person.Email;
            BirthDate = person.BirthDate;
            IsAlive = person.IsAlive;
            JobId = person.JobId;
        }
    }

    public IActionResult OnPost()
    {
        if (!ModelState.IsValid)
            return Page();

        var person = new Person();

        if (Id > 0)
            person = _dbContext.Set<Person>().Find(Id);

        var test = "";
        person = person ?? new Person();
        person.FullName = FullName;
        person.Email = Email;
        person.BirthDate = BirthDate;
        person.IsAlive = IsAlive;
        person.JobId = JobId;
        
        _dbContext.Update(person);
        _dbContext.SaveChanges();

        return RedirectToPage("List");
    }
}