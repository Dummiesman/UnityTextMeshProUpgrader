using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class TMProUpgrader : MonoBehaviour
{
#if UNITY_EDITOR
    static bool ConvertText(GameObject textObj)
    {
        var text = textObj.GetComponent<Text>();
        if (text == null)
        {
            Debug.Log("No text found to upgrade.");
            return false;
        }

        //search for font asset
        string[] fontNames = text.font.fontNames;
        var results = AssetDatabase.FindAssets("t:TMP_FontAsset");
        TMP_FontAsset chosenFontAsset = null;

        foreach (var result in results)
        {
            string path = AssetDatabase.GUIDToAssetPath(result);
            var asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset is TMP_FontAsset fontAsset)
            {
                if(Array.IndexOf(fontNames, fontAsset.faceInfo.familyName) >= 0)
                {
                    chosenFontAsset = fontAsset;
                    break;
                }
            }
        }

        //can't find font asset'
        if (chosenFontAsset == null)
        {
            Debug.LogError($"Can't find suitable font for upgrade from ({string.Join(",", fontNames)}).");
            return false;
        }

        //preform upgrade!
        //store recttransform since TMP insists on changing it
        var rectTransform = textObj.transform as RectTransform;
        var rt_offMin = rectTransform.offsetMin;
        var rt_offMax = rectTransform.offsetMax;

        //store text properties
        int textSize = text.fontSize;
        int textResizeMaxSize = text.resizeTextMaxSize;
        int textResizeMinSize = text.resizeTextMinSize;
        bool textResize = text.resizeTextForBestFit;
        TextAnchor textAlign = text.alignment;
        FontStyle textStyle = text.fontStyle;
        string textText = text.text;
        Color textColor = text.color;
        bool textAllowRich = text.supportRichText;
        HorizontalWrapMode textHorizontalOverflow = text.horizontalOverflow;
        VerticalWrapMode textVerticalOverflow = text.verticalOverflow;
        float textLineSpacing = text.lineSpacing;
        bool textRaycastTarget = text.raycastTarget;

        //delete text
        DestroyImmediate(text, false);

        //add TMP
        var textTMP = textObj.AddComponent<TextMeshProUGUI>();
        textTMP.font = chosenFontAsset;
        textTMP.richText = textAllowRich;
        textTMP.enableWordWrapping = (textVerticalOverflow == VerticalWrapMode.Overflow) && (textHorizontalOverflow == HorizontalWrapMode.Wrap);
        textTMP.overflowMode = (textHorizontalOverflow == HorizontalWrapMode.Wrap) && (textVerticalOverflow == VerticalWrapMode.Overflow )
            ? TextOverflowModes.Overflow
            : TextOverflowModes.Truncate;
        textTMP.lineSpacing = textLineSpacing; //TODO: finish this!
        textTMP.raycastTarget = textRaycastTarget;

        //setup TMP overflow
        if (textStyle == FontStyle.Bold || textStyle == FontStyle.BoldAndItalic)
            textTMP.fontStyle |= FontStyles.Bold;
        if (textStyle == FontStyle.Italic || textStyle == FontStyle.BoldAndItalic)
            textTMP.fontStyle |= FontStyles.Italic;

        //setup TMP alignment
        switch (textAlign)
        {
            case TextAnchor.LowerCenter:
                textTMP.alignment = TextAlignmentOptions.Bottom;
                break;
            case TextAnchor.LowerLeft:
                textTMP.alignment = TextAlignmentOptions.BottomLeft;
                break;
            case TextAnchor.LowerRight:
                textTMP.alignment = TextAlignmentOptions.BottomRight;
                break;
            case TextAnchor.MiddleCenter:
                textTMP.alignment = TextAlignmentOptions.Center;
                break;
            case TextAnchor.MiddleLeft:
                textTMP.alignment = TextAlignmentOptions.Left;
                break;
            case TextAnchor.MiddleRight:
                textTMP.alignment = TextAlignmentOptions.Right;
                break;
            case TextAnchor.UpperCenter:
                textTMP.alignment = TextAlignmentOptions.Top;
                break;
            case TextAnchor.UpperLeft:
                textTMP.alignment = TextAlignmentOptions.TopLeft;
                break;
            case TextAnchor.UpperRight:
                textTMP.alignment = TextAlignmentOptions.TopRight;
                break;
        }

        //setup resize parameters
        if (textResize)
        {
            textTMP.enableAutoSizing = true;
            textTMP.fontSizeMax = textResizeMaxSize;
            textTMP.fontSizeMin = textResizeMinSize;
        }
        else
        {
            textTMP.fontSize = textSize;
        }

        //replace rich text stuff
        if (textAllowRich)
        {
            textText = textText.Replace("<color=grey", "<color=#808080");
            textText = textText.Replace("<color=gray", "<color=#808080");
            textText = textText.Replace("<color=\"grey\"", "<color=#808080");
            textText = textText.Replace("<color=\"gray\"", "<color=#808080");
        }

        //set text and color
        textTMP.text = textText;
        textTMP.color = textColor;

        //fix up RectTransform (this is ugly :( )
        var tempObj = new GameObject("TMPUpgrader_TempObj");
        var resetter = tempObj.AddComponent<RectTransformReset>();
        resetter.Init(rectTransform, rt_offMin, rt_offMax);

        //DONE!
        return true;
    }

    [MenuItem("Dummiesmans Tools/Upgrade Scene To TMP")]
    static void UpgradeScene()
    {
        bool shouldProceed = EditorUtility.DisplayDialog("Upgrade entire scene?",
                "Are you sure you want to upgrade the entire scene? Note that no undo history will be created, and you should save any unsaved work before running this.", "Convert Entire Scene", "Cancel");
        if (!shouldProceed)
            return;

        var allTextObjs = Resources.FindObjectsOfTypeAll<Text>();
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();

        int numConverted = 0;
        int numFailed = 0;
        foreach (var textObj in allTextObjs)
        {
            var go = textObj.gameObject;
            if (go == null || go.scene != scene)
                continue;
            if (ConvertText(go))
            {
                numConverted++;
            }
            else
            {
                numFailed++;
            }
        }

        if(numConverted > 0)
            Debug.Log($"Successfully converted {numConverted} texts.");
        if(numFailed > 0)
            Debug.Log($"Failed to convert {numFailed} texts.");
        if(numConverted == 0 && numFailed == 0)
            Debug.Log("No texts found to convert.");
    }

    [MenuItem("Dummiesmans Tools/Upgrade Selected To TMP")]
    static void UpgradeSelected()
    {
        var activeTransforms = Selection.transforms;
        foreach (var transform in activeTransforms)
        {
            ConvertText(transform.gameObject);
        }
    }
#endif
}
