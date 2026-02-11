using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WebApplicationRazorPages.Persistence;
using WebApplicationRazorPages.Persistence.Entities;

namespace WebApplicationRazorPages.Pages.Persons;

public class List : PageModel
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<List> _logger;
    public List<PersonViewModel> Persons { get; set; }

    // Shortcut : ALT + Enter on 'dbContext'
    // 'Introduce read-only field _dbContext'
    public List(ApplicationDbContext dbContext, ILogger<List> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        // TODO: retrieve persons from database
        // and map them to PersonViewModel
        // _dbContext.Person or _dbContext.Set<Person>

        // Same as : 'select * from Person'
        // ToList() executes the query
        // List<Person> persons = _dbContext.Set<Person>().ToList();

        // Same as : 'select Id, FullName from Person'
        // Persons = _dbContext.Set<Person>()
        //     .Select(person => new PersonViewModel()
        //     {
        //         Id = person.Id,
        //         FullName = person.FullName
        //     })
        //     .ToList();

        // SELECT "p"."Id", "p"."FullName", "p"."BirthDate", "p"."Email", "p"."IsAlive", "j"."Name" AS "JobName"
        // FROM "Person" AS "p"
        // INNER JOIN "Job" AS "j" ON "p"."JobId" = "j"."Id"
        // To avoid redundant mappings use AutoMapper
        Persons = await _dbContext.Set<Person>()
            .Select(person => new PersonViewModel()
            {
                Id = person.Id,
                FullName = person.FullName,
                BirthDate = person.BirthDate,
                Email = person.Email,
                IsAlive = person.IsAlive,
                JobName = person.Job.Name
            })
            .ToListAsync(cancellationToken: cancellationToken);

        // Persons = _dbContext.Set<Person>()
        //     .Include()
        //     .Select(person => new PersonViewModel()
        //     {
        //         Id = person.Id,
        //         FullName = person.FullName,
        //         BirthDate = person.BirthDate,
        //         Email = person.Email,
        //         IsAlive = person.IsAlive,
        //         JobName = person.Job.Name
        //     })
        //     .ToList();
    }

    // /!\ Don't forget HTTP verb => OnDelete won't work.
    // OnGetDelete or OnPostDelete is expected.
    public async Task<IActionResult> OnGetDeleteAsync(int id, CancellationToken cancellationToken)
    {
        // var person = _dbContext.Set<Person>().FirstOrDefault(x => x.Id == id);
        // only works on primary key search
        var person = await _dbContext.Set<Person>().FindAsync(id, cancellationToken, cancellationToken);

        if (person != null)
        {
            _dbContext.Set<Person>().Remove(person);
            // same as 'COMMIT;'
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        else
        {
            throw new KeyNotFoundException($"Person with id {id} not found");
        }

        // try
        // {
        //     // Step 1 : je me connecte à la DB
        //     if (person != null)
        //     {
        //         _dbContext.Set<Person>().Remove(person);
        //         // same as 'COMMIT;'
        //         _dbContext.SaveChanges();
        //     }
        //     else
        //     {
        //         throw new KeyNotFoundException($"Person with id {id} not found");
        //     }
        // }
        // catch (Exception ex)
        // {
        //     // catch KeyNotFoundException
        //     _logger.LogError(ex.ToString());
        //     // throw ex;
        //     // throw new Exception("Toute nouvelle exception");
        // }
        // finally
        // {
        //     // Execute this code in all cases
        //     _logger.LogInformation($"Person with id {id} has been deleted");
        //     // Dans tous les cas : clôturer la connexion DB
        // }
        // // clôture connexion DB

        return RedirectToPage("List");
    }

    public IActionResult OnGetCreateOrUpdate(int id)
    {
        var person = _dbContext.Set<Person>().Find(id);

        return person != null
            ? RedirectToPage("CreateOrUpdate", new { id = id })
            : throw new KeyNotFoundException($"Person with id {id} not found");
    }

    public class PersonViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string? Email { get; set; }
        public DateTime BirthDate { get; set; }
        public bool IsAlive { get; set; }
        public string JobName { get; set; }
    }
}