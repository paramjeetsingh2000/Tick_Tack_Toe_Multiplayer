using UnityEngine;
using Firebase;
using Firebase.Analytics;
using Firebase.Database;
using System.Threading.Tasks;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    public DatabaseReference DBreference;

    private void Awake()
    {
        // Make this persist across scenes
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private async void Start()
    {
        await InitializeFirebase();
    }

    private async Task InitializeFirebase()
    {
        // Check dependencies
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            // Get the default Firebase app
            var app = FirebaseApp.DefaultInstance;

            // Initialize Analytics
            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAppOpen);
            Debug.Log("✅ Firebase Analytics initialized successfully.");

            // Initialize Realtime Database reference
            //DBreference = FirebaseDatabase.DefaultInstance.RootReference;
            Debug.Log("✅ Firebase Database connected successfully.");
        }
        else
        {
            Debug.LogError($"❌ Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
    }
}
