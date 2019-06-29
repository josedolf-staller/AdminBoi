﻿using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class MassMention
    {
        SocketCommandContext Context;

        public MassMention(SocketCommandContext context) => Context = context;

        public async Task WarnAsync()
        {
            await Context.Channel.SendMessageAsync($"\\warn {Context.User.Mention} 0.5 Big mass mention");
            await Context.Message.DeleteAsync();
        }

        public static async Task<bool> IsMassMentionAsync(SocketCommandContext context)
        {
            int count = context.Message.MentionedRoles.Count + context.Message.MentionedUsers.Count;
            bool hasMassMention = count >= 5;

            return await Task.Run(() => hasMassMention);
        }
    }
}