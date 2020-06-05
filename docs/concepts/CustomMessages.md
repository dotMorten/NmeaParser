# Creating custom NMEA messages

Custom NMEA messages can be registered for parsing as well.
To create a new message, add the NmeaMessageType attribute to the class, and declare the 5-character message type.

*Note: You can use `--` as the first two characters to make it independent of the Talker Type.*

Next ensure you have a constructor that takes the `TypeName` string parameter first, and a second `string[]` parameter that will contain all the message values:


Example:
```cs
[NmeaMessageType("PTEST")]
public class CustomMessage : NmeaMessage
{
    public CustomMessage(string type, string[] parameters) : base(type, parameters)
    {
        Value = parameters[0];
    }
    public string Value { get; }
}
```

Next register this with the NMEA Parser using either:

```cs
NmeaMessage.RegisterAssembly(typeof(CustomMessage).Assembly); //Registers all types in the provided assembly
NmeaMessage.RegisterMessage(typeof(CustomMessage).GetTypeInfo());  //Registers a single NMEA message
```

Note that these methods will throw if the NMEA type has already been registered (there's an overload where you can declare the `replace` parameter to `true` to overwrite already registered messages.

Next you should be able to test this method using the Parse method:
```cs
var input = "$PTEST,TEST*7C";
var msg = NmeaMessage.Parse(input);
```

# Creating a multi-sentence message

A NMEA message cannot exceed 82 characters, so often messages are split into multiple sentences. To create a custom multi message, either implement `IMultiSentenceMessage` or simply subclass `NmeaMultiSentenceMessage`.


```cs
[NmeaMessageType("PTST2")]
private class CustomMultiMessage : NmeaMultiSentenceMessage, IMultiSentenceMessage
{   
    public CustomMultiMessage(string type, string[] parameters) : base(type, parameters)
    {
    }
    public string Id { get; private set; }
    public List<string> Values { get; } = new List<string>();
    // Set index in the message where the total count is:
    protected override int MessageCountIndex => 0;
    // Set index in the message where the message number is:
    protected override int MessageNumberIndex => 1;
    protected override bool ParseSentences(Talker talkerType, string[] message)
    {
        // Ensure this message matches the previous message.
        // Use any indicator to detect message difference, so you can error out and avoid
        // appending the wrong message
        if (Id == null)
            Id = message[2]; //First time it's not set
        else if (Id != message[2])
            return false;
        Values.AddRange(message.Skip(3));
        return true;
    }
}
```
Note that the message is parsed in the `ParseSentences` method, and not the constructor. Also note that the talkerType is parsed to you, because multi-sentence messages allows a mix of talker types, if you use the `--` prefex in the NMEA type.

Next we can parse the two messages and have the second one be appended to the first one:

```cs
NmeaMessage.RegisterNmeaMessage(typeof(CustomMultiMessage).GetTypeInfo());
var input1 = "$PTST2,2,1,123,A,B,C,D*2A";
var input2 = "$PTST2,2,2,123,E,F,G,H*21";
var msg1 = NmeaMessage.Parse(input1);
var msg2 = NmeaMessage.Parse(input2, msg1 as IMultiSentenceMessage);
```

If msg1 and msg2 aren't the same instance, it means the message couldn't be added to the previous message, and a new message was generated.
