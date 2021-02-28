using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;

namespace SBReceiveTopicConApp
{
  class Program
  {
    static NamespaceManager _namespaceManager;
    static void Main(string[] args)
    {
      CollectSBDetails();
      ReceiveMessage();
      Console.WriteLine($"All Done {DateTime.Now}");
    }

    private static void ReceiveMessage()
    {
      TokenProvider tokenProvider = _namespaceManager.Settings.TokenProvider; ;

      if (_namespaceManager.TopicExists("DataCollectionTopic"))
      {
        MessagingFactory factory = MessagingFactory.Create(_namespaceManager.Address, tokenProvider);

        //Same as Queue ReceiveMode.PeekLock is default
        MessageReceiver receiver = factory.CreateMessageReceiver("DataCollectionTopic/subscriptions/Inventory");
        MessageReceiver receiver1 = factory.CreateMessageReceiver("DataCollectionTopic/subscriptions/Dashboard");

        BrokeredMessage receivedMessage = null;
        try
        {
          Console.WriteLine($"From Inventory");
          while ((receivedMessage = receiver.Receive()) != null)
          {
            ProcessMessage(receivedMessage);
            receivedMessage.Complete();
          }
          Console.WriteLine($"From Dashboard");
          while ((receivedMessage = receiver1.Receive()) != null)
          {
            ProcessMessage(receivedMessage);
            receivedMessage.Complete();
          }
          factory.Close();
          _namespaceManager.DeleteSubscription("DataCollectionTopic", "Inventory");
          _namespaceManager.DeleteSubscription("DataCollectionTopic", "Dashboard");
          _namespaceManager.DeleteTopic("DataCollectionTopic");

        }
        catch (Exception ex)
        {
          Console.WriteLine(ex.ToString());
          receivedMessage.Abandon();
        }

        #region With Collection
        //IEnumerable<BrokeredMessage> receivedMessageList = receiver.ReceiveBatch(2);
        //try
        //{
        //    foreach (var item in receivedMessageList)
        //    {
        //        ProcessMessage(item);
        //        item.Complete();
        //    }
        //    factory.Close();
        //    _namespaceManager.DeleteSubscription("DataCollectionTopic", "Inventory");
        //    //_namespaceManager.DeleteSubscription("DataCollectionTopic", "Dashboard");
        //    //_namespaceManager.DeleteTopic("DataCollectionTopic");
        //}
        //catch (Exception ex)
        //{
        //    Console.WriteLine(ex.ToString());
        //} 
        #endregion
      }

    }

    private static void ProcessMessage(BrokeredMessage receivedMessage)
    {
      //Console.WriteLine("Label: {0}, MessageID: {1}, StoreName: {2}, MachineID: {3}", receivedMessage.Label, receivedMessage.MessageId, receivedMessage.Properties["StoreName"], receivedMessage.Properties["MachineID"]);
      Console.WriteLine($"Label: {receivedMessage.Label}, MessageID: {receivedMessage.MessageId}, StoreName: {receivedMessage.Properties["StoreName"]}, MachineID: {receivedMessage.Properties["MachineID"]}");
    }
    private static void CollectSBDetails()
    {
      _namespaceManager = NamespaceManager.CreateFromConnectionString(ConfigurationManager.AppSettings["Microsoft.ServiceBus.ConnectionString"].ToString());
    }
  }
}
