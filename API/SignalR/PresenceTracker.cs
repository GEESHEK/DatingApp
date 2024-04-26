namespace API.SignalR;

public class PresenceTracker
{
    //username, connectionIDs
    private static readonly Dictionary<string, List<String>> OnlineUsers = 
        new Dictionary<string, List<string>>();
    
    //dictionaries aren't thread safe, use lock
    public Task<bool> UserConnected(string username, string connectionId)
    {
        bool isOnline = false;
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
                isOnline = true;
            }
        }

        return Task.FromResult(isOnline);
    }

    public Task<bool> UserDisconnected(string username, string connectionId)
    {
        bool isOffline = false;
        
        lock (OnlineUsers)
        {
            if (!OnlineUsers.ContainsKey(username)) return Task.FromResult(isOffline);

            OnlineUsers[username].Remove(connectionId);

            if (OnlineUsers[username].Count == 0)
            {
                OnlineUsers.Remove(username);
                isOffline = true;
            }
        }

        return Task.FromResult(isOffline);
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

    public static Task<List<string>> GetConnectionsForUser(string username)
    {
        List<string> connectionIds;
        //scale problem > use db if you don't have Redis
        lock (OnlineUsers)
        {
            connectionIds = OnlineUsers.GetValueOrDefault(username);
        }

        return Task.FromResult(connectionIds);
    }
}