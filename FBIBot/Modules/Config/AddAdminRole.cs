﻿using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AddAdminRole : ModuleBase<SocketCommandContext>
    {
        [Command("add-adminrole")]
        [RequireOwner()]
        public async Task AddAdminRoleAsync(SocketRole role)
        {
            if ((await GetAdminRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Members with the {role.Name} role are already local directors of the FBI.");
                return;
            }

            await AddAdminAsync(role, Context.Guild);
            if ((await AddModRole.GetModRolesAsync(Context.Guild)).Contains(role))
            {
                await Context.Channel.SendMessageAsync($"Members with the {role.Name} role have been promoted to local directors of the FBI.");
            }
            else
            {
                await Context.Channel.SendMessageAsync($"Members with the {role.Name} role are now local directors of the FBI.");
            }
        }

        [Command("add-adminrole")]
        [RequireOwner()]
        public async Task AddAdminRoleAsync(string role)
        {
            SocketRole r;
            if (ulong.TryParse(role, out ulong roleID) && (r = Context.Guild.GetRole(roleID)) != null)
            {
                await AddAdminRoleAsync(r);
                return;
            }
            await Context.Channel.SendMessageAsync("Our intelligence team has informed us that the given role does not exist.");
        }

        public static async Task<List<SocketRole>> GetAdminRolesAsync(SocketGuild g)
        {
            List<SocketRole> roles = new List<SocketRole>();

            string getRoles = "SELECT role_id FROM Admins WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getRoles, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    ulong roleID = ulong.Parse(reader["role_id"].ToString());
                    SocketRole role = g.GetRole(roleID);
                    if (role != null)
                    {
                        roles.Add(role);
                    }
                }
                reader.Close();
            }

            return await Task.Run(() => roles);
        }

        public static async Task AddAdminAsync(SocketRole role, SocketGuild g)
        {
            string insert = "INSERT INTO Admins (guild_id, role_id) SELECT @guild_id, @role_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM Admins WHERE guild_id = @guild_id AND role_id = @role_id));";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnModRoles))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                cmd.Parameters.AddWithValue("@role_id", role.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}