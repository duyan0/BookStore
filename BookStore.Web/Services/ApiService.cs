using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Net;

namespace BookStore.Web.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<ApiService> _logger;

        public ApiService(HttpClient httpClient, IConfiguration configuration, IHttpContextAccessor httpContextAccessor, ILogger<ApiService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _httpContextAccessor = httpContextAccessor;
            _logger = logger;

            // Đảm bảo base URL được set đúng với trailing slash
            var baseUrl = _configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5274/api";

            // Ensure base URL ends with trailing slash for proper concatenation
            if (!baseUrl.EndsWith("/"))
            {
                baseUrl += "/";
            }

            _logger.LogInformation("Setting API BaseAddress to: {BaseUrl}", baseUrl);

            if (_httpClient.BaseAddress == null || _httpClient.BaseAddress.ToString() != baseUrl)
            {
                _httpClient.BaseAddress = new Uri(baseUrl);
                _logger.LogInformation("BaseAddress set to: {Address}", _httpClient.BaseAddress);
            }
            else
            {
                _logger.LogInformation("BaseAddress already set to: {Address}", _httpClient.BaseAddress);
            }
        }

        private void SetAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString("Token");
            if (!string.IsNullOrEmpty(token) && !IsTokenExpired(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
                if (!string.IsNullOrEmpty(token) && IsTokenExpired(token))
                {
                    HandleUnauthorized();
                }
            }
        }

        private bool IsTokenExpired(string token)
        {
            try
            {
                // Simple check for JWT token format and expiration
                var parts = token.Split('.');
                if (parts.Length != 3) return true;

                var payload = parts[1];
                // Add padding if needed
                switch (payload.Length % 4)
                {
                    case 2: payload += "=="; break;
                    case 3: payload += "="; break;
                }

                var jsonBytes = Convert.FromBase64String(payload);
                var jsonString = System.Text.Encoding.UTF8.GetString(jsonBytes);
                var json = JsonConvert.DeserializeObject<Dictionary<string, object>>(jsonString);

                if (json != null && json.ContainsKey("exp"))
                {
                    var exp = Convert.ToInt64(json["exp"]);
                    var expDateTime = DateTimeOffset.FromUnixTimeSeconds(exp).DateTime;
                    return expDateTime <= DateTime.UtcNow;
                }
                return true;
            }
            catch
            {
                return true;
            }
        }

        private void HandleUnauthorized()
        {
            // Clear session when token is invalid
            _httpContextAccessor.HttpContext?.Session.Clear();
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.GetAsync(endpoint);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(content);
                }

                // Handle 401 Unauthorized
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HandleUnauthorized();
                    throw new UnauthorizedAccessException("Token đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.");
                }

                // Log error response
                var errorContent = await response.Content.ReadAsStringAsync();
                throw new HttpRequestException($"API call failed with status {response.StatusCode}: {errorContent}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error calling API endpoint {endpoint}: {ex.Message}", ex);
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                SetAuthorizationHeader();
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var fullUrl = $"{_httpClient.BaseAddress}{endpoint}";
                _logger.LogInformation("Making POST request to: {Url}", fullUrl);
                _logger.LogInformation("Request body: {Body}", json);

                var response = await _httpClient.PostAsync(endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                _logger.LogInformation("Response status: {StatusCode}", response.StatusCode);
                _logger.LogInformation("Response content: {Content}", responseContent);

                if (response.IsSuccessStatusCode)
                {
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }

                // Handle specific error cases
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HandleUnauthorized();
                    throw new UnauthorizedAccessException("Token đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.");
                }

                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    // Try to parse error message from response
                    try
                    {
                        var errorResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
                        if (errorResponse?.ContainsKey("message") == true)
                        {
                            throw new HttpRequestException($"400: {errorResponse["message"]}");
                        }
                    }
                    catch (JsonException)
                    {
                        // If can't parse JSON, use raw content
                    }

                    throw new HttpRequestException($"400: {responseContent}");
                }

                // Log error response
                throw new HttpRequestException($"API POST call failed with status {response.StatusCode}: {responseContent}");
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (HttpRequestException)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calling API endpoint {Endpoint}", endpoint);
                throw new HttpRequestException($"Error calling API endpoint {endpoint}: {ex.Message}", ex);
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                SetAuthorizationHeader();
                var json = JsonConvert.SerializeObject(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseContent = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<T>(responseContent);
                }

                // Handle 401 Unauthorized
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HandleUnauthorized();
                    throw new UnauthorizedAccessException("Token đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.");
                }

                return default;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error calling API endpoint {endpoint}: {ex.Message}", ex);
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                SetAuthorizationHeader();
                var response = await _httpClient.DeleteAsync(endpoint);

                // Handle 401 Unauthorized
                if (response.StatusCode == HttpStatusCode.Unauthorized)
                {
                    HandleUnauthorized();
                    throw new UnauthorizedAccessException("Token đã hết hạn hoặc không hợp lệ. Vui lòng đăng nhập lại.");
                }

                return response.IsSuccessStatusCode;
            }
            catch (UnauthorizedAccessException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new HttpRequestException($"Error calling API endpoint {endpoint}: {ex.Message}", ex);
            }
        }

        public async Task<T?> PostFormAsync<T>(string endpoint, MultipartFormDataContent formData)
        {
            SetAuthorizationHeader();
            var response = await _httpClient.PostAsync(endpoint, formData);
            
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<T>(responseContent);
            }
            
            return default;
        }
    }
} 