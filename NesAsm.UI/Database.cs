using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace NesAsm.UI;

public class Database
{
    private readonly string _gameDataFolder;
    private string? _currentGameName;
    private readonly List<Cheat> _currentGameCheats = [];

    public Database(string gameDataFolder)
    {
        _gameDataFolder = gameDataFolder;
    }

    public void ChangeCurrentGame(string gameName)
    {
        PersistActiveCheats();

        _currentGameName = gameName;
        _currentGameCheats.Clear();

        LoadCheatsForCurrentGame();
        LoadActiveCheats();
    }

    public void PersistActiveCheats()
    {
        if (_currentGameName == null) return;

        var activeCheats = _currentGameCheats.Where(c => c.IsActive).Select(c => c.Description);
        var cheatsFolder = Path.Combine(_gameDataFolder, "Cheats");
        Directory.CreateDirectory(cheatsFolder);
        var gameCheatsFilePath = Path.Combine(cheatsFolder, _currentGameName + ".cht");

        File.WriteAllLines(gameCheatsFilePath, activeCheats);
    }

    public void LoadActiveCheats()
    {
        if (_currentGameName == null) return;

        var cheatsFolder = Path.Combine(_gameDataFolder, "Cheats");
        var gameCheatsFilePath = Path.Combine(cheatsFolder, _currentGameName + ".cht");
        if (File.Exists(gameCheatsFilePath))
        {
            var activeCheatDescriptions = new HashSet<string>(File.ReadAllLines(gameCheatsFilePath));
            foreach (var cheat in _currentGameCheats)
            {
                cheat.IsActive = activeCheatDescriptions.Contains(cheat.Description);
            }
        }
    }

    public IEnumerable<(string Description, bool IsActive)> GetCheats()
    {
        return _currentGameCheats.Select(c => (c.Description, c.IsActive));
    }

    public void ToggleCheat(string cheatName, bool isActive)
    {
        var cheat = _currentGameCheats.FirstOrDefault(c => c.Description == cheatName);
        if (cheat != null)
        {
            cheat.IsActive = isActive;

            if (!string.IsNullOrWhiteSpace(cheat.GenieCode))
            {
                var cheatInfo = cheat.DecodeGameGenieCode();
            }
            else
            {

            }
        }
    }

    private void LoadCheatsForCurrentGame()
    {
        // https://github.com/SourMesen/Mesen/blob/master/GUI.NET/Dependencies/MesenCheatDb.xml
        var cheatsFilePath = Path.Combine(_gameDataFolder, "MesenCheatDb.xml");
        if (File.Exists(cheatsFilePath))
        {
            var cheatsDoc = XDocument.Load(cheatsFilePath);

            var cheats = cheatsDoc.Descendants("cheat").Where(e => e.Attribute("gameName")?.Value == _currentGameName);
            if (!cheats.Any()) { }

            foreach (var cheat in cheats)
            {
                var description = cheat.Element("description")?.Value ?? "";
                var genieCode = cheat.Element("genie")?.Value ?? "";
                var address = cheat.Element("address")?.Value ?? "";
                var value = cheat.Element("value")?.Value ?? "";

                _currentGameCheats.Add(new Cheat(description, genieCode, address, value));
            }
        }
    }

    private record Cheat(string Description, string? GenieCode, string? Address, string? Value)
    {
        public bool IsActive { get; set; }

        public (ushort Address, byte Value, byte? Compare) DecodeGameGenieCode()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(GenieCode);

            var normalizedCode = new string(GenieCode
                .Where(character => character != '-' && !char.IsWhiteSpace(character))
                .Select(char.ToUpperInvariant)
                .ToArray());

            if (normalizedCode.Length is not (6 or 8))
            {
                throw new FormatException("A NES Game Genie code must contain 6 or 8 characters.");
            }

            Span<int> code = stackalloc int[normalizedCode.Length];
            const string alphabet = "APZLGITYEOXUKSVN";

            for (var index = 0; index < normalizedCode.Length; index++)
            {
                var value = alphabet.IndexOf(normalizedCode[index]);
                if (value < 0)
                {
                    throw new FormatException($"'{normalizedCode[index]}' is not a valid NES Game Genie character.");
                }

                code[index] = value;
            }

            var address = (ushort)(0x8000
                | ((code[3] & 0x07) << 12)
                | ((code[5] & 0x07) << 8)
                | ((code[4] & 0x08) << 8)
                | ((code[2] & 0x07) << 4)
                | ((code[1] & 0x08) << 4)
                | (code[4] & 0x07)
                | (code[3] & 0x08));

            var valueByte = (normalizedCode.Length == 6)
                ? (byte)(((code[1] & 0x07) << 4) | ((code[0] & 0x08) << 4) | (code[0] & 0x07) | (code[5] & 0x08))
                : (byte)(((code[1] & 0x07) << 4) | ((code[0] & 0x08) << 4) | (code[0] & 0x07) | (code[7] & 0x08));

            byte? compareByte = (normalizedCode.Length == 6)
                ? null
                : (byte)(((code[7] & 0x07) << 4) | ((code[6] & 0x08) << 4) | (code[6] & 0x07) | (code[5] & 0x08));

            return (address, valueByte, compareByte);
        }
    }
}