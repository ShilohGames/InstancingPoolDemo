using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ShilohGames
{
    public class LasersBluePool : InstancePoolLasersBaseClass
    {
        public static LasersBluePool current;

        void Awake()
        {
            current = this;
        }

    }
}
