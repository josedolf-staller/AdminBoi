﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class Ban : ModuleBase<SocketCommandContext>
    {
        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(SocketGuildUser user, string prune = "0", [Remainder] string reason = null)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsMod(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            if (int.TryParse(prune, out int pruneDays))
            {
                await user.BanAsync(pruneDays, reason);
            }
            else
            {
                await user.BanAsync(0, reason);
            }

            await Context.Channel.SendMessageAsync($"The communist spy {user.Mention} has been given the ~~ban~~ freedom hammer.");
        }

        [Command("ban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        public async Task BanAsync(string user, string prune = "0", [Remainder] string reason = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await BanAsync(u, prune, reason);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
