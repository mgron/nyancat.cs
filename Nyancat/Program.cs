﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Threading;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nyancat.Graphics;

namespace Nyancat
{
    [Command(Name = "nyancat", FullName = "nyancat", Description = "Terminal nyancat runner")]
    [VersionOptionFromMember(MemberName = nameof(GetVersion))]
    class Program
    {

        [Option(Description = "Do not display the timer", ShortName = "n")]
        public bool NoCounter { get; set; } = false;

        [Option(Description = "Do not set the titlebar text", ShortName = "s")]
        public bool NoTitle { get; set; } = false;

        [Option(Description = "Display the requested number of frames, then quit", ShortName = "f")]
        public int Frames { get; set; } = int.MaxValue;

        public void OnExecute()
        {
            var host = new ConsoleGraphicsHostBuilder()
                .ConfigureServices((context, services) =>
                {
                    services.Configure<NyancatSceneOptions>(options =>
                    {
                        options.ShowCounter = !NoTitle;
                        options.ShowCounter = !NoCounter;
                        options.Frames = Frames;
                    });

                    services.AddSingleton<IScene, NyancatScene>();
                })
                .Build();

            host.Run();
        }

        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}
