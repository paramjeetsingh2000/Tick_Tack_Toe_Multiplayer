using System.Threading.Tasks;
using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private TMP_Text _roomId;
    [SerializeField] private TMP_InputField _inputField;

    public async void CreateRoom()
    {
        string roomKey = await GetRoomId();

        if (!string.IsNullOrEmpty(roomKey))
        {
            _roomId.text = roomKey;
            FirebaseManager.Instance.CurrentRoom = roomKey;
            FirebaseManager.Instance.ListenToNodeChanges(roomKey);
            FirebaseManager.Instance.PlayerIndex = 1;
            Debug.Log($"Room created: {roomKey}");
        }
        else
        {
            Debug.LogError("Failed to create room.");
        }
    }

    private async Task<string> GetRoomId()
    {
        if (FirebaseManager.Instance.DBreference == null)
        {
            Debug.LogError("Database reference not initialized!");
            return null;
        }

        try
        {
            var snapshot = await FirebaseManager.Instance.DBreference.GetValueAsync();

            int nodeCount = snapshot.Exists ? (int)snapshot.ChildrenCount : 0;

            string newRoomKey = $"R{nodeCount + 1}";

            await FirebaseManager.Instance.DBreference.Child(newRoomKey).SetRawJsonValueAsync("{ \"player1\": \"1\", \"player2\": \"0\" }");

            Debug.Log($"Created room node: {newRoomKey}");
            return newRoomKey;
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to create room: " + e.Message);
            return null;
        }
    }
  
    public async void JoinRoom()
    {
        string roomKey = _inputField.text;

        if (string.IsNullOrEmpty(roomKey))
        {
            Debug.Log("No value in room ID.");
            return;
        }

        if (FirebaseManager.Instance.DBreference == null)
        {
            Debug.LogError("Database reference not initialized!");
            return;
        }

        try
        {
            DatabaseReference roomRef = FirebaseManager.Instance.DBreference.Child(roomKey);
            var snapshot = await roomRef.GetValueAsync();

            if (!snapshot.Exists)
            {
                Debug.LogWarning($"Room '{roomKey}' does not exist.");
                return;
            }

            string currentJson = snapshot.GetRawJsonValue();

            if (currentJson == "{\"player1\":\"1\",\"player2\":\"0\"}")
            {
                string updatedJson = "{\"player1\":\"1\",\"player2\":\"1\"}";
                await roomRef.SetRawJsonValueAsync(updatedJson);
                Debug.Log($"Room '{roomKey}' updated: {updatedJson}");
                FirebaseManager.Instance.CurrentRoom = roomKey;
                FirebaseManager.Instance.ListenToNodeChanges(roomKey);
                FirebaseManager.Instance.PlayerIndex = 2;
                SceneManager.LoadScene("GameScene");
                return;
            }
            else
            {
                Debug.LogWarning($"Room '{roomKey}' cannot be joined. Current value: {currentJson}");
                return;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError("Failed to join room: " + e.Message);
            return;
        }
    }
}
