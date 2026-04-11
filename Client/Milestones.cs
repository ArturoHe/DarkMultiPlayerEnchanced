using System;
using System.Collections.Generic;
using DarkMultiPlayerCommon;
using MessageStream2;

namespace DarkMultiPlayer
{
    public class Milestones
    {
        public bool synced;
        //Services
        private DMPGame dmpGame;
        private NetworkWorker networkWorker;
        //Backing
        private Queue<ByteArray> messageQueue = new Queue<ByteArray>();
        internal Dictionary<string, MilestoneEntry> milestoneEntries = new Dictionary<string, MilestoneEntry>();
        private NamedAction updateAction;

        public Milestones(DMPGame dmpGame, NetworkWorker networkWorker)
        {
            this.dmpGame = dmpGame;
            this.networkWorker = networkWorker;
            updateAction = new NamedAction(ProcessMessages);
            dmpGame.updateEvent.Add(updateAction);
        }

        private void ProcessMessages()
        {
            lock (messageQueue)
            {
                while (messageQueue.Count > 0)
                {
                    ByteArray queueByteArray = messageQueue.Dequeue();
                    HandleMessage(queueByteArray.data);
                    ByteRecycler.ReleaseObject(queueByteArray);
                }
            }
        }

        public void Stop()
        {
            dmpGame.updateEvent.Remove(updateAction);
        }

        public void RequestMilestones()
        {
            networkWorker.SendMilestoneRequest();
        }

        public void QueueMessage(ByteArray data)
        {
            lock (messageQueue)
            {
                ByteArray queueByteArray = ByteRecycler.GetObject(data.Length);
                Array.Copy(data.data, 0, queueByteArray.data, 0, data.Length);
                messageQueue.Enqueue(queueByteArray);
            }
        }

        private void HandleMessage(byte[] data)
        {
            using (MessageReader mr = new MessageReader(data))
            {
                MilestoneMessageType type = (MilestoneMessageType)mr.Read<int>();
                lock (milestoneEntries)
                {
                    switch (type)
                    {
                        case MilestoneMessageType.MILESTONE_RESET:
                            synced = false;
                            milestoneEntries.Clear();
                            break;
                        case MilestoneMessageType.MILESTONE_INFO:
                            {
                                MilestoneEntry entry = new MilestoneEntry();
                                entry.key = mr.Read<string>();
                                entry.title = mr.Read<string>();
                                entry.playerName = mr.Read<string>();
                                entry.utcTicks = mr.Read<long>();
                                milestoneEntries[entry.key] = entry;
                            }
                            break;
                        case MilestoneMessageType.MILESTONE_SYNCED:
                            synced = true;
                            break;
                    }
                }
            }
        }
    }

    public class MilestoneEntry
    {
        public string key;
        public string title;
        public string playerName;
        public long utcTicks;
    }
}
