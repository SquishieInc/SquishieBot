using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

class Leaderboard
{
    private static Dictionary<string, int> messageCount = new Dictionary<string, int>();

    public static void TrackMessage(string username)
    {
        if (!messageCount.ContainsKey(username))
            messageCount[username] = 0;

        messageCount[username]++;
    }

    public static async Task ShowLeaderboard(SocketMessage message)
    {
        var topUsers = messageCount.OrderByDescending(x => x.Value).Take(5);
        string leaderboard = "**📊 Weekly Leaderboard:**\n";

        int position = 1;
        foreach (var user in topUsers)
        {
            leaderboard += $"{position}. **{user.Key}** - {user.Value} messages\n";
            position++;
        }

        await message.Channel.SendMessageAsync(leaderboard);
    }

    public static async Task AssignTopRole(SocketGuild guild, ulong roleId)
    {
        var topUser = messageCount.OrderByDescending(x => x.Value).FirstOrDefault();
        if (topUser.Key != null)
        {
            var user = guild.Users.FirstOrDefault(u => u.Username == topUser.Key);
            if (user != null)
            {
                var role = guild.GetRole(roleId);
                await user.AddRoleAsync(role);
                Console.WriteLine($"🎖 Assigned top role to {topUser.Key}");
            }
        }
    }
}
