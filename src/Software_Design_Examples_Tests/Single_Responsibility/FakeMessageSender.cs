using Software_Design_Examples.Single_Responsibility;

namespace Software_Design_Examples_Tests.Single_Responsibility
{
    public class FakeMessageSender : ISendMessages
    {
        public IMessage SentMessage { get; private set; }

        public void Send(IMessage message)
        {
            SentMessage = message;
        }
    }
}