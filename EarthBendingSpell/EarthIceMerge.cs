using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using UnityEngine;

namespace EarthBendingSpell
{
    class EarthIceMerge : SpellMergeData
    {
		public float frostMinCharge;
		public string frostEffectId;
		public float frostRadius;
		public float frozenDuration;

		public string frozenEffectId;

		private EffectData frostEffectData;
		private EffectData frozenEffectData;

		public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();
			frostEffectData = Catalog.GetData<EffectData>(frostEffectId);
			frozenEffectData = Catalog.GetData<EffectData>(frozenEffectId);
		}

        public override void Merge(bool active)
		{
			base.Merge(active);
			if (active)
			{
				return;
			}
			Vector3 from = Player.local.transform.rotation * PlayerControl.GetHand(Side.Left).GetHandVelocity();
			Vector3 from2 = Player.local.transform.rotation * PlayerControl.GetHand(Side.Right).GetHandVelocity();

			if (from.magnitude > SpellCaster.throwMinHandVelocity && from2.magnitude > SpellCaster.throwMinHandVelocity)
			{
				if (Vector3.Angle(from, mana.casterLeft.magicSource.position - mana.mergePoint.position) < 45f || Vector3.Angle(from2, mana.casterRight.magicSource.position - mana.mergePoint.position) < 45f)
				{
					if (currentCharge > frostMinCharge && !EarthBendingController.IceActive)
					{
						EarthBendingController.IceActive = true;
						mana.StartCoroutine(IceSpikesCoroutine());
						currentCharge = 0;
					}
				}
			}
		}

		IEnumerator IceSpikesCoroutine()
        {
			Vector3 pos = Player.currentCreature.transform.position;

			EffectInstance frostEffectInstance= frostEffectData.Spawn(pos, Quaternion.identity);
			frostEffectInstance.Play();

			foreach (Creature creature in Creature.allActive)
            {
				if (creature != Player.currentCreature && !creature.isKilled)
                {
					float dist = Vector3.Distance(creature.transform.position, pos);
					if (dist < frostRadius)
                    {
						mana.StartCoroutine(FreezeCreature(creature, frozenDuration));
                    }
                }
            }

			yield return new WaitForSeconds(6f);
			//stop
			frostEffectInstance.Stop();
			EarthBendingController.IceActive = false;
		}


		IEnumerator FreezeCreature(Creature targetCreature, float duration)
		{
			EffectInstance effectInstance = frozenEffectData.Spawn(targetCreature.transform.position, Quaternion.identity, targetCreature.transform);
			effectInstance.Play();

			targetCreature.animator.speed = 0;
			targetCreature.locomotion.SetSpeedModifier(this, 0, 0, 0, 0, 0);

			targetCreature.brain.Stop();

			yield return new WaitForSeconds(duration);

			if (!targetCreature.isKilled)
			{
				targetCreature.ragdoll.SetState(Ragdoll.State.Destabilized);

				targetCreature.animator.speed = 1;
				targetCreature.locomotion.ClearSpeedModifiers();

				targetCreature.brain.Load(targetCreature.brain.instance.id);
			}

			effectInstance.Despawn();
		}

		/*
		IEnumerator FreezeCreature(Creature targetCreature, float duration)
        {
			//
			EffectInstance effectInstance = frozenEffectData.Spawn(targetCreature.ragdoll.hipsPart.transform, true, Array.Empty<Type>());
			effectInstance.SetRenderer(targetCreature.bodyMeshRenderer, false);
			effectInstance.Play(0);
			effectInstance.SetIntensity(1f);
			//

			//targetCreature.StopBrain();

			targetCreature.animator.speed = 0;
			targetCreature.locomotion.speed = 0;

			yield return new WaitForSeconds(0.1f);

			EffectInstance effectInstance = frozenEffectData.Spawn(targetCreature.transform.position, Quaternion.identity);
			effectInstance.Play();
			Animator animator = effectInstance.effects[0].GetComponentInChildren<Animator>();

			yield return new WaitForSeconds(duration);

			CollisionInstance collisionStruct = new CollisionInstance(new DamageStruct(DamageType.Energy, 20.0f));

			targetCreature.Damage(collisionStruct);

			targetCreature.animator.speed = 1;
			targetCreature.locomotion.speed = targetCreature.data.locomotionSpeed;

			if (!targetCreature.isKilled && !targetCreature.brain.instance.isActive)
			{
				//targetCreature.StartBrain();
			}

			animator.SetBool("Play", true);

			yield return new WaitForSeconds(5f);

			effectInstance.Despawn();
		}*/

	}
}
