using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Siotrix.Discord
{
    public static class ActionResult
    {
        private static bool is_success = false;
        private static string cmd_name = null;
        private static IUserMessage instance = null;
        private static long case_id = 0;
        private static long user_id = 0;
        private static int time_length = 0;
        private static bool is_founded_case_number = false;

        public static bool IsSuccess
        {
            get
            {
                return is_success;
            }
            set
            {
                is_success = value;
            }
        }

        public static string CommandName
        {
            get
            {
                return cmd_name;
            }
            set
            {
                cmd_name = value;
            }
        }

        public static int TimeLength
        {
            get
            {
                return time_length;
            }
            set
            {
                time_length = value;
            }
        }

        public static IUserMessage Instance
        {
            get
            {
                return instance;
            }
            set
            {
                instance = value;
            }
        }

        public static long CaseId
        {
            get
            {
                return case_id;
            }
            set
            {
                case_id = value;
            }
        }

        public static long UserId
        {
            get
            {
                return user_id;
            }
            set
            {
                user_id = value;
            }
        }
        
        public static bool IsFoundedCaseNumber
        {
            get
            {
                return is_founded_case_number;
            }
            set
            {
                is_founded_case_number = value;
            }
        }
    }
}
