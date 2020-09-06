﻿using Discord;
using Discord.WebSocket;
using FBIBot.Modules.Config;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FBIBot.Modules.Mod.ModLog
{
    public static class VerifyAllModLog
    {
        public static async Task SendToModLogAsync(SocketGuildUser invoker)
        {
            await ModLogBase.SendToModLogAsync(
                new ModLogBase.ModLogInfo(
                    new ModLogBase.ModLogInfo.RequiredInfo(
                        invoker,
                        new Color(255, 255, 255),
                        "Verify All Users",
                        "Running"
                    )
                )
            );
        }

        public static async Task<bool> SetStateAsync(SocketGuild g, ulong id, string state)
        {
            IUserMessage msg = await ModLogManager.GetModLogAsync(g, id);
            if (msg == null || msg.Embeds.Count == 0)
            {
                return false;
            }
            EmbedBuilder e = msg.Embeds.ToList()[0].ToEmbedBuilder();
            List<EmbedFieldBuilder> fields = e.Fields.ToList();

            EmbedBuilder embed = new EmbedBuilder()
                .WithColor(e.Color ?? SecurityInfo.botColor)
                .WithTitle($"Federal Bureau of Investigation - Log {id}")
                .WithCurrentTimestamp();

            EmbedFieldBuilder field = e.Fields.FirstOrDefault(x => x.Name == "Verify All Users");
            if (field == null)
            {
                return false;
            }
            int index = fields.IndexOf(field);
            fields.Remove(field);

            field.WithValue(state);
            fields.Insert(index, field);
            embed.WithFields(fields);

            await msg.ModifyAsync(x => x.Embed = embed.Build());

            return true;
        }
    }
}