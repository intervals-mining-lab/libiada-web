namespace Libiada.Web.Helpers;

/// <summary>
/// The file helper.
/// </summary>
public static class FileHelper
{
    /// <summary>
    /// The read file stream.
    /// </summary>
    /// <param name="file">
    /// The file.
    /// </param>
    /// <returns>
    /// The <see cref="Stream"/>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if file is null or empty.
    /// </exception>
    public static Stream GetFileStream(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            throw new ArgumentNullException(nameof(file), "File is null or empty.");
        }

        var stream = new MemoryStream();

        file.CopyTo(stream);
        stream.Position = 0;

        return stream;
    }
}
