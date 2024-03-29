using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data;

public class UserRepository : IUserRepository
{
    private readonly DataContext _context;
    private readonly IMapper _mapper;

    public UserRepository(DataContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public void Update(AppUser user)
    {
        _context.Entry(user).State = EntityState.Modified;
    }

    public async Task<bool> SaveAllAsync()
    {
        return await _context.SaveChangesAsync() > 0; //returns how many changes made to the db.
    }

    public async Task<IEnumerable<AppUser>> GetUsersAsync()
    {
        return await _context.Users.Include(p => p.Photos)
            .ToListAsync();
    }

    public async Task<AppUser> GetUserByIdAsync(int id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<AppUser> GetUserByUsernameAsync(string username)
    {
        return await _context.Users
            .Include(p => p.Photos)
            .SingleOrDefaultAsync(x => x.UserName == username);
    }

    public async Task<PagedList<MemberDto>> GetMembersAsync(UserParams userParams)
    {
        // var query = _context.Users
        //     .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
        //     .AsNoTracking();
        
        //building up the query below to filter current user out and opposite gender if not selected
        var query = _context.Users.AsQueryable();
        
        query = query.Where(u => u.UserName != userParams.CurrentUsername);
        query = query.Where(u => u.Gender == userParams.Gender);
        
        //storing DOB > workout min DOB and max DOB
        //1920 is lower than 1980 i.e. older and maxDob is youngest i.e. higher DOB
        var minDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MaxAge - 1));
        var maxDob = DateOnly.FromDateTime(DateTime.Today.AddYears(-userParams.MinAge - 1));

        query = query.Where(u => u.DateOfBirth >= minDob && u.DateOfBirth <= maxDob);
        //order results
        query = userParams.OrderBy switch
        {
            "created" => query.OrderByDescending(u => u.Created),
            _ => query.OrderByDescending(u => u.LastActive)
        };
        
        return await PagedList<MemberDto>.CreateAsync(
            query.AsNoTracking().ProjectTo<MemberDto>(_mapper.ConfigurationProvider), 
            userParams.PageNumber, userParams.PageSize);

    }

    public async Task<MemberDto> GetMemberAsync(string username)
    {
        return await _context.Users
            .Where(x => x.UserName == username)
            .ProjectTo<MemberDto>(_mapper.ConfigurationProvider)
            .SingleOrDefaultAsync();
    }
}