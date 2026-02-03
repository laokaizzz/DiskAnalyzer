using DiskAnalyzer.Services;
using System;
using Xunit;

namespace DiskAnalyzer.Tests
{
    public class FileClassifierTests
    {
        [Theory]
        [InlineData(@"C:\Windows\System32\kernel32.dll", ".dll", 0)]
        [InlineData(@"C:\Program Files\App\app.exe", ".exe", 0)]
        [InlineData(@"C:\Drivers\driver.sys", ".sys", 0)]
        [InlineData(@"C:\Users\User\Documents\doc.pdf", ".pdf", 1)] // User path (assuming User is current user, might fail if path doesn't match Environment.SpecialFolder, but the Logic uses strict path start.
        // Wait, UserPaths logic uses Environment.GetFolderPath(Environment.SpecialFolder.UserProfile).
        // In test environment, this might vary. I should check logic or mock environment?
        // For simple unit test, I can't easily mock Environment.GetFolderPath without abstraction.
        // However, I can test extensions that are safe regardless of path (except system path).
        [InlineData(@"D:\Data\archive.zip", ".zip", 1)] // Safe extension
        [InlineData(@"D:\Data\movie.mp4", ".mp4", 1)] // Safe extension
        [InlineData(@"D:\Data\unknown.xyz", ".xyz", 2)] // Unknown
        public void TestClassify(string path, string ext, int expectedLevel)
        {
            // For user path test, we construct a valid user path dynamically
            if (path.Contains("C:\\Users\\User"))
            {
                 path = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Documents", "doc.pdf");
            }
            
            var result = FileClassifier.Classify(path, ext);
            Assert.Equal(expectedLevel, result);
        }
    }
}
