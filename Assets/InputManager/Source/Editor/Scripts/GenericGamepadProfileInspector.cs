#region [Copyright (c) 2018 Cristian Alexandru Geambasu]

//	Distributed under the terms of an MIT-style license:
//
//	The MIT License
//
//	Copyright (c) 2018 Cristian Alexandru Geambasu
//
//	Permission is hereby granted, free of charge, to any person obtaining a copy of this software
//	and associated documentation files (the "Software"), to deal in the Software without restriction,
//	including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
//	and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so,
//	subject to the following conditions:
//
//	The above copyright notice and this permission notice shall be included in all copies or substantial
//	portions of the Software.
//
//	THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED,
//	INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR
//	PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE
//	FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE,
//	ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

#endregion [Copyright (c) 2018 Cristian Alexandru Geambasu]
using UnityEngine;
using UnityEditor;
using Luminosity.IO;

namespace LuminosityEditor.IO
{
    [CustomEditor(typeof(GenericGamepadProfile))]
    public class GenericGamepadProfileInspector : Editor
    {
        private SerializedProperty m_name;
        private SerializedProperty m_comment;
        private SerializedProperty m_dpadType;
        private SerializedProperty m_triggerType;
        private SerializedProperty m_leftStickButton;
        private SerializedProperty m_rightStickButton;
        private SerializedProperty m_leftBumperButton;
        private SerializedProperty m_rightBumperButton;
        private SerializedProperty m_leftTriggerButton;
        private SerializedProperty m_rightTriggerButton;
        private SerializedProperty m_dpadUpButton;
        private SerializedProperty m_dpadDownButton;
        private SerializedProperty m_dpadLeftButton;
        private SerializedProperty m_dpadRightButton;
        private SerializedProperty m_backButton;
        private SerializedProperty m_startButton;
        private SerializedProperty m_actionTopButton;
        private SerializedProperty m_actionBottomButton;
        private SerializedProperty m_actionLeftButton;
        private SerializedProperty m_actionRightButton;
        private SerializedProperty m_leftStickXAxis;
        private SerializedProperty m_leftStickYAxis;
        private SerializedProperty m_rightStickXAxis;
        private SerializedProperty m_rightStickYAxis;
        private SerializedProperty m_dpadXAxis;
        private SerializedProperty m_dpadYAxis;
        private SerializedProperty m_leftTriggerAxis;
        private SerializedProperty m_rightTriggerAxis;
        private string[] m_buttonNames;
        private string[] m_axisNames;

        private void OnEnable()
        {
            m_name = serializedObject.FindProperty("m_name");
            m_comment = serializedObject.FindProperty("m_comment");
            m_dpadType = serializedObject.FindProperty("m_dpadType");
            m_triggerType = serializedObject.FindProperty("m_triggerType");
            m_leftStickButton = serializedObject.FindProperty("m_leftStickButton");
            m_rightStickButton = serializedObject.FindProperty("m_rightStickButton");
            m_leftBumperButton = serializedObject.FindProperty("m_leftBumperButton");
            m_rightBumperButton = serializedObject.FindProperty("m_rightBumperButton");
            m_leftTriggerButton = serializedObject.FindProperty("m_leftTriggerButton");
            m_rightTriggerButton = serializedObject.FindProperty("m_rightTriggerButton");
            m_dpadUpButton = serializedObject.FindProperty("m_dpadUpButton");
            m_dpadDownButton = serializedObject.FindProperty("m_dpadDownButton");
            m_dpadLeftButton = serializedObject.FindProperty("m_dpadLeftButton");
            m_dpadRightButton = serializedObject.FindProperty("m_dpadRightButton");
            m_backButton = serializedObject.FindProperty("m_backButton");
            m_startButton = serializedObject.FindProperty("m_startButton");
            m_actionTopButton = serializedObject.FindProperty("m_actionTopButton");
            m_actionBottomButton = serializedObject.FindProperty("m_actionBottomButton");
            m_actionLeftButton = serializedObject.FindProperty("m_actionLeftButton");
            m_actionRightButton = serializedObject.FindProperty("m_actionRightButton");
            m_leftStickXAxis = serializedObject.FindProperty("m_leftStickXAxis");
            m_leftStickYAxis = serializedObject.FindProperty("m_leftStickYAxis");
            m_rightStickXAxis = serializedObject.FindProperty("m_rightStickXAxis");
            m_rightStickYAxis = serializedObject.FindProperty("m_rightStickYAxis");
            m_dpadXAxis = serializedObject.FindProperty("m_dpadXAxis");
            m_dpadYAxis = serializedObject.FindProperty("m_dpadYAxis");
            m_leftTriggerAxis = serializedObject.FindProperty("m_leftTriggerAxis");
            m_rightTriggerAxis = serializedObject.FindProperty("m_rightTriggerAxis");

            m_buttonNames = EditorToolbox.GenerateJoystickButtonNames();
            m_axisNames = EditorToolbox.GenerateJoystickAxisNames();
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            EditorGUILayout.Space();

            DrawHeader("Description");
            EditorGUILayout.PropertyField(m_name);
            EditorGUILayout.PropertyField(m_comment);

            //  SETTINGS
            DrawHeader("Settings");
            EditorGUILayout.PropertyField(m_dpadType);
            EditorGUILayout.PropertyField(m_triggerType);

            //  BUTTONS
            DrawHeader("Buttons");
            DrawButtonField(m_leftStickButton);
            DrawButtonField(m_rightStickButton);
            DrawButtonField(m_leftBumperButton);
            DrawButtonField(m_rightBumperButton);

            if(m_triggerType.enumValueIndex == (int)GamepadTriggerType.Button)
            {
                DrawButtonField(m_leftTriggerButton);
                DrawButtonField(m_rightTriggerButton);
            }

            if(m_dpadType.enumValueIndex == (int)GamepadDPadType.Button)
            {
                DrawButtonField(m_dpadUpButton);
                DrawButtonField(m_dpadDownButton);
                DrawButtonField(m_dpadLeftButton);
                DrawButtonField(m_dpadRightButton);
            }

            DrawButtonField(m_backButton);
            DrawButtonField(m_startButton);
            DrawButtonField(m_actionTopButton);
            DrawButtonField(m_actionBottomButton);
            DrawButtonField(m_actionLeftButton);
            DrawButtonField(m_actionRightButton);

            //  AXES
            DrawHeader("Axes");
            DrawAxisField(m_leftStickXAxis);
            DrawAxisField(m_leftStickYAxis);
            DrawAxisField(m_rightStickXAxis);
            DrawAxisField(m_rightStickYAxis);

            if(m_triggerType.enumValueIndex == (int)GamepadTriggerType.Axis)
            {
                DrawAxisField(m_leftTriggerAxis);
                DrawAxisField(m_rightTriggerAxis);
            }

            if(m_dpadType.enumValueIndex == (int)GamepadDPadType.Axis)
            {
                DrawAxisField(m_dpadXAxis);
                DrawAxisField(m_dpadYAxis);
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawHeader(string label)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
        }

        private void DrawButtonField(SerializedProperty button)
        {
            button.intValue = EditorGUILayout.Popup(button.displayName, button.intValue, m_buttonNames);
        }

        private void DrawAxisField(SerializedProperty axis)
        {
            axis.intValue = EditorGUILayout.Popup(axis.displayName, axis.intValue, m_axisNames);
        }
    }
}