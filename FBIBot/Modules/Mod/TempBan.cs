﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod
{
    public class TempBan : ModuleBase<SocketCommandContext>
    {
        [Command("tempban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireOwner()]
        public async Task TempBanAsync(SocketGuildUser user, string length, [Remainder] string reason = null)
        {
            if (!double.TryParse(length, out double days))
            {
                await Context.Channel.SendMessageAsync($"Unfortunately, {length} is not a valid prison sentence length.");
            }

            await user.BanAsync(0, reason);
            await Context.Channel.SendMessageAsync($"The communist spy {user.Mention} has been deported to Guatemala Bay for {length} days.");

            await Task.Delay((int)(days * 24 * 60 * 60 * 1000));
            await Context.Guild.RemoveBanAsync(user);
        }

        [Command("tempban")]
        [RequireBotPermission(GuildPermission.BanMembers)]
        [RequireOwner()]
        public async Task TempBanAsync(string user, string length, [Remainder] string reason = null)
        {
            SocketGuildUser u;
            if (ulong.TryParse(user, out ulong userID) && (u = Context.Guild.GetUser(userID)) != null)
            {
                await TempBanAsync(u, length, reason);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given user does not exist.");
        }
    }
}
