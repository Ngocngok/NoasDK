using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using UnityEngine.UI;
using NoasDK.Template;

namespace NoasDK.UI
{

    public enum UIName
    {
        None = 0,
        Gameplay = 1,
        Setting = 2,
        Onboarding = 3,
        Main = 4,
        ARFilterSelection = 5,
        CameraView = 6,
        ARVideoSelection = 7,
        PreviewVideo = 8,
        PreviewPicture = 9,
        SelectLanguage = 10,
        Settings = 11,
        Rate = 12,
        GetPremium = 13,
        Collection = 14,
        Policy = 15,
        Disclaimer = 16,
    }
    public class DataUIBase
    {
        public int rootIdx;
        public int topIdx;
        public string loadPath;

        public DataUIBase(int rootIdx, int topIdx, string loadPath)
        {
            this.rootIdx = rootIdx;
            this.topIdx = topIdx;
            this.loadPath = loadPath;
        }
    }

    public class UIManager : Singleton<UIManager>
    {
        private const string PATH_UI = "UI/";
        //public UITop topUI;
        [Space]
        public Transform[] rootOb;
        public UIBase[] listSceneCache;
        public Canvas canvas;
        private List<UIBase> listScreenActive = new List<UIBase>();
        private Dictionary<UIName, UIBase> cacheScreen = new Dictionary<UIName, UIBase>();
        /// <summary>
        /// 0: UiController, shop, event, main, guild, free (on tab)
        /// 1: General (down top)
        /// 2: top, changeScene,login, ....
        /// 3: loading, NotiUpdate
        /// top: -1: not action || -2: Hide top || >=0: Show top
        /// </summary>
        private Dictionary<UIName, string> dir = new Dictionary<UIName, string>
    {
            // UIName, rootIdx,topIdx,loadPath
            {UIName.Gameplay, "0,0,UIGameplay"},
            {UIName.Setting,"0,0,UISetting" },
            {UIName.Onboarding,"0,0,UIOnboarding" },
            {UIName.Main,"0,0,UIMain" },
            {UIName.ARFilterSelection,"0,0,UIARFilterSelection" },
            {UIName.CameraView,"0,0,UICameraView" },
            {UIName.ARVideoSelection,"0,0,UIARVideoSelection" },
            {UIName.PreviewVideo,"0,0,UIPreviewVideo" },
            {UIName.PreviewPicture,"0,0,UIPreviewPicture" },
            {UIName.SelectLanguage,"0,0,UISelectLanguage" },
            {UIName.Settings,"0,0,UISettings" },
            {UIName.Rate,"0,0,UIRate" },
            {UIName.GetPremium,"0,0,UIGetPremium" },
            {UIName.Collection,"0,0,UICollection" },
            {UIName.Policy,"0,0,UIPolicy" },
            {UIName.Disclaimer,"0,0,UIDisclaimer" },
        };

        private Dictionary<UIName, DataUIBase> dic2;
        public UIName CurrentName { get; private set; }
        public bool IsAction { get; set; }
        private void Awake()
        {
            dic2 = new Dictionary<UIName, DataUIBase>();
            foreach (var i in dir)
            {
                if (!dic2.ContainsKey(i.Key))
                {
                    var t = i.Value.Split(',');
                    dic2.Add(i.Key, new DataUIBase(int.Parse(t[0]), int.Parse(t[1]), t[2]));
                }
            }
            for (int i = 0; i < listSceneCache.Length; i++)
            {
                if (!cacheScreen.ContainsKey(listSceneCache[i].uiName))
                {
                    cacheScreen.Add(listSceneCache[i].uiName, listSceneCache[i]);
                }
            }
            if (DeviceUtility.IsIpad())
            {
                GetComponent<CanvasScaler>().matchWidthOrHeight = 1f;
            }
            else
            {
                GetComponent<CanvasScaler>().matchWidthOrHeight = 0f;
            }
            IsAction = false;
        }
        private void Start()
        {
            ConfigSize();
        }
        public void ShowUI(UIName uIScreen, Action onHideDone = null)
        {
            ShowTop(dic2[uIScreen].topIdx);
            UIBase current = listScreenActive.Find(x => x.uiName == uIScreen);
            if (!current)
            {
                current = LoadUI(uIScreen);
                current.uiName = uIScreen;
                AddScreenActive(current, true);
            }
            current.transform.SetAsLastSibling();
            current.Show(onHideDone);
            CurrentName = uIScreen;
        }

        public T ShowUI<T>(UIName uIScreen, Action onHideDone = null) where T : UIBase
        {
            ShowTop(dic2[uIScreen].topIdx);
            UIBase current = listScreenActive.Find(x => x.uiName == uIScreen);
            if (!current)
            {
                current = LoadUI(uIScreen);
                current.uiName = uIScreen;
                AddScreenActive(current, true);
            }
            current.transform.SetAsLastSibling();
            current.Show(onHideDone);
            CurrentName = uIScreen;
            return current as T;
        }

