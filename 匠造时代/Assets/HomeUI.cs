using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditorInternal.Profiling.Wasm;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


[System.Serializable]
public class SaveData
{
    public string saveName;
    public string saveTime;
    public string description;
    public string screenshotPath;
}

[System.Serializable]
public class SaveDataList
{
    public List<SaveData> saves = new List<SaveData>();
};


public class HomeUI : MonoBehaviour
{
    public Canvas targetCanvas;
    public Font fontToUse;
    public Sprite buttonRoundedSprite; // Բ��Sprite������Inspector��ֵ

    private Button quitBtnComponent, enterSaveBtnComponent, createSaveBtnComponent, editSaveBtnComponent;

    private DynamicSaveListUI dynamicSaveListUI;

    private GameObject titleText, startButton, savasListText, quitButton, enterSaveButton, createSaveButton, editSaveButton;

    private TMP_FontAsset tmpFontAsset; // ����ʱ������TMP������Դ

    void Start()
    {
        // ����ṩ�˴�ͳFont�����Դ�����Ӧ��TMP_FontAsset�Թ�TextMeshProʹ��
        if (fontToUse != null)
        {
            tmpFontAsset = TMP_FontAsset.CreateFontAsset(fontToUse);
        }

        RenderHome();
    }

    public void ExitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public GameObject NewButton(Vector2 anchor, Vector2 buttonPosition, Vector2 size, string buttonName = "NewButton", string text = "Text", float scale = 2f)
    {
        GameObject btn = new GameObject(buttonName);
        RectTransform buttonRect = btn.AddComponent<RectTransform>();
        btn.transform.SetParent(targetCanvas.transform);
        buttonRect.localScale = new Vector3(scale, scale, scale);
        buttonRect.anchorMin = anchor;
        buttonRect.anchorMax = anchor;
        buttonRect.pivot = anchor;
        buttonRect.anchoredPosition = buttonPosition;
        buttonRect.sizeDelta = size;

        Image buttonImage = btn.AddComponent<Image>();
        if (buttonRoundedSprite != null)
        {
            buttonImage.sprite = buttonRoundedSprite;
            buttonImage.type = Image.Type.Sliced; // ֧�־Ź�������
        }

        Button buttonComponent = btn.AddComponent<Button>();

        // ������Object��Ϊ��ť�ı�
        GameObject textGO = new GameObject("Text");
        RectTransform textRect = textGO.AddComponent<RectTransform>();
        textGO.transform.SetParent(btn.transform);
        textRect.localScale = Vector3.one;
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.pivot = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = Vector2.zero;
        textRect.sizeDelta = Vector2.zero;

        // Ϊ��Object���TextMeshProUGUI���������
        TextMeshProUGUI textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = 27;
        textComponent.alignment = TextAlignmentOptions.Center;
        textComponent.color = Color.white;
        textComponent.enableWordWrapping = true;
        if (tmpFontAsset != null)
        {
            textComponent.font = tmpFontAsset;
            textComponent.fontStyle = FontStyles.Bold;
        }

        buttonComponent.targetGraphic = buttonImage;
        return btn;
    }


    public GameObject NewText(Vector2 anchor, Vector2 position, string textName = "NewText", string textContent = "Text", int fontSize = 27)
    {
        // anchorΪ(0.5,0.5)��ê����������
        GameObject text = new GameObject(textName);
        RectTransform rectTransform = text.AddComponent<RectTransform>();
        TextMeshProUGUI textComponent = text.AddComponent<TextMeshProUGUI>();
        text.transform.SetParent(targetCanvas.transform);
        rectTransform.localScale = Vector3.one;
        rectTransform.anchorMin = anchor;
        rectTransform.anchorMax = anchor;
        rectTransform.pivot = anchor;
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = new Vector2(1000, 1000); // ���ÿ��
        textComponent.text = textContent;
        textComponent.fontSize = fontSize;
        textComponent.alignment = TextAlignmentOptions.Top;
        if (tmpFontAsset != null)
        {
            textComponent.font = tmpFontAsset;
        }
        return text;
    }


