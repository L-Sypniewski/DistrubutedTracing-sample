using Microsoft.EntityFrameworkCore;

namespace WebApiSecond.EfCore;

public class WebApiSecondDbContext : DbContext
{
    public DbSet<Person> Persons { get; set; }

    public WebApiSecondDbContext() { }
    public WebApiSecondDbContext(DbContextOptions<WebApiSecondDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>()
            .HasKey(p => p.PersonId);
    }
}

public class Person
{
    public int PersonId { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
}
