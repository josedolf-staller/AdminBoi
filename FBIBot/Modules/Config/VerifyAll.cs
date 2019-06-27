﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class VerifyAll : ModuleBase<SocketCommandContext>
    {
        [Command("verifyall")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        public async Task VerifyAllAsync()
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            SocketRole role = await Modules.Config.SetVerify.GetVerificationRoleAsync(Context.Guild);
            if (role == null)
            {
                await Context.Channel.SendMessageAsync("Our intelligence tells us that there is no role to give to verified members.");
                return;
            }

            foreach (SocketGuildUser user in Context.Guild.Users)
            {
                if (!user.IsBot)
                {
                    await user.AddRoleAsync(role);
                }
            }

            await Context.Channel.SendMessageAsync("All current members have been given citizenship.");
        }
    }
}
