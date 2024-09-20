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
			GetButtonIcon(true, (s) => Debug.Log("Pixels per unit: " + s.pixelsPerUnit + " Rect: " + s.rect.width + ", " + s.rect.height));
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
				rotatingMergePoint.transform.rotation = Quaternion.LookRotation((mana.casterLeft.magicSource.transform.up + mana.casterRight.magicSource.transform.up));
				rotatingMergePoint.transform.position = Vector3.Lerp(mana.casterLeft.magicSource.transform.position, mana.casterRight.magicSource.transform.position, 0.5f);
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

        [ModOption(category = "Bullet Barrage", name = "Bullet Damage", tooltip = "Damage of each bullet")]
        [ModOptionIntValues(1, 20, 1)]
        public static int bulletDamage = 1;

        [ModOption(category = "Bullet Barrage", name = "Bullet Burn Duration", tooltip = "How long enemies will be burning for when hit by a bullet")]
        [ModOptionIntValues(1, 30, 1)]
        public static int burnTime = 12;

        [ModOption(category = "Bullet Barrage", name = "Bullet Burn Damage", tooltip = "Damage caused by burning")]
        [ModOptionIntValues(1, 30, 1)]
        public static int burnDamage = 5;


        [ModOption(category = "Bullet Barrage", name = "Bullet Radius", tooltip = "How big of an explosion each bullet causes")]
        [ModOptionFloatValues(0, 1, 0.1f)]
        public static float bulletRadius = 0.2f;

        private void OnParticleCollision(GameObject other)
		{
			int numCollisionEvents = part.GetCollisionEvents(other, collisionEvents);

			foreach (ParticleCollisionEvent pE in collisionEvents)
			{
				bulletColData.Spawn(pE.intersection, Quaternion.identity).Play();

				foreach (Collider collider in Physics.OverlapSphere(pE.intersection, bulletRadius))
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
									creature.Inflict("Burning", EarthBendingController.Instance, burnTime, burnDamage, true);

									CollisionInstance collisionStruct = new CollisionInstance(new DamageStruct(DamageType.Pierce, bulletDamage));
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
