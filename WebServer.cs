using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

class WebServer
{
    public static void Start()
    {
        var host = new WebHostBuilder()
            .UseKestrel()
            .UseUrls("http://0.0.0.0:5000") // Change when deploying
            .Configure(app =>
            {
                app.Run(async context =>
                {
                    if (context.Request.Path == "/")
                    {
                        await context.Response.WriteAsync(File.ReadAllText("index.html"));
                    }
                    else if (context.Request.Path == "/register" && context.Request.Method == "POST")
                    {
                        var form = await context.Request.ReadFormAsync();
                        string discordToken = form["discord_token"];
                        string twitchToken = form["twitch_token"];
                        string twitchUsername = form["twitch_username"];

                        await FirebaseDatabase.SaveUserAsync(discordToken, twitchToken, twitchUsername);
                        await context.Response.WriteAsync("✅ Registration successful! Restart the bot.");
                    }
                });
            })
            .Build();

        host.Run();
    }
}
