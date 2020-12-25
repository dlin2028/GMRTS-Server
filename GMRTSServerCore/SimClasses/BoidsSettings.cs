using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GMRTSServerCore.SimClasses
{
    public struct BoidsSettings
    {
        public readonly float CohesionMaxMag;
        public readonly float CohesionStrength;
        public readonly float CohesionDistance;
        public readonly float CohesionDistanceSquared;
        public readonly float SeparationStrength;
        public readonly float SeparationDistance;
        public readonly float SeparationDistanceSquared;

        public readonly float LargerDistance;
        public readonly float LargerDistanceSquared;

        public readonly float BoidsStrength;
        public readonly float FlowfieldStrength;

        public readonly float MaximumBoidsVelocity;

        public BoidsSettings(float cohesionMaxMag, float cohesionStrength, float cohesionDistance, float separationStrength, float separationDistance, float boidsStrength, float flowfieldStrength, float maximumBoidsVelocity) : this()
        {
            CohesionMaxMag = cohesionMaxMag;
            CohesionStrength = cohesionStrength;
            CohesionDistance = cohesionDistance;
            SeparationStrength = separationStrength;
            SeparationDistance = separationDistance;

            CohesionDistanceSquared = CohesionDistance * CohesionDistance;
            SeparationDistanceSquared = SeparationDistance * SeparationDistance;

            LargerDistance = Math.Max(CohesionDistance, separationDistance);
            LargerDistanceSquared = LargerDistance * LargerDistance;

            BoidsStrength = boidsStrength;
            FlowfieldStrength = flowfieldStrength;

            MaximumBoidsVelocity = maximumBoidsVelocity;
        }
    }
}
