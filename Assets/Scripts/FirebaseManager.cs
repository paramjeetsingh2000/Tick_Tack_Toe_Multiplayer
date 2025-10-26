using System.Threading.Tasks;
using Firebase;
using Firebase.Analytics;
using Firebase.Database;
using Google.MiniJSON;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance { get; private set; }
    public DatabaseReference DBreference;
    public string CurrentRoom = "";

    private void Awake()
    {
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
        var dependencyStatus = await FirebaseApp.CheckAndFixDependenciesAsync();

        if (dependencyStatus == DependencyStatus.Available)
        {
            var app = FirebaseApp.DefaultInstance;

            TextAsset dbUrlFile = Resources.Load<TextAsset>("DatabaseUrl");
            string dbUrl = null;

            if (dbUrlFile != null)
            {
                dbUrl = dbUrlFile.text.Trim();
                Debug.Log($"Loaded Firebase Database URL");
            }
            else
            {
                Debug.LogError("DatabaseUrl.txt not found in Resources folder!");
            }

            FirebaseAnalytics.SetAnalyticsCollectionEnabled(true);
            FirebaseAnalytics.LogEvent(FirebaseAnalytics.EventAppOpen);
            Debug.Log("Firebase Analytics initialized successfully.");

            if (!string.IsNullOrEmpty(dbUrl))
            {
                DBreference = FirebaseDatabase.GetInstance(app, dbUrl).RootReference;
                Debug.Log("Firebase Database connected successfully.");
            }
            else
            {
                Debug.LogError("Firebase Database URL missing or invalid!");
            }
        }
        else
        {
            Debug.LogError($"Could not resolve all Firebase dependencies: {dependencyStatus}");
        }
    }

    public async Task CreateNodeWithJson(string key, string json)
    {
        if (DBreference == null)
        {
            Debug.LogError("Database reference not initialized!");
            return;
        }

        try
        {
            await DBreference.Child(key).SetRawJsonValueAsync(json);
            Debug.Log($"Node '{key}' created/updated successfully with JSON.");

            var snapshot = await DBreference.Child(key).GetValueAsync();

            if (snapshot.Exists)
            {
                Debug.Log($"Fetched value of '{key}': {snapshot.GetRawJsonValue()}");
            }
            else
            {
                Debug.LogWarning($"Node '{key}' was created but no value found.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to create or fetch node '{key}': {e.Message}");
        }
    }

    public async Task UpdateNodeValue(string key, string json)
    {
        if (DBreference == null)
        {
            Debug.LogError("Database reference not initialized!");
            return;
        }

        try
        {
            // Update the node (overwrite)
            await DBreference.Child(key).SetRawJsonValueAsync(json);
            Debug.Log($"Node '{key}' updated successfully with JSON.");

            // Fetch the updated node to confirm
            var snapshot = await DBreference.Child(key).GetValueAsync();
            if (snapshot.Exists)
            {
                Debug.Log($"Updated value of '{key}': {snapshot.GetRawJsonValue()}");
            }
            else
            {
                Debug.LogWarning($"Node '{key}' exists but no value found after update.");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Failed to update node '{key}': {e.Message}");
        }
    }

    public void ListenToNodeChanges(string key)
    {
        if (DBreference == null)
        {
            Debug.LogError("Database reference not initialized!");
            return;
        }

        DatabaseReference nodeRef = DBreference.Child(key);

        nodeRef.ValueChanged -= HandleNodeValueChanged;

        nodeRef.ValueChanged += HandleNodeValueChanged;

        Debug.Log($"Listening for changes on node '{key}'...");
    }

    private void HandleNodeValueChanged(object sender, ValueChangedEventArgs e)
    {
        if (e.DatabaseError != null)
        {
            Debug.LogError("Error while listening to node changes: " + e.DatabaseError.Message);
            return;
        }

        if (e.Snapshot.Exists)
        {
            Debug.Log($"Node value changed: {e.Snapshot.GetRawJsonValue()}");
        }
        else
        {
            Debug.Log("Node was deleted or is empty.");
        }
    }
}