        private void AddScreenActive(UIBase current, bool isTop)
        {
            var idx = listScreenActive.FindIndex(x => x.uiName == current.uiName);
            if (isTop)
            {
                if (idx >= 0)
                {
                    listScreenActive.RemoveAt(idx);
                }
                listScreenActive.Add(current);
            }
            else
            {
                if (idx < 0)
                {
                    listScreenActive.Add(current);
                }
            }
        }

        public void ShowTop(int showTopIndex = -1)
        {
            //if (showTopIndex == -1)
            //{
            //    return;
            //}
            //if (showTopIndex < -1)
            //{
            //    topUI.Hide();
            //}
            //else
            //{
            //    topUI.Show(null);
            //    topUI.ShowTopGroup(showTopIndex);
            //}
        }


        public void RefreshUI()
        {
            var idx = 0;
            while (listScreenActive.Count > idx)
            {
                listScreenActive[idx].RefreshUI();
                idx++;
            }
            //topUI.RefreshUI();
            //GameManager.OnRefreshUI?.Invoke();
        }

        //private UIToast _uiToast;
        //public UIToast UIToast()
        //{
        //    if (_uiToast == null)
        //    {
        //        _uiToast = GetComponentInChildren<UIToast>();
        //    }
        //    return _uiToast;
        //}


        public T GetUI<T>(UIName uIScreen) where T : UIBase
        {
            var c = LoadUI(uIScreen);
            return c as T;
        }

        public UIBase GetUI(UIName uIScreen)
        {
            return LoadUI(uIScreen);
        }

        public UIBase GetUiActive(UIName uIScreen)
        {
            return listScreenActive.Find(x => x.uiName == uIScreen);
        }

        public T GetUiActive<T>(UIName uIScreen) where T : UIBase
        {
            var ui = listScreenActive.Find(x => x.uiName == uIScreen);
            if (ui)
            {
                return ui as T;
            }
            else
            {
                return default;
            }
        }

        private UIBase LoadUI(UIName uIScreen)
        {
            UIBase current = null;
            if (cacheScreen.ContainsKey(uIScreen))
            {
                current = cacheScreen[uIScreen];
                if (current == null)
                {
                    var idx = dic2[uIScreen].rootIdx;
                    current = Instantiate(Resources.Load<UIBase>(PATH_UI + dic2[uIScreen].loadPath), rootOb[idx]);
                    cacheScreen[uIScreen] = current;
                }
            }
            else
            {
                var idx = dic2[uIScreen].rootIdx;
                current = Instantiate(Resources.Load<UIBase>(PATH_UI + dic2[uIScreen].loadPath), rootOb[idx]);
                cacheScreen.Add(uIScreen, current);
            }
            return current;
        }

        public void RemoveActiveUI(UIName uiName)
        {
            var idx = listScreenActive.FindIndex(x => x.uiName == uiName);
            if (idx >= 0)
            {
                var ui = listScreenActive[idx];
                listScreenActive.RemoveAt(idx);
                if (!ui.isCache && cacheScreen.ContainsKey(uiName))
                {
                    cacheScreen[uiName] = null;
                }
                if (listScreenActive.Count > 0)
                {
                    ShowTop(dic2[listScreenActive.Last().uiName].topIdx);
                }
            }
        }


        public void HideAll()
        {
            while (listScreenActive.Count > 0)
            {
                listScreenActive[0].Hide();
            }
            //topUI.Hide();
        }
        public void HideAllUiActive()
        {
            while (listScreenActive.Count > 0)
            {
                listScreenActive[0].Hide();
            }
        }
        public void HideUiActive(UIName uiName)
        {
            var ui = listScreenActive.Find(x => x.uiName == uiName);
            if (ui)
            {
                ui.Hide();
            }
        }

        public UIBase GetLastUiActive()
        {
            if (listScreenActive.Count == 0) return null;
            return listScreenActive.Last();
        }


        #region SetMaskSize
        [Space]
        public RectTransform topMask;
        public RectTransform bottomMask;
        public void ConfigSize()
        {

            //#if UNITY_EDITOR
            //        bottomMask.sizeDelta = new Vector2(bottomMask.sizeDelta.x, 0);
            //#elif UNITY_ANDROID
            //        bottomMask.sizeDelta = new Vector2(bottomMask.sizeDelta.x, 50 * (Screen.dpi / 160.0f) / canvas.scaleFactor);
            //#elif UNITY_IPHONE
            //        bottomMask.sizeDelta = new Vector2(bottomMask.sizeDelta.x,  (Screen.width * 50 / GameHelper.getScreenWidth() + SafeArea.rect.y) /canvas.scaleFactor);

            //#else
            //#endif

            topMask.sizeDelta = new Vector2(topMask.sizeDelta.x, (Screen.height - SafeArea.rect.y - SafeArea.rect.height) / canvas.scaleFactor);
            bottomMask.sizeDelta = new Vector2(bottomMask.sizeDelta.x, DeviceUtility.GetSizeBanner());
            Debug.Log("safe" + " " + SafeArea.rect.y + " " + SafeArea.rect.width + " " + SafeArea.rect.height + " " + canvas.pixelRect.width + " " + canvas.pixelRect.height + " " + canvas.scaleFactor);
        }
        #endregion
    }


}