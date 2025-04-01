using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace appleHunger
{
    public static class AppleHungerTools
    {
        public static T FindByName<T>(T[] list, string targetName) where T : Component
        {
            foreach (var item in list)
            {
                if (item.gameObject.name == targetName && item.gameObject.scene.IsValid())
                {
                    return item;
                }
            }
            return null;
        }

    }
}
