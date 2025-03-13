using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using TwitchLib.Client;

class Giveaway
{
    private static List<string> entries = new List<string>();

    public static void EnterGiveaway(string username)
    {
        if (!entries.Contains(username))
        {
            entries.Add(username);
        }
    }

    public static async Task PickWinner(SocketTextChannel discordChannel, TwitchClient twitchClient)
    {
        if (entries.Count > 0)
        {
            Random random = new Random();
            string winner = entries[random.Next(entries.Count)];

            // Announce in Discord
            await discordChannel.SendMessageAsync($"🏆 **The giveaway winner is {winner}!** 🎉");

            // Announce in Twitch
            twitchClient.SendMessage(twitchClient.JoinedChannels[0], $"🏆 The giveaway winner is {winner}! 🎉");

            // Reset the list
            entries.Clear();
        }
        else
        {
            await discordChannel.SendMessageAsync("❌ No entries yet.");
        }
    }
}
