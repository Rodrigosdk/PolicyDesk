using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using PolicyDesk.Infrastructure.DataAccess;

namespace PolicyDesk.Api.IntegrationTests;

public abstract class BaseIntegrationTest : IClassFixture<PolicyDeskApplicationFactory>, IDisposable
{
    internal readonly PolicyDeskDbContext DbContext;

    protected PolicyDeskApplicationFactory Factory { get; }

    private readonly HttpClient _httpClient;
    private readonly HttpClient _anonymousClient;
    private readonly IServiceScope _scope;

    protected BaseIntegrationTest(PolicyDeskApplicationFactory factory)
    {
        Factory = factory;
        _httpClient = factory.CreateClient();
        _anonymousClient = factory.CreateAnonymousClient();
        _scope = factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<PolicyDeskDbContext>();
    }

    protected async Task<HttpResponseMessage> Post(string requestUri, object request) =>
        await _httpClient.PostAsJsonAsync(requestUri, request);

    protected async Task<HttpResponseMessage> Put(string requestUri, object request) =>
        await _httpClient.PutAsJsonAsync(requestUri, request);

    protected async Task<HttpResponseMessage> Get(string requestUri) =>
        await _httpClient.GetAsync(requestUri);

    protected async Task<HttpResponseMessage> Delete(string requestUri) =>
        await _httpClient.DeleteAsync(requestUri);

    protected void RenewAccessToken() => Factory.Authorize(_httpClient);

    protected async Task<HttpResponseMessage> PostWithoutToken(string requestUri, object request) =>
        await _anonymousClient.PostAsJsonAsync(requestUri, request);

    protected async Task<HttpResponseMessage> GetWithoutToken(string requestUri) =>
        await _anonymousClient.GetAsync(requestUri);

    public void Dispose()
    {
        _httpClient.Dispose();
        _anonymousClient.Dispose();
        _scope.Dispose();
        DbContext.Dispose();
        GC.SuppressFinalize(this);
    }
}
