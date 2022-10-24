using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace basicDiscordServerBot.Services
{
    public class CommandHandler : InitializedService
    {
        private readonly IServiceProvider _provider;
        private readonly DiscordSocketClient _client;
        private readonly CommandService _service;
        private readonly IConfiguration _config;

        public CommandHandler(IServiceProvider provider, DiscordSocketClient client, CommandService service, IConfiguration config)
        {
            _provider = provider;
            _client = client;
            _service = service;
            _config = config;
        }

        //Event Handler
        public override async Task InitializeAsync(CancellationToken cancellationToken)
        {
            //  LOGGING
            _client.MessageDeleted += MessageDeleted;           //Deleted Messages
            _client.MessageUpdated += MessageEdited;            //Edited Messages
            _client.UserJoined += MemberJoinedServer;           //Member Joined Server
            _client.UserLeft += MemberLeftServer;               //Member Left Server
            _client.UserUpdated += OnUserUpdated;               //User Profile Updated
            _client.UserBanned += OnUserBanned;                 //User Banned
            _client.UserUnbanned += OnUserUnBanned;             //User Unbaned
            _client.MessageReceived += OnMessageReceived;       //Message Sent in chat

            //  Execution Handler
            _service.CommandExecuted += OnCommandExecuted;
            await _service.AddModulesAsync(Assembly.GetEntryAssembly(), _provider);
        }

        private async Task OnMessageReceived(SocketMessage arg)
        {
            if (!(arg is SocketUserMessage message)) return;
            if (message.Source != MessageSource.User) return;

            var argPos = 0;
            var context = new SocketCommandContext(_client, message);

            //Bot listening for prefix / command
            if (!message.HasStringPrefix(_config["prefix"], ref argPos) && !message.HasMentionPrefix(_client.CurrentUser, ref argPos)) return;
            await _service.ExecuteAsync(context, argPos, _provider);
        }

        private async Task OnCommandExecuted(Optional<CommandInfo> command, ICommandContext context, IResult result)
        {
            if (command.IsSpecified && !result.IsSuccess) await context.Channel.SendMessageAsync($"Error: {result}");
        }

        #region AUTOMATED LOGGING

        /*SUMMARY
            //      Logs Edited Messages            | Yellow Embed
            //      - #audit-log
        */
        private async Task MessageEdited(Cacheable<IMessage, ulong> log, SocketMessage message, ISocketMessageChannel channel)
        {
            // check if the message exists in cache; if not, we cannot report what was removed
            if (!log.HasValue) return;
            var _oldMSG = log.Value;
            var logChannel = _client.GetChannel(Channels.Audit_Log) as SocketTextChannel;
            var builder = new EmbedBuilder()
                .WithAuthor(message.Author)
                .WithColor(new Color(169, 169, 0))
                .WithTitle("MESSAGE EDITED")
                .AddField("CHANNEL", $"<#{channel.Id}>", false)
                .AddField("OLD CONTENT", $"{_oldMSG.Content}", false)
                .AddField("NEW CONTENT", $"{message.Content}", false)
                .WithFooter($"USER ID: {message.Author.Id}")
                .WithCurrentTimestamp();
            var embed = builder.Build();
            await logChannel.SendMessageAsync(null, false, embed);
        }

        /*SUMMARY
            //      Logs Deleted Messages           | Yellow Embed
            //      - #audit-log
        */
        private async Task MessageDeleted(Cacheable<IMessage, ulong> msg, ISocketMessageChannel channel)
        {
            // check if the message exists in cache; if not, we cannot report what was removed
            if (!msg.HasValue) return;
            var message = msg.Value;
            if (message.Author.IsBot) return;
            if (message.Content.Contains("squanch.")) return;
            var logChannel = _client.GetChannel(Channels.Audit_Log) as SocketTextChannel;
            var builder = new EmbedBuilder()
                .WithAuthor(message.Author)
                .WithColor(new Color(169, 169, 0))
                .WithTitle("MESSAGE DELETED")
                .AddField("CHANNEL", $"<#{channel.Id}>", false)
                .AddField("CONTENT", $"{message.Content}", false)
                .WithFooter($"USER ID: {message.Author.Id}")
                .WithCurrentTimestamp();
            var embed = builder.Build();
            await logChannel.SendMessageAsync(null, false, embed);
        }

        /*SUMMARY
            //      Logs Member Left                | Red Embed
        */
        //<SocketGuild, SocketUser, Task>
        private async Task MemberLeftServer(SocketGuildUser arg)
        {
            var channel = _client.GetChannel(Channels.Audit_Log) as SocketTextChannel;
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(arg.GetAvatarUrl() ?? arg.GetDefaultAvatarUrl())
                .WithDescription($"{arg.Mention} has left **{channel.Guild.Name}**")
                .WithColor(new Color(169, 0, 0))
                .AddField("User Name", arg, true)
                .AddField("Created", arg.CreatedAt.ToString("MM/dd/yyyy"))
                .AddField($"Joined {arg.Guild}", arg.JoinedAt.Value.ToString("MM/dd/yyyy"))
                .AddField($"Roles", String.Join(" ", arg.Roles.Select(x => x.Mention)))
                .WithFooter($"USER ID: {arg.Id}")
                .WithCurrentTimestamp();
            var embed = builder.Build();
            await channel.SendMessageAsync(null, false, embed);

            //Remove roles
            var roles = arg.Roles.Select(x => x.Id);
            if (roles.Count() > 0)
                await arg.RemoveRolesAsync(roles);
        }

        /*SUMMARY
            //      Kicks new account (<36hrs)      | Red Embed
            //      - #audit-log
            ///    Logs Member Joined               | Green Embed
            ///    - #audit-log
        */
        private async Task MemberJoinedServer(SocketGuildUser arg)
        {
            //New Account Detection (ANTI-ALT)
            var timeCreated = arg.CreatedAt.UtcDateTime;
            var kickTimer = arg.JoinedAt.Value.UtcDateTime;
            var LOGEVENT = _client.GetChannel(Channels.Audit_Log) as SocketTextChannel;

            //New Account ~36hrs = Kick
            if ((kickTimer - timeCreated).TotalHours <= 36)
            {
                //LOG EVENT
                var builder1 = new EmbedBuilder()
                    .WithThumbnailUrl(arg.GetAvatarUrl() ?? arg.GetDefaultAvatarUrl())
                    .WithDescription($"{arg.Mention} has joined **{LOGEVENT.Guild.Name}**")
                    .WithTitle("KICKED NEW ACCOUNT!")
                    .WithColor(new Color(169, 0, 0))
                    .AddField("Username", arg, true)
                    .AddField("Created", arg.CreatedAt.ToString("MM/dd/yyyy"))
                    .AddField($"Joined {arg.Guild}", arg.JoinedAt.Value.ToString("MM/dd/yyyy"))
                    .AddField($"Roles", String.Join(" ", arg.Roles.Select(x => x.Mention)))
                    .WithFooter($"USER ID: {arg.Id}")
                    .WithCurrentTimestamp();
                var LOG = builder1.Build();

                //Log Event
                await LOGEVENT.SendMessageAsync(null, false, LOG);

                //Remove any roles 
                var roles = arg.Roles.Select(x => x.Id);
                if (roles.Count() > 0)
                    await arg.RemoveRolesAsync(roles);

                //Kick User
                await arg.KickAsync("Account is less than 24hrs old");
                return;
            }

            //Log Event of Member Joining
            var channel = _client.GetChannel(Channels.Audit_Log) as SocketTextChannel;
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(arg.GetAvatarUrl() ?? arg.GetDefaultAvatarUrl())
                .WithDescription($"{arg.Mention} has joined **{channel.Guild.Name}**")
                .WithColor(new Color(0, 169, 0))
                .AddField("Username", arg, true)
                .AddField("Created", arg.CreatedAt.ToString("MM/dd/yyyy"))
                .AddField($"Joined {arg.Guild}", arg.JoinedAt.Value.ToString("MM/dd/yyyy"))
                .AddField($"Roles", String.Join(" ", arg.Roles.Select(x => x.Mention)))
                .WithFooter($"USER ID: {arg.Id}")
                .WithCurrentTimestamp();
            var embed = builder.Build();
            await LOGEVENT.SendMessageAsync(null, false, embed);
        }

        /*SUMMARY
            //      Logs Avatar and Name Changes    | Purple Embed
            //      - #audit-log
        */
        private async Task OnUserUpdated(SocketUser arg1, SocketUser arg2)
        {
            var channel = _client.GetChannel(Channels.Audit_Log) as SocketTextChannel;

            //Name Changed
            if (arg1.Username.ToString() != arg2.Username.ToString())
            {
                var builder = new EmbedBuilder()
                .WithAuthor(arg2)
                .WithThumbnailUrl(arg2.GetAvatarUrl() ?? arg2.GetDefaultAvatarUrl())
                .WithTitle("NAME CHANGE")
                .WithDescription($"NEW: {arg2.Mention}")
                .WithDescription($"OLD: {arg1}")
                .WithColor(new Color(169, 0, 169))
                .WithFooter($"USER ID: {arg2.Id}")
                .WithCurrentTimestamp();
                var embed = builder.Build();
                await channel.SendMessageAsync(null, false, embed);
            }
        }

        /*SUMMARY
            //      Logs banned users               | Red Embed
            //      - #audit-log
        */
        private async Task OnUserBanned(SocketUser arg1, SocketGuild arg2)
        {
            var channel = _client.GetChannel(Channels.Audit_Log) as SocketTextChannel;
            var builder = new EmbedBuilder()
            .WithAuthor(arg1)
            .WithThumbnailUrl(arg1.GetAvatarUrl() ?? arg1.GetDefaultAvatarUrl())
            .WithTitle("MEMBER BANNED")
            .WithDescription($"{arg1.Mention}")
            .WithColor(new Color(169, 0, 0))
            .WithFooter($"USER ID: {arg1.Id}")
            .WithCurrentTimestamp();
            var embed = builder.Build();
            await channel.SendMessageAsync(null, false, embed);
        }

        /*SUMMARY
            //      Logs unbanned users             | Red Embed
            //      - #audit-log
        */
        private async Task OnUserUnBanned(SocketUser arg1, SocketGuild arg2)
        {
            var channel = _client.GetChannel(Channels.Audit_Log) as SocketTextChannel;
            var builder = new EmbedBuilder()
            .WithAuthor(arg1)
            .WithThumbnailUrl(arg1.GetAvatarUrl() ?? arg1.GetDefaultAvatarUrl())
            .WithTitle("MEMBER UN-BANNED")
            .WithDescription($"{arg1.Mention}")
            .WithColor(new Color(0, 169, 0))
            .WithFooter($"USER ID: {arg1.Id}")
            .WithCurrentTimestamp();
            var embed = builder.Build();
            await channel.SendMessageAsync(null, false, embed);
        }

        #endregion
    }
}