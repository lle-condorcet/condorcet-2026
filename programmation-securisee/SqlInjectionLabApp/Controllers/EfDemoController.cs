using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SqlInjectionLabApp.Data;
using SqlInjectionLabApp.Models;

namespace SqlInjectionLabApp.Controllers;

/// <summary>
/// Demonstration de 3 approches avec Entity Framework :
/// 1. FromSqlRaw (VULNERABLE si mal utilise)
/// 2. LINQ (SECURISE par defaut)
/// 3. FromSqlInterpolated (SECURISE avec SQL brut)
/// </summary>
public class EfDemoController : Controller
{
    private readonly AppDbContext _context;

    public EfDemoController(AppDbContext context)
    {
        _context = context;
    }

    // ================================================================
    // Vue d'ensemble des 3 approches
    // ================================================================
    public IActionResult Index()
    {
        return View();
    }

    // ================================================================
    // 1. FromSqlRaw VULNERABLE (string concatenation dans du raw SQL)
    // ================================================================
    public IActionResult FromSqlRawVulnerable(string? q)
    {
        var model = new EfDemoViewModel
        {
            SearchTerm = q,
            Approach = "FromSqlRaw (VULNERABLE)",
            IsVulnerable = true,
        };

        if (!string.IsNullOrEmpty(q))
        {
            // DANGEREUX : string concatenation dans FromSqlRaw
            var sql = $"SELECT * FROM EfPersons WHERE LastName LIKE '%{q}%' OR City LIKE '%{q}%'";
            model.GeneratedSql = sql;

            try
            {
                model.Results = _context.Persons
                    .FromSqlRaw(sql)
                    .ToList();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
            }
        }

        return View(model);
    }

    // ================================================================
    // 2. LINQ (securise par defaut - la methode recommandee)
    // ================================================================
    public IActionResult Linq(string? q)
    {
        var model = new EfDemoViewModel
        {
            SearchTerm = q,
            Approach = "LINQ (SECURISE)",
            IsVulnerable = false,
        };

        if (!string.IsNullOrEmpty(q))
        {
            try
            {
                model.Results = _context.Persons
                    .Where(p => p.LastName.Contains(q) || p.City.Contains(q))
                    .ToList();

                // Montrer le SQL genere par EF
                model.GeneratedSql = _context.Persons
                    .Where(p => p.LastName.Contains(q) || p.City.Contains(q))
                    .ToQueryString();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
            }
        }

        return View(model);
    }

    // ================================================================
    // 3. FromSqlInterpolated (securise - SQL brut avec parametres)
    // ================================================================
    public IActionResult FromSqlInterpolated(string? q)
    {
        var model = new EfDemoViewModel
        {
            SearchTerm = q,
            Approach = "FromSqlInterpolated (SECURISE)",
            IsVulnerable = false,
        };

        if (!string.IsNullOrEmpty(q))
        {
            try
            {
                var pattern = $"%{q}%";

                model.Results = _context.Persons
                    .FromSqlInterpolated($"SELECT * FROM EfPersons WHERE LastName LIKE {pattern} OR City LIKE {pattern}")
                    .ToList();

                // Montrer le SQL genere
                model.GeneratedSql = _context.Persons
                    .FromSqlInterpolated($"SELECT * FROM EfPersons WHERE LastName LIKE {pattern} OR City LIKE {pattern}")
                    .ToQueryString();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
            }
        }

        return View(model);
    }
}
