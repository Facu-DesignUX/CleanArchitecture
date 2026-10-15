using CommerX.Domain.Customers.Entities;
using CommerX.Domain.Customers.Repositories;
using CommerX.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CommerX.Infrastructure.Customers;

public sealed class CustomerRepository : ICustomerRepository
{
    private readonly CommerXDbContext _db;

    public CustomerRepository(CommerXDbContext db)
    {
        _db = db;
    }

    // CU-CLI-001 — se compara contra la propiedad Value del Value Object: EF la traduce a SQL
    public async Task<Customer?> FindByDocumentAsync(string document)
    {
        return await _db.Customers
            .FirstOrDefaultAsync(c => c.Document.Value == document);
    }

    public async Task<Customer?> FindByEmailAsync(string email)
    {
        return await _db.Customers
            .FirstOrDefaultAsync(c => c.Email.Value == email);
    }

    public async Task<bool> ExistsByDocumentAsync(string document)
    {
        return await _db.Customers
            .AnyAsync(c => c.Document.Value == document);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _db.Customers
            .AnyAsync(c => c.Email.Value == email);
    }

    public async Task AddAsync(Customer customer)
    {
        await _db.Customers.AddAsync(customer);
        await _db.SaveChangesAsync();
    }

    // CU-CLI-002
    public async Task<Customer?> FindByIdAsync(Guid id)
    {
        return await _db.Customers
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<Customer?> GetByIdAsync(Guid id)
    {
        return await FindByIdAsync(id);
    }

    public async Task UpdateAsync(Customer customer)
    {
        _db.Customers.Update(customer);
        await _db.SaveChangesAsync();
    }
}
