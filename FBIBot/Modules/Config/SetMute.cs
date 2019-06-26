﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class SetMute : ModuleBase<SocketCommandContext>
    {
        public async Task SetMuteAsync()
        {
            if (GetMuteRole(Context.Guild) == null)
            {
                await Context.Channel.SendMessageAsync("Our intelligence team has informed us that you already don't have a muted role.");
            }
            await RemoveMuteRoleAsync(Context.Guild);
            await Context.Channel.SendMessageAsync($"You no longer have a muted role. What a shame.");
        }

        [Command("setmute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireOwner()]
        public async Task SetMuteAsync(SocketRole role)
        {
            await SetMuteRoleAsync(role, Context.Guild);
            await Context.Channel.SendMessageAsync($"All who commit treason will now receive the {role.Name} role.");
        }

        [Command("setmute")]
        [RequireBotPermission(GuildPermission.ManageRoles)]
        [RequireOwner()]
        public async Task SetMuteAsync(string role)
        {
            SocketRole r;
            if (ulong.TryParse(role, out ulong roleID) && (r = Context.Guild.GetRole(roleID)) != null)
            {
                await SetMuteAsync(r);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given role does not exist.");
        }

        public static async Task<SocketRole> GetMuteRole(SocketGuild g)
        {
            SocketRole role = null;

            string getRole = "SELECT role_id FROM Muted WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getRole, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id);

                SqliteDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    ulong roleID = ulong.Parse(reader["role_id"].ToString());
                    role = g.GetRole(roleID);
                }
            }

            return await Task.Run(() => role);
        }

        public static async Task SetMuteRoleAsync(SocketRole role, SocketGuild g)
        {
            string update = "UPDATE Muted SET role_id = @role_id WHERE guild_id = @guild_id;";
            string insert = "INSERT INTO Muted (guild_id, role_id) SELECT @guild_id, @role_id WHERE (Select Changes() = 0);";

            using (SqliteCommand cmd = new SqliteCommand(update + insert, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveMuteRoleAsync(SocketGuild g)
        {
            string delete = "DELETE FROM Muted WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}