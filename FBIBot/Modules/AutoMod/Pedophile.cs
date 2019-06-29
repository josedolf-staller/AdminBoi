﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FBIBot.Modules.AutoMod
{
    public class Pedophile
    {
        readonly SocketCommandContext Context;

        public Pedophile(SocketCommandContext context) => Context = context;

        public async Task ArrestAsync()
        {
            await Context.Message.DeleteAsync();
            await Context.User.SendMessageAsync("You want to explain yourself?");
            await Context.Channel.SendMessageAsync($"\\arrest {Context.User.Mention} 5");
        }

        public static async Task<bool> IsPedophileAsync(SocketUserMessage msg)
        {
            bool isPedophile = false;

            List<string> bad = new List<string>()
            {
                "i like",
                "i love"
            };
            List<string> stillBad = new List<string>()
            {
                "kids",
                "children",
                "little kids",
                "little children"
            };
            foreach (string b in bad)
            {
                foreach (string s in stillBad)
                {
                    if (msg.Content.ToLower().Contains($"{b} {s}"))
                    {
                        isPedophile = true;
                        break;
                    }
                }
                if (isPedophile)
                {
                    break;
                }
            }

            return await Task.Run(() => isPedophile);
        }
    }
}