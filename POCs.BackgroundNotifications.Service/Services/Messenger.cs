namespace POCs.BackgroundNotifications.WindowsService.Services;
public class Messenger
{
    private readonly Queue<string> _messages;
    public bool HasMessages => _messages.Any();
    public Messenger()
    {
        _messages = new Queue<string>();
    }

    public void EnqueueMessage(string message)
    {
        _messages.Enqueue(message);
    }

    public string DequeueMessage()
    {
        return _messages.Dequeue();
    }
}
