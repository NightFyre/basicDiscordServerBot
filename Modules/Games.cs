using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace basicDiscordServerBot.modules
{
    public class Games : ModuleBase<SocketCommandContext>
    {
        [Command("meme")]
        public async Task reddit(string subreddit = null)
        {
            var client = new HttpClient();
            var result = await client.GetStringAsync($"https://reddit.com/r/{subreddit ?? "DankMemes"}/random.json?limit=1");
            if (!result.StartsWith("["))
            {
                await Context.Channel.SendMessageAsync("This subreddit doesnt exist");
                return;
            }
            JArray arr = JArray.Parse(result);
            JObject post = JObject.Parse(arr[0]["data"]["children"][0]["data"].ToString());
            var builder = new EmbedBuilder()
                .WithImageUrl(post["url"].ToString())
                .WithColor(new Color(169, 0, 169))
                .WithTitle(post["title"].ToString())
                .WithUrl("https://reddit.com" + post["permalink"].ToString())
                .WithFooter($"⬆️ {post["ups"]} Up-Votes | 🗨 {post["num_comments"]} Comments");
            var embed = builder.Build();
            var message = Context.Message;
            await Context.Channel.SendMessageAsync(null, false, embed);
        }

        [Command("ColorMe")]
        public async Task CustomizeUser(string Color)
        {
            await Context.Message.DeleteAsync();                                                            //  Delete Calling Message
            var CustomColor = new Color(uint.Parse(Color, System.Globalization.NumberStyles.HexNumber));    //  Get custom color
            var RoleName = Context.Message.Author.Id.ToString();                                            //  Get USER ID to apply as role name
            var GuildUser = (Context.User as SocketGuildUser);                                              //  Get pointer to User Context
            SocketGuild Guild = Context.Guild;                                                              //  Get pointer to Guild Context
            foreach (SocketRole role in ((SocketGuildUser)Context.Message.Author).Roles)                    //  Check if user already has custom role
            {
                if (role.Name == RoleName)
                {
                    await role.ModifyAsync(x =>
                    {
                        x.Color = CustomColor;      //  Assign new color
                        x.Position = 1;             //  Adjust Role Position
                    });
                    return;     // Exit
                }
            }


            var CustomRole = await Guild.CreateRoleAsync($"{RoleName}", new GuildPermissions(false), null, false, null);     //  Create Custom Role

            // Get Bot Role Postion
            int CustomRolePosition = 0;
            {
                SocketGuild tempGuild = Context.Guild;
                var BOT = Context.Client.CurrentUser;
                var BOTRoles = tempGuild.GetUser(BOT.Id).Roles;
                foreach (SocketRole role in BOTRoles)
                {
                    if (role.Name == "@everyone")
                        continue;
                    CustomRolePosition = (role.Position - 1);
                }
            }

            await CustomRole.ModifyAsync(x => x.Position = CustomRolePosition);                                                     //  Assign Position
            await GuildUser.AddRoleAsync(CustomRole);                                                                               //  Add role to user
        }
    }
}