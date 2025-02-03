using Godot;
using SERVER.Contexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SERVER.Helpers
{
    internal static class PlayerHelper
    {
        public static PlayerCharacter ConstructPlayerInstance(long playerID, User user, PackedScene playerScene)
        {
            var playerObject = playerScene.Instantiate() as PlayerCharacter;

            playerObject.ID = user.ID;
            playerObject.Name = playerID.ToString();
            playerObject.PeerID = playerID;

            playerObject.Position = new Vector2(
                user.PlayerDisconnectedAtPosX,
                user.PlayerDisconnectedAtPosY
            );

            // TO DO -> PUT REAL DB VALUES
            playerObject.MaxHealth = 100;
            playerObject.CurrentHealth = playerObject.MaxHealth;

            return playerObject;
        }
    }
}