    void RenderHome()
    {
        // ��Ⱦ��ҳ
        if (titleText != null || startButton != null)
        {
            return;
        }
        titleText = NewText(new Vector2(0.5f, 1f), new Vector2(0, -30), "GameTitle", "����ʱ��\r\nArtisanEra", 70);
        // ͨ��TextMeshProUGUI���������ɫ��������ʽ
        TextMeshProUGUI titleComp = titleText.GetComponent<TextMeshProUGUI>();
        if (titleComp != null)
        {
            titleComp.color = Color.black;   // ����������ɫ
            titleComp.fontStyle = FontStyles.Bold; // ����Ϊ����
        }

        startButton = NewButton(new Vector2(0.5f, 0.5f), new Vector2(0, 0), new Vector2(160, 50), "StartGameButton", "��ʼ��Ϸ", 2f);
        Button buttonComponent = startButton.GetComponent<Button>();
        buttonComponent.onClick.AddListener(OnStartGameButtonClicked);
    }

    void OnStartGameButtonClicked()
    {
        // ���¿�ʼ��Ϸ��ťʱ�Ļص�
        Destroy(titleText.gameObject);
        Destroy(startButton.gameObject);
        titleText = startButton = null;
        RenderSavasList();
    }

    void RenderSavasList()
    {
        // ��Ⱦ�浵�б����

        savasListText = NewText(new Vector2(0.5f, 1f), new Vector2(0, -30), "SavesListText", "�浵�б�", 40);
        // ͨ��TextMeshProUGUI���������ɫ��������ʽ
        TextMeshProUGUI textComp = savasListText.GetComponent<TextMeshProUGUI>();
        if (textComp != null)
        {
            textComp.color = Color.black;   // ����������ɫ
            textComp.fontStyle = FontStyles.Bold; // ����Ϊ����
        }
        
        quitButton = NewButton(new Vector2(0.5f, 0f), new Vector2(330, 20), new Vector2(160, 50), "QuitButton", "����", 1f);
        quitBtnComponent = quitButton.GetComponent<Button>();
        quitBtnComponent.onClick.AddListener(OnQuitButtonClicked);

        enterSaveButton = NewButton(new Vector2(0.5f, 0f), new Vector2(-70, 20), new Vector2(250, 50), "EnterSaveButton", "����ѡ�еĴ浵", 1f);
        enterSaveBtnComponent = enterSaveButton.GetComponent<Button>();
        enterSaveBtnComponent.onClick.AddListener(OnEnterSaveButtonClicked);

        createSaveButton = NewButton(new Vector2(0.5f, 0f), new Vector2(150, 20), new Vector2(160, 50), "CreateSaveButton", "�����浵", 1f);
        createSaveBtnComponent = createSaveButton.GetComponent<Button>();
        createSaveBtnComponent.onClick.AddListener(OnCreateSaveButtonClicked);

        editSaveButton = NewButton(new Vector2(0.5f, 0f), new Vector2(-290, 20), new Vector2(160, 50), "EditSaveButton", "�༭�浵", 1f);
        editSaveBtnComponent = editSaveButton.GetComponent<Button>();
        editSaveBtnComponent.onClick.AddListener(OnEditSaveButtonClicked);

        enterSaveBtnComponent.interactable = false;
        editSaveBtnComponent.interactable = false;

        // �� HomeUI �� RenderSavasList �����У����� dynamicSaveListUI �� homeUI �ֶ�
        dynamicSaveListUI = targetCanvas.gameObject.AddComponent<DynamicSaveListUI>();
        dynamicSaveListUI.canvas = targetCanvas;
        dynamicSaveListUI.tmpFontAsset = tmpFontAsset;
        dynamicSaveListUI.homeUI = this; // ������������������
        dynamicSaveListUI.CreateUI();
    }

    public void SelectSaveed()
    {
        enterSaveBtnComponent.interactable = true;
        editSaveBtnComponent.interactable = true;
    }

