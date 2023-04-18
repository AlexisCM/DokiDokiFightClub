using Mirror;

namespace DokiDokiFightClub
{
    public class ScoreKeeper : NetworkBehaviour
    {
        [SyncVar]
        private uint _p0Score;

        [SyncVar]
        private uint _p1Score;

        [ServerCallback]
        public void AddScore(int playerId)
        {
            if (playerId == 0)
                _p0Score += 1;
            else
                _p1Score += 1;
        }

        public uint GetScore(int playerId)
        {
            return playerId == 0 ? _p0Score : _p1Score;
        }

        /// <summary>Returns the ID of the winning player, or null in case of a draw.</summary>
        public int? GetWinner()
        {
            if (_p0Score > _p1Score)
            {
                // p0 wins
                return 0;
            }
            else if (_p0Score < _p1Score)
            {
                // p1 wins
                return 1;
            }
            else
            {
                // draw
                return null;
            }
        }
    }
}
