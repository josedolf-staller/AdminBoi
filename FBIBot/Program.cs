﻿using Discord;
using Discord.WebSocket;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FBIBot
{
    public class Program
    {
        private DiscordSocketConfig _config;
        private DiscordSocketClient _client;
        private CommandHandler _handler;

        public static readonly Random rng = new Random();

        public static readonly bool isConsole = Console.OpenStandardInput(1) != Stream.Null;

        static void Main(string[] args) => new Program().StartAsync(args).GetAwaiter().GetResult();

        public async Task StartAsync(string[] args)
        {
            if (isConsole)
            {
                Console.Title = SecurityInfo.botName;
            }

            bool isRunning = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1;
            if (isRunning)
            {
                await Task.Delay(1000);
                isRunning = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() > 1;

                if (isRunning)
                {
                    MessageBox.Show("Program is already running", SecurityInfo.botName);
                    return;
                }
            }

            _config = new DiscordSocketConfig
            {
                AlwaysDownloadUsers = false
            };

            _client = new DiscordSocketClient(_config);

            await _client.LoginAsync(TokenType.Bot, SecurityInfo.token);
            await _client.StartAsync();

            await _client.SetGameAsync("@The FBI help", null, ActivityType.Listening);

            IServiceProvider _services = new ServiceCollection().BuildServiceProvider();

            _handler = new CommandHandler(_client, _services);
            Task initCmd = _handler.InitCommandsAsync();

            if (isConsole)
            {
                Console.WriteLine($"{SecurityInfo.botName} has finished loading");
            }

            await initCmd;
            await InitSqlite();

            await Task.Delay(-1);
        }

        async Task InitSqlite()
        {
            using (SqliteConnection cn = new SqliteConnection("Filename=Verification.db"))
            {
                cn.Open();

                List<Task> cmds = new List<Task>();
                using (SqliteCommand cmd = new SqliteCommand("CREATE TABLE IF NOT EXISTS Users (id INTEGER NOT NULL UNIQUE PRIMARY KEY AUTOINCREMENT, user_id TEXT NOT NULL UNIQUE);", cn))
                {
                    cmds.Add(cmd.ExecuteNonQueryAsync());
                }
                using (SqliteCommand cmda = new SqliteCommand("CREATE TABLE IF NOT EXISTS Captcha (id INTEGER NOT NULL UNIQUE PRIMARY KEY, captcha TEXT NOT NULL);", cn))
                {
                    cmds.Add(cmda.ExecuteNonQueryAsync());
                }
                using (SqliteCommand cmdb = new SqliteCommand("CREATE TABLE IF NOT EXISTS Verified (id INTEGER NOT NULL UNIQUE PRIMARY KEY);", cn))
                {
                    cmds.Add(cmdb.ExecuteNonQueryAsync());
                }

                await Task.WhenAll(cmds);
            }
        }
    }
}