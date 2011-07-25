using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Hearts.Models;

namespace UI.Controllers.Interfaces
{
    public interface IPlayerInfo
    {
        string Name { get; }
        Func<PlayerBase> Provider { get; }
    }
}
