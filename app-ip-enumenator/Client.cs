using System.Collections.Generic;

namespace app_ip_enumenator
{
    class Client
    {
        public string Status { get; set; }
        public string Ip { get; set; }
        public int Occurrence { get; set; }
        public string Error { get; set; }
        public List<string> Errors { get; set; }
        public List<Occurrence> Occurrences { get; set; }

        public Client(string _status, string _ip, int _occurrence, string _error, Occurrence _oc)
        {
            Errors = new List<string>();
            Occurrences = new List<Occurrence>();
            Status = _status;
            Ip = _ip;
            Occurrence = _occurrence;
            Error = Error + " " + _error;
            Errors.Add(_error);
            Occurrences.Add(_oc);
        }

        public void AddError(string _error)
        {
            for (int i = 0; i < Errors.Count; i++)
            {
                if (!WasError(_error))
                {
                    Errors.Add(_error);
                    return;
                }
            }
        }

        private bool WasError(string _error)
        {
            foreach (string e in Errors)
            {
                if (e == _error)
                    return false;
            }

            return true;
        }
    }
}
