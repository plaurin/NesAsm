using BigGustave;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;

namespace NesAsm.Analyzers.Visitors;

internal class CharVisitorContext : VisitorContext
{
    private Dictionary<Pixel, (Pixel color, int x, int y)> _mismatchColors = new();
    private List<(string name, string numericValue, string asmValue)> _consts = new();

    public CharVisitorContext(VisitorContext visitorContext, Location importCharAttributeLocation) : base(visitorContext)
    {
        Location = importCharAttributeLocation;
    }

    public Location Location { get; }

    public IEnumerable<(Pixel actualColor, Pixel expectedColor, int x, int y)> MismatchColors => _mismatchColors.Take(10).Select(m => (m.Key, m.Value.color, m.Value.x, m.Value.y));

    public IEnumerable<(string name, string numericValue, string asmValue)> Consts => _consts;

    internal void RecordColorMismatch(Pixel expectedColor, Pixel actualColor, int x, int y) => _mismatchColors[actualColor] = (expectedColor, x, y);

    internal void AddConst(string fieldName, string numericValue, string asmValue)
    {
        _consts.Add((fieldName, numericValue, asmValue));
    }
}