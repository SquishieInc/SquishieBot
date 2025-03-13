using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

class RoleSync
{
    public static async Task SyncRoles(SocketGuild guild, ulong subRoleId, List<string> subscribers)
    {
        foreach (var user in guild.Users)
        {
            if (subscribers.Contains(user.Username))
            {
                var role = guild.GetRole(subRoleId);
                await user.AddRoleAsync(role);
                Console.WriteLine($"✅ {user.Username} has been given the Subscriber role.");
            }
        }
    }
}
