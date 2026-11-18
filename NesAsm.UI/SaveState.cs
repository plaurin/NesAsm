using System;
using System.IO;
using System.Linq;

namespace NesAsm.UI;

public class SaveState
{
    private readonly byte[] _state;

    public SaveState(string filePath, ReadOnlySpan<byte> saveData)
    {
        FilePath = filePath;

        var parentTicks = BitConverter.ToInt64(saveData.Slice(0x04, 8));
        ParentTimestamp = parentTicks != 0 ? new DateTime(parentTicks) : null;
        Timestamp = new DateTime(BitConverter.ToInt64(saveData.Slice(0x0C, 8)));
        FrameIndex = BitConverter.ToInt32(saveData.Slice(0x14, 4));
        var stateLength = BitConverter.ToInt32(saveData.Slice(0x18, 4));
        _state = saveData.Slice(0x1C, stateLength).ToArray();
    }

    public SaveState(string filePath, DateTime? parentTimestamp, DateTime timestamp, int frameIndex, byte[] state)
    {
        FilePath = filePath;
        ParentTimestamp = parentTimestamp;
        Timestamp = timestamp;
        FrameIndex = frameIndex;
        _state = state;
    }

    public string FilePath { get; }
    public DateTime? ParentTimestamp { get; }
    public DateTime Timestamp { get; }
    public int FrameIndex { get; }
    public ReadOnlySpan<byte> State => _state;

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

    public byte[] GenerateSaveData()
    {

        var saveData =
        // Header (4 bytes)
            "STT1"u8.ToArray()
        // Previous Save State Timestamp (8 bytes)
            .Concat(ParentTimestamp != null ? BitConverter.GetBytes(ParentTimestamp.Value.Ticks) : new byte[8])
        // Current Save State Timestamp (8 bytes)
            .Concat(BitConverter.GetBytes(Timestamp.Ticks))
        // Frame index (4 bytes)
            .Concat(BitConverter.GetBytes(FrameIndex))
        // State length (4 bytes)
            .Concat(BitConverter.GetBytes(_state.Length))
        // State (variable length)
            .Concat(_state)
            .ToArray();

        return saveData;
    }
}

