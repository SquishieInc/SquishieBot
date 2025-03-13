using System;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Stripe;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

class AnalyticsServer
{
    public static void Start()
    {
        var host = new WebHostBuilder()
            .UseKestrel()
            .UseUrls("http://0.0.0.0:5001")
            .Configure(app =>
            {
                app.Run(async context =>
                {
                    if (context.Request.Path == "/analytics")
                    {
                        int activeSubs = await GetActiveSubscribers();
                        decimal earnings = await GetTotalEarnings();

                        string response = $"<h2>📊 Subscription Analytics</h2>" +
                                          $"<p><b>Active Subscribers:</b> {activeSubs}</p>" +
                                          $"<p><b>Total Earnings:</b> ${earnings:F2}</p>";

                        await context.Response.WriteAsync(response);
                    }
                });
            })
            .Build();

        host.Run();
    }

    private static async Task<int> GetActiveSubscribers()
    {
        QuerySnapshot snapshot = await FirebaseDatabase._firestoreDb.Collection("users").GetSnapshotAsync();
        int count = 0;
        foreach (var doc in snapshot.Documents)
        {
            if (doc.ContainsField("stripe_secret")) count++;
        }
        return count;
    }

    private static async Task<decimal> GetTotalEarnings()
    {
        StripeConfiguration.ApiKey = Environment.GetEnvironmentVariable("STRIPE_SECRET_KEY");
        var balanceService = new BalanceService();
        var balance = await balanceService.GetAsync();
        return balance.Available[0].Amount / 100m;
    }
}
