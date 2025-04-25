using System;
using System.IO;

namespace R.Paper_Parser.backend;

public class FileUploadService
{
    private readonly string _uploadPath;

    public FileUploadService()
    {
        _uploadPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "uploads");
        Directory.CreateDirectory(_uploadPath);
    }

    public string SaveFile(Stream fileStream, string fileName)
    {
        string fullPath = Path.Combine(_uploadPath, fileName);
        using var file = File.Create(fullPath);
        fileStream.CopyTo(file);
        return fullPath;
    }

    public string ReadTextFromFile(string path)
    {
        return File.ReadAllText(path);
    }
}