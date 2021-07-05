using System;
using System.Collections.Generic;
using System.Text;

namespace GoGame
{
    public class KilledEventArgs : EventArgs
    {
        public int TokenID { get; }
        public KilledEventArgs(int tokenID)
        {
            TokenID = tokenID;
        }
    }
}
