using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace MajorInteractiveBot.Data
{
    public class MajorContextDesignFactory : IDesignTimeDbContextFactory<MajorContext>
    {
        public MajorContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<MajorContext>();
            optionsBuilder.UseSqlite("Data Source=Major.sqlite");
            return new MajorContext(optionsBuilder.Options);
        }
    }
}
