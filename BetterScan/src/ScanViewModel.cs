
using System;
using System.Collections.Generic;


namespace BetterScan
{

    public class Candidate : ObservableObject
    {
        public string EntryFilePath { get; set; }
        private bool _Selected;
        public bool Selected
        {
            get => _Selected;
            set
            {
                _Selected = value;
                OnPropertyChanged("Selected");
            }
        }
        public System.Windows.Media.ImageSource IconData { get; set; }
    }

    public class ScanViewModel : ObservableObject
    {
        private string targetFolder = "";
        private bool optionEarlyStop = false;
        private List<Candidate> candidateList = new List<Candidate> { };
        public string TargetFolder
        {
            get => targetFolder;
            set => SetValue(ref targetFolder, value);
        }
        public bool OptionEarlyStop
        {
            get => optionEarlyStop;
            set => SetValue(ref optionEarlyStop, value);
        }
        public List<Candidate> CandidateList
        {
            get => candidateList;
            set => SetValue(ref candidateList, value);
        }
    }
}
