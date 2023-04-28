// ReSharper disable InconsistentNaming

namespace shared_library.Services.ConfigServiceCommonFolder;

public interface IConfigServiceCommon
{
    public string APPLICATION_NAME { get; }

    public string MINIMUM_LOG_LEVEL { get; }

    public string? SEQ_URL { get; }

    public string? SEQ_API_KEY { get; }

    public string? GRAYLOG_URL { get; }

    public int? GRAYLOG_PORT { get; }

    public string? GRAYLOG_PROTOCOL { get; }
}