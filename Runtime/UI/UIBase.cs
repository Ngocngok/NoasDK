using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace NoasDK.UI
{

    public class UIBase : MonoBehaviour
    {
        public UIName uiName = UIName.None;
        public bool isCache = false;

        protected bool isSetup = false;
        public System.Action onHideDone;

        protected virtual void Setup()
        {
        }

        public virtual void Show(System.Action onHideDone)
        {
            if (!isSetup)
            {
                isSetup = true;
                Setup();
            }
            gameObject.SetActive(true);
            this.onHideDone = onHideDone;
        }

        public virtual void RefreshUI()
        {
        }

        public virtual void Hide(bool hasShowOutAnim = false)
        {
            UIManager.Instance.RemoveActiveUI(uiName);

            if (!hasShowOutAnim)
            {
                if (!isCache)
                {
                    Destroy(gameObject);
                }
                else
                {
                    gameObject.SetActive(false);
                }
                onHideDone?.Invoke();
                onHideDone = null;
            }
            else
            {
                //Animation will call to OnAnimationHideDone on complete
                var animComponent = GetComponent<Animation>();
                var animHide = animComponent.GetClip("Hide");
                if (animHide != null)
                {
                    animComponent.Play("Hide");
                    var length = animHide.length;
                    StartCoroutine(DelayAnimHide(length));
                }

            }
        }

        public virtual void OnAnimHideDone()
        {
            if (!isCache)
            {
                Destroy(gameObject);
            }
            else
            {
                gameObject.SetActive(false);
            }

            onHideDone?.Invoke();
            onHideDone = null;
        }

        private IEnumerator DelayAnimHide(float dur)
        {
            yield return new WaitForSeconds(dur);
            OnAnimHideDone();
        }
        private IEnumerator DelayShowOut()
        {
            yield return new WaitForSeconds(0.5f);
            gameObject.SetActive(false);
        }

        protected virtual void OnClickClose()
        {
            Hide();
            //AudioManager.Instance.PlayClickBtnFx();
        }
    } 
}