using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Expenses.Core;

public sealed class ExpensesDbContext(DbContextOptions<ExpensesDbContext> options) : DbContext(options)
{
    public DbSet<BookkeepingPeriod> Periods => Set<BookkeepingPeriod>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<ExpenseDraft> Drafts => Set<ExpenseDraft>();
    public DbSet<Reminder> Reminders => Set<Reminder>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<BookkeepingPeriod>().HasIndex(p => new { p.Year, p.Kind, p.Number, p.StateCode }).IsUnique();
        model.Entity<Expense>().HasOne<BookkeepingPeriod>().WithMany().HasForeignKey(e => e.BookkeepingPeriodId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Expense>().HasQueryFilter(e => !e.IsDeleted);
        model.Entity<Expense>().Property(e => e.Revision).IsConcurrencyToken();
        model.Entity<Expense>().Ignore(e => e.Amount).Ignore(e => e.BusinessAmount);
        model.Entity<Expense>().HasIndex(e => new { e.BookkeepingPeriodId, e.PaidDate });
        model.Entity<ExpenseDraft>().HasOne<BookkeepingPeriod>().WithMany().HasForeignKey(d => d.BookkeepingPeriodId).OnDelete(DeleteBehavior.Restrict);
        model.Entity<Reminder>().HasOne<BookkeepingPeriod>().WithMany().HasForeignKey(r => r.BookkeepingPeriodId).OnDelete(DeleteBehavior.Restrict);
    }
}

public sealed class ExpensesDesignTimeFactory : IDesignTimeDbContextFactory<ExpensesDbContext>
{
    public ExpensesDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<ExpensesDbContext>().UseSqlite("Data Source=expenses-design.db").Options);
}
