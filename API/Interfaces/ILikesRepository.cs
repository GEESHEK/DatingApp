using API.DTOs;
using API.Entities;

namespace API.Interfaces;

public interface ILikesRepository
{
    Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);
    Task<AppUser> GetUserWithLikes(int userId);
    //to get the user they liked or liked by
    Task<IEnumerable<LikeDto>> GetUserLikes(string predicate, int userId); 
}