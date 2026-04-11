using System;
using System.Collections.Generic;
using UnityEngine;

namespace DarkMultiPlayer
{
    public class MilestonesWindow
    {
        public bool display;
        //Services
        private DMPGame dmpGame;
        private Milestones milestones;

        private bool isWindowLocked = false;
        private bool initialized;
        private Vector2 scrollPosition;
        //GUI Layout
        private Rect windowRect;
        private Rect moveRect;
        private GUILayoutOption[] layoutOptions;
        //Styles
        private GUIStyle windowStyle;
        private GUIStyle buttonStyle;
        private GUIStyle headerStyle;
        private GUIStyle cellStyle;
        //const
        private const float WINDOW_HEIGHT = 420;
        private const float WINDOW_WIDTH = 680;
        private NamedAction drawAction;

        public MilestonesWindow(DMPGame dmpGame, Milestones milestones)
        {
            this.dmpGame = dmpGame;
            this.milestones = milestones;
            drawAction = new NamedAction(Draw);
            dmpGame.drawEvent.Add(drawAction);
        }

        public void Stop()
        {
            dmpGame.drawEvent.Remove(drawAction);
        }

        private void InitGUI()
        {
            //Setup GUI stuff
            windowRect = new Rect(Screen.width * 0.5f - WINDOW_WIDTH / 2f, Screen.height * 0.5f - WINDOW_HEIGHT / 2f, WINDOW_WIDTH, WINDOW_HEIGHT);
            moveRect = new Rect(0, 0, 10000, 20);

            windowStyle = new GUIStyle(GUI.skin.window);
            buttonStyle = new GUIStyle(GUI.skin.button);
            headerStyle = new GUIStyle(GUI.skin.label);
            headerStyle.fontStyle = FontStyle.Bold;
            cellStyle = new GUIStyle(GUI.skin.label);
            cellStyle.wordWrap = true;

            layoutOptions = new GUILayoutOption[4];
            layoutOptions[0] = GUILayout.MinWidth(WINDOW_WIDTH);
            layoutOptions[1] = GUILayout.MaxWidth(WINDOW_WIDTH);
            layoutOptions[2] = GUILayout.MinHeight(WINDOW_HEIGHT);
            layoutOptions[3] = GUILayout.MaxHeight(WINDOW_HEIGHT);
        }

        private void Draw()
        {
            if (!display)
            {
                RemoveWindowLock();
                return;
            }

            if (!initialized)
            {
                initialized = true;
                InitGUI();
            }

            Vector2 mousePos = Input.mousePosition;
            mousePos.y = Screen.height - mousePos.y;
            bool shouldLock = windowRect.Contains(mousePos);

            if (shouldLock && !isWindowLocked)
            {
                InputLockManager.SetControlLock(ControlTypes.ALLBUTCAMERAS, "DMP_MilestonesWindowLock");
                isWindowLocked = true;
            }
            if (!shouldLock && isWindowLocked)
            {
                RemoveWindowLock();
            }

            windowRect = DMPGuiUtil.PreventOffscreenWindow(GUILayout.Window(6715 + Client.WINDOW_OFFSET, windowRect, DrawContent, "DarkMultiPlayer - Milestones", windowStyle, layoutOptions));
        }

        private void RemoveWindowLock()
        {
            if (isWindowLocked)
            {
                isWindowLocked = false;
                InputLockManager.RemoveControlLock("DMP_MilestonesWindowLock");
            }
        }

        private void DrawContent(int windowID)
        {
            GUI.DragWindow(moveRect);
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label("Milestones unlocked on this server");
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Refresh", buttonStyle, GUILayout.Width(90f)))
            {
                milestones.RequestMilestones();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            DrawHeader();
            GUILayout.Space(4);

            List<MilestoneEntry> entries = new List<MilestoneEntry>();
            lock (milestones.milestoneEntries)
            {
                foreach (MilestoneEntry entry in milestones.milestoneEntries.Values)
                {
                    entries.Add(entry);
                }
            }
            entries.Sort((a, b) => string.Compare(a.title, b.title, StringComparison.Ordinal));

            scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
            if (entries.Count == 0)
            {
                GUILayout.Label("No milestones unlocked yet.", cellStyle);
            }
            else
            {
                foreach (MilestoneEntry entry in entries)
                {
                    DrawRow(entry);
                }
            }
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
        }

        private void DrawHeader()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Milestone", headerStyle, GUILayout.Width(360f));
            GUILayout.Label("Player", headerStyle, GUILayout.Width(140f));
            GUILayout.Label("UTC", headerStyle, GUILayout.Width(150f));
            GUILayout.EndHorizontal();
        }

        private void DrawRow(MilestoneEntry entry)
        {
            string utcTime = "-";
            if (entry.utcTicks > 0)
            {
                utcTime = new DateTime(entry.utcTicks, DateTimeKind.Utc).ToString("yyyy-MM-dd HH:mm");
            }

            GUILayout.BeginHorizontal();
            GUILayout.Label(entry.title, cellStyle, GUILayout.Width(360f));
            GUILayout.Label(entry.playerName, cellStyle, GUILayout.Width(140f));
            GUILayout.Label(utcTime, cellStyle, GUILayout.Width(150f));
            GUILayout.EndHorizontal();
        }
    }
}
