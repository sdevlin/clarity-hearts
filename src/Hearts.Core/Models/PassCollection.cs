using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Hearts.Models
{
    internal class PassCollection
    {
        public PassingMode Mode { get; private set; }
        public IEnumerable<Pass> Passes { get; private set; }
        public PassCollection(PassingMode mode, IEnumerable<Pass> passes)
        {
            Mode = mode;
            Passes = passes;
        }
    }
}
