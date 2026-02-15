using UnityEngine;
using UnityEngine.EventSystems;
using Core;
using Core.Audio;
using Core.UI;

namespace Game
{
    using static CoreUtility;

    public class UIPointerEventSound : UIPointerEvent
    {
        [Header("_")]
        [SerializeField, Required] private AudioClip onClickSound = null;
        [SerializeField, Required] private AudioClip onHoverSound = null;

        private bool canPlaySound = true;

        protected override void OnBeginDragInternal(PointerEventData eventData)
        {
            base.OnBeginDragInternal(eventData);

            canPlaySound = false;
        }
        protected override void OnEndDragInternal(PointerEventData eventData)
        {
            base.OnEndDragInternal(eventData);

            canPlaySound = true;
        }

        protected override void OnSubmitInternal(BaseEventData eventData)
        {
            base.OnSubmitInternal(eventData);

            PlaySound(onClickSound);
        }
        protected override void OnSelectInternal(BaseEventData eventData)
        {
            base.OnSelectInternal(eventData);

            PlaySound(onHoverSound);
        }
        protected override void OnPointerClickInternal(PointerEventData eventData)
        {
            base.OnPointerClickInternal(eventData);

            PlaySound(onClickSound);
        }
        protected override void OnPointerEnterInternal(PointerEventData eventData)
        {
            base.OnPointerEnterInternal(eventData);

            PlaySound(onHoverSound);
        }
        private void PlaySound(AudioClip clip)
        {
            if (!canPlaySound)
            {
                return;
            }

            if (!gameObject.activeSelf)
            {
                return;
            }

            if (clip == null)
            {
                return;
            }

            ManagerCoreAudio.Instance.PlaySound(clip, AudioGroup.MASTER, Vector3.zero, 0, 1, 1, 1, 1, false);
        }
    }
}