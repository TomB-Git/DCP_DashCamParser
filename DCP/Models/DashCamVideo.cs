using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DCP.Models
{
    public partial class DashCamVideo : ObservableObject
    {
        private string _videoPath;
        public string VideoPath
        {
            get => _videoPath;
            set => _videoPath = value;
        }
        private string _date;
        public string Date
        {
            get => _date;
            set => SetProperty(ref _date, value);
        }
        private bool _isFront;
        public bool IsFront
        {
            get => _isFront;
            set => _isFront = value;
        }

        private string _name;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private string _extension;
        public string Extension
        {
            get => _extension;
            set => SetProperty(ref _extension, value);
        }
        [ObservableProperty]
        private int _progress;
        public DashCamVideo(string path, bool front)
        {
            VideoPath = path??string.Empty;
            Date = path.Substring(path.LastIndexOf("\\")+1,8)??string.Empty;
            IsFront = front;
            if (front)
            {
                Name = "AV_";
            }
            else 
            { 
                Name = "AR_"; 
            }
            Name += path.Substring(path.LastIndexOf("\\") + 1, 8)??string.Empty;
            Name += "_";
            Name += path.Substring(path.LastIndexOf("\\") + 9, 6);
            Extension = Path.GetExtension(path) ?? string.Empty;
        }
    }
}
