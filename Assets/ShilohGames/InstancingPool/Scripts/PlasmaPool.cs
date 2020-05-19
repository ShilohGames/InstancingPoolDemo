using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ShilohGames
{
    public class PlasmaPool : InstancePoolLasersBaseClass
    {
        public static PlasmaPool current;

        void Awake()
        {
            current = this;
        }

    }
}
