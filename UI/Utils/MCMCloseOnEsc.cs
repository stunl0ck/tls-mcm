using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using TPLib;
using TheLastStand.View.Generic;
using TheLastStand.View.Modding;

namespace Stunl0ck.ModConfigManager.UI
{
    internal sealed class MCMCloseOnEsc : MonoBehaviour
    {
        public GenericPopUp popup;

        void Update()
        {
            if (!popup) { Destroy(this); return; }

            // Mapped Cancel (same id ModsView uses)
            bool esc = TheLastStand.Manager.InputManager.GetButtonDown(23);
            if (!esc || !GenericPopUp.IsOpen) return;

            StartCoroutine(CloseAndRestore());
        }

        private IEnumerator CloseAndRestore()
        {
            popup.StartCoroutine("CloseAtEndOfFrame");
            yield return null;

            var mv = TPSingleton<ModsView>.Instance;
            if (mv != null)
                TheLastStand.View.Camera.CameraView.AttenuateWorldForPopupFocus(mv);

            Destroy(this);
        }
    }
}
