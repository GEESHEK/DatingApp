using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces;

public interface ILikesRepository
{
    Task<UserLike> GetUserLike(int sourceUserId, int targetUserId);
    Task<AppUser> GetUserWithLikes(int userId);
    //to get the user they liked or liked by
    Task<PagedList<LikeDto>> GetUserLikes(LikesParams likesParams); 
}