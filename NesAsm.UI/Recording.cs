using System;
using System.Linq;

namespace NesAsm.UI;

public class Recording
{
    private readonly byte[] _state;

    public Recording(string filePath, ReadOnlySpan<byte> saveData)
    {
        FilePath = filePath;

        Timestamp = new DateTime(BitConverter.ToInt64(saveData.Slice(0x04, 8)));
        StartFrameIndex = BitConverter.ToInt32(saveData.Slice(0x0C, 4));
        EndFrameIndex = BitConverter.ToInt32(saveData.Slice(0x10, 4));
        var stateLength = BitConverter.ToInt32(saveData.Slice(0x14, 4));
        _state = saveData.Slice(0x18, stateLength).ToArray();
        var inputLength = BitConverter.ToInt32(saveData.Slice(0x18 + stateLength, 4));
        Inputs = saveData.Slice(0x18 + stateLength + 4, inputLength).ToArray();
    }

    public Recording(string filePath, DateTime timestamp, byte[] state, int startFrameIndex, int endFrameIndex, byte[] inputs, int? index = null)
    {
        FilePath = filePath;
        Index = index;
        Timestamp = timestamp;
        _state = state;
        StartFrameIndex = startFrameIndex;
        EndFrameIndex = endFrameIndex;
        Inputs = inputs;
    }

    public string FilePath { get; }
    public int? Index { get; }
    public ReadOnlySpan<byte> State => _state;
    public DateTime Timestamp { get; }
    public int StartFrameIndex { get; }
    public int EndFrameIndex { get; }
    public byte[] Inputs { get; }

    internal byte[] GenerateRecordingData()
    {
        var saveData =
        // Header (4 bytes)
            "MOV1"u8.ToArray()
        // Current Timestamp (8 bytes)
            .Concat(BitConverter.GetBytes(Timestamp.Ticks))
        // Start Frame index (4 bytes)
            .Concat(BitConverter.GetBytes(StartFrameIndex))
        // End Frame index (4 bytes)
            .Concat(BitConverter.GetBytes(EndFrameIndex))
        // State length (4 bytes)
            .Concat(BitConverter.GetBytes(_state.Length))
        // State (variable length)
            .Concat(_state)
        // Input length (4 bytes)
            .Concat(BitConverter.GetBytes(Inputs.Length))
        // Input (variable length)
            .Concat(Inputs)
            .ToArray();

        return saveData;
    }
}