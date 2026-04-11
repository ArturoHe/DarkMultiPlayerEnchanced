using System.Collections.Generic;
using DarkMultiPlayerCommon;
using MessageStream2;

namespace DarkMultiPlayerServer.Messages
{
    public class MilestoneMessage
    {
        public static void HandleMessage(ClientObject client, byte[] data)
        {
            using (MessageReader mr = new MessageReader(data))
            {
                int type = mr.Read<int>();
                switch ((MilestoneMessageType)type)
                {
                    case MilestoneMessageType.MILESTONE_REQUEST:
                        SendMilestonesToClient(client);
                        break;
                }
            }
        }

        public static void SendMilestonesToAll()
        {
            Dictionary<string, MilestoneRecord> milestones = MilestoneSystem.GetMilestonesCopy();
            ServerMessage resetMessage = new ServerMessage();
            resetMessage.type = ServerMessageType.MILESTONE;
            using (MessageWriter mw = new MessageWriter())
            {
                mw.Write<int>((int)MilestoneMessageType.MILESTONE_RESET);
                resetMessage.data = mw.GetMessageBytes();
            }
            ClientHandler.SendToAll(null, resetMessage, true);

            foreach (KeyValuePair<string, MilestoneRecord> kvp in milestones)
            {
                MilestoneRecord milestone = kvp.Value;
                ServerMessage infoMessage = new ServerMessage();
                infoMessage.type = ServerMessageType.MILESTONE;
                using (MessageWriter mw = new MessageWriter())
                {
                    mw.Write<int>((int)MilestoneMessageType.MILESTONE_INFO);
                    mw.Write<string>(milestone.key);
                    mw.Write<string>(milestone.title);
                    mw.Write<string>(milestone.playerName);
                    mw.Write<long>(milestone.utcTicks);
                    infoMessage.data = mw.GetMessageBytes();
                }
                ClientHandler.SendToAll(null, infoMessage, true);
            }

            ServerMessage syncMessage = new ServerMessage();
            syncMessage.type = ServerMessageType.MILESTONE;
            using (MessageWriter mw = new MessageWriter())
            {
                mw.Write<int>((int)MilestoneMessageType.MILESTONE_SYNCED);
                syncMessage.data = mw.GetMessageBytes();
            }
            ClientHandler.SendToAll(null, syncMessage, true);
        }

        public static void SendMilestonesToClient(ClientObject client)
        {
            Dictionary<string, MilestoneRecord> milestones = MilestoneSystem.GetMilestonesCopy();
            ServerMessage resetMessage = new ServerMessage();
            resetMessage.type = ServerMessageType.MILESTONE;
            using (MessageWriter mw = new MessageWriter())
            {
                mw.Write<int>((int)MilestoneMessageType.MILESTONE_RESET);
                resetMessage.data = mw.GetMessageBytes();
            }
            ClientHandler.SendToClient(client, resetMessage, true);

            foreach (KeyValuePair<string, MilestoneRecord> kvp in milestones)
            {
                MilestoneRecord milestone = kvp.Value;
                ServerMessage infoMessage = new ServerMessage();
                infoMessage.type = ServerMessageType.MILESTONE;
                using (MessageWriter mw = new MessageWriter())
                {
                    mw.Write<int>((int)MilestoneMessageType.MILESTONE_INFO);
                    mw.Write<string>(milestone.key);
                    mw.Write<string>(milestone.title);
                    mw.Write<string>(milestone.playerName);
                    mw.Write<long>(milestone.utcTicks);
                    infoMessage.data = mw.GetMessageBytes();
                }
                ClientHandler.SendToClient(client, infoMessage, true);
            }

            ServerMessage syncMessage = new ServerMessage();
            syncMessage.type = ServerMessageType.MILESTONE;
            using (MessageWriter mw = new MessageWriter())
            {
                mw.Write<int>((int)MilestoneMessageType.MILESTONE_SYNCED);
                syncMessage.data = mw.GetMessageBytes();
            }
            ClientHandler.SendToClient(client, syncMessage, true);
        }
    }
}
