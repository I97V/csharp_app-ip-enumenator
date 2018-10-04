namespace app_ip_enumenator
{
    class Occurrence
    {
        public string Date { get; set; }
        public string Time { get; set; }
        public string Error { get; set; }

        public Occurrence(string _date, string _time, string _error)
        {
            Date = _date;
            Time = _time;
            Error = _error;
        }
    }
}
