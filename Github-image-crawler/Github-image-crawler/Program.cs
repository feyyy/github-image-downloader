using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

// See https://aka.ms/new-console-template for more information

namespace Mobge.GitFileCrawler {
    public class Program {
    
        public static async Task Main(params string[] args) {
            Console.WriteLine("Enter your token and press enter:");
            string token = Console.ReadLine();
            FileCrawler fc = new FileCrawler(token);
            string owner = "MobgeGames";
            string[] repos = new string[] {
                "hybrid-mount-and-dinos"
            };
            string root = "C:\\Users\\Fey\\Documents\\Projects\\github-image-downloader\\ImagesDump";
            string prefix = "Assets/Game/Art/UI/";
            foreach(var repo in repos) {
                var targetPath = Path.Combine(root, repo);
                Console.WriteLine("Downloading images of: " + repo);
                await fc.DownloadAllPng(owner, repo, "main", targetPath, prefix);
            }
        }
        static async Task Run3() {
            var c = new FileCrawler("");
            string repoOwner = "MobgeGames";
            string repoName = "hybrid-drinktopia";
            string branch = "main";
            string prefix = "Assets/Game/Art/UI/";
            await c.DownloadAllPng(repoOwner, repoName, branch, "", prefix);
        }

        
    }
    

    

}