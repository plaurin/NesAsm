using Microsoft.CodeAnalysis;

namespace NesAsm.Analyzers.Visitors;

internal class CharVisitorContext : VisitorContext
{
    public CharVisitorContext(VisitorContext visitorContext, Location importCharAttributeLocation) : base(visitorContext)
    {
        Location = importCharAttributeLocation;
    }

    public Location Location { get; }
}