namespace Software_Design_Examples.Single_Responsibility
{
    public interface ISendMessages
    {
        void Send(IMessage message);
    }

    public interface IMessage{}
}