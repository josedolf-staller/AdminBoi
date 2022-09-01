﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using FBIBot.Modules.Mod.ModLog;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static FBIBot.DatabaseManager;

namespace FBIBot.Modules.Mod
{
    public class Free : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("free", "Frees the user from Guantanamo Bay because the Constitution exists")]
        [RequireMod]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireBotPermission(GuildPermission.ManageChannels)]
        public async Task FreeAsync([RequireInvokerHierarchy("free")] SocketGuildUser user)
        {
            SocketRole role = await modRolesDatabase.PrisonerRole.GetPrisonerRoleAsync(Context.Guild);
            List<SocketRole> roles = await modRolesDatabase.UserRoles.GetUserRolesAsync(user);
            if ((role == null || !user.Roles.Contains(role)) && roles.Count == 0)
            {
                await Context.Interaction.RespondAsync($"Our security team has informed us that {user.Nickname ?? user.Username} is not held captive.");
                return;
            }

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(new Color(12, 156, 24))
                .WithDescription($"{user.Mention} has been freed from Guantanamo Bay after a good amount of ~~torture~~ re-education.");

            List<Task> cmds = new()
            {
                modRolesDatabase.Prisoners.RemovePrisonerAsync(user),
                Context.Interaction.RespondAsync(embed: embed.Build()),
                FreeModLog.SendToModLogAsync(Context.User as SocketGuildUser, user)
            };
            if (roles.Count > 0)
            {
                cmds.AddRange(new List<Task>()
                {
                    user.AddRolesAsync(roles),
                    modRolesDatabase.UserRoles.RemoveUserRolesAsync(user)
                });
            }
            if (role != null)
            {
                cmds.Add(user.RemoveRoleAsync(role));
            }
            await Task.WhenAll(cmds);

            if (!await modRolesDatabase.Prisoners.HasPrisoners(Context.Guild))
            {
                SocketTextChannel channel = await modRolesDatabase.PrisonerChannel.GetPrisonerChannelAsync(Context.Guild);

                cmds = new()
                {
                    modRolesDatabase.PrisonerChannel.RemovePrisonerChannelAsync(Context.Guild),
                    modRolesDatabase.PrisonerRole.RemovePrisonerRoleAsync(Context.Guild)
                };
                if (channel != null)
                {
                    cmds.Add(channel.DeleteAsync());
                }
                if (role != null)
                {
                    cmds.Add(role.DeleteAsync());
                }
                await Task.WhenAll(cmds);
            }
        }
    }
}