namespace NesAsm.Recompiler;

public static class Input
{
    public static void LoadCustomLabels(string outputPath)
    {
        var labelsFilePath = Path.Combine(outputPath, "Labels.txt");
        if (File.Exists(labelsFilePath))
        {
            var lines = File.ReadAllLines(labelsFilePath);
            foreach (var line in lines)
            {
                var address = line[0..4];
                var label = line[5..].Trim();
                if (int.TryParse(address, System.Globalization.NumberStyles.HexNumber, null, out var addr))
                {
                    Labels.AddMemoryLabel(addr, label);
                }
            }
        }
        else
        {
            File.Create(labelsFilePath);
        }
    }

    public static IEnumerable<ushort> LoadDynamicDispatchs(string outputPath)
    {
        var dispatchs = new List<ushort>();

        var labelsFilePath = Path.Combine(outputPath, "Dispatchs.txt");
        if (File.Exists(labelsFilePath))
        {
            var lines = File.ReadAllLines(labelsFilePath);
            foreach (var line in lines)
            {
                var sourceAddress = line[0..4];
                var targetAddress = line[5..].Trim();
                if (int.TryParse(targetAddress, System.Globalization.NumberStyles.HexNumber, null, out var addr))
                {
                    dispatchs.Add((ushort)addr);
                }
            }
        }
        else
        {
            File.Create(labelsFilePath);
        }

        return dispatchs;
    }

    public static void LoadSymbols(string outputPath)
    {
        var labelsFilePath = Path.Combine(outputPath, "Symbols.sym");
        if (File.Exists(labelsFilePath))
        {
            var lines = File.ReadAllLines(labelsFilePath);
            foreach (var line in lines)
            {
                if (line.Trim().StartsWith('#') || string.IsNullOrWhiteSpace(line))
                {
                    // Skip
                }
                else
                {
                    var parts = line.Split(' ');
                    var address = parts[0]!;
                    var label = parts[1];
                    var type = parts[2];

                    if (type == "func" || type == "ram")
                        if (int.TryParse(address, System.Globalization.NumberStyles.HexNumber, null, out var addr))
                            if (string.IsNullOrWhiteSpace(Labels.GetLabel(addr)))
                                Labels.AddMemoryLabel(addr, label);
                }
            }
        }
    }
}