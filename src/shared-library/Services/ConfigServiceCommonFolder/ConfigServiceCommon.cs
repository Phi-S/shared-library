﻿using System.Runtime.CompilerServices;

namespace shared_library.Services.ConfigServiceCommonFolder;

public class ConfigServiceCommon : IConfigServiceCommon
{
    public static string GetEnvironmentVariableOrThrow([CallerMemberName] string callerName = "")
    {
        var valueFromEnvironmentVariable = GetEnvironmentVariableOrNull(callerName);
        if (valueFromEnvironmentVariable is null || string.IsNullOrWhiteSpace(valueFromEnvironmentVariable))
        {
            throw new Exception($"Failed to get environment variable \"{callerName}\"");
        }

        return valueFromEnvironmentVariable;
    }

    public static string? GetEnvironmentVariableOrNull([CallerMemberName] string callerName = "")
    {
        var environmentVariable = Environment.GetEnvironmentVariable(callerName);
        return string.IsNullOrWhiteSpace(environmentVariable) ? null : environmentVariable;
    }

    public static string GetEnvironmentVariableOrDefaultValue(string defaultValue,
        [CallerMemberName] string callerName = "")
    {
        return GetEnvironmentVariableOrNull(callerName) ?? defaultValue;
    }

    public string APPLICATION_NAME
    {
        get
        {
            var assembly = System.Reflection.Assembly.GetEntryAssembly();
            assembly ??= System.Reflection.Assembly.GetCallingAssembly();
            return GetEnvironmentVariableOrDefaultValue($"{assembly.GetName().Name}");
        }
    }

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