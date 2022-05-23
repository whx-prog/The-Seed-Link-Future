/*
 * Copyright (c) Facebook, Inc. and its affiliates.
 *
 * This source code is licensed under the license found in the
 * LICENSE file in the root directory of this source tree.
 */

using UnityEditor;
using UnityEngine;
using Facebook.WitAi.Data.Configuration;

namespace Facebook.WitAi.Windows
{
    public class WitWindow : WitConfigurationWindow
    {
        protected WitConfigurationEditor witInspector;
        protected string serverToken;
        protected override GUIContent Title => WitStyles.SettingsTitleContent;
        protected override string HeaderUrl => witInspector ? witInspector.HeaderUrl : base.HeaderUrl;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (string.IsNullOrEmpty(serverToken))
            {
                serverToken = WitAuthUtility.ServerToken;
            }
            SetWitEditor();
        }

        protected virtual void SetWitEditor()
        {
            if (witConfiguration)
            {
                witInspector = (WitConfigurationEditor)Editor.CreateEditor(witConfiguration);
                witInspector.drawHeader = false;
                witInspector.Initialize();
            }
        }

        protected override void LayoutContent()
        {
            // Server access token
            GUILayout.BeginHorizontal();
            bool updated = false;
            WitEditorUI.LayoutPasswordField(WitStyles.SettingsServerTokenContent, ref serverToken, ref updated);
            if (updated && witInspector != null)
            {
                witInspector.ApplyServerToken(serverToken);
            }
            if (WitEditorUI.LayoutTextButton(WitStyles.Texts.SettingsRelinkButtonLabel))
            {
                RelinkServerToken();
            }
            if (WitEditorUI.LayoutTextButton(WitStyles.Texts.SettingsAddButtonLabel))
            {
                int newIndex = WitConfigurationUtility.CreateConfiguration(serverToken);
                if (newIndex != -1)
                {
                    SetConfiguration(newIndex);
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(WitStyles.ButtonMargin);

            // Configuration select
            base.LayoutContent();
            // Update inspector if needed
            if (witInspector == null || witInspector.configuration != witConfiguration)
            {
                SetWitEditor();
            }

            // Layout configuration inspector
            if (witConfiguration && witInspector)
            {
                witInspector.OnInspectorGUI();
            }
        }
        // Apply server token
        private void RelinkServerToken()
        {
            // Open Setup if Invalid
            if (!WitConfigurationUtility.IsServerTokenValid(serverToken))
            {
                // Open Setup
                WitWindowUtility.OpenSetupWindow(WitWindowUtility.OpenConfigurationWindow);
                // Close this Window
                Close();
                return;
            }
            // Set server token
            WitConfigurationUtility.SetServerToken(serverToken, (e) =>
            {
                serverToken = WitAuthUtility.ServerToken;
            });
        }
    }
}
