namespace RabbitMq.QueueLibrary;

public interface IQueueConsumer
{
    event EventHandler<QueueReceivedEventArgs> OnReceived;
    void StartReceive();
    void StopReceive();
}
