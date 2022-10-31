using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;
using System.Runtime.CompilerServices;
using UnityEngine.PlayerLoop;

namespace EarthBendingSpell
{
	class EarthLightningMerge : SpellMergeData
	{
		public float stormMinCharge;
		public float stormRadius;

		public string stormEffectId;
		public string stormStartEffectId;
		public string spikesCollisionEffectId;

		private EffectData stormEffectData;

		private EffectData spikesCollisionEffectData;
		private EffectData stormStartEffectData;

		private EffectInstance cloudEffectInstance;

		public override void OnCatalogRefresh()
		{
			base.OnCatalogRefresh();
			stormEffectData = Catalog.GetData<EffectData>(stormEffectId, true);
			spikesCollisionEffectData = Catalog.GetData<EffectData>(spikesCollisionEffectId, true);
			stormStartEffectData = Catalog.GetData<EffectData>(stormStartEffectId, true);
		}

		public override void Merge(bool active)
		{
			base.Merge(active);
			if (active)
			{
				if (cloudEffectInstance != null)
				{
					cloudEffectInstance.Despawn();
				}

				cloudEffectInstance = stormStartEffectData.Spawn(Player.currentCreature.transform.position, Quaternion.identity);
				cloudEffectInstance.Play();

				return;
			}
			Vector3 from = Player.local.transform.rotation * PlayerControl.GetHand(Side.Left).GetHandVelocity();
			Vector3 from2 = Player.local.transform.rotation * PlayerControl.GetHand(Side.Right).GetHandVelocity();

			if (from.magnitude > SpellCaster.throwMinHandVelocity && from2.magnitude > SpellCaster.throwMinHandVelocity)
			{
				if (Vector3.Angle(from, mana.casterLeft.magicSource.position - mana.mergePoint.position) < 45f || Vector3.Angle(from2, mana.casterRight.magicSource.position - mana.mergePoint.position) < 45f)
				{
					if (currentCharge > stormMinCharge && !EarthBendingController.LightningActive)
					{
						EarthBendingController.LightningActive = true;
						mana.StartCoroutine(StormCoroutine());
						mana.StartCoroutine(DespawnEffectDelay(cloudEffectInstance, 15f));
						currentCharge = 0;
						return;
					}
				}
			}

			mana.StartCoroutine(DespawnEffectDelay(cloudEffectInstance, 1f));
		}

		public IEnumerator StormCoroutine()
		{
			Vector3 playerPos = Player.currentCreature.transform.position;

			//Get all creatures in range
			foreach (Creature creature in Creature.allActive)
            {
				if (creature != Player.currentCreature)
                {
					if (creature.state != Creature.State.Dead)
                    {
						float dist = Vector3.Distance(playerPos, creature.transform.position);
						if (dist < stormRadius)
                        {
							EffectInstance stormInst = stormEffectData.Spawn(creature.transform.position, Quaternion.identity);
							stormInst.Play();


							foreach (ParticleSystem particleSystem in stormInst.effects[0].gameObject.GetComponentsInChildren<ParticleSystem>())
                            {
								if (particleSystem.gameObject.name == "CollisionDetector")
                                {
									ElectricSpikeCollision scr = particleSystem.gameObject.AddComponent<ElectricSpikeCollision>();
									scr.part = particleSystem;
									scr.spikesCollisionEffectData = spikesCollisionEffectData;
								}
                            }

							mana.StartCoroutine(DespawnEffectDelay(stormInst, 15f));

							yield return new WaitForSeconds(UnityEngine.Random.Range(0.1f, 0.4f));
                        }
                    }
                } else
                {
					EffectInstance stormInst = stormEffectData.Spawn(creature.transform.position + creature.transform.forward * 2, Quaternion.identity);
					stormInst.Play();


					foreach (ParticleSystem particleSystem in stormInst.effects[0].gameObject.GetComponentsInChildren<ParticleSystem>())
					{
						if (particleSystem.gameObject.name == "CollisionDetector")
						{
							ElectricSpikeCollision scr = particleSystem.gameObject.AddComponent<ElectricSpikeCollision>();
							scr.part = particleSystem;
							scr.spikesCollisionEffectData = spikesCollisionEffectData;
						}
					}

					mana.StartCoroutine(DespawnEffectDelay(stormInst, 15f));
				}
            }

			yield return new WaitForSeconds(10f);
			EarthBendingController.LightningActive = false;
		}

		IEnumerator DespawnEffectDelay(EffectInstance effect, float delay)
        {
			yield return new WaitForSeconds(delay);
			effect.Despawn();
			
		}

		public IEnumerator PlayEffectSound(float delay, EffectData effectData, Vector3 position, float despawnDelay = 0)
		{
			yield return new WaitForSeconds(delay);
			EffectInstance effectInstance = effectData.Spawn(position, Quaternion.identity);
			effectInstance.Play();
			if (despawnDelay != 0)
			{
				yield return new WaitForSeconds(despawnDelay);
				effectInstance.Stop();
			}
		}

	}

	public class ElectricSpikeCollision : MonoBehaviour
	{
		public EffectData spikesCollisionEffectData;

		public ParticleSystem part;
		public List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();


		private void OnParticleCollision(GameObject other)
		{
			int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

			foreach (ParticleCollisionEvent particleCollisionEvent in collisionEvents)
			{
				EffectInstance effectInstance = spikesCollisionEffectData.Spawn(particleCollisionEvent.intersection, Quaternion.identity);
				effectInstance.Play();

				foreach (Collider collider in Physics.OverlapSphere(particleCollisionEvent.intersection, 1f))
				{
					if (collider.attachedRigidbody)
					{
						if (collider.GetComponentInParent<Creature>())
						{
							Creature creature = collider.GetComponentInParent<Creature>();
							if (creature != Player.currentCreature)
							{
								if (creature.state != Creature.State.Dead)
								{
									creature.TryElectrocute(10, 12, true, false);
									creature.Damage(new CollisionInstance(new DamageStruct(DamageType.Energy, 5f)));
								}
							}
							else
							{
								continue;
							}
						}
					}
				}
			}
		}
	}
}
