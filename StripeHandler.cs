using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Stripe;
using Stripe.Checkout;
using TwitchLib.Api.Helix.Models.Subscriptions;

class StripeHandler
{
    public static void Start()
    {
        var host = new WebHostBuilder()
            .UseKestrel()
            .UseUrls("http://0.0.0.0:5000")
            .Configure(app =>
            {
                app.Run(async context =>
                {
                    if (context.Request.Path == "/stripe-webhook")
                    {
                        string discordUserId = context.Request.Query["user_id"];
                        var userKeys = await FirebaseDatabase.GetUserStripeKeys(discordUserId);
                        if (userKeys == null)
                        {
                            Console.WriteLine($"❌ No Stripe keys found for user {discordUserId}");
                            return;
                        }

                        using (var reader = new StreamReader(context.Request.Body))
                        {
                            var json = await reader.ReadToEndAsync();
                            var stripeEvent = EventUtility.ConstructEvent(json,
                                context.Request.Headers["Stripe-Signature"],
                                userKeys.Value.webhookSecret);

                            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                            {
                                var session = stripeEvent.Data.Object as Session;
                                string discordUsername = session.Metadata["discord_username"];
                                await GrantVIPRole(discordUsername);
                            }
                            else if (stripeEvent.Type == Events.CustomerSubscriptionDeleted ||
                                     stripeEvent.Type == Events.InvoicePaymentFailed)
                            {
                                var subscription = stripeEvent.Data.Object as Subscription;
                                string discordUsername = subscription.Metadata["discord_username"];
                                await RemoveVIPRole(discordUsername);
                            }
                        }
                    }
                });
            })
            .Build();

        host.Run();
    }

    private static async Task GrantVIPRole(string username)
    {
        var guild = Program._client.GetGuild(ulong.Parse(Environment.GetEnvironmentVariable("YOUR_DISCORD_SERVER_ID")));
        var user = guild.Users.FirstOrDefault(u => u.Username == username);
        if (user != null)
        {
            var role = guild.GetRole(ulong.Parse(Environment.GetEnvironmentVariable("YOUR_DISCORD_SUBSCRIBER_ROLE_ID")));
            await user.AddRoleAsync(role);
            Console.WriteLine($"🎖 Granted VIP Role to {username}");
        }
    }

    private static async Task RemoveVIPRole(string username)
    {
        var guild = Program._client.GetGuild(ulong.Parse(Environment.GetEnvironmentVariable("YOUR_DISCORD_SERVER_ID")));
        var user = guild.Users.FirstOrDefault(u => u.Username == username);
        if (user != null)
        {
            var role = guild.GetRole(ulong.Parse(Environment.GetEnvironmentVariable("YOUR_DISCORD_SUBSCRIBER_ROLE_ID")));
            await user.RemoveRoleAsync(role);
            Console.WriteLine($"❌ Removed VIP Role from {username}");
        }
    }
}
