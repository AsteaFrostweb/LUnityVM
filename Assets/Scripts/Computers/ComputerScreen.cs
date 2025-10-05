using UnityEngine;
using TMPro;
using Unity.Mathematics;
using UnityEngine.UI;

public class ComputerScreen
{
    private MeshRenderer screenMesh;
    private int2 screenSize;
    private RenderTexture screenRenderTex;
    private Canvas screenCanvas;
    private Camera screenCamera;
    private TextMeshProUGUI screenText;


    // Hidden text box for calculating wrapped lines
    private TextMeshProUGUI hiddenTextBox;
    public ComputerScreen(MeshRenderer mesh, int2 size)
    {
        screenMesh = mesh;
        screenSize = size;
        CreateScreen();      
    }

    public void CreateScreen()
    {
        screenRenderTex = new RenderTexture(screenSize.x, screenSize.y, 24);
        screenRenderTex.antiAliasing = 1;
        screenRenderTex.name = "screenRenderTex";
        screenRenderTex.Create();

        screenCamera = new GameObject("ScreenCamera", typeof(Camera)).GetComponent<Camera>();
        screenCamera.transform.localPosition = Vector3.zero;
        screenCamera.transform.localRotation = Quaternion.Euler(0, 0, 0);
        screenCamera.orthographic = true;
        screenCamera.orthographicSize = 1;
        screenCamera.targetTexture = screenRenderTex;
        screenCamera.clearFlags = CameraClearFlags.SolidColor;
        screenCamera.backgroundColor = new Color(0.05f, 0.05f, 0.05f, 1f);
        screenCamera.cullingMask = 1 << LayerMask.NameToLayer("UI");

        screenCanvas = new GameObject("ScreenCanvas", typeof(RectTransform), typeof(Canvas)).GetComponent<Canvas>();
        screenCanvas.gameObject.layer = LayerMask.NameToLayer("UI");
        screenCanvas.renderMode = RenderMode.ScreenSpaceCamera;
        screenCanvas.worldCamera = screenCamera;

        screenText = new GameObject("ScreenText", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        screenText.transform.SetParent(screenCanvas.transform, false);
        screenText.textWrappingMode = TextWrappingModes.Normal;
        screenText.rectTransform.anchorMin = new Vector2(0, 0); // Bottom-left corner
        screenText.rectTransform.anchorMax = new Vector2(1, 1); // Top-right corner
        screenText.rectTransform.offsetMin = new Vector2(0, 0); // Left and bottom offsets
        screenText.rectTransform.offsetMax = new Vector2(0, 0); // Right and top offsets
        screenText.fontSize = 20;

        hiddenTextBox = new GameObject("HiddenText", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        hiddenTextBox.transform.SetParent(screenCanvas.transform, false);
        screenText.textWrappingMode = TextWrappingModes.Normal;
        hiddenTextBox.color = new Color(0f, 0f, 0f, 0f);
        hiddenTextBox.rectTransform.anchorMin = new Vector2(0, 0); // Bottom-left corner
        hiddenTextBox.rectTransform.anchorMax = new Vector2(1, 1); // Top-right corner
        hiddenTextBox.rectTransform.offsetMin = new Vector2(0, 0); // Left and bottom offsets
        hiddenTextBox.rectTransform.offsetMax = new Vector2(0, 0); // Right and top offsets
        hiddenTextBox.fontSize = 20;
   

        screenMesh.material.mainTexture = screenRenderTex;
    }

    public void DestroyScreen()
    {
        GameObject.Destroy(screenCanvas.gameObject);
        GameObject.Destroy(screenCamera.gameObject);
        screenRenderTex.Release();
        screenRenderTex = null;
    }

    public void UpdateScreen(string[] lines, int MAX_LINES)
    {
        //Debug.Log("Updating scrren for computer ID:" + );
        if (lines == null || lines.Length == 0 || lines[0] == "") return;

        int remainingLines = MAX_LINES;
        int lastIndex = lines.Length - 1;
        int startIndex = 0;

        // Calculate the start index of non-empty lines
        for (int i = 0; i < lines.Length; i++)
        {
            if (string.IsNullOrEmpty(lines[i]))
            {
                lastIndex = i - 1;
                break;
            }
        }

        // Calculate how many lines fit within MAX_LINES
        for (int i = lastIndex; i >= 0; i--)
        {
            int wrappedLines = CalculateWrappedLines(lines[i]);

            if (remainingLines >= wrappedLines)
            {
                remainingLines -= wrappedLines;
                startIndex = i;
            }
            else
            {
                break;
            }
        }

        //
        string final = string.Join("\n", lines, startIndex, lines.Length - startIndex);
        if (screenText.text != final)
        {
            screenText.text = final;
        }

 
        //Debug.Log("Remaining Lines: " + remainingLines);
    }


    private int CalculateWrappedLines(string text)
    {
        // Clear the hidden text box
        hiddenTextBox.text = "";

        // Set the new text and let it calculate the wrapping
        hiddenTextBox.text = text;
        hiddenTextBox.ForceMeshUpdate();
        // Get the number of wrapped lines
        int wrappedLines = hiddenTextBox.textInfo.lineCount;
      // Debug.Log("Lines: " + wrappedLines.ToString() + " - " + text);
        return wrappedLines;
    }
}
