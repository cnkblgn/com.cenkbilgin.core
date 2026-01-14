using UnityEngine;

namespace Core.UI
{
    using static CoreUtility;

    [DisallowMultipleComponent]
    public class UITextBinderGameVersion : UITextBinder
    {
        protected override string Get() => ManagerCoreGame.Instance.Version;
    }
}