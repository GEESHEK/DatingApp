namespace API.SignalR;

public class PresenceTracker
{
    //username, connectionIDs
    private static readonly Dictionary<string, List<String>> OnlineUsers = 
        new Dictionary<string, List<string>>();
    
    //dictionaries aren't thread safe, use lock
    public Task UserConnected(string username, string connectionId)
    {
        //users have to wait for their turn to be added to this
        lock (OnlineUsers)
        {
            if (OnlineUsers.ContainsKey(username))
            {
                OnlineUsers[username].Add(connectionId);
            }
            //if the user isn't already logged in.
            else
            {
                OnlineUsers.Add(username, new List<string>{connectionId});
            }
        }

        return Task.CompletedTask;
    }

    public Task UserDisconnected(string username, string connectionId)
    {
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(username)) return Task.CompletedTask;

            OnlineUsers[username].Remove(connectionId);

            if (OnlineUsers[username].Count == 0)
            {
                OnlineUsers.Remove(username);
            }
        }

        return Task.CompletedTask;
    }
    
    //method to return who is online
    public Task<string[]> GetOnlineUsers()
    {
        string[] onlineUsers;
        lock (OnlineUsers)
        {
            onlineUsers = OnlineUsers.OrderBy(k => k.Key)
                //only interested in the key because that's the username
                .Select(k => k.Key).ToArray();
        }

        return Task.FromResult(onlineUsers);
    }
}