using API.Interfaces;
using AutoMapper;

namespace API.Data;

//Inject this inside our controllers instead of the individual repos
public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UnitOfWork(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public IUserRepository UserRepository => new UserRepository(_context, _mapper);
    public IMessageRepository MessageRepository => new MessageRepository(_context, _mapper);
    public ILikesRepository LikesRepository => new LikesRepository(_context);

    public async Task<bool> Complete()
    {
        //as long as there is more than 0 changes it will return as true
        return await _context.SaveChangesAsync() > 0;
    }

    public bool HasChanges()
    {
        return _context.ChangeTracker.HasChanges();
    }
}