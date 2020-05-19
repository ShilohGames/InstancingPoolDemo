using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace ShilohGames
{
    public class LasersRedPool : InstancePoolLasersBaseClass
    {
        public static LasersRedPool current;

        void Awake()
        {
            current = this;
        }

    }
}
