using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DCP.Models
{
    public class UserData
    {
        private string _sourcePath;
        public string SourcePath { get { return _sourcePath; } set { _sourcePath = value; } }

        private string _destPath;
        public string DestPath { get { return _destPath; } set { _destPath = value; } }

        public UserData()
        {

        }
    }
}
