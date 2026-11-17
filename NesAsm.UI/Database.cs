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
    private readonly List<SaveStateInfo> _currentGameSaves = [];
    private byte[] _currentSaveStateTimestamp = new byte[8];
    private readonly List<Cheat> _currentGameCheats = [];

    public Database(string gameDataFolder)
    {
        _gameDataFolder = gameDataFolder;
    }

    private string SavedStatesFolder => Path.Combine(_gameDataFolder, "SavedStates");
    private string CheatsFolder => Path.Combine(_gameDataFolder, "Cheats");

    public void ChangeCurrentGame(string gameName)
    {
        PersistActiveCheats();

        _currentGameName = gameName;
        _currentGameCheats.Clear();

        LoadCheatsForCurrentGame();
        LoadActiveCheats();
        LoadSaveStates();
    }

    private void LoadSaveStates()
    {
        _currentGameSaves.Clear();
        _currentGameSaves.AddRange(Directory.GetFiles(SavedStatesFolder, $"{_currentGameName}*.sst").Select(f => new SaveStateInfo(f)));
    }

    private record SaveStateInfo(string FilePath)
    {
        public int? Index
        {
            get
            {
                var filePart = Path.GetFileNameWithoutExtension(FilePath).Split('_');
                if (filePart.Length == 2 && int.TryParse(filePart[1], out var index))
                    return index;

                return null;
            }
        }
    }

    public void SaveState(byte[] state, int frame)
    {
        var nextIndex = _currentGameSaves.Max(s => s.Index) + 1;
        SaveState(state, frame, $"{nextIndex:D3}");
    }

    public void SaveLastStateOnExit(byte[] state, int frame) => SaveState(state, frame);

    private void SaveState(byte[] state, int frame, string? suffix = null)
    {
        if (_currentGameName == null) return;
        Directory.CreateDirectory(SavedStatesFolder);

        // Name game_N3.sst or  game.sst (last)
        var saveName = string.IsNullOrWhiteSpace(suffix) ? _currentGameName : $"{_currentGameName}_{suffix}";
        var gameStateFilePath = Path.Combine(SavedStatesFolder, $"{saveName}.sst");

        var now = DateTime.Now;
        var saveTimestamp = new byte[] { (byte)(now.Year / 256), (byte)(now.Year % 256), (byte)now.Month, (byte)now.Day, (byte)now.Hour, (byte)now.Minute, (byte)now.Second, (byte)now.Microsecond };

        var thumpnail = new byte[0]; // TODO

        var saveData =
        // Header (4 bytes)
            "STT1"u8.ToArray()
        // Previous Save State Timestamp (8 bytes)
            .Concat(_currentSaveStateTimestamp)
        // Current Save State Timestamp (8 bytes)
            .Concat(saveTimestamp)
        // Game Time (3 bytes, HMS) Frame / 60
            .Concat(new byte[] { (byte)(frame / 216000), (byte)((frame / 3600) % 60), (byte)((frame / 60) % 60) })
        // State length (2 bytes)
            .Concat(new byte[] { (byte)(state.Length / 256), (byte)(state.Length % 256) })
        // State (variable length)
            .Concat(state)
        // Thumbnail length (2 bytes)
            .Concat(new byte[] { (byte)(thumpnail.Length / 256), (byte)(thumpnail.Length % 256) })
        // Thumbnail (variable length) TODO
            .Concat(thumpnail)
            .ToArray();

        File.WriteAllBytes(gameStateFilePath, saveData);
        _currentGameSaves.Add(new SaveStateInfo(gameStateFilePath));
        _currentSaveStateTimestamp = saveTimestamp;
    }

    public void PersistActiveCheats()
    {
        if (_currentGameName == null) return;

        var activeCheats = _currentGameCheats.Where(c => c.IsActive).Select(c => c.Description);
        Directory.CreateDirectory(CheatsFolder);
        var gameCheatsFilePath = Path.Combine(CheatsFolder, _currentGameName + ".cht");

        File.WriteAllLines(gameCheatsFilePath, activeCheats);
    }

    public void LoadActiveCheats()
    {
        if (_currentGameName == null) return;

        var gameCheatsFilePath = Path.Combine(CheatsFolder, _currentGameName + ".cht");
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

    public IEnumerable<(ushort Address, byte Value, byte? Compare)> GetActiveCheatsInfo()
    {
        return _currentGameCheats
            .Where(c => c.IsActive)
            .Select(c => c.Info);
    }

    public void ToggleCheat(string cheatName, bool isActive)
    {
        var cheat = _currentGameCheats.FirstOrDefault(c => c.Description == cheatName);
        if (cheat != null)
        {
            cheat.IsActive = isActive;
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

        public (ushort Address, byte Value, byte? Compare) Info
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(GenieCode))
                {
                    return DecodeGameGenieCode();
                }
                else
                {
                    ushort address = 0;
                    byte value = 0;
                    byte? compare = null;
                    if (!string.IsNullOrWhiteSpace(Address) && !string.IsNullOrWhiteSpace(Value))
                    {
                        address = Convert.ToUInt16(Address, 16);
                        value = Convert.ToByte(Value, 16);
                    }
                    return (address, value, compare);
                }
            }
        }

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