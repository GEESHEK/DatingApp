using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class Group
{
    public Group()
    {
        
    }
    
    public Group(string name)
    {
        Name = name;
    }

    //Key: makes the group name unique inside all db, can't add the same group twice
    [Key]
    public string Name { get; set; }

    public ICollection<Connection> Connections { get; set; } = new List<Connection>();
}