using System;
using System.IO;
using System.Reflection;
using NUnit.Framework;

namespace UnityFolderGenerator.Editor.Tests
{
    [TestFixture]
    public class FolderGeneratorTests
    {
        private string _testRootPath = string.Empty;
        private MethodInfo _createDirectoryAndKeepMethod;

        [SetUp]
        public void SetUp()
        {
            _testRootPath = Path.Combine(Path.GetTempPath(), "FolderGeneratorTests", Guid.NewGuid().ToString());
            _createDirectoryAndKeepMethod = typeof(FolderGenerator).GetMethod(
                "CreateDirectoryAndKeep",
                BindingFlags.NonPublic | BindingFlags.Static);

            Directory.CreateDirectory(_testRootPath);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testRootPath))
            {
                Directory.Delete(_testRootPath, true);
            }
        }

        [Test]
        public void IsValidFolderName_ReturnsTrue_ForValidNames()
        {
            Assert.IsTrue(FolderGenerator.IsValidFolderName("MyFolder"));
            Assert.IsTrue(FolderGenerator.IsValidFolderName("Folder_123"));
        }

        [Test]
        public void IsValidFolderName_ReturnsFalse_ForNullOrWhitespace()
        {
            Assert.IsFalse(FolderGenerator.IsValidFolderName(null));
            Assert.IsFalse(FolderGenerator.IsValidFolderName(string.Empty));
            Assert.IsFalse(FolderGenerator.IsValidFolderName("   "));
        }

        [Test]
        public void IsValidFolderName_ReturnsFalse_ForInvalidCharacters()
        {
            string invalidName = $"Invalid{Path.GetInvalidFileNameChars()[0]}Name";
            Assert.IsFalse(FolderGenerator.IsValidFolderName(invalidName));
        }

        [Test]
        public void CreateDirectoryAndKeep_CreatesGitKeep_WhenRequested()
        {
            string folderPath = Path.Combine(_testRootPath, "NewFolder");

            bool created = InvokeCreateDirectoryAndKeep(folderPath, true);

            Assert.IsTrue(created, "Expected folder to be created on first invocation.");
            string gitKeepPath = Path.Combine(folderPath, ".gitkeep");
            Assert.IsTrue(File.Exists(gitKeepPath), ".gitkeep file should be created when requested.");
        }

        [Test]
        public void CreateDirectoryAndKeep_ReturnsFalse_WhenFolderAlreadyExists()
        {
            string folderPath = Path.Combine(_testRootPath, "ExistingFolder");
            Directory.CreateDirectory(folderPath);

            bool created = InvokeCreateDirectoryAndKeep(folderPath, true);

            Assert.IsFalse(created, "Expected method to report existing folder without recreating it.");
            string gitKeepPath = Path.Combine(folderPath, ".gitkeep");
            Assert.IsFalse(File.Exists(gitKeepPath), ".gitkeep should not be added when folder already exists.");
        }

        private bool InvokeCreateDirectoryAndKeep(string path, bool createGitKeep)
        {
            Assert.IsNotNull(_createDirectoryAndKeepMethod, "Could not locate CreateDirectoryAndKeep via reflection.");
            object result = _createDirectoryAndKeepMethod.Invoke(null, new object[] { path, createGitKeep });
            return result is bool boolean && boolean;
        }
    }
}
