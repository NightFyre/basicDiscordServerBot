using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Linq;
using System.Threading.Tasks;

namespace basicDiscordServerBot.modules
{
    public class General : ModuleBase<SocketCommandContext>
    {
        [Command("info")]
        public async Task Info(SocketGuildUser user = null)
        {
            if (user == null)
            {
                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl())
                    .WithDescription("")
                    .WithColor(new Color(169, 0, 169))
                    .AddField("User", Context.User, true)
                    .AddField("Created", Context.User.CreatedAt.ToString("MM/dd/yyyy"))
                    .AddField($"Joined {Context.Guild}", (Context.User as SocketGuildUser).JoinedAt.Value.ToString("MM/dd/yyyy"))
                    .AddField("Roles", string.Join(" ", (Context.User as SocketGuildUser).Roles.Select(x => x.Mention)))
                    .WithFooter($"USER ID: {Context.User.Id}")
                    .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
            else
            {
                var builder = new EmbedBuilder()
                    .WithThumbnailUrl(user.GetAvatarUrl() ?? user.GetDefaultAvatarUrl())
                    .WithDescription("")
                    .WithColor(new Color(169, 0, 169))
                    .AddField("User ID", user, true)
                    .AddField("Created", user.CreatedAt.ToString("MM/dd/yyyy"))
                    .AddField($"Joined {Context.Guild}", user.JoinedAt.Value.ToString("MM/dd/yyyy"))
                    .AddField("Roles", string.Join(" ", user.Roles.Select(x => x.Mention)))
                    .WithFooter($"USER ID: {user.Id}")
                    .WithCurrentTimestamp();
                var embed = builder.Build();
                await Context.Channel.SendMessageAsync(null, false, embed);
            }
        }

        [Command("info server")]
        public async Task ServerInfo()
        {
            var builder = new EmbedBuilder()
                .WithThumbnailUrl(Context.Guild.IconUrl)
                .WithDescription("")
                .WithTitle($"{Context.Guild.Name} Information")
                .WithColor(new Color(169, 0, 169))
                .AddField("Created at", Context.Guild.CreatedAt.ToString("MM/dd/yyyy"))
                .AddField("Member Count", (Context.Guild as SocketGuild).MemberCount + " members", true)
                .AddField("Online Members", (Context.Guild as SocketGuild).Users.Where(x => x.Status != UserStatus.Offline).Count() + " members", true)
                .WithCurrentTimestamp();
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
        
        [Command("help")]
        public async Task Help()
        {
            string message = "`prefix.meme` -Displays a random meme\n`prefix.info <user>` - Display user info\n`prefix.info server` - Displays server info\n`prefix.ColorMe <hex formatting>` - Changes username color";
            var builder = new EmbedBuilder()
                .WithDescription("")
                .WithTitle($"BOT Information")
                .WithColor(new Color(169, 0, 169))
                .AddField("COMMANDS", message, false)
                .WithCurrentTimestamp();
            var embed = builder.Build();
            await Context.Channel.SendMessageAsync(null, false, embed);
        }
    }
}
