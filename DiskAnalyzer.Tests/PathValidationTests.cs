using DiskAnalyzer.ViewModels;
using System;
using System.IO;
using Xunit;

namespace DiskAnalyzer.Tests
{
    public class PathValidationTests
    {
        [Fact]
        public void TestSetInitialPath_ValidatesPath()
        {
            var vm = new MainViewModel();

            // Act
            string validPath = Environment.CurrentDirectory;
            vm.SetInitialPath(validPath);

            // Assert
            Assert.Equal(validPath, vm.SelectedPath);
            Assert.True(string.IsNullOrEmpty(vm.ErrorMessage));
        }

        [Fact]
        public void TestSetInitialPath_InvalidPath_SetsErrorMessage()
        {
            var vm = new MainViewModel();
            string invalidPath = @"Z:\NonExistentPath_" + Guid.NewGuid();

            vm.SelectedPath = invalidPath;

            Assert.Equal(invalidPath, vm.SelectedPath);
            Assert.False(string.IsNullOrEmpty(vm.ErrorMessage));
            Assert.Contains("不存在", vm.ErrorMessage);
        }

        [Fact]
        public void TestEmptyPath_SetsErrorMessage()
        {
            var vm = new MainViewModel();
            vm.SelectedPath = "";

            Assert.False(string.IsNullOrEmpty(vm.ErrorMessage));
        }
    }
}
