using Microsoft.Data.Sqlite;
using System.Text.Json;
using Webefinity.Content.FileIndex.Models;
using Webefinity.ContentRoot.Abstractions;

namespace Webefinity.Content.FileIndex.Services;

public class FileIndexDatabaseService
{
    private static readonly string connectionString = "Data Source=fileindex.db";
    private static readonly HashSet<string> tablesExist = new();

    public FileIndexDatabaseService()
    {

    }

    public void WriteMeta<TMeta>(CollectionDef collectionDef, string file, string key, TMeta meta)
    {
        EnsureCreated(collectionDef);
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"INSERT OR REPLACE INTO [{collectionDef.Collection}] (File, Key, Meta) VALUES (@File, @Key, @Meta);";
            command.Parameters.AddWithValue("@File", file);
            command.Parameters.AddWithValue("@Key", key);
            var json = JsonSerializer.Serialize(meta);
            command.Parameters.AddWithValue("@Meta", json);
            var result = command.ExecuteNonQuery();
            if (result != 1)
            {
                throw new Exception($"Failed to insert meta data for {file} in {collectionDef.Collection}");
            }
        }
    }
    
    public void WriteMetaString(CollectionDef collectionDef, string file, string key, string meta)
    {
        EnsureCreated(collectionDef);
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"INSERT OR REPLACE INTO [{collectionDef.Collection}] (File, Key, Meta) VALUES (@File, @Key, @Meta);";
            command.Parameters.AddWithValue("@File", file);
            command.Parameters.AddWithValue("@Key", key);
            command.Parameters.AddWithValue("@Meta", meta);
            var result = command.ExecuteNonQuery();
            if (result != 1)
            {
                throw new Exception($"Failed to insert meta data for {file} in {collectionDef.Collection}");
            }
        }
    }

    public TMeta? ReadMeta<TMeta>(CollectionDef collectionDef, string file, string key)
    {
        EnsureCreated(collectionDef);
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT Meta FROM [{collectionDef.Collection}] WHERE File=@File AND Key=@Key;";
            command.Parameters.AddWithValue("@File", file);
            command.Parameters.AddWithValue("@Key", key);
            var result = command.ExecuteScalar();
            if (result == null)
            {
                return default(TMeta);
            }

            return JsonSerializer.Deserialize<TMeta>((string)result);
        }
    }

    public void DeleteMeta(CollectionDef collectionDef, string file)
    {
        EnsureCreated(collectionDef);
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM [{collectionDef.Collection}] WHERE File=@File;";
            command.Parameters.AddWithValue("@File", file);
            var result = command.ExecuteNonQuery();
            if (result <= 0)
            {
                throw new Exception($"Failed to delete meta data for {file} in {collectionDef.Collection}");
            }
        }

    }
    
    public void ClearMeta(CollectionDef collectionDef)
    {
        EnsureCreated(collectionDef);
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"DELETE FROM [{collectionDef.Collection}];";
            var result = command.ExecuteNonQuery();
            if (result < 0)
            {
                throw new Exception($"Failed to delete meta data {collectionDef.Collection}");
            }
        }

    }


    private bool TableExists(string tableName)
    {
        if (tablesExist.Contains(tableName))
        {
            return true;
        }

        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}';";
            var result = command.ExecuteScalar();
            var exists = result != null;
            if (exists)
            {
                tablesExist.Add(tableName);
            }

            return exists;
        }
    }

    private void EnsureCreated(CollectionDef collectionDef)
    {
        if (!TableExists(collectionDef.Collection))
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"CREATE TABLE [{collectionDef.Collection}] (File TEXT, Key TEXT, Meta TEXT, PRIMARY KEY (File, Key));";
                command.ExecuteNonQuery();
            }
        }
    }


    public string SerializeMeta(CollectionDef collectionDef)
    {
        var collectionMeta = new CollectionMeta
        {
            Collection = collectionDef.Collection
        };
        IDictionary<string, FileMeta> files = new Dictionary<string, FileMeta>();
        EnsureCreated(collectionDef);
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT File, Key, Meta FROM [{collectionDef.Collection}];";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var file = reader.GetString(0);
                var key = reader.GetString(1);
                var meta = reader.GetString(2);

                if (!files.ContainsKey(file))
                {
                    files[file] = new FileMeta
                    {
                        FileName = file,
                        Meta = new Dictionary<string, JsonDocument>()
                    };
                }
                files[file].Meta[key] = JsonDocument.Parse(meta);
            }

            collectionMeta.Files.AddRange(files.Values);
            return JsonSerializer.Serialize(collectionMeta);
        }
    }

    public void DeserializeMeta(string json)
    {
        var writtenFiles = new HashSet<string>();
        var presentFiles = new HashSet<string>();
        var collectionMeta = JsonSerializer.Deserialize<CollectionMeta>(json);
        if (collectionMeta == null)
        {
            throw new Exception("Failed to deserialize meta data");
        }

        var collectionDef = new CollectionDef(collectionMeta.Collection);
        EnsureCreated(new CollectionDef(collectionMeta.Collection));

        // list files in the database
        using (var connection = new SqliteConnection(connectionString))
        {
            connection.Open();
            var command = connection.CreateCommand();
            command.CommandText = $"SELECT File FROM [{collectionDef.Collection}];";
            var reader = command.ExecuteReader();
            while (reader.Read())
            {
                presentFiles.Add(reader.GetString(0));
            }
        }

        foreach (var fileMeta in collectionMeta.Files)
        {
            foreach (var meta in fileMeta.Meta)
            {
                WriteMetaString(collectionDef, fileMeta.FileName, meta.Key, JsonSerializer.Serialize(meta.Value));
                writtenFiles.Add(fileMeta.FileName);
            }
        }

        foreach (var file in presentFiles.Except(writtenFiles))
        {
            DeleteMeta(collectionDef, file);
        }
    }
}
