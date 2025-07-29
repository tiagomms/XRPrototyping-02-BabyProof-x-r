using System.Collections.Generic;
using System.Net.Http;

/// <summary>
/// Interface for API clients providing access to HttpClient, BaseUrl, and custom headers.
/// </summary>
public interface IApiClient
{
    HttpClient httpClient { get; }
    string BaseUrl { get; }
    Dictionary<string, string> ToCustomHeader();
}
