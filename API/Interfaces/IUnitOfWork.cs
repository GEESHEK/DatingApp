namespace API.Interfaces;

public interface IUnitOfWork
{
    IUserRepository UserRepository { get; }
    IMessageRepository MessageRepository { get; }
    ILikesRepository LikesRepository { get; }
    //If we want to use all 3 repos in the same go, if one fails then they should all fail
    //This makes sure the data is consistent 
    //If complete fails we roll back to a previous state, can argue save changes async already does that
    Task<bool> Complete();
    //Notify us if EF is tracking anything that's been changed inside its transaction
    bool HasChanges();
}