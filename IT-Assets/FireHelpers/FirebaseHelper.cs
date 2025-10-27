using IT_Assets.Global;
using IT_Assets.Models;
using Microsoft.Maui.ApplicationModel.Communication;
using Microsoft.Maui.Storage;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace IT_Assets.FireHelpers
{
    public class FirebaseHelper
    {
        private readonly HttpClient _httpClient = new HttpClient();

        private readonly string _apiKey = "AIzaSyDa9OICrMbvvv58QkN4ilT38zCzyq66T3g";
        private readonly string _databaseUrl = "https://assetsmanagement-b8a6f-default-rtdb.firebaseio.com/";

        private string _idToken; // used for authenticated requests
        private string _email;
        private string _password;

        // ------------------------------
        // 🔐 AUTHENTICATION
        // ------------------------------
        public async Task<bool> RegisterUserAsync(string email, string password)
        {
            try
            {
                var signupUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={_apiKey}";
                var payload = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                var response = await _httpClient.PostAsync(signupUrl,
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    _idToken = result.GetProperty("idToken").GetString();
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[RegisterUser Error] {ex.Message}");
                return false;
            }
        }

        public async Task<bool> LoginUserAsync(string email, string password)
        {
            _email = email;
            _password = password;
            try
            {
                var loginUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={_apiKey}";
                var payload = new
                {
                    email = email,
                    password = password,
                    returnSecureToken = true
                };

                var response = await _httpClient.PostAsync(loginUrl,
                    new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json"));

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
                    _idToken = result.GetProperty("idToken").GetString();
                    GlobalVar.IdToken = _idToken;
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[LoginUser Error] {ex.Message}");
                return false;
            }
        }

        // ------------------------------
        // 🔧 CRUD OPERATIONS
        // ------------------------------
        public async Task<bool> AddAssetAsync(AssetModel asset)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(asset.Code))
                {
                    Console.WriteLine("[AddAssetAsync Error] Asset Code cannot be null or empty.");
                    return false;
                }

                // Use the Code as the key in the database
                var url = $"{_databaseUrl}assets/{asset.Code}.json?auth={GlobalVar.IdToken}";
                var response = await _httpClient.PutAsync(url,
                    new StringContent(JsonSerializer.Serialize(asset), Encoding.UTF8, "application/json"));

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AddAssetAsync Error] {ex.Message}");
                return false;
            }
        }

        public async Task<List<AssetModel>> GetAllAssetsAsync()
        {
            try
            {
                var url = $"{_databaseUrl}assets.json?auth={GlobalVar.IdToken}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"[GetAllAssetsAsync Error] HTTP {response.StatusCode}");
                    return new List<AssetModel>();
                }

                var json = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"[GetAllAssetsAsync Response] {json}");

                var dict = JsonSerializer.Deserialize<Dictionary<string, AssetModel>>(json);

                var assets = new List<AssetModel>();
                if (dict != null)
                {
                    foreach (var kvp in dict)
                    {
                        try
                        {
                            var asset = kvp.Value;
                            asset.Id = kvp.Key; // Assign the Firebase key as the asset ID
                            assets.Add(asset);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[GetAllAssetsAsync Error] Failed to process asset {kvp.Key}: {ex.Message}");
                        }
                    }
                }

                return assets;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAllAssetsAsync Error] {ex.Message}");
                return new List<AssetModel>();
            }
        }

        // Async method returning Task<AssetModel>
        public async Task<AssetModel> GetAssetById(string id)
        {
            try
            {
                var url = $"{_databaseUrl}assets/{id}.json?auth={GlobalVar.IdToken}";
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync();
                var asset = JsonSerializer.Deserialize<AssetModel>(json);
                asset.Id = id;
                return asset;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetAssetById Error] {ex.Message}");
                return null;
            }
        }

        public async Task<bool> UpdateAssetAsync(string id, AssetModel asset)
        {
            try
            {
                var url = $"{_databaseUrl}assets/{id}.json?auth={GlobalVar.IdToken}";
                var response = await _httpClient.PatchAsync(url,
                    new StringContent(JsonSerializer.Serialize(asset), Encoding.UTF8, "application/json"));

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[UpdateAsset Error] {ex.Message}");
                return false;
            }
        }

        public async Task<bool> DeleteAssetAsync(string id)
        {
            try
            {
                var url = $"{_databaseUrl}assets/{id}.json?auth={GlobalVar.IdToken}";
                var response = await _httpClient.DeleteAsync(url);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[DeleteAsset Error] {ex.Message}");
                return false;
            }
        }

        internal async Task LogoutUser()
        {
            // Clear the saved token
            _idToken = null;

            // Optionally clear any cached user info
            await Task.CompletedTask;
        }
    }

    // Small helper extension to support PATCH with HttpClient
    public static class HttpClientExtensions
    {
        public static async Task<HttpResponseMessage> PatchAsync(this HttpClient client, string requestUri, HttpContent content)
        {
            var request = new HttpRequestMessage(new HttpMethod("PATCH"), requestUri) { Content = content };
            return await client.SendAsync(request);
        }
    }
}
