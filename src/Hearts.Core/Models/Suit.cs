using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace Hearts.Models
{
    public enum Suit
    {
        [Description("c")]
        Clubs,
        [Description("d")]
        Diamonds,
        [Description("s")]
        Spades,
        [Description("h")]
        Hearts
    }
}
