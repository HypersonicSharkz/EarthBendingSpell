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
    class EarthGravMerge : SpellMergeData
    {
        public string bubbleEffectId;
		public float bubbleMinCharge;
		public float rockExplosionRadius;
		public float rockExplosionForce;

		public string portalEffectId;
		public string rockCollisionEffectId;

		private EffectData bubbleEffectData;

		private EffectData portalEffectData;
		private EffectData rockCollisionEffectData;

		public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();
			bubbleEffectData = Catalog.GetData<EffectData>(bubbleEffectId, true);
			portalEffectData = Catalog.GetData<EffectData>(portalEffectId, true);
			rockCollisionEffectData = Catalog.GetData<EffectData>(rockCollisionEffectId, true);
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
					if (currentCharge > bubbleMinCharge && !EarthBendingController.GravActive)
					{
						EarthBendingController.GravActive = true;
						mana.StartCoroutine(BubbleCoroutine());
						currentCharge = 0;
					}
				}
			}
		}

		public IEnumerator BubbleCoroutine()
		{
			Vector3 centerPoint = mana.mergePoint.transform.position;


			EffectInstance bubbleEffect = null;

			bubbleEffect = bubbleEffectData.Spawn(centerPoint, Quaternion.identity);
			bubbleEffect.SetIntensity(0f);
			bubbleEffect.Play(0);

			ParticleSystem parentParticleSystem = bubbleEffect.effects[0].gameObject.GetComponent<ParticleSystem>();


			foreach (ParticleSystem particleSystem in parentParticleSystem.gameObject.GetComponentsInChildren<ParticleSystem>())
            {
				if (particleSystem.gameObject.name == "Portal")
                {
					float startDelay = particleSystem.main.startDelay.constant;
					Player.currentCreature.mana.StartCoroutine(PlayEffectSound(startDelay, portalEffectData, particleSystem.transform.position, 3f));
				}

				if (particleSystem.gameObject.name == "Rock")
                {
					RockCollision scr = particleSystem.gameObject.AddComponent<RockCollision>();
					scr.rockCollisionEffectData = rockCollisionEffectData;
					scr.rockExplosionForce = rockExplosionForce;
					scr.rockExplosionRadius = rockExplosionRadius;
					scr.part = particleSystem;
                }
            }
			yield return new WaitForSeconds(4.5f);
			bubbleEffect.Stop();
			EarthBendingController.GravActive = false;
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

	public class RockCollision : MonoBehaviour
    {
		public EffectData rockCollisionEffectData;

		public ParticleSystem part;
		public List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

		public float rockExplosionRadius;
		public float rockExplosionForce;


		private void OnParticleCollision(GameObject other)
		{
			int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

			foreach (ParticleCollisionEvent particleCollisionEvent in collisionEvents)
			{
				EffectInstance effectInstance = rockCollisionEffectData.Spawn(particleCollisionEvent.intersection, Quaternion.identity);
				effectInstance.Play();

				foreach (Collider collider in Physics.OverlapSphere(particleCollisionEvent.intersection, rockExplosionRadius))
				{
					if (collider.attachedRigidbody)
					{
						if (collider.GetComponentInParent<Creature>())
						{
							Creature creature = collider.GetComponentInParent<Creature>();
							if (creature != Player.currentCreature)
							{
								if (creature.state == Creature.State.Alive)
								{
									creature.ragdoll.SetState(Ragdoll.State.Destabilized);
								}
								StartCoroutine(AddForceCoroutine(collider.attachedRigidbody, particleCollisionEvent.intersection));
							} 
						} else if (collider.GetComponentInParent<Item>())
                        {
							Item item = collider.GetComponentInParent<Item>();

							if (item.mainHandler)
                            {
								if (item.mainHandler.creature != Player.currentCreature)
                                {
									StartCoroutine(AddForceCoroutine(collider.attachedRigidbody, particleCollisionEvent.intersection));
								}
                            }
						}
					}
				}
			}

			IEnumerator AddForceCoroutine(Rigidbody rb, Vector3 expPos) 
            {
				yield return new WaitForEndOfFrame();
				rb.AddExplosionForce(rockExplosionForce, expPos, rockExplosionForce, 1f, ForceMode.Impulse);

			}
		}
    }
}
