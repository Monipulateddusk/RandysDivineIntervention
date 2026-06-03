using System.Collections.Generic;
using UnityEngine;

public class CursorManager
{
    private static CursorManager instance;
    public static CursorManager Instance {  get { return instance; } }

    private GameObject cursorObject;
    UnityEngine.UI.Image cursorObjectImageComp;

    private readonly Dictionary<CursorIcons, Sprite> CursorIconDict = new();

    Vector3 previousCursorPosition;
    void InitaliseDictionary(Texture2D cursorTextures)
    {
        for(int i = 0; i < (int)CursorIcons.END; i++)
        {
            int x = 32 * i;
            Sprite spr = Sprite.Create(cursorTextures, new Rect(x, 0, 32, 32), new Vector2(0.5f, 0.5f), 64);
            this.CursorIconDict.Add((CursorIcons)i, spr);
        }
    }

    void InitaliseCursorObject(Transform parentTransform, GameObject cursorPrefab)
    {
        /*  Only allow one cursor object at a time. */
        GameObject instanciatedCursorObject = GameObject.Find("Cursor(Clone)");
        if (instanciatedCursorObject == null) { this.cursorObject = GameObject.Instantiate(cursorPrefab); }
        else { this.cursorObject =  instanciatedCursorObject; }

        this.cursorObject.transform.SetParent(parentTransform);

        /*  Get the Image component and assign it */
        this.cursorObjectImageComp = this.cursorObject.transform.GetChild(0).GetComponent<UnityEngine.UI.Image>();
        this.cursorObjectImageComp.raycastTarget = false;
        SetCursorImageState(CursorIcons.Cursor);

       // tmpUGUI = this.cursorObject.GetComponentsInChildren<TextMeshProUGUI>().FirstOrDefault();
    }

    public CursorManager(Transform parentTransform, GameObject cursorObject, Texture2D cursorTextures)
    {
        InitaliseDictionary(cursorTextures);
        InitaliseCursorObject(parentTransform, cursorObject);

        if (instance == null)
        {
            instance = this;
        }          
    }

    public void OnDestroy()
    {
        if (instance != null && instance == this)
        {
            instance = null;
        }
    }

    public void SetCursorImageState(CursorIcons icon)
    {
        this.cursorObjectImageComp.sprite = CursorIconDict[icon];

    }

    void UpdateCursorPosition()
    {
        if(previousCursorPosition != Input.mousePosition)
        {
            previousCursorPosition = Input.mousePosition;
            this.cursorObject.transform.position = previousCursorPosition;
            //tmpUGUI.text = "XPos: " + this.cursorObject.transform.position.x + "YPos: " + this.cursorObject.transform.position.y;

           
        }
    }
    public void Update()
    {
        UpdateCursorPosition();
    }

    void GetMousePositionAccountAspect()
    {

    }

    public Vector3 GetPreviousMousePosition() => previousCursorPosition;
    public bool HasCursorMoved(Vector3 mousePos) { return previousCursorPosition != mousePos; }
}
