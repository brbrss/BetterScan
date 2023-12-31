﻿
using System;
using System.Collections.Generic;


namespace BetterScan
{

    public class Candidate
    {
        public string EntryFilePath { get; set; }
        public bool Selected { get; set; }
        public System.Windows.Media.ImageSource IconData { get; set; }
    }

    public class ScanViewModel : ObservableObject
    {
        private string targetFolder = "";

        private List<Candidate> candidateList = new List<Candidate> { };
        public string TargetFolder
        {
            get => targetFolder;
            set => SetValue(ref targetFolder, value);
        }
        public List<Candidate> CandidateList
        {
            get => candidateList;
            set => SetValue(ref candidateList, value);
        }
    }
}
