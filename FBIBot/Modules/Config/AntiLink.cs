﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AntiLink : ModuleBase<SocketCommandContext>
    {
        [Command("anti-link")]
        [Alias("antilink")]
        [RequireAdmin]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiLinkAsync(string enable)
        {
            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await GetAntiLinkAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-link is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }

            List<Task> cmds = new List<Task>()
            {
                Context.Channel.SendMessageAsync($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} messages linking external, communist media.")
            };
            if (isEnable)
            {
                cmds.Add(SetAntiLinkAsync(Context.Guild));
            }
            else
            {
                cmds.Add(RemoveAntiLinkAsync(Context.Guild));
            }

            await Task.WhenAll(cmds);
        }

        public static async Task<bool> GetAntiLinkAsync(SocketGuild g)
        {
            bool isAntiLink = false;

            string getSpam = "SELECT guild_id FROM AntiLink WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getSpam, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = await cmd.ExecuteReaderAsync();
                isAntiLink = await reader.ReadAsync();
                reader.Close();
            }

            return isAntiLink;
        }

        public static async Task SetAntiLinkAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiLink (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiLink WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveAntiLinkAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiLink WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}
