// Reference : https://github.com/EliotJones/BigGustave
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace BigGustave.PngUtil
{
    /// <summary>
    /// A PNG image. Call <see cref="Open(byte[],IChunkVisitor)"/> to open from file or bytes.
    /// </summary>
    public class Png
    {
        private readonly RawPngData data;
        private readonly bool hasTransparencyChunk;

        /// <summary>
        /// The header data from the PNG image.
        /// </summary>
        public ImageHeader Header { get; }

        /// <summary>
        /// The width of the image in pixels.
        /// </summary>
        public int Width => Header.Width;

        /// <summary>
        /// The height of the image in pixels.
        /// </summary>
        public int Height => Header.Height;

        /// <summary>
        /// Whether the image has an alpha (transparency) layer.
        /// </summary>
        public bool HasAlphaChannel => (Header.ColorType & ColorType.AlphaChannelUsed) != 0 || hasTransparencyChunk;

        internal Png(ImageHeader header, RawPngData data, bool hasTransparencyChunk)
        {
            Header = header;
            this.data = data ?? throw new ArgumentNullException(nameof(data));
            this.hasTransparencyChunk = hasTransparencyChunk;
        }

        /// <summary>
        /// Get the pixel at the given column and row (x, y).
        /// </summary>
        /// <remarks>
        /// Pixel values are generated on demand from the underlying data to prevent holding many items in memory at once, so consumers
        /// should cache values if they're going to be looped over many time.
        /// </remarks>
        /// <param name="x">The x coordinate (column).</param>
        /// <param name="y">The y coordinate (row).</param>
        /// <returns>The pixel at the coordinate.</returns>
        public Pixel GetPixel(int x, int y) => data.GetPixel(x, y);

        [SuppressMessage("MicrosoftCodeAnalysisCorrectness", "RS1035:Do not use APIs banned for analyzers", Justification = "<Pending>")]
        public static Png Open(string filename)
        {
            using (var stream = File.OpenRead(filename))
            {
                return PngOpener.Open(stream);
            }
        }
    }
}
