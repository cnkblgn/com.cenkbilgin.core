using System.Collections;
using UnityEngine;
using TMPro;

namespace Core.UI
{
    using static CoreUtility;

    [RequireComponent(typeof(Canvas))]
    internal sealed class UISubtitleController : MonoBehaviour
    {
        private const string CMD_SPEAKER = "SPEAKER";
        private const string CMD_LINE = "LINE";
        private const string CMD_WAIT = "WAIT";

        [Header("_")]
        [SerializeField, Required] private TextMeshProUGUI subtitle = null;

        private Canvas thisCanvas = null;
        private Coroutine thisCoroutine = null;

        private void Start()
        {
            thisCanvas = GetComponent<Canvas>();

            Hide();
        }

        public void Show(string text)
        {
            thisCanvas.Show();

            if (thisCoroutine != null)
            {
                StopCoroutine(thisCoroutine);
                thisCoroutine = null;
            }

            thisCoroutine = StartCoroutine(ShowInternal(text));
        }
        public void Hide() => thisCanvas.Hide();

        private IEnumerator ShowInternal(string value)
        {
            float currentTime = 0f;
            string currentSpeaker = "";
            string[] blocks = value.Split('\n');

            foreach (string rawBlock in blocks)
            {
                if (string.IsNullOrWhiteSpace(rawBlock))
                {
                    continue;
                }

                string[] main = rawBlock.Split("::");

                string left = main[0].Trim();
                string text = main.Length > 1 ? main[1].Trim() : "";

                string cmd = left;
                string param = null;

                if (left.Contains("["))
                {
                    int i0 = left.IndexOf('[');
                    int i1 = left.IndexOf(']');

                    cmd = left[..i0];
                    param = left.Substring(i0 + 1, i1 - i0 - 1);
                }


                float startTime = -1f;
                float endTime = -1f;

                if (cmd == CMD_LINE && param != null)
                {
                    string[] t = param.Split('-');
                    startTime = float.Parse(t[0]);
                    endTime = float.Parse(t[1]);
                }

                if (startTime >= 0 && startTime > currentTime)
                {
                    yield return new WaitForSeconds(startTime - currentTime);
                    currentTime = startTime;
                }

                switch (cmd)
                {
                    case CMD_SPEAKER:
                        currentSpeaker = text;
                        break;
                    case CMD_LINE:
                        this.subtitle.text = string.IsNullOrEmpty(currentSpeaker) ? text : $"{currentSpeaker.ToYellow().ToBold()}\n\n{text}";
                        break;
                    case CMD_WAIT:
                        if (!string.IsNullOrEmpty(param))
                        {
                            float wait = float.Parse(param);
                            yield return new WaitForSeconds(wait);
                            currentTime += wait;
                        }
                        break;
                }

                if (endTime >= 0)
                {
                    yield return new WaitForSeconds(endTime - currentTime);
                    currentTime = endTime;
                }
            }

            subtitle.text = "";
            Hide();
        }
    }
}
