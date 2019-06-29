﻿using Discord;
using Discord.Commands;
using Discord.WebSocket;
using FBIBot.Modules.AutoMod;
using FBIBot.Modules.Config;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;

namespace FBIBot
{
    public class CommandHandler
    {
        public const string prefix = "\\";
        public static int argPos = 0;

        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _services;

        public CommandHandler(DiscordSocketClient client, IServiceProvider services)
        {
            _client = client;
            _services = services;

            CommandServiceConfig config = new CommandServiceConfig()
            {
                DefaultRunMode = RunMode.Async
            };
            _commands = new CommandService(config);
        }

        public async Task InitCommandsAsync()
        {
            _client.Connected += SendConnectMessage;
            _client.Disconnected += SendDisconnectError;
            _client.JoinedGuild += SendJoinMessage;
            _client.UserJoined += SendWelcomeMessage;
            _client.MessageReceived += HandleCommandAsync;

            await _commands.AddModulesAsync(Assembly.GetEntryAssembly(), _services);
        }

        private async Task SendConnectMessage()
        {
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync($"{SecurityInfo.botName} is online");
            }
        }

        private async Task SendDisconnectError(Exception e)
        {
            if (Program.isConsole)
            {
                await Console.Out.WriteLineAsync(e.Message);
            }
        }

        private async Task SendJoinMessage(SocketGuild g) => await g.DefaultChannel.SendMessageAsync("Someone called for some democracy and justice?");

        private async Task SendWelcomeMessage(SocketGuildUser u)
        {
            if (await RaidMode.GetVerificationLevelAsync(u.Guild) != null && !u.IsBot)
            {
                await u.SendMessageAsync($":rotating_light: :rotating_light: The FBI of {u.Guild.Name} is currently in Raid Mode. As a result, you may not join the server at this time.:rotating_light: :rotating_light:");
                await RaidMode.AddBlockedUserAsync(u);
                await u.KickAsync("FBI RAID MODE");
                return;
            }

            SocketTextChannel channel = u.Guild.DefaultChannel;
            List<string> messages = new List<string>()
            {
                "Don't even think about it.",
                "**FBI OPEN U**...wait...we'll be back shortly with a warrant.",
                "Where do you think you're going?",
                "Ladies and gentlemen, we got 'em.",
                "Give us a moment while we send a representative into your camera...",
                "You thought it was a normal server, but it was me! THE FBI!",
                "Walk out slowly with your arms i...wait, wrong person.",
                "You want to know how we figured it out?"
            };
            int index = Program.rng.Next(messages.Count);
            await channel.SendMessageAsync($"{u.Mention} {messages[index]}");

            if (await Verify.IsVerifiedAsync(u))
            {
                SocketRole role = await SetVerify.GetVerificationRoleAsync(u.Guild);
                if (role != null && u.Guild.CurrentUser.GetPermissions(u.Guild.DefaultChannel).ManageRoles)
                {
                    await u.AddRoleAsync(role);
                }
            }
            else if (!u.IsBot && await SetVerify.GetVerificationRoleAsync(u.Guild) != null)
            {
                await Verify.SendCaptchaAsync(u);
            }
        }

        private async Task HandleCommandAsync(SocketMessage m)
        {
            if (!(m is SocketUserMessage msg) || (msg.Author.IsBot && msg.Author.Id != _client.CurrentUser.Id))
            {
                return;
            }

            SocketCommandContext context = new SocketCommandContext(_client, msg);
            string _prefix = context.Guild != null ? await Prefix.GetPrefixAsync(context.Guild) : prefix;
            bool isCommand = msg.HasMentionPrefix(_client.CurrentUser, ref argPos) || msg.HasStringPrefix(_prefix, ref argPos);
            if (isCommand)
            {
                var result = await _commands.ExecuteAsync(context, argPos, _services);

                if (!result.IsSuccess && result.Error != CommandError.UnknownCommand)
                {
                    await context.Channel.SendMessageAsync($"Error: {result.ErrorReason}");
                }

                if (msg.Author.IsBot)
                {
                    await msg.DeleteAsync();
                }
            }

            if (msg.Author.IsBot)
            {
                return;
            }

            await AutoModAsync(context, isCommand);
        }

        private async Task AutoModAsync(SocketCommandContext context, bool isCommand)
        {
            if (await Pedophile.IsPedophileAsync(context.Message))
            {
                await new Pedophile(context).ArrestAsync();
            }
            else if (await Spam.IsSpamAsync(context) && !isCommand && await AntiSpam.GetAntiSpamAsync(context.Guild))
            {
                await new Spam(context).WarnAsync();
            }
            else if (await MassMention.IsMassMentionAsync(context) && await AntiMassMention.GetAntiMassMentionAsync(context.Guild))
            {
                await new MassMention(context).WarnAsync();
            }
            else if (await Invite.HasInviteAsync(context) && await AntiInvite.GetAntiInviteAsync(context.Guild))
            {
                await new Invite(context).RemoveAsync();
            }
            else if (await Link.HasLinkAsync(context) && await AntiLink.GetAntiLinkAsync(context.Guild))
            {
                await new Link(context).RemoveAsync();
            }
        }
    }
}
