using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using iText.Kernel.Pdf.Canvas.Parser.Listener;

namespace R.Paper_Parser.backend
{
    public class FileUploadService
    {
        private const int MaxFileSizeInMB = 10; // max file size 10MB
        private readonly string[] AllowedExtensions = { ".pdf", ".docx", ".doc" }; // allowed file types
        
        // checks if file is ok for upload
        public bool ValidateFile(FileResult file)
        {
            if (file == null)
            {
                Console.WriteLine("File validation failed: File is null");
                return false;
            }
            
            // check extension - log for debugging
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            Console.WriteLine($"Validating file: {file.FileName}, Extension: {extension}, Platform: {DeviceInfo.Platform}");
            
            if (!Array.Exists(AllowedExtensions, ext => ext == extension))
            {
                Console.WriteLine($"File validation failed: Invalid extension {extension}");
                return false;
            }
            
            try
            {
                // get file size in MB - handle errors cross-platform
                using (Stream fileStream = file.OpenReadAsync().Result)
                {
                    double fileSizeMB = fileStream.Length / (1024.0 * 1024.0);
                    Console.WriteLine($"File size: {fileSizeMB:F2}MB");
                    
                    // check size
                    if (fileSizeMB > MaxFileSizeInMB)
                    {
                        Console.WriteLine($"File validation failed: Size {fileSizeMB:F2}MB exceeds {MaxFileSizeInMB}MB limit");
                        return false;
                    }
                }
                
                return true;
            }
            catch (Exception ex)
            {
                // log error but don't crash
                Console.WriteLine($"Error during file validation: {ex.Message}, Stack trace: {ex.StackTrace}");
                
                // for iOS, try different approach if regular validation fails
                if (DeviceInfo.Platform == DevicePlatform.iOS && extension == ".pdf")
                {
                    Console.WriteLine("iOS PDF validation failed, trying alternative approach...");
                    return true; // assume valid and try to process anyway
                }
                
                return false;
            }
        }
        
        // get file as byte array - with better iOS handling
        public async Task<byte[]?> GetFileBytes(FileResult file)
        {
            if (file == null)
                return null;
            
            try
            {
                // special handling for iOS PDF files
                if (DeviceInfo.Platform == DevicePlatform.iOS && 
                    Path.GetExtension(file.FileName).ToLowerInvariant() == ".pdf")
                {
                    Console.WriteLine("Using iOS-specific file reading approach");
                    
                    // try FileSystem API first for iOS PDFs
                    try
                    {
                        // iOS files might be in temp location - try to copy to app data
                        string localFilePath = Path.Combine(
                            FileSystem.CacheDirectory, 
                            Path.GetFileName(file.FullPath)
                        );
                        
                        Console.WriteLine($"iOS: Reading from: {file.FullPath}");
                        Console.WriteLine($"iOS: Copying to: {localFilePath}");
                        
                        using (var sourceStream = await file.OpenReadAsync())
                        using (var destinationStream = File.Create(localFilePath))
                        {
                            await sourceStream.CopyToAsync(destinationStream);
                        }
                        
                        // read the copied file
                        return File.ReadAllBytes(localFilePath);
                    }
                    catch (Exception iosEx)
                    {
                        Console.WriteLine($"iOS file handling error: {iosEx.Message}, falling back to direct stream");
                        // fall back to standard approach
                    }
                }
                
                // standard approach - works for most platforms
                using var stream = await file.OpenReadAsync();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);
                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file bytes: {ex.Message}, Stack trace: {ex.StackTrace}");
                return null;
            }
        }
        
        // extract text from PDF/DOCX files
        public async Task<string> ExtractTextFromFile(FileResult file)
        {
            if (file == null)
                return string.Empty;
                
            string extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            Console.WriteLine($"Extracting text from {extension} file: {file.FileName}");
            
            // extra logging for iOS
            if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                Console.WriteLine($"iOS file details - Path: {file.FullPath}, ContentType: {file.ContentType}");
            }
            
            try
            {
                // get file bytes
                byte[] fileBytes = await GetFileBytes(file);
                if (fileBytes == null || fileBytes.Length == 0)
                {
                    return "Error: Unable to read file content.";
                }
                
                // log bytes info
                Console.WriteLine($"Read {fileBytes.Length} bytes from file");
                
                // create temp file to work with
                string tempFilePath = Path.Combine(
                    DeviceInfo.Platform == DevicePlatform.iOS ? 
                        FileSystem.CacheDirectory : 
                        Path.GetTempPath(),
                    Path.GetFileName(file.FileName)
                );
                
                Console.WriteLine($"Writing to temp file: {tempFilePath}");
                await File.WriteAllBytesAsync(tempFilePath, fileBytes);
                
                string extractedText = "";
                
                try
                {
                    if (extension == ".pdf")
                    {
                        extractedText = ExtractTextFromPdf(tempFilePath);
                    }
                    else if (extension == ".docx" || extension == ".doc")
                    {
                        extractedText = ExtractTextFromDocx(tempFilePath);
                    }
                    else
                    {
                        extractedText = $"Unsupported file type: {extension}";
                    }
                }
                finally
                {
                    // clean up temp file
                    if (File.Exists(tempFilePath))
                    {
                        try
                        {
                            File.Delete(tempFilePath);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Failed to delete temp file: {ex.Message}");
                        }
                    }
                }
                
                return !string.IsNullOrEmpty(extractedText) 
                    ? extractedText 
                    : "No text could be extracted from the document.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error extracting text: {ex.Message}, Stack trace: {ex.StackTrace}");
                return $"Error extracting text: {ex.Message}";
            }
        }
        
        // extract text from PDF using iText7
        private string ExtractTextFromPdf(string filePath)
        {
            StringBuilder text = new StringBuilder();
            
            try
            {
                Console.WriteLine($"Opening PDF: {filePath}");
                using (PdfReader pdfReader = new PdfReader(filePath))
                {
                    using (PdfDocument pdfDoc = new PdfDocument(pdfReader))
                    {
                        int pages = pdfDoc.GetNumberOfPages();
                        Console.WriteLine($"PDF has {pages} pages");
                        
                        for (int i = 1; i <= pages; i++)
                        {
                            var page = pdfDoc.GetPage(i);
                            var strategy = new SimpleTextExtractionStrategy();
                            string pageText = PdfTextExtractor.GetTextFromPage(page, strategy);
                            text.Append(pageText);
                            
                            // add page separator
                            if (i < pages)
                            {
                                text.AppendLine();
                                text.AppendLine("-------------------");
                                text.AppendLine();
                            }
                        }
                    }
                }
                
                return text.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"PDF extraction error: {ex.Message}, Stack trace: {ex.StackTrace}");
                return $"PDF extraction error: {ex.Message}";
            }
        }
        
        // extract text from DOCX using OpenXml
        private string ExtractTextFromDocx(string filePath)
        {
            StringBuilder text = new StringBuilder();
            
            try
            {
                using (WordprocessingDocument wordDoc = WordprocessingDocument.Open(filePath, false))
                {
                    if (wordDoc.MainDocumentPart != null)
                    {
                        Body body = wordDoc.MainDocumentPart.Document.Body;
                        if (body != null)
                        {
                            // get text from paragraphs
                            foreach (var para in body.Elements<Paragraph>())
                            {
                                text.AppendLine(para.InnerText);
                            }
                        }
                    }
                }
                
                return text.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"DOCX extraction error: {ex.Message}");
                return $"DOCX extraction error: {ex.Message}";
            }
        }
    }
}