using System;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TwitchLib.Client;
using TwitchLib.Client.Events;
using TwitchLib.Client.Models;

class Program
{
    public static DiscordSocketClient _client;
    public static TwitchClient _twitchClient;

    static async Task Main(string[] args)
    {
        FirebaseDatabase.Initialize();
        WebServer.Start(); // Web dashboard for registering Stripe API keys
        StripeHandler.Start(); // Handles Stripe payments and VIP roles
        AnalyticsServer.Start(); // Subscription analytics dashboard

        _client = new DiscordSocketClient(new DiscordSocketConfig
        {
            GatewayIntents = GatewayIntents.Guilds | GatewayIntents.GuildMessages | GatewayIntents.MessageContent
        });

        _client.Log += Log;
        _client.MessageReceived += MessageReceived;
        await _client.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable("DISCORD_BOT_TOKEN"));
        await _client.StartAsync();

        _twitchClient = new TwitchClient();
        _twitchClient.Initialize(new ConnectionCredentials("YOUR_TWITCH_USERNAME", Environment.GetEnvironmentVariable("TWITCH_OAUTH_TOKEN")));
        _twitchClient.OnMessageReceived += TwitchMessageReceived;
        _twitchClient.Connect();

        await Task.Delay(-1); // Keep bot running
    }

    private static async Task MessageReceived(SocketMessage message)
    {
        if (message.Author.IsBot) return;

        if (message.Content.ToLower() == "!subscribe")
        {
            await message.Channel.SendMessageAsync("💎 Subscribe here: https://buy.stripe.com/YOUR_STRIPE_LINK");
        }

        if (message.Content.ToLower() == "!leaderboard")
        {
            await Leaderboard.ShowLeaderboard(message);
        }
    }

    private static void TwitchMessageReceived(object sender, OnMessageReceivedArgs e)
    {
        Console.WriteLine($"[Twitch] {e.ChatMessage.Username}: {e.ChatMessage.Message}");
    }

    private static Task Log(LogMessage msg)
    {
        Console.WriteLine($"[Discord] {msg}");
        return Task.CompletedTask;
    }
}
