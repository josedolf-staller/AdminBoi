﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using System.Threading.Tasks;

namespace FBIBot.Modules.Config
{
    public class AntiSpam : ModuleBase<SocketCommandContext>
    {
        [Command("antispam")]
        [RequireBotPermission(GuildPermission.ManageMessages)]
        public async Task AntiSpamAsync(string enable)
        {
            SocketGuildUser u = Context.Guild.GetUser(Context.User.Id);
            if (!await VerifyUser.IsAdmin(u))
            {
                await Context.Channel.SendMessageAsync("You are not a local director of the FBI and cannot use this command.");
                return;
            }

            bool isEnable = enable == "true" || enable == "enable";
            bool isEnabled = await GetAntiSpamAsync(Context.Guild);

            if (isEnable == isEnabled)
            {
                await Context.Channel.SendMessageAsync($"Our security team has informed us that anti-spam is already {(isEnabled ? "enabled" : "disabled")}.");
                return;
            }
            
            if (isEnable)
            {
                await SetAntiSpamAsync(Context.Guild);
            }
            else
            {
                await RemoveAntiSpamAsync(Context.Guild);
            }

            await Context.Channel.SendMessageAsync($"We are now {(isEnable ? "permitted to remove" : "prohibited from removing")} spammy, anti-American messages.");
        }

        public static async Task<bool> GetAntiSpamAsync(SocketGuild g)
        {
            bool isAntiSpam = false;

            string getSpam = "SELECT guild_id FROM AntiSpam WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(getSpam, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());

                SqliteDataReader reader = cmd.ExecuteReader();
                isAntiSpam = reader.Read();
                reader.Close();
            }

            return await Task.Run(() => isAntiSpam);
        }

        public static async Task SetAntiSpamAsync(SocketGuild g)
        {
            string insert = "INSERT INTO AntiSpam (guild_id) SELECT @guild_id\n" +
                "WHERE NOT EXISTS (SELECT 1 FROM AntiSpam WHERE guild_id = @guild_id);";

            using (SqliteCommand cmd = new SqliteCommand(insert, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }

        public static async Task RemoveAntiSpamAsync(SocketGuild g)
        {
            string delete = "DELETE FROM AntiSpam WHERE guild_id = @guild_id;";
            using (SqliteCommand cmd = new SqliteCommand(delete, Program.cnConfig))
            {
                cmd.Parameters.AddWithValue("@guild_id", g.Id.ToString());
                await cmd.ExecuteNonQueryAsync();
            }
        }
    }
}