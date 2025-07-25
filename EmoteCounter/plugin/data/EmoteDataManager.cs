﻿using Dalamud.Game.ClientState.Objects.SubKinds;
using Dalamud.Utility;
using System;

namespace EmoteCounter
{
    public class EmoteDataManager : IDisposable
    {
        private EmoteOwnerDB? ownerDB;
        private string ownerName = string.Empty;
        private ulong ownerCID;
        private bool isInitialized = false;

        public void ConditionalInitialize()
        {
            if (!isInitialized)
            {
                UpdateOwner();
            }
        }

        public void Dispose()
        {
        }

        public void OnLogin()
        {
            UpdateOwner();
        }

        public void OnLogout()
        {
            ownerName = string.Empty;
            ownerCID = 0;
            isInitialized = false;

            OnOwnerChanged();
        }

        public void OnEmote(IPlayerCharacter instigator, ushort emoteId)
        {
            UpdateOwner();
            var needsSave = false;

            foreach (var counter in Service.emoteCounters)
            {
                var hasChanges = counter.OnEmote(instigator, emoteId);
                if (!hasChanges)
                {
                    continue;
                }

                needsSave = true;
            }

            if (needsSave)
            {
                SaveOwnerDB();
            }
        }

        public void OnOwnerChanged()
        {
            SaveOwnerDB();

            LoadOrCreateOwnerDB();
            CopyDBValuesToCounters();
        }

        private void UpdateOwner()
        {
            if (!ThreadSafety.IsMainThread)
            {
                return;
            }

            if (Service.clientState == null || Service.clientState.LocalContentId == 0)
            {
                return;
            }

            var localPlayer = Service.clientState.LocalPlayer;
            if (localPlayer == null || localPlayer.Name == null)
            {
                return;
            }

            var newCID = Service.clientState.LocalContentId;
            var newName = localPlayer.Name.TextValue;

            if (newCID != ownerCID || newName != ownerName)
            {
                ownerName = newName;
                ownerCID = newCID;
                isInitialized = true;

                OnOwnerChanged();
            }
        }

        private void SaveOwnerDB()
        {
            if (ownerDB == null || ownerDB.CID == 0 || ownerDB.Counters.Count == 0)
            {
                return;
            }

            if (!ownerDB.isFromConfig)
            {
                // shouldn't exist in config, but verify just in case to avoid duplicates
                bool isAdded = false;
                for (int idx = 0; idx < Service.pluginConfig.EmoteData.Count; idx++)
                {
                    if (Service.pluginConfig.EmoteData[idx].IsOwnerMatching(ownerDB))
                    {
                        Service.pluginConfig.EmoteData[idx] = ownerDB;
                        isAdded = true;
                    }
                }

                if (!isAdded)
                {
                    Service.pluginConfig.EmoteData.Add(ownerDB);
                }
            }

            Service.pluginConfig.Save();
        }

        private void LoadOrCreateOwnerDB()
        {
            if (ownerCID == 0)
            {
                // no DB if owner doesn't exist
                ownerDB = null;
                return;
            }

            foreach (var testDB in Service.pluginConfig.EmoteData)
            {
                if (testDB.IsOwnerMatching(ownerCID, ownerName))
                {
                    ownerDB = testDB;
                    ownerDB.isFromConfig = true;

                    break;
                }
            }

            if (ownerDB == null)
            {
                ownerDB = new EmoteOwnerDB();
            }

            // always assign all ids, some loaded DB matches may miss data (migration, manual edits, etc)
            ownerDB.CID = ownerCID;
            ownerDB.Name = ownerName;

            Service.logger.Debug($"Loaded emote counters for: {ownerName} ({ownerCID})");
        }

        private void CopyDBValuesToCounters()
        {
            foreach (var counter in Service.emoteCounters)
            {
                var assignedValue = false;

                if (ownerDB != null)
                {
                    var counterData = ownerDB.Counters.Find(x => x.Name == counter.Name);
                    if (counterData != null)
                    {
                        counter.dataLink = counterData;
                        assignedValue = true;
                    }
                }

                if (!assignedValue)
                {
                    var counterData = new EmoteCounterDB() { Name = counter.Name, Value = 0 };
                    counter.dataLink = counterData;

                    if (ownerDB != null)
                    {
                        ownerDB.Counters.Add(counterData);
                    }
                }
            }
        }
    }
}
