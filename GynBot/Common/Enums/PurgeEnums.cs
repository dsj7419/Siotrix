using System;
using System.Collections.Generic;
using System.Text;

namespace GynBot.Common.Enums
{
    public enum DeleteStrategy
    {
        BulkDelete = 0,
        Manual = 1
    }

    public enum DeleteType
    {
        Self = 0,
        Bot = 1,
        All = 2
    }
}
