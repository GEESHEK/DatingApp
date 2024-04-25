namespace API.Entities;

public class Connection
{
    //To satisfy EF when it creates the schema for our db, we need empty Constructor
    public Connection()
    {
        
    }
    
    public Connection(string connectionId, string username)
    {
        this.ConnectionId = connectionId;
        Username = username;
    }

    public string ConnectionId { get; set; }
    public string Username { get; set; }
}