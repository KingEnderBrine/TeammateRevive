﻿using System.Linq;
using RoR2;
using TeammateRevive.Revive;
using UnityEngine;

namespace TeammateRevive.Skull
{
    public class SkullLongRangeActivationManager
    {
        private static readonly float RaycastExtraRadius = 3f;
        
        private readonly RunTracker run;
        private readonly SkullTracker skullTracker;

        public SkullLongRangeActivationManager(RunTracker run, SkullTracker skullTracker)
        {
            this.run = run;
            this.skullTracker = skullTracker;
            On.RoR2.Interactor.FindBestInteractableObject += hook_Interactor_FindBestInteractableObject;
        }

        private GameObject hook_Interactor_FindBestInteractableObject(On.RoR2.Interactor.orig_FindBestInteractableObject orig, RoR2.Interactor self, Ray raycastRay, float maxRaycastDistance, Vector3 overlapposition, float overlapradius)
        {
            var go = orig(self, raycastRay, maxRaycastDistance, overlapposition, overlapradius);
            if (go != null || !this.run.IsDeathCurseEnabled) return go;

            maxRaycastDistance = GetMaxReviveDistance();
            if (maxRaycastDistance == 0) return go;

            if (Physics.Raycast(raycastRay, out var hitInfo, maxRaycastDistance, LayerIndex.CommonMasks.interactable,
                QueryTriggerInteraction.Collide))
            {
                var entity = EntityLocator.GetEntity(hitInfo.collider.gameObject);
                if (entity != null && CheckSkullRangeAllowed(entity, hitInfo))
                {
                    var component = entity.GetComponent<IInteractable>();
                    if (component is ReviveInteraction interaction)
                    {
                        if (interaction.isActiveAndEnabled && component.GetInteractability(self) != Interactability.Disabled)
                            return entity;
                    }
                }
            }

            return go;
        }

        float GetMaxReviveDistance()
        {
            if (this.skullTracker.skulls.Count == 0) return 0;
            return this.skullTracker.skulls.Max(skull => skull.cachedRadius + RaycastExtraRadius);
        }

        bool CheckSkullRangeAllowed(GameObject gameObject, RaycastHit hitInfo)
        {
            var skull = gameObject.GetComponent<DeadPlayerSkull>();
            if (!skull)
                return false;
            
            return skull.cachedRadius + RaycastExtraRadius >= hitInfo.distance;
        }
    }
}