using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace Bandwidth.Net.Api
{
  /// <summary>
  ///   Access to Message Api
  /// </summary>
  public interface IMessage
  {
    /// <summary>
    ///   Get a list of messages
    /// </summary>
    /// <param name="query">Optional query parameters</param>
    /// <param name="cancellationToken">>Optional token to cancel async operation</param>
    /// <returns>Collection with <see cref="Message" /> instances</returns>
    /// <example>
    ///   <code>
    /// var messages = client.Message.List(); 
    /// </code>
    /// </example>
    IEnumerable<Message> List(MessageQuery query = null,
      CancellationToken? cancellationToken = null);

    /// <summary>
    ///   Send a message.
    /// </summary>
    /// <param name="data">Parameters of new message</param>
    /// <param name="cancellationToken">Optional token to cancel async operation</param>
    /// <returns>Created message Id</returns>
    /// <example>
    ///   <code>
    /// var messageId = await client.Message.SendAsync(new MessageData{ From = "from", To = "to", Text = "Hello"});
    /// </code>
    /// </example>
    Task<string> SendAsync(MessageData data, CancellationToken? cancellationToken = null);

    /// <summary>
    ///   Send a message.
    /// </summary>
    /// <param name="data">Array of parameters of new messages</param>
    /// <param name="cancellationToken">Optional token to cancel async operation</param>
    /// <returns>Instance of created message</returns>
    /// <example>
    ///   <code>
    /// var messages = await client.Message.SendAsync(new[]{new MessageData{ From = "from", To = "to", Text = "Hello"}});
    /// </code>
    /// </example>
    Task<SendMessageResult[]> SendAsync(MessageData[] data, CancellationToken? cancellationToken = null);


    /// <summary>
    ///   Get information about a message
    /// </summary>
    /// <param name="messageId">Id of message to get</param>
    /// <param name="cancellationToken">Optional token to cancel async operation</param>
    /// <returns>Task with <see cref="Message" />Message instance</returns>
    /// <example>
    ///   <code>
    /// var message = await client.Message.GetAsync("messageId");
    /// </code>
    /// </example>
    Task<Message> GetAsync(string messageId, CancellationToken? cancellationToken = null);

    /// <summary>
    ///  Redact the text of a previously sent message
    /// </summary>
    /// <param name="messageId">Id of message to update</param>
    /// <param name="data">data to redact</param>
    /// <param name="cancellationToken">Optional token to cancel async operation</param>
    /// <returns>Task instance for async operation</returns>
    /// <example>
    ///   <code>
    /// await client.Message.PatchAsync("messageId", new PatchMessageData{Text = ""});
    /// </code>
    /// </example>
    Task PatchAsync(string messageId, PatchMessageData data, CancellationToken? cancellationToken = null);

  }

  internal class MessageApi : ApiBase, IMessage
  {
    public IEnumerable<Message> List(MessageQuery query = null, CancellationToken? cancellationToken = null)
    {
      return new LazyEnumerable<Message>(Client,
        () =>
          Client.MakeJsonRequestAsync(HttpMethod.Get, $"/users/{Client.UserId}/messages", cancellationToken, query));
    }

    public Task<string> SendAsync(MessageData data,
      CancellationToken? cancellationToken = null)
    {
      return Client.MakePostJsonRequestAsync($"/users/{Client.UserId}/messages", cancellationToken, data);
    }

    public async Task<SendMessageResult[]> SendAsync(MessageData[] data,
      CancellationToken? cancellationToken = null)
    {
      var list = await Client.MakeJsonRequestAsync<SendMessageResult[]>(HttpMethod.Post,
        $"/users/{Client.UserId}/messages", cancellationToken, null, data);
      var l = data.Length;
      for (var i = 0; i < l; i++)
      {
        list[i].Message = data[i];
      }
      return list;
    }

    public Task<Message> GetAsync(string messageId, CancellationToken? cancellationToken = null)
    {
      return Client.MakeJsonRequestAsync<Message>(HttpMethod.Get,
        $"/users/{Client.UserId}/messages/{messageId}", cancellationToken);
    }

    public Task PatchAsync(string messageId, PatchMessageData data, CancellationToken? cancellationToken = null)
    {
      return Client.MakeJsonRequestWithoutResponseAsync(new HttpMethod("PATCH"),
        $"/users/{Client.UserId}/messages/{messageId}", cancellationToken, null, data);
    }
  }


  /// <summary>
  ///   Message information
  /// </summary>
  public class Message
  {
    /// <summary>
    ///   The unique identifier for the message.
    /// </summary>
    public string Id { get; set; }

    /// <summary>
    /// The message sender's telephone number (or short code).
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// Message recipient telephone number (or short code).
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// Direction of message
    /// </summary>
    public MessageDirection Direction { get; set; }

    /// <summary>
    /// The message contents.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Array containing list of media urls to be sent as content for an mms.
    /// </summary>
    public string[] Media { get; set; }

    /// <summary>
    /// Message state
    /// </summary>
    public MessageState State { get; set; }

    /// <summary>
    /// The time when the message was completed.
    /// </summary>
    public DateTime Time { get; set; }


    /// <summary>
    /// The complete URL where the events related to the outgoing message will be sent.
    /// </summary>
    public string CallbackUrl { get; set; }

    /// <summary>
    /// Determine how long should the platform wait for callbackUrl's response before timing out (milliseconds).
    /// </summary>
    public int CallbackTimeout { get; set; }

    /// <summary>
    /// The server URL used to send message events if the request to callbackUrl fails.
    /// </summary>
    public string FallbackUrl { get; set; }

    /// <summary>
    /// A string that will be included in the callback events of the message.
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// Requested receipt option for outbound messages
    /// </summary>
    public MessageReceiptRequested ReceiptRequested { get; set; }

    /// <summary>
    /// Message delivery state 
    /// </summary>
    public MessageDeliveryState DeliveryState { get; set; }

    /// <summary>
    /// Numeric value of deliver code, see table for values.
    /// </summary>
    public int DeliveryCode { get; set; }

    /// <summary>
    /// Message delivery description for the respective delivery code
    /// </summary>
    public string DeliveryDescription { get; set; }

    /// <summary>
    /// Number of segments the message sent as
    /// </summary>
    public int SegmentCount { get; set; }
  }

  /// <summary>
  /// Possible message states
  /// </summary>
  public enum MessageState
  {
    /// <summary>
    /// The message was received.
    /// </summary>
    Received,

    /// <summary>
    /// The message is waiting in queue and will be sent soon.
    /// </summary>
    Queued,

    /// <summary>
    /// The message was removed from queue and is being sent.
    /// </summary>
    Sending,

    /// <summary>
    /// The message was sent successfully.
    /// </summary>
    Sent,

    /// <summary>
    /// There was an error sending or receiving a message (check errors resource for details).
    /// </summary>
    Error
  }

  /// <summary>
  /// Requested receipt options for outbound messages
  /// </summary>
  public enum MessageReceiptRequested
  {
    /// <summary>
    /// Delivery receipt will not be sent as callback event.
    /// </summary>
    None,

    /// <summary>
    /// Success or error delivery receipt maybe sent as callback event.
    /// </summary>
    All,

    /// <summary>
    /// Only error delivery receipt event maybe sent as callback event.
    /// </summary>
    Error
  }

  /// <summary>
  /// Possible delivery states
  /// </summary>
  public enum MessageDeliveryState
  {
    /// <summary>
    /// Waiting for receipt.
    /// </summary>
    Waiting,

    /// <summary>
    /// Receipt indicating that message was delivered.
    /// </summary>
    Delivered,

    /// <summary>
    /// Receipt indicating that message was not delivered.
    /// </summary>
    NotDelivered
  }

  /// <summary>
  /// Directions of message
  /// </summary>
  public enum MessageDirection
  {
    /// <summary>
    /// A message that came from the telephone network to one of your numbers (an "inbound" message)
    /// </summary>
    In,

    /// <summary>
    /// A message that was sent from one of your numbers to the telephone network (an "outbound" message)
    /// </summary>
    Out
  }

  /// <summary>
  ///   Query to get messages
  /// </summary>
  public class MessageQuery
  {
    /// <summary>
    /// The phone number to filter the messages that came from (must be in E.164 format, like +19195551212).
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// The phone number to filter the messages that was sent to (must be in E.164 format, like +19195551212).
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// The starting date time to filter the messages
    /// </summary>
    public MessageQueryDateTime FromDateTime { get; set; }

    /// <summary>
    /// The ending date time to filter the messages
    /// </summary>
    public MessageQueryDateTime ToDateTime { get; set; }

    /// <summary>
    /// Filter by direction of message
    /// </summary>
    public MessageDirection? Direction { get; set; }

    /// <summary>
    /// The message state to filter. 
    /// </summary>
    public MessageState? State { get; set; }

    /// <summary>
    /// The message delivery state to filter. 
    /// </summary>
    public MessageDeliveryState? DeliveryState { get; set; }

    /// <summary>
    /// How to sort the messages.
    /// </summary>
    public SortOrder? SortOrder { get; set; }

    /// <summary>
    ///   Used for pagination to indicate the size of each page requested for querying a list of messages. If no value is
    ///   specified the default value is 25 (maximum value 1000).
    /// </summary>
    public int? Size { get; set; }
  }

  /// <summary>
  ///   Parameters to send an message
  /// </summary>
  public class MessageData
  {
    /// <summary>
    /// The message sender's telephone number (or short code).
    /// </summary>
    public string From { get; set; }

    /// <summary>
    /// Message recipient telephone number (or short code).
    /// </summary>
    public string To { get; set; }

    /// <summary>
    /// The message contents.
    /// </summary>
    public string Text { get; set; }

    /// <summary>
    /// Array containing list of media urls to be sent as content for an mms.
    /// </summary>
    public string[] Media { get; set; }

    /// <summary>
    /// The complete URL where the events related to the outgoing message will be sent.
    /// </summary>
    public string CallbackUrl { get; set; }

    /// <summary>
    /// Determine how long should the platform wait for callbackUrl's response before timing out (milliseconds).
    /// </summary>
    public int? CallbackTimeout { get; set; }

    /// <summary>
    /// The server URL used to send message events if the request to callbackUrl fails.
    /// </summary>
    public string FallbackUrl { get; set; }

    /// <summary>
    /// A string that will be included in the callback events of the message.
    /// </summary>
    public string Tag { get; set; }

    /// <summary>
    /// Requested receipt option for outbound messages
    /// </summary>
    public MessageReceiptRequested? ReceiptRequested { get; set; }

  }

  /// <summary>
  /// Rsult of batch send of some messages
  /// </summary>
  public class SendMessageResult
  {
    /// <summary>
    /// Operation result
    /// </summary>
    public SendMessageResults Result { get; set; }

    /// <summary>
    /// Url to new message
    /// </summary>
    public string Location { get; set; }

    /// <summary>
    /// Id of new message
    /// </summary>
    public string Id => Location.Split('/').Last();

    /// <summary>
    /// Error information (if Result is Error)
    /// </summary>
    public Error Error { get; set; }

    /// <summary>
    /// Message data
    /// </summary>
    public MessageData Message { get; set; }
  }

  /// <summary>
  /// Send message results
  /// </summary>
  public enum SendMessageResults
  {
    /// <summary>
    /// Accepted
    /// </summary>
    Accepted,

    /// <summary>
    /// Error
    /// </summary>
    Error
  }

  /// <summary>
  /// Custom DateTime type to support specific serializing to string. It can be converted to/from date time implicitly
  /// </summary>
  public class MessageQueryDateTime
  {
    private readonly DateTime _time;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="time">DateTime instance</param>
    public MessageQueryDateTime(DateTime time)
    {
      _time = time;
    }

    /// <summary>
    /// MessageQueryDateTime -> DateTime  implicit convert 
    /// </summary>
    /// <param name="time">Instance to convert</param>
    public static implicit operator DateTime(MessageQueryDateTime time)
    {
      return time._time;
    }

    /// <summary>
    /// DateTime -> MessageQueryDateTime implicit convert
    /// </summary>
    /// <param name="time">Instance to convert</param>
    public static implicit operator MessageQueryDateTime(DateTime time)
    {
      return new MessageQueryDateTime(time);
    }

    /// <summary>
    /// Convert to string
    /// </summary>
    /// <returns>String presentation</returns>
    public override string ToString()
    {
      return _time.ToString("yyyy-MM-dd HH:mm:ss");
    }
  }

  /// <summary>
  ///   Parameters to patch a message
  /// </summary>
  public class PatchMessageData
  {
    /// <summary>
    /// The contents of the text must be the empty string (""). Any other value will fail.
    /// </summary>
    public string Text { get; set; }
  }
}
