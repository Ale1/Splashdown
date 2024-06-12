using NUnit.Framework;
using System.IO;
using UnityEngine;

namespace Splashdown.Tests
{
	[TestFixture]
    public class SplashdownTests
    {
        private string _projectRoot;
        
      	[SetUp]
        public void Setup()
        {
            _projectRoot = Path.GetDirectoryName(Application.dataPath);
        }

        [Test][Category("Constants")]
        public void FontPath_Roboto_Exists()
        {
            var path = Path.Combine(_projectRoot, Constants.FontPath_Roboto);
            Assert.IsTrue(File.Exists(path), $"Font file at path {path} does not exist.");
        }

        [Test][Category("Constants")]
        public void FontPath_NanumGothic_Exists()
        {
            var path = Path.Combine(_projectRoot, Constants.FontPath_NanumGothic);
            Assert.IsTrue(File.Exists(path), $"Font file at path {path} does not exist.");
        }
    }
}

