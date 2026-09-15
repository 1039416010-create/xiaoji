using UnityEngine;

namespace GroundChickenKing.Diagnostics
{
    public static class BuildInfo
    {
        public static string ProductName => Application.productName;
        public static string ApplicationVersion => Application.version;
        public static string UnityVersion => Application.unityVersion;
        public static string Summary => $"{ProductName} {ApplicationVersion} | Unity {UnityVersion}";
    }
}
