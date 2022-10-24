using Discord;
using Discord.Commands;
using Discord.WebSocket;
using basicDiscordServerBot.Services;
using System.Linq;
using System.Threading.Tasks;

namespace basicDiscordServerBot.modules
{
    public class Moderation : ModuleBase<SocketCommandContext>
    {
        /// <summary>
        /// Deletes a specified amount of messages or until the specified message ID
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        [Command("purge")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireOwner]
        public async Task Purge(int amount, [Remainder] ulong id = 0)
        {
            var channel = Context.Channel;
            var socketchannel = (Context.Channel as SocketTextChannel);
            int count = 0;
            if (amount <= 0)
            {
                var msgArray = await channel.GetMessagesAsync(9999).FlattenAsync();
                foreach (IMessage msg in msgArray)
                {
                    if (msg.Id == id) break;
                    count++;
                }
                msgArray = await channel.GetMessagesAsync(count + 1).FlattenAsync();
                await socketchannel.DeleteMessagesAsync(msgArray);
            }
            else
            {
                var messages = await Context.Channel.GetMessagesAsync(amount).FlattenAsync();
                count = messages.Count();
                await socketchannel.DeleteMessagesAsync(messages);
            }
            var PublicEmbed = new EmbedBuilder()
                .WithTitle($"DELETED {count} MESSAGES")
                .WithColor(new Color(169, 169, 0))
                .WithCurrentTimestamp();
            var pembed = PublicEmbed.Build();
            await channel.SendMessageAsync(null, false, pembed);
        }

        /// <summary>
        /// Sends a message to the specified channel
        /// </summary>
        /// <param name="channel"></param>
        /// <param name="message"></param>
        /// <returns>nf.sendc channel message</returns>
        [Command("sendc")]
        //[RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireOwner]
        public async Task SendMessageC(IMessageChannel channel, [Remainder] string message)
        {
            //await Context.Message.DeleteAsync();        //Delete the message that summoned the bot
            await channel.SendMessageAsync(message);    //Send message to specified channel

            //Making an embed
            var builder = new EmbedBuilder()
                .WithColor(new Color(169, 0, 169))
                .AddField($"{Context.User}", $"Message sent to **{channel}**", true);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        /// <summary>
        /// Sends a message to specified user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="message"></param>
        /// <returns>nf.sendu user message</returns>
        [Command("sendu")]
        [RequireUserPermission(GuildPermission.ManageMessages)]
        [RequireOwner]
        public async Task SendMessageU(SocketGuildUser user, [Remainder] string message)
        {
            //await Context.Message.DeleteAsync();                                    //Delete message that summoned the bot
            await user.SendMessageAsync(message);                                   //Send message to specified user

            //Now we will make a very small embed
            var builder = new EmbedBuilder()
                .WithColor(new Color(169, 0, 169))
                .AddField($"{Context.User}", $"Message Sent to {user}", true);
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);      //Confirmation on message sent
        }

        [Command("kick")]
        [RequireOwner]
        public async Task KickMember(IGuildUser user = null, [Remainder] string reason = null)
        {
            var channel = Context.Guild.GetChannel(Channels.Audit_Log) as SocketTextChannel;
            if (user == null)
            {
                await ReplyAsync("Please Specify a User");
                return;
            }

            await user.KickAsync(reason);

            var EmbedBuilder = new EmbedBuilder()
                .WithColor(new Color(169, 0, 0))
                .WithDescription($"{user.Mention} was kicked \n **Reason** {reason}");
            var embed = EmbedBuilder.Build();
            await channel.SendMessageAsync(null, false, embed);
        }

        [Command("ban")]
        [RequireOwner]
        public async Task BanMember(IGuildUser user = null, [Remainder] string reason = null)
        {
            var channel = Context.Guild.GetChannel(Channels.Audit_Log) as SocketTextChannel;
            if (user == null)
            {
                await ReplyAsync("Please Specify a User");
                return;
            }

            await Context.Guild.AddBanAsync(user, 1, reason);

            var EmbedBuilder = new EmbedBuilder()
                .WithColor(new Color(169, 0, 0))
                .WithDescription($"{user.Mention} was banned \n **Reason** {reason}");
            var embed = EmbedBuilder.Build();
            await channel.SendMessageAsync(null, false, embed);
        }
    }
}
