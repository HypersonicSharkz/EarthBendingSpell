using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using ThunderRoad;

namespace EarthBendingSpell
{
    

    class EarthFireMerge : SpellMergeData
    {
        public string bulletEffectId;
		public float bulletMinCharge;

		public string bulletCollisionEffectId;

        private EffectData bulletEffectData;
		private EffectData bulletCollisionEffectData;

		private EffectInstance bulletInstance;

		private GameObject rotatingMergePoint;
		public override void OnCatalogRefresh()
        {
            base.OnCatalogRefresh();
            bulletEffectData = Catalog.GetData<EffectData>(bulletEffectId);
			bulletCollisionEffectData = Catalog.GetData<EffectData>(bulletCollisionEffectId);

		}

        public override void Merge(bool active)
        {
            base.Merge(active);
			if (!active)
            {
				currentCharge = 0f;
				if (bulletInstance != null)
				{
					bulletInstance.Despawn();
					bulletInstance = null;
				}
			}
        }

        public override void Update()
        {
            base.Update();
			if (currentCharge > bulletMinCharge)
            {
				if (bulletInstance == null)
                {
					SpawnBulletInstance();
				}
            } else
            {
				if (bulletInstance != null)
                {
					bulletInstance.Despawn();
					bulletInstance = null;
                }
				if (rotatingMergePoint != null)
				{
					GameObject.Destroy(rotatingMergePoint);
				}
			}

			if (rotatingMergePoint != null)
            {
				rotatingMergePoint.transform.rotation = Quaternion.LookRotation((mana.casterLeft.magic.transform.up + mana.casterRight.magic.transform.up));
				rotatingMergePoint.transform.position = Vector3.Lerp(mana.casterLeft.magic.transform.position, mana.casterRight.magic.transform.position, 0.5f);
			}
        }

		private void SpawnBulletInstance()
        {
			rotatingMergePoint = new GameObject("rotmpoint");

			bulletInstance = bulletEffectData.Spawn(rotatingMergePoint.transform);
			bulletInstance.Play();
			bulletInstance.SetIntensity(1f);

			foreach (ParticleSystem child in bulletInstance.effects[0].GetComponentsInChildren<ParticleSystem>())
            {
				if (child.gameObject.name == "Bullets")
                {
					BulletCollisionClass scr = child.gameObject.AddComponent<BulletCollisionClass>();
					scr.part = child;
					scr.bulletColData = bulletCollisionEffectData;
				}
            }
		}
    }

	public class BulletCollisionClass : MonoBehaviour
    {
		public ParticleSystem part;
		public List<ParticleCollisionEvent> collisionEvents = new List<ParticleCollisionEvent>();

		public EffectData bulletColData;

		private void OnParticleCollision(GameObject other)
		{
			int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

			foreach (ParticleCollisionEvent pE in collisionEvents)
			{
				bulletColData.Spawn(pE.intersection, Quaternion.identity).Play();

				foreach (Collider collider in Physics.OverlapSphere(pE.intersection, .2f))
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
									creature.brain.TryAction(new ActionShock(10, 12), true);

									CollisionInstance collisionStruct = new CollisionInstance(new DamageStruct(DamageType.Pierce, 0.1f));
									creature.Damage(collisionStruct);
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
