using System;
using System.IO;
using System.Text;
using System.Text.Json;

namespace Fm2ndParser
{
    class Program
    {
        static void Main(string[] args)
        {
            var parser = new PlayerParse();
            var player = parser.Parse(@"C:\git\menaduro-bitellonico\Character  1.player");
            var json = JsonSerializer.Serialize(player);
            File.WriteAllText("player.json", json);

        }
    }
}
