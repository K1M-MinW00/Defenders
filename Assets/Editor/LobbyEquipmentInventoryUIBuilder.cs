using System.Linq;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LobbyEquipmentInventoryUIBuilder
{
    private const string ScenePath = "Assets/Scenes/LobbyScene.unity";
    private static TMP_FontAsset font;
    private static Sprite frameSprite;
    private static readonly Color Navy = new(.08f,.16f,.24f,1f);
    private static readonly Color Pale = new(.84f,.91f,.94f,1f);
    private static readonly Color Blue = new(.10f,.47f,.92f,1f);
    private static readonly Color Teal = new(.26f,.80f,.76f,1f);
    private static readonly Color Red = new(.93f,.16f,.22f,1f);

    [MenuItem("Tools/UI/Rebuild Lobby Equipment Inventory")]
    public static void Build()
    {
        Scene scene=EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        InventoryPanelView inventory=Object.FindFirstObjectByType<InventoryPanelView>(FindObjectsInactive.Include);
        if(inventory==null)throw new System.InvalidOperationException("InventoryPanelView not found.");
        font=Object.FindObjectsByType<TextMeshProUGUI>(FindObjectsInactive.Include,FindObjectsSortMode.None).Select(x=>x.font).FirstOrDefault(x=>x!=null);
        frameSprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Generated/Lobby/item_slot_frame_9slice.png");

        EquipmentInventoryPanelView oldView=inventory.GetComponentInChildren<EquipmentInventoryPanelView>(true);if(oldView!=null)Object.DestroyImmediate(oldView.gameObject);
        Transform popup=inventory.transform.Find("ModalCard")??inventory.transform.Find("Inventory_Popup");
        if(popup==null)throw new System.InvalidOperationException("Inventory modal card not found.");
        Transform previousHost=popup.Find("InventoryContentHost");
        Transform oldScroll=popup.Find("Scroll View");
        if(oldScroll==null)oldScroll=popup.Find("ConsumableInventoryBody");
        if(oldScroll==null&&previousHost!=null)oldScroll=previousHost.Find("ConsumableInventoryBody");
        if(oldScroll==null)throw new System.InvalidOperationException("Inventory scroll view not found.");
        oldScroll.SetParent(popup,false);
        if(previousHost!=null)Object.DestroyImmediate(previousHost.gameObject);
        oldScroll.name="ConsumableInventoryBody";
        Transform oldMaterial=popup.Find("MaterialInventoryBody");if(oldMaterial!=null)Object.DestroyImmediate(oldMaterial.gameObject);
        GameObject host=new("InventoryContentHost",typeof(RectTransform),typeof(LayoutElement));host.layer=5;host.transform.SetParent(popup,false);LayoutElement hostLayout=host.GetComponent<LayoutElement>();hostLayout.flexibleHeight=1;hostLayout.minHeight=100;SetRect(R(host),Vector2.zero,new Vector2(800,100),new Vector2(0,1),new Vector2(.5f,.5f));
        LayoutElement oldScrollLayout=oldScroll.GetComponent<LayoutElement>();if(oldScrollLayout!=null)Object.DestroyImmediate(oldScrollLayout);
        oldScroll.SetParent(host.transform,false);Stretch(R(oldScroll));
        GameObject materialBody=Object.Instantiate(oldScroll.gameObject,host.transform);materialBody.name="MaterialInventoryBody";materialBody.SetActive(false);Stretch(R(materialBody));
        ScrollRect consumableScroll=oldScroll.GetComponent<ScrollRect>();ScrollRect materialScroll=materialBody.GetComponent<ScrollRect>();

        GameObject root=Panel("EquipmentInventoryUI",host.transform,Pale);Stretch(R(root));
        EquipmentInventoryPanelView view=root.AddComponent<EquipmentInventoryPanelView>();

        TMP_Text capacity=Text("Capacity",root.transform,"0 / 100",42,TextAlignmentOptions.Center,Navy); SetRect(R(capacity),new Vector2(20,-20),new Vector2(250,90),new Vector2(0,1));
        Button dismantle=Button("DismantleButton",root.transform,"분해",Teal,new Vector2(300,-20),new Vector2(240,90),new Vector2(0,1));
        Button filter=Button("FilterButton",root.transform,"▼",new Color(.18f,.43f,.45f),new Vector2(-20,-20),new Vector2(110,90),new Vector2(1,1));
        ScrollRect mainScroll=Scroll("EquipmentScroll",root.transform,new Vector2(20,20),new Vector2(-20,-130));
        GridLayoutGroup mainGrid=mainScroll.content.gameObject.AddComponent<GridLayoutGroup>();ConfigureGrid(mainGrid);
        EquipmentSlotView slotTemplate=Slot("EquipmentSlotTemplate",root.transform);slotTemplate.gameObject.SetActive(false);
        view.ConfigureMain(capacity,dismantle,filter,mainScroll.content,slotTemplate);

        BuildDetail(root.transform,view,slotTemplate);
        BuildDismantle(root.transform,view,slotTemplate);
        BuildFilter(root.transform,view);
        BuildRewardOverlay(root.transform,view);

        Transform buttons=popup.Find("Buttons_Group");
        popup.Find("Title")?.SetSiblingIndex(0);host.transform.SetSiblingIndex(1);if(buttons!=null)buttons.SetSiblingIndex(2);
        SetLayoutSize(popup.Find("Title"),130,0);SetLayoutSize(buttons,145,0);
        VerticalLayoutGroup popupLayout=popup.GetComponent<VerticalLayoutGroup>();if(popupLayout!=null){popupLayout.childControlWidth=true;popupLayout.childForceExpandWidth=true;popupLayout.childControlHeight=true;popupLayout.childForceExpandHeight=false;popupLayout.spacing=12;popupLayout.padding=new RectOffset(25,25,25,25);}
        Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)popup);
        inventory.ConfigureInventoryBodies(oldScroll.gameObject,consumableScroll.content,materialBody,materialScroll.content);
        inventory.ConfigureEquipmentPanel(view);root.SetActive(false);
        EditorUtility.SetDirty(inventory);EditorUtility.SetDirty(view);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);
        LobbyUIRefactorMigration.Run();
        Debug.Log("[LobbyEquipmentInventoryUIBuilder] Equipment inventory UI rebuilt.");
    }

    private static void SetLayoutSize(Transform target,float preferred,float flexible)
    {
        if(target==null)return;LayoutElement layout=target.GetComponent<LayoutElement>()??target.gameObject.AddComponent<LayoutElement>();layout.preferredHeight=preferred;layout.flexibleHeight=flexible;
    }

    public static void DumpInventoryHierarchy()
    {
        EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        InventoryPanelView inventory=Object.FindFirstObjectByType<InventoryPanelView>(FindObjectsInactive.Include);
        Dump(inventory.transform,0);
    }

    public static void ValidateInventoryRects()
    {
        EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        InventoryPanelView inventory=Object.FindFirstObjectByType<InventoryPanelView>(FindObjectsInactive.Include);
        int invalid=0;
        EquipmentInventoryPanelView equipment=inventory.GetComponentInChildren<EquipmentInventoryPanelView>(true);
        foreach(RectTransform rect in equipment.GetComponentsInChildren<RectTransform>(true))
        {
            bool fixedWidth=Mathf.Approximately(rect.anchorMin.x,rect.anchorMax.x);
            bool fixedHeight=Mathf.Approximately(rect.anchorMin.y,rect.anchorMax.y);
            if((fixedWidth&&rect.sizeDelta.x<=0f)||(fixedHeight&&rect.sizeDelta.y<=0f))
            { Debug.LogError($"[InventoryRect] Invalid fixed size: {GetPath(rect)} size={rect.sizeDelta} anchors={rect.anchorMin}/{rect.anchorMax}");invalid++; }
        }
        Debug.Log($"[InventoryRect] Validation complete. Invalid={invalid}");
    }

    public static void DumpInventoryLayout()
    {
        EditorSceneManager.OpenScene(ScenePath,OpenSceneMode.Single);
        InventoryPanelView inventory=Object.FindFirstObjectByType<InventoryPanelView>(FindObjectsInactive.Include);
        bool wasActive=inventory.gameObject.activeSelf;inventory.gameObject.SetActive(true);
        RectTransform popup=(RectTransform)inventory.transform.Find("Inventory_Popup");
        Canvas.ForceUpdateCanvases();LayoutRebuilder.ForceRebuildLayoutImmediate(popup);
        foreach(Transform child in popup)
        {
            RectTransform rect=(RectTransform)child;
            Debug.Log($"[InventoryLayout] {child.name} active={child.gameObject.activeSelf} rect={rect.rect} pos={rect.anchoredPosition} anchors={rect.anchorMin}/{rect.anchorMax}");
            if(child.name=="Buttons_Group")foreach(Transform button in child){RectTransform br=(RectTransform)button;Debug.Log($"[InventoryLayout]   {button.name} rect={br.rect} pos={br.anchoredPosition} anchors={br.anchorMin}/{br.anchorMax}");}
        }
        inventory.gameObject.SetActive(wasActive);
    }

    private static string GetPath(Transform target)
    {
        string path=target.name;while(target.parent!=null){target=target.parent;path=$"{target.name}/{path}";}return path;
    }

    private static void Dump(Transform node,int depth)
    {
        string components=string.Join(",",node.GetComponents<Component>().Where(x=>x!=null).Select(x=>x.GetType().Name));
        Debug.Log($"[InventoryHierarchy] {new string(' ',depth*2)}{node.name} [{components}]");
        foreach(Transform child in node)Dump(child,depth+1);
    }

    private static void BuildDetail(Transform parent,EquipmentInventoryPanelView view,EquipmentSlotView template)
    {
        GameObject overlay=Overlay("EquipmentDetailOverlay",parent);GameObject card=Panel("Card",overlay.transform,Color.white);SetRect(R(card),Vector2.zero,new Vector2(820,1250),new Vector2(.5f,.5f));
        Image header=Panel("RarityHeader",card.transform,RarityPalette.Legend).GetComponent<Image>();SetHorizontalRect(R(header),0,0,0,115,1,1);
        TMP_Text rarity=Text("Rarity",header.transform,"전설",40,TextAlignmentOptions.Center,Color.white);Stretch(R(rarity));
        TMP_Text name=Text("Name",card.transform,"장비 이름",48,TextAlignmentOptions.Center,Navy);SetRect(R(name),new Vector2(0,-150),new Vector2(700,80),new Vector2(.5f,1));
        EquipmentSlotView slot=Slot("DetailSlot",card.transform);SetRect(R(slot),new Vector2(0,-270),new Vector2(240,240),new Vector2(.5f,1));
        GameObject effect=Panel("Effect",card.transform,Navy);SetRect(R(effect),new Vector2(0,-545),new Vector2(680,115),new Vector2(.5f,1));
        Image effectIcon=Image("EffectIcon",effect.transform,Color.white);effectIcon.preserveAspect=true;SetRect(R(effectIcon),new Vector2(20,0),new Vector2(75,75),new Vector2(0,.5f));
        TMP_Text effectName=Text("EffectName",effect.transform,"최대 체력",34,TextAlignmentOptions.Left,Color.white);SetRect(R(effectName),new Vector2(115,0),new Vector2(260,90),new Vector2(0,.5f));
        TMP_Text effectValue=Text("EffectValue",effect.transform,"+500",42,TextAlignmentOptions.Right,Color.white);SetRect(R(effectValue),new Vector2(-25,0),new Vector2(220,90),new Vector2(1,.5f));
        TMP_Text description=Text("Description",card.transform,"장비 설명",30,TextAlignmentOptions.TopLeft,Navy);description.textWrappingMode=TextWrappingModes.Normal;GameObject descriptionBg=Panel("DescriptionBg",card.transform,new Color(.78f,.84f,.88f));SetRect(R(descriptionBg),new Vector2(0,-750),new Vector2(680,250),new Vector2(.5f,1));description.transform.SetParent(descriptionBg.transform,false);SetStretch(R(description),25,25,25,25);
        Button cancel=Button("Cancel",card.transform,"취소",new Color(.52f,.55f,.57f),new Vector2(25,30),new Vector2(350,115),new Vector2(0,0));
        Button dismantle=Button("Dismantle",card.transform,"분해",Red,new Vector2(-25,30),new Vector2(350,115),new Vector2(1,0));
        EquipmentDetailOverlayView detailView=overlay.AddComponent<EquipmentDetailOverlayView>();
        detailView.Configure(header.transform.parent.Find("Label")?.GetComponent<TMP_Text>(),header,rarity,name,slot,effectIcon,effectValue,description,null,null,null,cancel,dismantle,null,null);
        view.ConfigureDetail(detailView);overlay.SetActive(false);
    }

    private static void BuildDismantle(Transform parent,EquipmentInventoryPanelView view,EquipmentSlotView template)
    {
        GameObject overlay=Overlay("EquipmentDismantleOverlay",parent);GameObject card=Panel("Card",overlay.transform,Color.white);SetRect(R(card),Vector2.zero,new Vector2(940,1510),new Vector2(.5f,.5f));
        GameObject header=Panel("Header",card.transform,Blue);SetHorizontalRect(R(header),0,0,0,110,1,1);TMP_Text headerTitle=Text("Title",header.transform,"분해",48,TextAlignmentOptions.Center,Color.white);Stretch(R(headerTitle));
        Button close=Button("Close",header.transform,"×",Red,new Vector2(-12,0),new Vector2(90,90),new Vector2(1,.5f));
        TMP_Text guide=Text("Guide",card.transform,"분해할 장비를 선택하세요.",34,TextAlignmentOptions.Left,Navy);SetRect(R(guide),new Vector2(35,-135),new Vector2(680,70),new Vector2(0,1));
        Button filter=Button("Filter",card.transform,"▼",new Color(.18f,.43f,.45f),new Vector2(-35,-125),new Vector2(100,80),new Vector2(1,1));
        ScrollRect scroll=Scroll("DismantleScroll",card.transform,new Vector2(30,565),new Vector2(-30,-230));GridLayoutGroup grid=scroll.content.gameObject.AddComponent<GridLayoutGroup>();ConfigureGrid(grid,125,10);ResponsiveSquareGrid responsive=scroll.content.gameObject.AddComponent<ResponsiveSquareGrid>();responsive.Configure(5,125);
        GameObject rewardTitleBg=Panel("RewardTitleBg",card.transform,new Color(.13f,.35f,.78f));SetHorizontalRect(R(rewardTitleBg),30,30,440,75,0,0);TMP_Text rewardTitle=Text("RewardTitle",rewardTitleBg.transform,"분해 보상",38,TextAlignmentOptions.Center,Color.white);Stretch(R(rewardTitle));
        GameObject rewards=Panel("Rewards",card.transform,new Color(.65f,.83f,.92f));SetHorizontalRect(R(rewards),30,30,205,220,0,0);HorizontalLayoutGroup rewardLayout=rewards.AddComponent<HorizontalLayoutGroup>();rewardLayout.childAlignment=TextAnchor.MiddleCenter;rewardLayout.spacing=25;rewardLayout.childControlWidth=false;rewardLayout.childControlHeight=false;
        GameObject expBg=Panel("TotalExpBg",card.transform,Navy);SetRect(R(expBg),new Vector2(0,125),new Vector2(540,75),new Vector2(.5f,0));TMP_Text exp=Text("TotalExp",expBg.transform,"합산 경험치 : 0",36,TextAlignmentOptions.Center,Color.white);Stretch(R(exp));
        Button confirm=Button("Confirm",card.transform,"분해",new Color(1f,.20f,.39f),new Vector2(0,15),new Vector2(820,100),new Vector2(.5f,0));
        view.ConfigureDismantle(overlay,scroll.content,filter,rewards.transform,exp,confirm,close);overlay.SetActive(false);
    }

    private static void BuildFilter(Transform parent,EquipmentInventoryPanelView view)
    {
        GameObject overlay=Overlay("EquipmentFilterOverlay",parent);Button backdrop=overlay.GetComponent<Button>();
        GameObject card=Panel("FilterCard",overlay.transform,Color.white);SetRect(R(card),new Vector2(0,80),new Vector2(820,900),new Vector2(.5f,.5f));
        GameObject header=Panel("Header",card.transform,Blue);SetHorizontalRect(R(header),0,0,0,105,1,1);TMP_Text title=Text("Title",header.transform,"필터",45,TextAlignmentOptions.Center,Color.white);Stretch(R(title));
        Button close=Button("Close",header.transform,"×",Red,new Vector2(-10,0),new Vector2(85,85),new Vector2(1,.5f));
        Button all=Button("All",card.transform,"All",new Color(.75f,.82f,.86f),new Vector2(0,-145),new Vector2(700,105),new Vector2(.5f,1));
        Button main=Button("Main",card.transform,"주무기",new Color(.75f,.82f,.86f),new Vector2(0,-270),new Vector2(700,105),new Vector2(.5f,1));
        Button sub=Button("Sub",card.transform,"보조무기",new Color(.75f,.82f,.86f),new Vector2(0,-395),new Vector2(700,105),new Vector2(.5f,1));
        Button helmet=Button("Helmet",card.transform,"헬멧",new Color(.75f,.82f,.86f),new Vector2(0,-520),new Vector2(700,105),new Vector2(.5f,1));
        Button armor=Button("Armor",card.transform,"갑옷",new Color(.75f,.82f,.86f),new Vector2(0,-645),new Vector2(700,105),new Vector2(.5f,1));
        Button reset=Button("Reset",card.transform,"Reset",Blue,new Vector2(0,30),new Vector2(700,100),new Vector2(.5f,0));
        view.ConfigureFilter(overlay,all,main,sub,helmet,armor,reset,close);overlay.SetActive(false);
    }

    private static void BuildRewardOverlay(Transform parent,EquipmentInventoryPanelView view)
    {
        GameObject overlay=Overlay("EquipmentRewardOverlay",parent);Button backdrop=overlay.GetComponent<Button>();if(backdrop!=null)Object.DestroyImmediate(backdrop);
        TMP_Text title=Text("Title",overlay.transform,"분해 보상 획득",56,TextAlignmentOptions.Center,Color.white);SetRect(R(title),new Vector2(0,280),new Vector2(850,100),new Vector2(.5f,.5f));
        GameObject rewards=Panel("RewardSlots",overlay.transform,Color.white);SetRect(R(rewards),Vector2.zero,new Vector2(680,220),new Vector2(.5f,.5f));HorizontalLayoutGroup layout=rewards.AddComponent<HorizontalLayoutGroup>();layout.childAlignment=TextAnchor.MiddleCenter;layout.spacing=35;layout.childControlHeight=false;layout.childControlWidth=false;
        TMP_Text hint=Text("Hint",overlay.transform,"배경을 탭하여 닫기",34,TextAlignmentOptions.Center,Color.white);SetRect(R(hint),new Vector2(0,-270),new Vector2(700,80),new Vector2(.5f,.5f));
        view.ConfigureReward(overlay,rewards.transform);ModalBackdropDismiss dismiss=overlay.AddComponent<ModalBackdropDismiss>();dismiss.Configure(null,overlay,view);overlay.AddComponent<ModalTransition>();overlay.SetActive(false);
    }

    private static EquipmentSlotView Slot(string name,Transform parent)
    {
        GameObject go=new(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(UnityEngine.UI.Image),typeof(Button),typeof(EquipmentSlotView));go.layer=5;go.transform.SetParent(parent,false);R(go).sizeDelta=new Vector2(170,170);Image frame=go.GetComponent<Image>();frame.sprite=frameSprite;frame.type=UnityEngine.UI.Image.Type.Sliced;frame.color=RarityPalette.Rare;
        Image icon=Image("Icon",go.transform,Color.white);SetStretch(R(icon),18,18,18,32);icon.preserveAspect=true;
        TMP_Text level=Text("Level",go.transform,"LV.0",28,TextAlignmentOptions.Bottom,Color.white);SetStretch(R(level),5,5,5,5);level.outlineWidth=.25f;level.outlineColor=Color.black;
        Image overlay=Image("Selected",go.transform,new Color(0,0,0,.45f));Stretch(R(overlay));overlay.raycastTarget=false;
        Image check=Image("Check",go.transform,Color.white);check.sprite=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Art/UI/Generated/Lobby/icon_check.png");check.preserveAspect=true;SetRect(R(check),Vector2.zero,new Vector2(105,105),new Vector2(.5f,.5f));check.raycastTarget=false;
        EquipmentSlotView view=go.GetComponent<EquipmentSlotView>();view.Configure(frame,icon,level,overlay,check,go.GetComponent<Button>());return view;
    }

    private static ScrollRect Scroll(string name,Transform parent,Vector2 offsetMin,Vector2 offsetMax)
    {
        GameObject go=Panel(name,parent,Color.white);RectTransform rect=R(go);rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=offsetMin;rect.offsetMax=offsetMax;
        ScrollRect scroll=go.AddComponent<ScrollRect>();scroll.horizontal=false;
        GameObject viewport=Panel("Viewport",go.transform,Color.white);Stretch(R(viewport));viewport.AddComponent<RectMask2D>();
        GameObject content=new("Content",typeof(RectTransform),typeof(ContentSizeFitter));content.transform.SetParent(viewport.transform,false);RectTransform cr=R(content);cr.anchorMin=new Vector2(0,1);cr.anchorMax=new Vector2(1,1);cr.pivot=new Vector2(.5f,1);cr.sizeDelta=new Vector2(0,10);content.GetComponent<ContentSizeFitter>().verticalFit=ContentSizeFitter.FitMode.PreferredSize;
        scroll.viewport=R(viewport);scroll.content=cr;return scroll;
    }
    private static void ConfigureGrid(GridLayoutGroup grid,float cell=170,float spacing=15){grid.cellSize=new Vector2(cell,cell);grid.spacing=new Vector2(spacing,spacing);grid.padding=new RectOffset(10,10,10,10);grid.constraint=GridLayoutGroup.Constraint.FixedColumnCount;grid.constraintCount=5;grid.childAlignment=TextAnchor.UpperLeft;}
    private static GameObject Overlay(string name,Transform parent){GameObject go=Panel(name,parent,new Color(0,0,0,.78f));Stretch(R(go));go.AddComponent<Button>().transition=Selectable.Transition.None;return go;}
    private static GameObject Panel(string name,Transform parent,Color color){GameObject go=new(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));go.layer=5;go.transform.SetParent(parent,false);go.GetComponent<Image>().color=color;return go;}
    private static Image Image(string name,Transform parent,Color color)=>Panel(name,parent,color).GetComponent<Image>();
    private static TMP_Text Text(string name,Transform parent,string value,float size,TextAlignmentOptions alignment,Color color){GameObject go=new(name,typeof(RectTransform),typeof(CanvasRenderer),typeof(TextMeshProUGUI));go.layer=5;go.transform.SetParent(parent,false);TMP_Text t=go.GetComponent<TMP_Text>();t.font=font;t.text=value;t.fontSize=size;t.alignment=alignment;t.color=color;t.fontStyle=FontStyles.Bold;return t;}
    private static Button Button(string name,Transform parent,string label,Color color,Vector2 position,Vector2 size,Vector2 anchor){GameObject go=Panel(name,parent,color);Button b=go.AddComponent<Button>();b.targetGraphic=go.GetComponent<Image>();SetRect(R(go),position,size,anchor);TMP_Text text=Text("Label",go.transform,label,36,TextAlignmentOptions.Center,Color.black);Stretch(R(text));return b;}
    private static RectTransform R(Object value)=>value switch{GameObject go=>(RectTransform)go.transform,Component c=>(RectTransform)c.transform,_=>null};
    private static void Stretch(RectTransform r){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=Vector2.zero;r.offsetMax=Vector2.zero;}
    private static void SetStretch(RectTransform r,float left,float bottom,float right,float top){r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=new Vector2(left,bottom);r.offsetMax=new Vector2(-right,-top);}
    private static void SetRect(RectTransform r,Vector2 pos,Vector2 size,Vector2 anchor,Vector2? pivot=null){r.anchorMin=anchor;r.anchorMax=anchor;r.pivot=pivot??anchor;r.anchoredPosition=pos;r.sizeDelta=size;}
    private static void SetHorizontalRect(RectTransform r,float left,float right,float y,float height,float anchorY,float pivotY){r.anchorMin=new Vector2(0,anchorY);r.anchorMax=new Vector2(1,anchorY);r.pivot=new Vector2(.5f,pivotY);r.anchoredPosition=new Vector2((left-right)*.5f,y);r.sizeDelta=new Vector2(-(left+right),height);}
}
