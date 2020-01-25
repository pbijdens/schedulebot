using System;
using System.Collections.Generic;
using System.Text;

namespace PB.ScheduleBot.Utils
{
    public static class StringExtensions
    {
        public static string GetQueryPart(this string input, int index)
        {
            string[] parts = (input ?? string.Empty).Split('.');
            if (index < parts.Length)
            {
                return parts[index];
            }
            return null;
        }

        public static string HtmlSafe(this string text)
        {
            return text?.Replace("<", "&lt;").Replace(">", "&gt;");
        }
    }
}
