using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Mobge.GitFileCrawler {
    public class FileCrawler {
        private GitApi _api;
        private byte[] _tempBytes = new byte[4096];

        public FileCrawler(string token) {
            _api = new GitApi(token);
            
        }
        public async Task DownloadAllPng(string owner, string repo, string bracnh, string targetPath, string prefix = null) {
            var en = GetPaths(owner, repo, bracnh, true).GetAsyncEnumerator();
            int count = 0;
            while(await en.MoveNextAsync()) {
                string path = en.Current;
                string savePath = Path.Combine(targetPath, path);
                if(!string.IsNullOrEmpty(prefix)) {
                    if(!path.StartsWith(prefix, StringComparison.CurrentCultureIgnoreCase)) {
                        continue;
                    }
                    savePath = Path.Combine(targetPath, path.Substring(prefix.Length));
                }
                if(path.EndsWith(".png")) {
                    count++;
                    Console.WriteLine("Saving: " + path + " -> " + savePath);
                    await SaveFile(owner, repo, path, savePath);
                }
            }
            await en.DisposeAsync();
            Console.WriteLine("Number of file: " + count);
        }

        private async Task SaveFile(string owner, string repo, string path, string savePath) {
            //var name = Path.GetFileName(path);
            var content = await _api.ReadFile(owner, repo, path);
            var directory = Path.GetDirectoryName(savePath);
            Directory.CreateDirectory(directory);
            using(var fs = new FileStream(savePath, FileMode.OpenOrCreate)) {
                using(var stream = await content.ReadAsStreamAsync()) {
                    int eCount;
                    while((eCount = stream.Read(_tempBytes, 0, _tempBytes.Length)) > 0) {
                        await fs.WriteAsync(_tempBytes, 0, eCount);
                    }
                    // Console.WriteLine(reader.ReadLine());
                    // var json = new JsonTextReader(reader);
                    // while(json.Read()) {
                    //     if(json.Path == "content" && json.TokenType == JsonToken.PropertyName) {
                    //         var bytes = await json.ReadAsBytesAsync();
                    //         await fs.WriteAsync(bytes, 0, bytes.Length);
                    //     }
                    // }

                }
                
            }
        }

        private async IAsyncEnumerable<string> GetPaths(string owner, string repo, string branch, bool recursive) {
            string sha = await _api.ReadLastCommitSHA(owner, repo, branch);
            var treeContent = await _api.ReadTree(owner, repo, sha, recursive);
            using(var stream = await treeContent.ReadAsStreamAsync()) {
                var reader = new StreamReader(stream);
                JsonTextReader r = new JsonTextReader(reader);
                while(await r.ReadAsync()) {
                    if(r.TokenType == JsonToken.StartArray && r.Path == "tree") {
                        break;
                    }
                }
                while(await r.ReadAsync()) {
                    if(r.TokenType == JsonToken.EndArray && r.Path == "tree") {
                        break;
                    }

                    if(r.TokenType == JsonToken.PropertyName && r.Value.Equals("path")) {
                        await r.ReadAsync();
                        yield return r.Value.ToString();
                    }
                }
                stream.Close();
                stream.Dispose();
            }
        }

    }
}