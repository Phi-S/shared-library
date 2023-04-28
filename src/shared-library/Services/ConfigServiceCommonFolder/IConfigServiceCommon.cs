// ReSharper disable InconsistentNaming

using System.Runtime.CompilerServices;

namespace shared_library.Services.ConfigServiceCommonFolder;

public interface IConfigServiceCommon
{
    protected static string? GetEnvironmentVariableOrNull([CallerMemberName] string callerName = "")
    {
        var environmentVariable = Environment.GetEnvironmentVariable(callerName);
        return string.IsNullOrWhiteSpace(environmentVariable) ? null : environmentVariable;
    }

    protected static string GetEnvironmentVariableOrDefaultValue(string defaultValue,
        [CallerMemberName] string callerName = "")
    {
        return GetEnvironmentVariableOrNull(callerName) ?? defaultValue;
    }

    public string APPLICATION_NAME =>
        GetEnvironmentVariableOrDefaultValue(
            $"{System.Reflection.Assembly.GetCallingAssembly().GetName().Name}_{Environment.MachineName}");

    public string MINIMUM_LOG_LEVEL => GetEnvironmentVariableOrDefaultValue("Information");

    public string? SEQ_URL => GetEnvironmentVariableOrNull();

    public string? SEQ_API_KEY => GetEnvironmentVariableOrNull();

    public string? GRAYLOG_URL => GetEnvironmentVariableOrNull();

    public int? GRAYLOG_PORT
    {
        get
        {
            var environmentVariable = GetEnvironmentVariableOrNull();
            return !string.IsNullOrWhiteSpace(environmentVariable) &&
                   int.TryParse(environmentVariable, out var port)
                ? port
                : null;
        }
    }

    public string? GRAYLOG_PROTOCOL => GetEnvironmentVariableOrNull();
}