    void DestorySavasList()
    {
        // ���ٴ浵�б����
        Destroy(savasListText.gameObject);
        Destroy(quitButton.gameObject);
        Destroy(enterSaveButton.gameObject);
        Destroy(createSaveButton.gameObject);
        Destroy(editSaveButton.gameObject);
        savasListText = quitButton = enterSaveButton = createSaveButton = editSaveButton = null;

        // ���� DynamicSaveListUI ������ UI ���Ƴ����
        if (dynamicSaveListUI != null)
        {
            dynamicSaveListUI.DestroyUI();
            dynamicSaveListUI = null;
        }
    }

    void OnQuitButtonClicked()
    {
        // ���·��ذ�ťʱ�Ļص�
        DestorySavasList();
        RenderHome();
    }

    void OnEnterSaveButtonClicked()
    {
        // ���½���ѡ�еĴ浵��ťʱ�Ļص�
        Debug.Log("����ѡ�еĴ浵...");
        FunctionManager.instance.LockMouse(true);
        DestorySavasList();
        FunctionManager.instance.Show3DScene();
        FunctionManager.instance.DeleteAllUI();
    }

    void OnCreateSaveButtonClicked()
    {
        // ���´����浵��ťʱ�Ļص�
        DestorySavasList();
    }

    void OnEditSaveButtonClicked()
    {
        // ���±༭�浵��ťʱ�Ļص�
        DestorySavasList();
    }
}


public class DynamicSaveListUI : MonoBehaviour
{
    [Header("UI Settings")]
    public int itemSpacing = 10;
    public Vector2 scrollViewSize = new Vector2(1000, 500);
    public Vector2 itemSize = new Vector2(560, 100);
    public Vector2 screenshotSize = new Vector2(80, 80);

    [Header("Appearance")]
    public Color backgroundColor = new Color(0.9f, 0.9f, 0.9f, 1f);
    public Color itemColor = new Color(1f, 1f, 1f, 1f);
    public Color selectedItemColor = new Color(0.8f, 0.9f, 1f, 1f);

    public Canvas canvas;
    public TMP_FontAsset tmpFontAsset; // ��HomeUI���������
    public HomeUI homeUI; // ���� HomeUI ������

    private GameObject scrollView;
    private GameObject viewport;
    private GameObject content;
    private ScrollRect scrollRect;
    private Dictionary<GameObject, SaveData> saveItemMap = new Dictionary<GameObject, SaveData>();
    private GameObject selectedSaveItem;



    public void CreateUI()
    {
        CreateScrollView();
        CreateScrollViewComponents();
        LoadAndDisplaySaves();
    }

    void CreateScrollView()
    {
        // ����ScrollView����
        scrollView = new GameObject("ScrollView");
        scrollView.transform.SetParent(canvas.transform);

        // ����RectTransform
        RectTransform scrollViewRT = scrollView.AddComponent<RectTransform>();
        scrollViewRT.sizeDelta = scrollViewSize;
        scrollViewRT.anchorMin = new Vector2(0.5f, 0.5f);
        scrollViewRT.anchorMax = new Vector2(0.5f, 0.5f);
        scrollViewRT.pivot = new Vector2(0.5f, 0.5f);
        scrollViewRT.anchoredPosition = Vector2.zero;

        // ���Image�����Ϊ����
        Image scrollViewImage = scrollView.AddComponent<Image>();
        scrollViewImage.color = backgroundColor;
        scrollViewImage.type = Image.Type.Sliced;

        // ���ScrollRect���
        scrollRect = scrollView.AddComponent<ScrollRect>();
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
        scrollRect.movementType = ScrollRect.MovementType.Clamped;
        scrollRect.decelerationRate = 0.135f;
        scrollRect.scrollSensitivity = 25f;
    }

