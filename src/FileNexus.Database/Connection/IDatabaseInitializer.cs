using System.Threading.Tasks;
using Microsoft.Data.Sqlite;

namespace FileNexus.Database.Connection;

public interface IDatabaseInitializer
{
    string DbPath { get; }
    Task InitializeAsync();
    SqliteConnection CreateConnection();
}
