using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts.DataStructures
{
    public struct ReinforcementFunc
    {
        public static float prize = 100.0f;
        public static float punish = -10.0f;
    }

    public struct LearningRatio
    {
        public static float alpha = 0.3f;
        public static float gamma = 0.92f;
    }

    public struct ExplorationRandomness
    {
        public static float rho = 0.4f;
        public static float ExploringFunc(int episode)
        {
            return 1 / (episode + 1.0f);
        }
    }
}
