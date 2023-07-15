
using System;
using System.Collections.Generic;


namespace BetterScan
{
    public class ScanViewModel : ObservableObject
    {
        private string targetFolder = "";
        public string TargetFolder
        {
            get => targetFolder;
            set => SetValue(ref targetFolder, value);
        }

    }
}
