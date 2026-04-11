using System;
using DarkMultiPlayerCommon;
using MessageStream2;

namespace DarkMultiPlayerServer.Messages
{
    public class VesselUpdate
    {
        public static void HandleVesselUpdate(ClientObject client, byte[] messageData)
        {
            using (MessageReader mr = new MessageReader(messageData))
            {
                mr.Read<double>();
                string vesselGuid = mr.Read<string>();
                string bodyName = mr.Read<string>();
                MilestoneSystem.TryRegisterFromVesselUpdate(client.playerName, vesselGuid, bodyName);
            }

            ServerMessage newMessage = new ServerMessage();
            newMessage.type = ServerMessageType.VESSEL_UPDATE;
            newMessage.data = messageData;
            ClientHandler.SendToAll(client, newMessage, false);
        }
    }
}

