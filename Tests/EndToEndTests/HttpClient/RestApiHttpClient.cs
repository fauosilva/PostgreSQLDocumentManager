using ApplicationCore.Dtos.Requests;
using ApplicationCore.Dtos.Responses;
using EndToEndTests.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;

namespace EndToEndTests
{
    public class RestApiHttpClient
    {
        private readonly HttpClient httpClient;
        private readonly TestSettings testSettings;
        private string? AdminJwtToken;

        public RestApiHttpClient(HttpClient httpClient, TestSettings testSettings)
        {
            this.httpClient = httpClient;
            this.testSettings = testSettings;
        }

        public async Task<LoginResponse> GenerateJwtAsync(LoginRequest loginRequest)
        {
            return await SendJsonPostAsync<LoginResponse>("/api/v1/authentication/login", loginRequest);
        }

        public async Task<CreateUserResponse> CreateUserAsync(CreateUserRequest createUserRequest)
        {
            return await SendJsonPostAsync<CreateUserResponse>("/api/v1/users", createUserRequest, await GetAdminJwtToken());
        }

        public async Task<CreateGroupResponse> CreateGroupAsync(CreateGroupRequest createGroupRequest)
        {
            return await SendJsonPostAsync<CreateGroupResponse>("/api/v1/groups", createGroupRequest, await GetAdminJwtToken());
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            return await SendDeleteAsync("/api/v1/users", id, await GetAdminJwtToken());
        }

        public async Task<bool> DeleteDocumentAsync(int id)
        {
            return await SendDeleteAsync("/api/v1/documents", id, await GetAdminJwtToken());
        }

        public async Task<bool> DeleteGroupAsync(int id)
        {
            return await SendDeleteAsync("/api/v1/groups", id, await GetAdminJwtToken());
        }

        public async Task<CreateDocumentResponse> FileUploadAsync(MultipartFormDataContent fileUploadRequest)
        {
            return await SendPostAsync<CreateDocumentResponse>("/api/v1/documents", fileUploadRequest, await GetAdminJwtToken());
        }

        public async Task<GroupResponse> AddUserToGroupAsync(int groupId, int userId)
        {
            var request = new UserGroupRequest() { UserId = userId };
            return await SendJsonPostAsync<GroupResponse>($"/api/v1/groups/{groupId}/users", request, await GetAdminJwtToken());
        }

        public async Task<CreateDocumentResponse> AddUserPermissionAsync(int documentId, int userId)
        {
            var request = new PermissionRequest() { UserId = userId };
            return await SendJsonPostAsync<CreateDocumentResponse>($"/api/v1/documents/{documentId}/permissions", request, await GetAdminJwtToken());
        }

        public async Task<CreateDocumentResponse> AddGroupPermissionAsync(int documentId, int groupId)
        {
            var request = new PermissionRequest() { GroupId = groupId };
            return await SendJsonPostAsync<CreateDocumentResponse>($"/api/v1/documents/{documentId}/permissions", request, await GetAdminJwtToken());
        }

        public async Task<Stream> DownloadDocumentAsync(int documentId, string jwtToken)
        {            
            return await GetFileAsync($"/api/v1/documents/{documentId}", jwtToken);
        }

        private async Task<string> GetJwtTokenAsync(LoginRequest loginRequest)
        {
            return (await GenerateJwtAsync(loginRequest)).JwtToken;
        }

        private async Task<string> GetAdminJwtToken()
        {
            if (AdminJwtToken == null)
            {
                var request = new LoginRequest { Username = testSettings.Api.AdminUser, Password = testSettings.Api.AdminPassword };
                AdminJwtToken = await GetJwtTokenAsync(request);
            }
            return AdminJwtToken;
        }

        private async Task<Stream> GetFileAsync(string url, string? jwtToken = null)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Get, url);
            if (jwtToken != null)
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            var httpResult = await httpClient.SendAsync(requestMessage);
            httpResult.EnsureSuccessStatusCode();
            return await httpResult.Content.ReadAsStreamAsync();
        }

        private async Task<T> SendJsonPostAsync<T>(string url, object? jsonObject, string? jwtToken = null)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            if (jwtToken != null)
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            var jsonContent = JsonConvert.SerializeObject(jsonObject);
            var contentString = new StringContent(jsonContent, Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            requestMessage.Content = contentString;

            var httpResult = await httpClient.SendAsync(requestMessage);
            httpResult.EnsureSuccessStatusCode();
            var createUserResponse = await httpResult.Content.ReadAsStringAsync();
            var jsonReturn = JsonConvert.DeserializeObject<T>(createUserResponse);
            return jsonReturn;
        }

        private async Task<T> SendPostAsync<T>(string url, MultipartFormDataContent content, string? jwtToken = null)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Post, url);
            if (jwtToken != null)
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            requestMessage.Content = content;

            var httpResult = await httpClient.SendAsync(requestMessage);
            httpResult.EnsureSuccessStatusCode();
            var createUserResponse = await httpResult.Content.ReadAsStringAsync();
            var jsonReturn = JsonConvert.DeserializeObject<T>(createUserResponse);
            return jsonReturn;
        }

        private async Task<bool> SendDeleteAsync(string url, int entityId, string? jwtToken = null)
        {
            using var requestMessage = new HttpRequestMessage(HttpMethod.Delete, $"{url}/{entityId}");
            if (jwtToken != null)
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            var httpResult = await httpClient.SendAsync(requestMessage);
            httpResult.EnsureSuccessStatusCode();
            return true;
        }
    }
}
