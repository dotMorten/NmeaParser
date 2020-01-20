# Migrating from v1.x

A lot of API clean up was made for v2.0, based on lots of lessons learned in v1.
Therefore several breaking changes occured. Some or minor naming changes to properties and members, that should be pretty self-explanatory, but here are the main changes:

### Removal of talker-prefix
Most Talker specific messages has been made talker independent. This means the name of the NMEA messages most often just got the first two characters removed. For example `Gprmc` is now just `Rmc`. Instead there's a `TalkerId` property you use to tell messages apart if you get them from multiple talkers (like GLONASS and Galileo in addition to GPS).
Some messages that are not available across multiple talkers are still named by all 5 characters.

### Change of namespaces
Most NMEA messages has been moved to `NmeaParser.Messages` namespace, with a sub-namespace for brand specific messages. For example `NmeaParser.Messages.Garmin`.

### Multi-sentence messages are now merged into one
You no longer have to deal with message 1 of N messages with for instance GSV messages. Instead these are merged for you, and you won't receive an event until all the messages has been combined. This greatly simplifies dealing with multiple messages. You also won't receive an event for a partially-received multi-message, but will be discarded.
This also means the event args that were provided to attempt to help with this has been removed.