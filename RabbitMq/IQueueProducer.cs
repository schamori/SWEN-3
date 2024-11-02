namespace RabbitMq.QueueLibrary;

public interface IQueueProducer
{
    void Send(string body, Guid documentId);
}
