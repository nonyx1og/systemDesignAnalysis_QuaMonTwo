using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Friendship.Infrastructure;

public class FriendshipDbFactory : IDesignTimeDbContextFactory<FriendshipDb>
{
    public FriendshipDb CreateDbContext(string[] args) => new(new DbContextOptionsBuilder<FriendshipDb>()
        .UseSqlServer(@"Server=(localdb)\MSSQLLocalDB;Database=Friendship;Trusted_Connection=True;TrustServerCertificate=True")
        .Options);
}
