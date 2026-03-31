using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace KeycloakAuthLabApp.Services;

public class KeycloakAdminService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KeycloakAdminService> _logger;

    public KeycloakAdminService(HttpClient httpClient, IConfiguration configuration, ILogger<KeycloakAdminService> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _logger = logger;
    }

    private async Task<string> GetAdminTokenAsync()
    {
        var authority = _configuration["Keycloak:Authority"];
        var baseUrl = authority?.Replace("/realms/securite-lab", "") ?? "http://localhost:8080";

        var tokenUrl = $"{baseUrl}/realms/master/protocol/openid-connect/token";
        var content = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["grant_type"] = "client_credentials",
            ["client_id"] = _configuration["Keycloak:AdminClientId"] ?? "admin-cli",
            ["client_secret"] = _configuration["Keycloak:AdminClientSecret"] ?? "",
            ["username"] = _configuration["Keycloak:AdminUsername"] ?? "admin",
            ["password"] = _configuration["Keycloak:AdminPassword"] ?? "admin"
        });

        // Fallback to password grant if client_secret is empty
        if (string.IsNullOrEmpty(_configuration["Keycloak:AdminClientSecret"]))
        {
            content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "password",
                ["client_id"] = "admin-cli",
                ["username"] = _configuration["Keycloak:AdminUsername"] ?? "admin",
                ["password"] = _configuration["Keycloak:AdminPassword"] ?? "admin"
            });
        }

        var response = await _httpClient.PostAsync(tokenUrl, content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("access_token").GetString() ?? throw new InvalidOperationException("No access token returned");
    }

    public async Task<List<JsonElement>> GetUsersAsync()
    {
        try
        {
            var token = await GetAdminTokenAsync();
            var baseUrl = _configuration["Keycloak:Authority"]?.Replace("/realms/securite-lab", "") ?? "http://localhost:8080";
            var realm = _configuration["Keycloak:Realm"] ?? "securite-lab";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{baseUrl}/admin/realms/{realm}/users");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<JsonElement>>() ?? new List<JsonElement>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get users from Keycloak");
            return new List<JsonElement>();
        }
    }

    public async Task<List<JsonElement>> GetRealmRolesAsync()
    {
        try
        {
            var token = await GetAdminTokenAsync();
            var baseUrl = _configuration["Keycloak:Authority"]?.Replace("/realms/securite-lab", "") ?? "http://localhost:8080";
            var realm = _configuration["Keycloak:Realm"] ?? "securite-lab";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            var response = await _httpClient.GetAsync($"{baseUrl}/admin/realms/{realm}/roles");
            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<List<JsonElement>>() ?? new List<JsonElement>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get roles from Keycloak");
            return new List<JsonElement>();
        }
    }

    public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
    {
        try
        {
            var token = await GetAdminTokenAsync();
            var baseUrl = _configuration["Keycloak:Authority"]?.Replace("/realms/securite-lab", "") ?? "http://localhost:8080";
            var realm = _configuration["Keycloak:Realm"] ?? "securite-lab";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            // Get role details
            var roleResponse = await _httpClient.GetAsync($"{baseUrl}/admin/realms/{realm}/roles/{roleName}");
            roleResponse.EnsureSuccessStatusCode();
            var role = await roleResponse.Content.ReadFromJsonAsync<JsonElement>();

            // Assign role to user
            var roles = new[] { role };
            var content = new StringContent(JsonSerializer.Serialize(roles), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(
                $"{baseUrl}/admin/realms/{realm}/users/{userId}/role-mappings/realm",
                content);

            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to assign role {Role} to user {UserId}", roleName, userId);
            return false;
        }
    }

    public async Task<bool> CreateUserAsync(string username, string email, string password, string[] roles)
    {
        try
        {
            var token = await GetAdminTokenAsync();
            var baseUrl = _configuration["Keycloak:Authority"]?.Replace("/realms/securite-lab", "") ?? "http://localhost:8080";
            var realm = _configuration["Keycloak:Realm"] ?? "securite-lab";

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var userPayload = new
            {
                username,
                email,
                enabled = true,
                emailVerified = true,
                credentials = new[]
                {
                    new { type = "password", value = password, temporary = false }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(userPayload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{baseUrl}/admin/realms/{realm}/users", content);

            if (!response.IsSuccessStatusCode) return false;

            // Get the created user ID from Location header
            var location = response.Headers.Location?.ToString();
            var createdUserId = location?.Split('/').Last();

            if (createdUserId != null)
            {
                foreach (var role in roles)
                {
                    await AssignRoleToUserAsync(createdUserId, role);
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user {Username}", username);
            return false;
        }
    }
}
