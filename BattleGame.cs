using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

class BattleGame
{
    private static Dictionary<string, int> playerHealth = new Dictionary<string, int>();

    public static async Task StartBattle(SocketMessage message, string opponent)
    {
        string challenger = message.Author.Username;

        playerHealth[challenger] = 100;
        playerHealth[opponent] = 100;

        await message.Channel.SendMessageAsync($"⚔️ **{challenger} challenges {opponent} to a battle!**\nBoth players start with **100 HP**.");

        while (playerHealth[challenger] > 0 && playerHealth[opponent] > 0)
        {
            await Task.Delay(2000); // Pause between rounds
            int damage = new Random().Next(10, 30);
            playerHealth[opponent] -= damage;
            if (playerHealth[opponent] <= 0)
            {
                await message.Channel.SendMessageAsync($"🏆 **{challenger} wins the battle!**");
                return;
            }

            damage = new Random().Next(10, 30);
            playerHealth[challenger] -= damage;
            if (playerHealth[challenger] <= 0)
            {
                await message.Channel.SendMessageAsync($"🏆 **{opponent} wins the battle!**");
                return;
            }
        }
    }
}
