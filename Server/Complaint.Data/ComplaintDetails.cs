using System;

namespace Complaint.Data
{
    public class ComplaintDetails
    {

        public string Agency
        {
            get;
            set;
        }

        public string complaintType
        {
            get;
            set;
        }

        public string Descriptor
        {
            get;
            set;
        }

        public int incidentZip
        {
            get;
            set;
        }

        public int dayOfMonth
        {
            get;
            set;
        }

        public int dayOfWeek
        {
            get;
            set;
        }

        public int Month
        {
            get;
            set;
        }

        public double avgTemp
        {
            get;
            set;
        }

        public double complaintTimeToComplete
        {
            get;
            set;
        }

        public DateTime TimeOnServer
        {
            get;
            set;
        }

    }
}
