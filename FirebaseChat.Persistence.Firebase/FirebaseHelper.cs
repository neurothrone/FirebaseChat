using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Database.Streaming;

namespace FirebaseChat.Persistence.Firebase;

public class FirebaseHelper
{
    private readonly FirebaseClient _firebaseClient;

    public FirebaseHelper(string firebaseUrl)
    {
        _firebaseClient = new FirebaseClient(firebaseUrl);
    }

    public async Task<List<Message>> GetMessagesAsync()
    {
        try
        {
            var messages = await _firebaseClient
                .Child("Messages")
                .OrderByKey()
                .LimitToLast(50)
                .OnceAsync<Message>();

            return messages.Select(m => m.Object).ToList();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error retrieving messages: {ex.Message}");
            return new List<Message>();
        }
    }

    public async Task SendMessageAsync(Message message)
    {
        try
        {
            await _firebaseClient
                .Child("Messages")
                .PostAsync(message);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error sending message: {ex.Message}");
        }
    }

    public void SubscribeToMessages(Action<Message> onMessageReceived)
    {
        _firebaseClient
            .Child("Messages")
            .AsObservable<Message>()
            .Subscribe(d =>
            {
                if (d.EventType == FirebaseEventType.InsertOrUpdate)
                {
                    onMessageReceived.Invoke(d.Object);
                    // onMessageReceived(d.Object); // Short-hand
                }
            });
    }
}