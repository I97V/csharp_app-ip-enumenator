using System.Collections.Generic;
using System.Windows;

namespace app_ip_enumenator
{
    class Client
    {
        public string Status { get; set; }
        public string Ip { get; set; }
        public int Occurrence { get; set; }
        public string Error { get; set; }
        public List<int> Errors { get; set; }
        public List<Occurrence> Occurrences { get; set; }

        public Client(string _status, string _ip, int _occurrence, int _error, Occurrence _oc)
        {
            Errors = new List<int>();
            Occurrences = new List<Occurrence>();
            Status = _status;
            Ip = _ip;
            Occurrence = _occurrence;
            Error = Error + " " + _error;
            Errors.Add(_error);
            Occurrences.Add(_oc);
        }

        public void AddError(int _error)
        {
            for (int i = 0; i < Errors.Count; i++)
            {
                if (Errors[i] == _error)
                {
                    continue;
                }
                else
                {
                    Errors.Add(_error);
                    Error = Error + " " + _error;
                    continue;
                }
            }
        }
    }
}
