using System.Collections.Generic;

namespace Monopoly
{
    public interface ITurnHandler
    {
        void DoTurn(IPlayer player);
    }
}