    void CreateScrollViewComponents()
    {
        // ����Viewport
        viewport = new GameObject("Viewport");
        viewport.transform.SetParent(scrollView.transform);

        RectTransform viewportRT = viewport.AddComponent<RectTransform>();
        viewportRT.anchorMin = Vector2.zero;
        viewportRT.anchorMax = Vector2.one;
        viewportRT.sizeDelta = Vector2.zero;
        viewportRT.pivot = Vector2.zero;
        viewportRT.anchoredPosition = Vector2.zero;

        // ���Mask���
        Mask viewportMask = viewport.AddComponent<Mask>();
        viewportMask.showMaskGraphic = false;
        viewport.AddComponent<Image>();

        // ����Content
        content = new GameObject("Content");
        content.transform.SetParent(viewport.transform);

        RectTransform contentRT = content.AddComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);
        contentRT.anchoredPosition = Vector2.zero;

        // ���Vertical Layout Group
        VerticalLayoutGroup layoutGroup = content.AddComponent<VerticalLayoutGroup>();
        layoutGroup.padding = new RectOffset(20, 20, 20, 20);
        layoutGroup.spacing = itemSpacing;
        layoutGroup.childAlignment = TextAnchor.UpperCenter;
        layoutGroup.childControlWidth = true;
        layoutGroup.childControlHeight = false;
        layoutGroup.childForceExpandWidth = true;
        layoutGroup.childForceExpandHeight = false;

        // ���Content Size Fitter
        ContentSizeFitter sizeFitter = content.AddComponent<ContentSizeFitter>();
        sizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        sizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;

        // ����ScrollRect���
        scrollRect.viewport = viewportRT;
        scrollRect.content = contentRT;

