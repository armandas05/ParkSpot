using System.Collections.Generic;

namespace ParkSpot.Web.Database;

public class RawSqlCommand
{
    private readonly Dictionary<string, object?> _parameters = new();
    public string CommandText { get; }
    public IReadOnlyDictionary<string, object?> Parameters { get { return _parameters; } }
    public RawSqlCommand(string commandText)
    {
        CommandText = commandText;
    }

    public void AddParameter(string name, object? value)
    {
        _parameters.Add(name.TrimStart('@', '?'), value);
    }
}
