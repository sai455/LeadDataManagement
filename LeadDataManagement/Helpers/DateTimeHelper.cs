using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace LeadDataManagement.Helpers
{
    public static class DateTimeHelper
    {

        public static class TimeZoneList
        {
            public const string GMTStandardTime = "GMT Standard Time";
            public const string PacificStandardTime = "Pacific Standard Time";
            public const string CentralStandardTime = "Central Standard Time";
            public const string EasternStandardTime = "Eastern Standard Time";
            public const string IndiaStandardTime = "India Standard Time";
            public const string UTC = "UTC";
        }
        public static DateTime GetDateTimeNowByTimeZone(string timeZoneId)
        {
            DateTime retVal;
            try
            {
                TimeZoneInfo timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
                retVal = TimeZoneInfo.ConvertTime(DateTime.Now, timeZoneInfo);
            }
            catch (TimeZoneNotFoundException)
            {
                throw new Exception(string.Format("Unable to find the {0} zone in the registry.", timeZoneId));
            }
            catch (InvalidTimeZoneException)
            {
                throw new Exception(string.Format("Registry data on the {0} zone has been corrupted.", timeZoneId));
            }
            return retVal;
        }
    }
}