        // ����Scrollbar
        CreateScrollbar();
    }

    void CreateScrollbar()
    {
        // ������ֱ������
        GameObject scrollbar = new GameObject("Scrollbar");
        scrollbar.transform.SetParent(scrollView.transform);

        RectTransform scrollbarRT = scrollbar.AddComponent<RectTransform>();
        scrollbarRT.anchorMin = new Vector2(1, 0);
        scrollbarRT.anchorMax = new Vector2(1, 1);
        scrollbarRT.pivot = new Vector2(1, 0.5f);
        scrollbarRT.sizeDelta = new Vector2(20, 0);
        scrollbarRT.anchoredPosition = Vector2.zero;

        Scrollbar scrollbarComponent = scrollbar.AddComponent<Scrollbar>();
        scrollbarComponent.direction = Scrollbar.Direction.BottomToTop;

        // ������������
        GameObject slidingArea = new GameObject("Sliding Area");
        slidingArea.transform.SetParent(scrollbar.transform);

        RectTransform slidingAreaRT = slidingArea.AddComponent<RectTransform>();
        slidingAreaRT.anchorMin = Vector2.zero;
        slidingAreaRT.anchorMax = Vector2.one;
        slidingAreaRT.sizeDelta = Vector2.zero;
        slidingAreaRT.anchoredPosition = Vector2.zero;

        // �����ֱ�
        GameObject handle = new GameObject("Handle");
        handle.transform.SetParent(slidingArea.transform);

        RectTransform handleRT = handle.AddComponent<RectTransform>();
        handleRT.anchorMin = new Vector2(0, 0);
        handleRT.anchorMax = new Vector2(1, 1);
        handleRT.sizeDelta = new Vector2(-10, -10);
        handleRT.anchoredPosition = Vector2.zero;

        Image handleImage = handle.AddComponent<Image>();
        handleImage.color = new Color(0.8f, 0.8f, 0.8f, 1f);
        handleImage.type = Image.Type.Sliced;

        // ���ù�����
        scrollbarComponent.targetGraphic = handleImage;
        scrollbarComponent.handleRect = handleRT;

        // ����ScrollRect�Ĺ�����
        scrollRect.verticalScrollbar = scrollbarComponent;
    }

    void LoadAndDisplaySaves()
    {
        // ģ����ش浵���� - ʵ��Ӧ����Ӧ���ļ���ȡ
        List<SaveData> saveDataList = LoadSaveData();

        foreach (SaveData saveData in saveDataList)
        {
            CreateSaveItem(saveData);
        }
    }

    List<SaveData> LoadSaveData()
    {
        // ����Ӧ���ǴӴ��̼��ش浵���ݵ��߼�
        // ������ģ������
        return new List<SaveData>
        {
            new SaveData { saveName = "�浵��1", saveTime = "2023-10-01 12:30", description = "He110 wOrLd?", screenshotPath = "Save1_Icon.png" },
            new SaveData { saveName = "�Զ��浵", saveTime = "2023-10-01 13:45", description = "114???", screenshotPath = "ButtonImage.png" },
            new SaveData { saveName = "����Bossǰ", saveTime = "2023-10-02 15:20", description = "���������̣�", screenshotPath = "ButtonImage.png" }
        };
    }

    void CreateSaveItem(SaveData saveData)
    {
        // �����浵������
        GameObject saveItem = new GameObject("SaveItem");
        saveItem.transform.SetParent(content.transform);

        RectTransform itemRT = saveItem.AddComponent<RectTransform>();
        itemRT.sizeDelta = itemSize;

        // ��ӱ���Image
        Image itemImage = saveItem.AddComponent<Image>();
        itemImage.color = itemColor;
        itemImage.type = Image.Type.Sliced;

        // ���Button������ڵ������
        Button itemButton = saveItem.AddComponent<Button>();
        itemButton.onClick.AddListener(() => OnSaveItemClicked(saveItem));

        // ����ˮƽ��������
        GameObject horizontalLayout = new GameObject("HorizontalLayout");
        horizontalLayout.transform.SetParent(saveItem.transform);

        RectTransform layoutRT = horizontalLayout.AddComponent<RectTransform>();
        // ��ê������Ϊ���У�left-middle����������������
        layoutRT.anchorMin = new Vector2(0f, 0.5f);
        layoutRT.anchorMax = new Vector2(0f, 0.5f);
        layoutRT.pivot = new Vector2(0f, 0.5f);
        // ����һ����ȣ�ʹ���������ʾ����ȿ��Ը�����Ҫ������
        layoutRT.sizeDelta = new Vector2(itemSize.x - 20, itemSize.y - 20);
        layoutRT.anchoredPosition = new Vector2(20, 0f);

        HorizontalLayoutGroup horizontalGroup = horizontalLayout.AddComponent<HorizontalLayoutGroup>();
        horizontalGroup.padding = new RectOffset(10, 10, 10, 10);
        horizontalGroup.spacing = 15;
        // ��Ϊ���ж���
        horizontalGroup.childAlignment = TextAnchor.MiddleLeft;
         horizontalGroup.childControlWidth = false;
         horizontalGroup.childControlHeight = false;
         horizontalGroup.childForceExpandWidth = false;
         horizontalGroup.childForceExpandHeight = false;

         ContentSizeFitter layoutFitter = horizontalLayout.AddComponent<ContentSizeFitter>();
         layoutFitter.horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
         layoutFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

         // ����ͼ������
         CreateScreenshotArea(horizontalLayout, saveData);

         // �����ı���Ϣ����
         CreateTextInfoArea(horizontalLayout, saveData);

        // ����ӳ���ϵ
        saveItemMap.Add(saveItem, saveData);
    }

    void CreateScreenshotArea(GameObject parent, SaveData saveData)
    {
        // ������ͼ����
        GameObject screenshotArea = new GameObject("Screenshot");
        screenshotArea.transform.SetParent(parent.transform);

        RectTransform screenshotRT = screenshotArea.AddComponent<RectTransform>();
        screenshotRT.sizeDelta = screenshotSize;

        // ʹ��RawImage����ʾ��ͼ
        RawImage raw = screenshotArea.AddComponent<RawImage>();
        raw.color = Color.white; // Ĭ�ϱ���

        // ����ͨ��SaveManager���ش浵icon
        if (SaveManager.instance != null && !string.IsNullOrEmpty(saveData.saveName))
        {
            Texture2D tex = SaveManager.instance.LoadSaveIcon(saveData.saveName);
            if (tex != null)
            {
                raw.texture = tex;
                raw.uvRect = new Rect(0, 0, 1, 1);
            }
            else
            {
                // ����ļ������ڣ�����Ĭ����ɫ�������ռλͼ
                raw.color = Color.gray;
            }
        }
        else
        {
            raw.color = Color.gray;
        }
    }

    void CreateTextInfoArea(GameObject parent, SaveData saveData)
    {
        // �����ı���Ϣ����
        GameObject textArea = new GameObject("TextInfo");
        textArea.transform.SetParent(parent.transform);

        RectTransform textRT = textArea.AddComponent<RectTransform>();
        textRT.sizeDelta = new Vector2(400, 80);

        VerticalLayoutGroup textLayout = textArea.AddComponent<VerticalLayoutGroup>();
        textLayout.childAlignment = TextAnchor.MiddleLeft;
        textLayout.childControlWidth = true;
        textLayout.childControlHeight = true;
        textLayout.childForceExpandWidth = true;
        textLayout.childForceExpandHeight = true;

        // �����浵�����ı�
        CreateTextElement(textArea.transform, saveData.saveName, 24, FontStyles.Bold, TextAlignmentOptions.Left);

        // ���������ı�
        CreateTextElement(textArea.transform, saveData.description, 18, FontStyles.Normal, TextAlignmentOptions.Left);

        // �����浵ʱ���ı�
        CreateTextElement(textArea.transform, saveData.saveTime, 15, FontStyles.Normal, TextAlignmentOptions.Left);
    }

    void CreateTextElement(Transform parent, string text, int fontSize, FontStyles fontStyle, TextAlignmentOptions alignment)
    {
        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(parent);

        TextMeshProUGUI textComponent = textGO.AddComponent<TextMeshProUGUI>();
        textComponent.text = text;
        textComponent.fontSize = fontSize;
        textComponent.fontStyle = fontStyle;
        textComponent.alignment = alignment;
        textComponent.color = Color.black;
        textComponent.enableWordWrapping = true;
        textComponent.overflowMode = TextOverflowModes.Truncate;

        // ���ò���Ԫ��
        LayoutElement layoutElement = textGO.AddComponent<LayoutElement>();
        layoutElement.preferredHeight = fontSize + 4;

        if (tmpFontAsset != null)
        {
            textComponent.font = tmpFontAsset;
        }
    }

    void OnSaveItemClicked(GameObject saveItem)
    {
        // ȡ��֮ǰѡ�е���
        if (selectedSaveItem != null)
        {
            selectedSaveItem.GetComponent<Image>().color = itemColor;
        }

        // ���õ�ǰѡ�е���
        selectedSaveItem = saveItem;
        saveItem.GetComponent<Image>().color = selectedItemColor;

        // ��ȡ��Ӧ�Ĵ浵����
        if (saveItemMap.TryGetValue(saveItem, out SaveData saveData))
        {
            if (homeUI != null)
            {
                homeUI.SelectSaveed();
            }
        }
    }


    // ��̬����´浵��ķ���
    public void AddNewSaveItem(SaveData newSaveData)
    {
        CreateSaveItem(newSaveData);
    }

    // ������д浵��ķ���
    public void ClearAllSaveItems()
    {
        foreach (Transform child in content.transform)
        {
            Destroy(child.gameObject);
        }
        saveItemMap.Clear();
        selectedSaveItem = null;
    }

    // ����������������̬UI��������Դ
    public void DestroyUI()
    {
        // ��������ӳ��
        saveItemMap.Clear();
        selectedSaveItem = null;

        // ���ٴ�����UI����scrollView����viewport��content��
        if (scrollView != null)
        {
            Destroy(scrollView);
            scrollView = null;
        }
        else
        {
            if (content != null)
            {
                Destroy(content);
                content = null;
            }
            if (viewport != null)
            {
                Destroy(viewport);
                viewport = null;
            }
        }

        // ��������������Canvas�ϣ��Ƴ������
        Destroy(this);
    }
}