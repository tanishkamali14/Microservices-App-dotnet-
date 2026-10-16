using System.Linq.Expressions;
using Ordering.Application.Common.Contracts.Persistence;
using Ordering.Domain.Common;
using Ordering.Domain.Entities;

namespace Ordering.Application.Tests.Fakes;

/// <summary>In-memory repository that assigns ids the way the database identity column would.</summary>
public class FakeOrderRepository : IOrderRepository
{
    private int nextId = 1;

    public List<Order> Orders { get; } = new();

    public Task<Order> AddAsync(Order entity)
    {
        typeof(EntityBase).GetProperty(nameof(EntityBase.Id))!.SetValue(entity, nextId++);
        Orders.Add(entity);
        return Task.FromResult(entity);
    }

    public Task DeleteAsync(Order entity)
    {
        Orders.Remove(entity);
        return Task.CompletedTask;
    }

    public Task UpdateAsync(Order entity) => Task.CompletedTask;

    public Task<Order?> GetByIdAsync(int id) => Task.FromResult(Orders.FirstOrDefault(o => o.Id == id));

    public Task<IReadOnlyList<Order>> GetAllAsync() => Task.FromResult<IReadOnlyList<Order>>(Orders.ToList());

    public Task<IReadOnlyList<Order>> GetAsync(Expression<Func<Order, bool>> predicate) =>
        Task.FromResult<IReadOnlyList<Order>>(Orders.AsQueryable().Where(predicate).ToList());

    public Task<IReadOnlyList<Order>> GetAsync(Expression<Func<Order, bool>> predicate, Func<IQueryable<Order>, IOrderedQueryable<Order>>? orderBy = null, string? includeString = null, bool disableTracking = true) =>
        GetAsync(predicate);

    public Task<IReadOnlyList<Order>> GetAsync(Expression<Func<Order, bool>> predicate, Func<IQueryable<Order>, IOrderedQueryable<Order>>? orderBy = null, List<Expression<Func<Order, object>>>? includes = null, bool disableTracking = true) =>
        GetAsync(predicate);

    public Task<IEnumerable<Order>> GetOrdersByUserName(string userName) =>
        Task.FromResult<IEnumerable<Order>>(Orders.Where(o => o.Username == userName).ToList());
}
