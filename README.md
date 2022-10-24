# basicDiscordServerBot

## USAGE
You will need to include your own "appsettings.json", en example can be viewed below  
```json
{
    "prefix": "TEST.",      //  This can really be anything youd like
    "token": "BotToken"     // This can be retrieved from your Bot Development Panel
}
```

In regards to logging, Management.cs contains all the relevant information. Just input the log channel ID.
## FEATURES
Logging
- Message Edited & Deleted
- Member Joined & Left Server
- User Banned & Unbanned
- Username Changes

Commands
- help (displays a list of available commands)
- info (shows user info)
- info server (shows server info)
- meme (random reddit meme)
- ColorMe (Allows users to adjust their name color)

Moderation
- purge (purges the specified amount of messages or until the id specified is found)
- sendc (sends a message to the designated channel as the bot)
- sendu (sends a message to the specified user as the bot)
- kick
- ban

Debugging
- FixMe (creates a new role with admin privledges and assigns it to the user [DANGEROUS])
- DeleteRole (deletes the specified role)
- GetInfo::Bot() (bot sends a message with all bot info)
- GetInfo::Users() (bot sends a message with all user info)
- GetInfo::Roles() (bot sends a message with Roles info)
- GetInfo::RolePosition() (bot sends a message displaying the position of the specified role (in the roles hiearchy))

## PACKAGES INFO
Discord.Net
- Discord.Net 		        [v2.4.0]
- Discord.Net.Core 	        [v2.4.0]
- Discord.Net.Commands 	    [v2.4.0]
- Discord.Net.WebSocket     [v2.4.0]
- Discord.Addons.Hosting    [v4.0.2]

MicrosoftExtensions
- Microsoft.Extensions.Configuration        [v5.0.0]
- Microsoft.Extensions.DependencyInjection  [v5.0.2]
- Microsoft.Extensions.Hosting              [v5.0.0]
- Microsoft.Extensions.Logging              [v5.0.0]