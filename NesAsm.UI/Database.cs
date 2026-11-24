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
    private readonly List<SaveState> _currentGameSaves = [];
    private DateTime? _currentSaveStateTimestamp = null;
    private readonly List<Recording> _currentGameRecordings = [];
    private readonly List<Cheat> _currentGameCheats = [];

    public Database(string gameDataFolder)
    {
        _gameDataFolder = gameDataFolder;
    }

    private string SavedStatesFolder => Path.Combine(_gameDataFolder, "SavedStates");
    private string RecordingsFolder => Path.Combine(_gameDataFolder, "Recordings");
    private string CheatsFolder => Path.Combine(_gameDataFolder, "Cheats");

    public void ChangeCurrentGame(string gameName)
    {
        PersistActiveCheats();

        _currentGameName = gameName;
        _currentGameCheats.Clear();
        _currentGameSaves.Clear();
        _currentGameRecordings.Clear();

        LoadCheatsForCurrentGame();
        LoadActiveCheats();
        LoadSaveStates();
    }

    // SaveStates

    private void LoadSaveStates()
    {
        _currentGameSaves.AddRange(Directory
            .GetFiles(SavedStatesFolder, $"{_currentGameName}*.sst")
            .Select(f => new SaveState(f, File.ReadAllBytes(f))));
    }

    public IEnumerable<SaveState> SaveStates => _currentGameSaves;

    public byte[]? GetLastSaveState()
    {
        var savedStateInfos = _currentGameSaves.Where(s => s.Index == null);
        if (savedStateInfos.Count() > 1) { }
        else if (savedStateInfos.Any())
        {
            return savedStateInfos.Single().State.ToArray();
        }

        return null;
    }

    public void SaveGameState(byte[] state, int frameIndex, byte[] recordingState, byte[] recordingInputs)
    {
        var nextIndex = (_currentGameSaves.Max(s => s.Index) ?? 0) + 1;
        SaveGameState(state, frameIndex, recordingState, recordingInputs, index: nextIndex);
    }

    public void SaveLastStateOnExit(byte[] state, int frameIndex, byte[] recordingState, byte[] recordingInputs) => SaveGameState(state, frameIndex, recordingState, recordingInputs, index: null);

    private void SaveGameState(byte[] state, int frameIndex, byte[] recordingState, byte[] recordingInputs, int? index = null)
    {
        if (_currentGameName == null) return;
        Directory.CreateDirectory(SavedStatesFolder);

        // Name game_N3.sst or  game.sst (last)
        var saveName = index == null ? _currentGameName : $"{_currentGameName}_{index:D3}";
        var gameStateFilePath = Path.Combine(SavedStatesFolder, $"{saveName}.sst");

        var saveState = new SaveState(gameStateFilePath, _currentSaveStateTimestamp, DateTime.Now, frameIndex, state);

        File.WriteAllBytes(gameStateFilePath, saveState.GenerateSaveData());
        _currentGameSaves.Add(saveState);
        _currentSaveStateTimestamp = saveState.Timestamp;

        // Save File Recording
        var saveRecordingFilePath = Path.Combine(SavedStatesFolder, $"{saveName}.rmv");
        var recording = new Recording(saveRecordingFilePath, DateTime.Now, recordingState, 0, frameIndex, recordingInputs, index);
        File.WriteAllBytes(saveRecordingFilePath, recording.GenerateRecordingData());
    }

    // Recordings

    private byte[]? _recordingState;
    private int? _recordingFrameIndex;

    public int? GetRecordingFrameIndex() => _recordingFrameIndex;

    public void StartRecording(byte[] state, int frameIndex)
    {
        _recordingState = state;
        _recordingFrameIndex = frameIndex;
    }

    public void StopRecording(byte[] inputs, int endFrameIndex)
    {
        if (_currentGameName == null || _recordingState == null || _recordingFrameIndex == null) return;
        Directory.CreateDirectory(RecordingsFolder);

        var nextIndex = _currentGameRecordings.Any() ? _currentGameRecordings.Max(s => s.Index) + 1 : 1;
        var recordingFilePath = Path.Combine(RecordingsFolder, $"{_currentGameName}_{nextIndex:D3}.rmv");

        var recording = new Recording(recordingFilePath, DateTime.Now, _recordingState, _recordingFrameIndex.Value, endFrameIndex, inputs, nextIndex);

        File.WriteAllBytes(recordingFilePath, recording.GenerateRecordingData());
        _currentGameRecordings.Add(recording);

        _recordingState = null;
        _recordingFrameIndex = null;
    }

    public IEnumerable<Recording> Recordings
    {
        get
        {
            if (!_currentGameRecordings.Any())
            {
                _currentGameRecordings.AddRange(Directory
                    .GetFiles(RecordingsFolder, $"{_currentGameName}*.rmv")
                    .Select(f => new Recording(f, File.ReadAllBytes(f))));
            }

            return _currentGameRecordings;
        }
    }

    public Recording? GetSaveStateRecording(SaveState saveState)
    {
        var recordingFilePath = Path.ChangeExtension(saveState.FilePath, "rmv");
        if (File.Exists(recordingFilePath))
        {
            return new Recording(recordingFilePath, File.ReadAllBytes(recordingFilePath));
        }

        return null;
    }

    // Cheats

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