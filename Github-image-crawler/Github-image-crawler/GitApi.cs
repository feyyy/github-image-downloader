using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Mobge.GitFileCrawler {
    public class GitApi {
        private HttpClient _client;
        private string token;
        public GitApi(string token) {
            _client = new HttpClient();
            this.token = token;
        }
        public void Dispose() {
            if(_client != null) {
                _client.Dispose();
                _client = null;
            }
        }
        private void AddCommonHeaders(HttpRequestMessage request) {
            
            request.Headers.TryAddWithoutValidation("Authorization", $"Bearer {token}");
            request.Headers.TryAddWithoutValidation("X-GitHub-Api-Version", "2022-11-28"); 
            request.Headers.TryAddWithoutValidation("User-Agent", "FileCrawler"); 
        }
        
        public async Task<string> ReadLastCommitSHA(string repoOwner, string repoName, string branch) {
            try {
                var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.github.com/repos/{repoOwner}/{repoName}/commits/{branch}");
                request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github.sha");
                AddCommonHeaders(request);
                var response = await _client.SendAsync(request);
                return await response.Content.ReadAsStringAsync();
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }
        public async Task<HttpContent> ReadTree(string repoOwner, string repoName, string sha, bool recursive) {
            try {
                string uri = $"https://api.github.com/repos/{repoOwner}/{repoName}/git/trees/{sha}";
                if(recursive) {
                    uri += "?recursive=true";
                }
                var request = new HttpRequestMessage(new HttpMethod("GET"), uri);
                request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github+json");
                AddCommonHeaders(request);
                
                var response = await _client.SendAsync(request);
                return response.Content;
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }

        public async Task<HttpContent> ReadFile(string repoOwner, string repoName, string path) {
            try {
                var request = new HttpRequestMessage(new HttpMethod("GET"), $"https://api.github.com/repos/{repoOwner}/{repoName}/contents/{path}");
                request.Headers.TryAddWithoutValidation("Accept", "application/vnd.github.raw+json");
                AddCommonHeaders(request);
                var response = await _client.SendAsync(request);
                return response.Content;
            }
            catch (Exception e) {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}