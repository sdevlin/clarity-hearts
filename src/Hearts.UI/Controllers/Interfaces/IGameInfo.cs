using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UI.Controllers.Interfaces
{
    public interface IGameInfo
    {
        string Name { get; }
        IEnumerable<IPlayerInfo> Players { get; }
    }
}
