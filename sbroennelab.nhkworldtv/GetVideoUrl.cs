using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace sbroennelab.nhkworldtv
{
    public class VodProgram
    {
        public string ProgamUuid { get; set; }
    }

    public static class GetVideoUrl
    {
        // Create a single, static HttpClient
        private static HttpClient httpClient = new HttpClient();
        // Define a regular expression for repeated words.
        private static Regex rx = new Regex(@"'data-de-program-uuid','(.+?)'",
              RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public static async Task<String> GetProgramUuid(string vidId)
        {
            var playerUrl = String.Format("https://movie-s.nhk.or.jp/v/refid/nhkworld/prefid/{0}?embed=js&targetId=videoplayer&de-responsive=true&de-callback-method=nwCustomCallback&de-appid={1}&de-subtitle-on=false", vidId, vidId);
            var response = await httpClient.GetAsync(playerUrl);
            var contents = await response.Content.ReadAsStringAsync();

            // Extract the Program Uuid
            MatchCollection matches = rx.Matches(contents.ToString());
            string matched = matches[0].Value;
            matched = matched.Replace("\"", "");
            matched = matched.Replace("'", "");
            string programUuid = matched.Split(",")[1];

            return (programUuid);
        }

        [FunctionName("GetVideoUrl")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route="GetVideoUrl/{vidId}")] HttpRequest req, string vidId,
            ILogger log)
        {
            var programUuid = await GetProgramUuid(vidId);

            // Serialize output to Json
            var vodProgram = new VodProgram();
            vodProgram.ProgamUuid = programUuid;

            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string jsonString;
            jsonString = JsonSerializer.Serialize(vodProgram, options);

            return new OkObjectResult(jsonString);
        }
    }
}
