using System;
using System.Collections.Generic;
using System.Management;
using System.Threading.Tasks;
using Google.Cloud.Firestore;

class FirebaseDatabase
{
    public static FirestoreDb _firestoreDb;

    public static void Initialize()
    {
        string path = "firebase-adminsdk.json";
        Environment.SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", path);
        _firestoreDb = FirestoreDb.Create("YOUR_FIREBASE_PROJECT_ID");
        Console.WriteLine("✅ Firebase Initialized!");
    }

    public static async Task SaveUserStripeKeys(string discordUserId, string stripeSecret, string webhookSecret)
    {
        DocumentReference docRef = _firestoreDb.Collection("users").Document(discordUserId);
        Dictionary<string, object> userStripeData = new Dictionary<string, object>
        {
            { "stripe_secret", stripeSecret },
            { "webhook_secret", webhookSecret }
        };
        await docRef.SetAsync(userStripeData, SetOptions.MergeAll);
        Console.WriteLine($"✅ Saved Stripe API keys for {discordUserId}");
    }

    public static async Task<(string stripeSecret, string webhookSecret)?> GetUserStripeKeys(string discordUserId)
    {
        DocumentSnapshot doc = await _firestoreDb.Collection("users").Document(discordUserId).GetSnapshotAsync();
        if (doc.Exists)
        {
            Dictionary<string, object> data = doc.ToDictionary();
            return (
                data.ContainsKey("stripe_secret") ? data["stripe_secret"].ToString() : null,
                data.ContainsKey("webhook_secret") ? data["webhook_secret"].ToString() : null
            );
        }
        return null;
    }
}
