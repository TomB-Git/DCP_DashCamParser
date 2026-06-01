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

        [ObservableProperty]
        private bool _canRemoveItem = false;

        partial void OnProgressChanged(int value)
        {
            if(Progress == 100)
            {
                CanRemoveItem = true;
            }
        }

        public DashCamVideo(string path, bool front)
        {
            VideoPath = path??string.Empty;
            string dateTemp = path.Substring(path.LastIndexOf("\\") + 1, 8) ?? string.Empty;
            Date = dateTemp.Substring(0, 4) + " " + dateTemp.Substring(4, 2) + " " + dateTemp.Substring(6, 2);
            IsFront = front;
            if (front)
            {
                Name = "AV_";
            }
            else 
            { 
                Name = "AR_"; 
            }
            string nameTemp = path.Substring(path.LastIndexOf("\\") + 9, 6);
            Name = nameTemp.Substring(0, 2) + "h" + nameTemp.Substring(2, 2) + "m" + nameTemp.Substring(4, 2) + "s";
            Extension = Path.GetExtension(path) ?? string.Empty;
        }
    }
}
