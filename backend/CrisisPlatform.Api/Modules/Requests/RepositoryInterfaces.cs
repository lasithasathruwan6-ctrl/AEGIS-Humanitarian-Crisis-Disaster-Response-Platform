namespace CrisisPlatform.Api.Modules.Requests;

// ============================================================================
// Repository Pattern
// ============================================================================

/// <summary>
/// Abstracts data-access for a specific aggregate root <typeparamref name="T"/>.
/// <para>
/// <b>Pattern:</b> Repository (Fowler, PoEAA §322).  Controllers and application
/// services depend on this interface; the concrete EF Core implementation lives
/// in the Infrastructure layer, keeping the domain free of persistence concerns.
/// </para>
/// <para>Registered in DI as a scoped lifetime, one instance per HTTP request.</para>
/// </summary>
/// <typeparam name="T">The aggregate-root entity type.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Finds an entity by its surrogate primary key, or returns <c>null</c>.</summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Returns all entities that satisfy <paramref name="predicate"/>.</summary>
    Task<IReadOnlyList<T>> FindAsync(
        Func<T, bool> predicate,
        CancellationToken cancellationToken = default);

    /// <summary>Schedules <paramref name="entity"/> for insertion on the next commit.</summary>
    void Add(T entity);

    /// <summary>Schedules <paramref name="entity"/> for deletion on the next commit.</summary>
    void Remove(T entity);
}

// ============================================================================
// Unit of Work Pattern
// ============================================================================

/// <summary>
/// Groups one or more repository operations into a single atomic transaction.
/// <para>
/// <b>Pattern:</b> Unit of Work (Fowler, PoEAA §184).  Callers mutate entities
/// through <see cref="IRepository{T}"/> instances, then call
/// <see cref="CommitAsync"/> once to persist every pending change in a single
/// database round-trip.  This prevents partial writes and removes the need for
/// nested <c>SaveChanges</c> calls scattered across the codebase.
/// </para>
/// <example>
/// <code>
/// await _requests.Add(newRequest);
/// await _uow.CommitAsync(cancellationToken);
/// </code>
/// </example>
/// </summary>
public interface IUnitOfWork
{
    /// <summary>
    /// Persists all pending changes from every repository participating in the
    /// current scope and returns the number of state entries written.
    /// </summary>
    Task<int> CommitAsync(CancellationToken cancellationToken = default);
}

// ============================================================================
// Factory Pattern
// ============================================================================

/// <summary>
/// Creates fully constructed <typeparamref name="TProduct"/> instances, hiding
/// the construction logic from callers.
/// <para>
/// <b>Pattern:</b> Factory Method / Abstract Factory (GoF §107).  The concrete
/// <c>NotificationFactory</c> selects message templates keyed on the recipient's
/// locale (English, Sinhala, Tamil) and assembles a <c>Notification</c> value
/// object without exposing that locale-branching logic to controllers or services.
/// </para>
/// </summary>
/// <typeparam name="TInput">Input data needed to build the product.</typeparam>
/// <typeparam name="TProduct">The constructed product type.</typeparam>
public interface IFactory<in TInput, out TProduct>
{
    /// <summary>Constructs and returns a new <typeparamref name="TProduct"/> from <paramref name="input"/>.</summary>
    TProduct Create(TInput input);
}
