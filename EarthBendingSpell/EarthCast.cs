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
    public class EarthCastCharge : SpellCastCharge
    {
        public float shieldMinSpeed;
        public string shieldItemId;
        public float shieldFreezeTime;
        public float shieldHealth;
        public float shieldPushMul;

        public float pushMinSpeed;
        public string pushEffectId;
        public float pushForce;

        public List<string> rockItemIds = new List<string>();
        public Vector2 rockMassMinMax;
        public float rockForceMul;
        public float rockFreezeTime;
        public float rockHeightFromGround;
        public float rockMoveSpeed;
        public string rockSummonEffectId;
        public float punchForce;
        public string punchEffectId;
        public float punchAimPrecision;
        public float punchAimRandomness;

        public string spikeEffectId;
        public float spikeMinSpeed;
        public float spikeRange;
        public float spikeMinAngle;
        public float spikeDamage;

        public string shatterEffectId;
        public float shatterMinSpeed;
        public float shatterRange;
        public float shatterRadius;
        public float shatterForce;

        public float rockPillarMinSpeed;
        public string rockPillarPointsId;
        public string rockPillarItemId;
        public string rockPillarCollisionEffectId;
        public float rockPillarLifeTime;
        public float rockPillarSpawnDelay;

        public string imbueHitGroundEffectId;
        public float imbueHitGroundConsumption;
        public float imbueHitGroundExplosionUpwardModifier;
        public float imbueHitGroundRechargeDelay;
        public float imbueHitGroundMinVelocity;
        public float imbueHitGroundRadius;
        public float imbueHitGroundExplosionForce;

        public float imbueCrystalUseCost;
        public float imbueCrystalShootForce;

        private float imbueHitGroundLastTime;

        public override void Init()
        {
            EventManager.onPossess += EventManager_onPossess;

            base.Init();
        }

        private void EventManager_onPossess(Creature creature, EventTime eventTime)
        {
            if (creature.container.contents.Where(c => c.itemData.id == "SpellEarthItem").Count() <= 0)
                creature.container.AddContent(Catalog.GetData<ItemData>("SpellEarthItem"));

            if (creature.container.contents.Where(c => c.itemData.id == "SpellEarthFireMerge").Count() <= 0)
                creature.container.AddContent(Catalog.GetData<ItemData>("SpellEarthFireMerge"));

            if (creature.container.contents.Where(c => c.itemData.id == "SpellEarthIceMerge").Count() <= 0)
                creature.container.AddContent(Catalog.GetData<ItemData>("SpellEarthIceMerge"));

            if (creature.container.contents.Where(c => c.itemData.id == "SpellMeteorBarrage").Count() <= 0)
                creature.container.AddContent(Catalog.GetData<ItemData>("SpellMeteorBarrage"));

            if (creature.container.contents.Where(c => c.itemData.id == "LightningStorm").Count() <= 0)
                creature.container.AddContent(Catalog.GetData<ItemData>("LightningStorm"));
        }

        public override void Fire(bool active)
        {
            base.Fire(active);


            if (!active)
            {
                chargeEffectInstance.Despawn();
            }

            if (!spellCaster.mana.gameObject.GetComponent<EarthBendingController>())
            {
                spellCaster.mana.gameObject.AddComponent<EarthBendingController>();
                EarthBendingController scr2 = spellCaster.mana.gameObject.GetComponent<EarthBendingController>();
                scr2.shieldMinSpeed = shieldMinSpeed;
                scr2.shieldItemId = shieldItemId;
                scr2.shieldFreezeTime = shieldFreezeTime;
                scr2.shieldHealth = shieldHealth;
                scr2.shieldPushMul = shieldPushMul;


                scr2.pushEffectId = pushEffectId;
                scr2.pushMinSpeed = pushMinSpeed;
                scr2.pushForce = pushForce;


                scr2.rockItemIds = rockItemIds;
                scr2.rockForceMul = rockForceMul;
                scr2.rockFreezeTime = rockFreezeTime;
                scr2.rockHeightFromGround = rockHeightFromGround;
                scr2.rockMoveSpeed = rockMoveSpeed;
                scr2.rockMassMinMax = rockMassMinMax;
                scr2.rockSummonEffectId = rockSummonEffectId;
                scr2.punchForce = punchForce;
                scr2.punchEffectId = punchEffectId;
                scr2.punchAimPrecision = punchAimPrecision;
                scr2.punchAimRandomness = punchAimRandomness;

                scr2.spikeMinSpeed = spikeMinSpeed;
                scr2.spikeEffectId = spikeEffectId;
                scr2.spikeRange = spikeRange;
                scr2.spikeMinAngle = spikeMinAngle;
                scr2.spikeDamage = spikeDamage;

                scr2.shatterEffectId = shatterEffectId;
                scr2.shatterForce = shatterForce;
                scr2.shatterMinSpeed = shatterMinSpeed;
                scr2.shatterRadius = shatterRadius;
                scr2.shatterRange = shatterRange;

                scr2.rockPillarPointsId = rockPillarPointsId;
                scr2.rockPillarItemId = rockPillarItemId;
                scr2.rockPillarCollisionEffectId = rockPillarCollisionEffectId;
                scr2.rockPillarMinSpeed = rockPillarMinSpeed;
                scr2.rockPillarLifeTime = rockPillarLifeTime;
                scr2.rockPillarSpawnDelay = rockPillarSpawnDelay;

                scr2.Initialize();
            }

            EarthBendingController scr = spellCaster.mana.gameObject.GetComponent<EarthBendingController>();

            if (spellCaster.ragdollHand.side == Side.Left)
            {
                scr.leftHandActive = active;
            } else
            {
                scr.rightHandActive = active;
            }
        }


        public override void OnImbueCollisionStart(CollisionInstance collisionInstance)
        {
            base.OnImbueCollisionStart(collisionInstance);


            if (imbue.colliderGroup.modifier.imbueType == ColliderGroupData.ImbueType.Crystal && !collisionInstance.targetColliderGroup && collisionInstance.impactVelocity.magnitude > imbueHitGroundMinVelocity && imbue.CanConsume(imbueHitGroundConsumption) && Time.time - imbueHitGroundLastTime > imbueHitGroundRechargeDelay)
            {
                imbue.colliderGroup.collisionHandler.item.StartCoroutine(RockShockWaveCoroutine(collisionInstance.contactPoint, collisionInstance.contactNormal, collisionInstance.sourceColliderGroup.transform.up, collisionInstance.impactVelocity));
                imbueHitGroundLastTime = Time.time;
                return;
            }



            if (collisionInstance.damageStruct.hitRagdollPart)
            {
                Creature creature = collisionInstance.damageStruct.hitRagdollPart.ragdoll.creature;

                if (creature != Player.currentCreature)
                {
                    RagdollPart ragdollPart = collisionInstance.damageStruct.hitRagdollPart;


                    if (!ragdollPart.GetComponent<EarthBendingRagdollPart>())
                    {
                        EarthBendingRagdollPart scr = ragdollPart.gameObject.AddComponent<EarthBendingRagdollPart>();
                        scr.ragdollPart = ragdollPart;
                        scr.Initialize();
                    } else if (collisionInstance.damageStruct.damageType == DamageType.Blunt)
                    {
                        if (ragdollPart.data.bodyDamagerData.dismembermentAllowed)
                        {
                            creature.ragdoll.Slice(ragdollPart);
                            creature.Kill();
                        }
                        
                    }
                }

            }
            
        }

        IEnumerator RockShockWaveCoroutine(Vector3 contactPoint, Vector3 contactNormal, Vector3 contactNormalUpward, Vector3 impactVelocity)
        {

            EffectInstance effectInstance = Catalog.GetData<EffectData>(imbueHitGroundEffectId).Spawn(contactPoint, Quaternion.identity);
            effectInstance.Play();

            yield return new WaitForSeconds(0.4f);

            Collider[] sphereCast = Physics.OverlapSphere(contactPoint, imbueHitGroundRadius);

            foreach (Collider collider in sphereCast)
            {
                if (collider.attachedRigidbody)
                {
                    if (collider.attachedRigidbody.gameObject.layer != GameManager.GetLayer(LayerName.NPC) && imbue.colliderGroup.collisionHandler.rb != collider.attachedRigidbody)
                    {
                        //Is item
                        collider.attachedRigidbody.AddExplosionForce(imbueHitGroundExplosionForce, contactPoint, imbueHitGroundRadius * 2, imbueHitGroundExplosionUpwardModifier, ForceMode.Impulse);
                    }
                    if (collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.NPC) || collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.Ragdoll))
                    {
                        //Is creature
                        Creature creature = collider.GetComponentInParent<Creature>();
                        if (creature != Player.currentCreature && !creature.isKilled)
                        {
                            creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                            collider.attachedRigidbody.AddExplosionForce(imbueHitGroundExplosionForce, contactPoint, imbueHitGroundRadius * 2, imbueHitGroundExplosionUpwardModifier, ForceMode.Impulse);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        public override void OnCrystalUse(Side side, bool active)
        {
            Debug.Log("crystal use");
            if (!active)
            {
                Debug.Log("not active");
                Rigidbody rb;
                if (!imbue.colliderGroup.collisionHandler.item)
                {
                    RagdollPart ragdollPart = imbue.colliderGroup.collisionHandler.ragdollPart;
                    rb = ((ragdollPart != null) ? ragdollPart.rb : null);
                }
                else
                {
                    rb = imbue.colliderGroup.collisionHandler.item.rb;
                }
                rb.GetPointVelocity(imbue.colliderGroup.imbueShoot.position);
                if (rb.GetPointVelocity(imbue.colliderGroup.imbueShoot.position).magnitude > SpellCaster.throwMinHandVelocity && imbue.CanConsume(imbueCrystalUseCost))
                {
                    Debug.Log("magnitude good");
                    imbue.ConsumeInstant(imbueCrystalUseCost);
                    /*
                    if (this.imbueUseEffectData != null)
                    {
                        this.imbueUseEffectData.Spawn(this.imbue.colliderGroup.imbueShoot, true, Array.Empty<Type>()).Play(0);
                    }*/

                    Catalog.GetData<ItemData>("Rock Weapon 1").SpawnAsync(delegate (Item rock)
                    {
                        rock.transform.position = imbue.colliderGroup.imbueShoot.position;
                        rock.transform.rotation = imbue.colliderGroup.imbueShoot.rotation;

                        rock.IgnoreObjectCollision(imbue.colliderGroup.collisionHandler.item);

                        rock.rb.AddForce(imbue.colliderGroup.imbueShoot.forward * imbueCrystalShootForce * rb.GetPointVelocity(imbue.colliderGroup.imbueShoot.position).magnitude, ForceMode.Impulse);
                        rock.Throw(1f, Item.FlyDetection.Forced);
                    });                    
                }
            }
        }
    }


    public class EarthBendingRagdollPart : MonoBehaviour
    {
        public RagdollPart ragdollPart;
        private Creature creature;

        private List<HumanBodyBones> armLeftBones = new List<HumanBodyBones>(){HumanBodyBones.LeftLowerArm, HumanBodyBones.LeftUpperArm, HumanBodyBones.LeftHand};
        private List<HumanBodyBones> armRightBones = new List<HumanBodyBones>() { HumanBodyBones.RightLowerArm, HumanBodyBones.RightUpperArm, HumanBodyBones.RightHand};

        public void Initialize()
        {
            creature = ragdollPart.ragdoll.creature;

            RagdollPart.Type type = ragdollPart.type;

            RagdollPart.Type feetTypes = RagdollPart.Type.LeftFoot | RagdollPart.Type.RightFoot | RagdollPart.Type.LeftLeg | RagdollPart.Type.RightLeg;

            if (feetTypes.HasFlag(type))
            {
                creature.locomotion.speed = 0f;
            }

            RagdollPart.Type armTypesLeft = RagdollPart.Type.LeftArm | RagdollPart.Type.LeftHand;
            RagdollPart.Type armTypesRight = RagdollPart.Type.RightArm | RagdollPart.Type.RightHand;

            if (armTypesLeft.HasFlag(type) || armTypesRight.HasFlag(type))
            {
                Side side = Side.Right;
                bool check = false;

                if (armTypesLeft.HasFlag(type))
                {
                    side = Side.Left;
                    check = true;

                } else if (armTypesRight.HasFlag(type))
                {
                    side = Side.Right;
                    check = true;
                }

                if (check)
                {
                    if (creature.equipment.GetHeldHandle(side))
                    {
                        foreach (RagdollHand interactor in creature.equipment.GetHeldHandle(side).handlers)
                        {
                            if (interactor.creature == creature)
                            {
                                interactor.UnGrab(false);
                            }
                        }
                    }
                }
            }

            if (type == RagdollPart.Type.Head || type == RagdollPart.Type.Neck)
            {
                creature.brain.Stop();
                creature.locomotion.speed = 0f;
                creature.animator.speed = 0f;
            }

            Player.currentCreature.StartCoroutine(ImbueCoroutine());
        }

        public void ResetCreature()
        {
            ragdollPart.rb.constraints = RigidbodyConstraints.None;
            creature.locomotion.speed = creature.data.locomotionSpeed;

            if (!creature.brain.instance.isActive)
            {
                creature.brain.instance.Start();
            }
            creature.locomotion.speed = creature.data.locomotionSpeed;
            creature.animator.speed = 1f;
        }

        IEnumerator ImbueCoroutine()
        {
            creature.OnKillEvent += Creature_OnKillEvent;

            List<EffectInstance> effects = new List<EffectInstance>();

            foreach (Collider collider in ragdollPart.colliderGroup.colliders)
            {
                EffectInstance imbueEffect = Catalog.GetData<EffectData>("EarthRagdollImbue").Spawn(ragdollPart.transform);
                imbueEffect.SetCollider(collider);
                imbueEffect.Play();
                imbueEffect.SetIntensity(1f);

                effects.Add(imbueEffect);
            }

            ragdollPart.rb.constraints = RigidbodyConstraints.FreezeRotation;

            float startTime = Time.time;

            yield return new WaitUntil(() => Time.time - startTime > 30);

            ResetCreature();

            foreach (EffectInstance imbueEffect in effects)
            {
                imbueEffect.Stop();
            }

            Destroy(this);
        }

        private void Creature_OnKillEvent(CollisionInstance collisionStruct, EventTime eventTime)
        {
            foreach (RagdollPart ragdollPart in collisionStruct.damageStruct.hitRagdollPart.ragdoll.parts)
            {
                if (ragdollPart.gameObject.GetComponent<EarthBendingRagdollPart>())
                {
                    EarthBendingRagdollPart scr = ragdollPart.gameObject.GetComponent<EarthBendingRagdollPart>();
                    scr.ResetCreature();
                    Destroy(scr);
                }
            }
        }

        
    }

    public class EarthBendingController : MonoBehaviour
    {
        public bool leftHandActive;
        public bool rightHandActive;

        public float shieldMinSpeed;
        public string shieldItemId;
        public float shieldFreezeTime;
        public float shieldHealth;
        public float shieldPushMul;

        public float pushMinSpeed;
        public string pushEffectId;
        public float pushForce;

        public List<string> rockItemIds = new List<string>();
        public Vector2 rockMassMinMax;
        public float rockForceMul;
        public float rockFreezeTime;
        public float rockHeightFromGround;
        public float rockMoveSpeed;
        public string rockSummonEffectId;
        public float punchForce;
        public string punchEffectId;
        public float punchAimPrecision;
        public float punchAimRandomness;

        public string spikeEffectId;
        public float spikeMinSpeed;
        public float spikeRange;
        public float spikeMinAngle;
        public float spikeDamage;

        public string shatterEffectId;
        public float shatterMinSpeed;
        public float shatterRange;
        public float shatterRadius;
        public float shatterForce;

        private bool canSpawnShield = true;
        private bool canSpawnRockLeft = true;
        private bool canSpawnRockRight = true;
        private bool canSpawnSpikes = true;
        private bool canSpawnShatter = true;
        private bool canSpawnPillars = true;
        private bool canPush = true;

        public float rockPillarMinSpeed;
        public string rockPillarPointsId;
        public string rockPillarItemId;
        public string rockPillarCollisionEffectId;
        public float rockPillarLifeTime;
        public float rockPillarSpawnDelay;

        private bool pushCalled;
        private bool wallCalled;
        private bool shatterCalled;
        private Mana mana;

        private Vector3 leftVel;
        private Vector3 rightVel;

        public static bool GravActive;
        public static bool LightningActive;
        public static bool IceActive;
        public static bool FireActive;

        private bool abilityUsed;

        private bool handsPointingDown;


        private EffectData rockPillarCollisionEffectData;
        private EffectData rockPillarPointsData;
        private ItemData rockPillarItemData;

        private EffectData shatterEffectData;

        private EffectData spikeEffectData;

        private EffectData rockSummonEffectData;
        private EffectData punchEffectData;

        private EffectData pushEffectData;

        private ItemData shieldItemData;

        public void Initialize()
        {
            mana = Player.currentCreature.mana;

            // RockPillarData
            rockPillarCollisionEffectData = Catalog.GetData<EffectData>(rockPillarCollisionEffectId);
            rockPillarPointsData = Catalog.GetData<EffectData>(rockPillarPointsId);
            rockPillarItemData = Catalog.GetData<ItemData>(rockPillarItemId);

            // ShatterData
            shatterEffectData = Catalog.GetData<EffectData>(shatterEffectId);

            // SpikeData
            spikeEffectData = Catalog.GetData<EffectData>(spikeEffectId);

            // RockData
            rockSummonEffectData = Catalog.GetData<EffectData>(rockSummonEffectId);
            punchEffectData = Catalog.GetData<EffectData>(punchEffectId);

            // PushData
            pushEffectData = Catalog.GetData<EffectData>(pushEffectId);

            // ShieldData
            shieldItemData = Catalog.GetData<ItemData>(shieldItemId);
        }

        private void Update()
        {

            leftVel = Player.local.transform.rotation * PlayerControl.GetHand(Side.Left).GetHandVelocity();
            rightVel = Player.local.transform.rotation * PlayerControl.GetHand(Side.Right).GetHandVelocity();


            if (Vector3.Dot(Vector3.down, mana.casterLeft.magic.forward) > 0.8f && Vector3.Dot(Vector3.down, mana.casterRight.magic.forward) > 0.8f) //Hands are pointing down
            {
                handsPointingDown = true;
            }
            else
            {
                handsPointingDown = false;
            }
            

            UpdateValues(); //Bools

            if (leftHandActive && rightHandActive)
            {
                if (!PlayerControl.GetHand(Side.Right).gripPressed && !PlayerControl.GetHand(Side.Left).gripPressed)
                {
                    UpdatePush(); //Push logics

                    UpdateSpike(); //All the logic for spawning spikes
                } else if (PlayerControl.GetHand(Side.Right).gripPressed && PlayerControl.GetHand(Side.Left).gripPressed)
                {
                    UpdateShatter();

                    UpdateShield(); //Shield logics

                    UpdateRockPillar();
                }
            }

            UpdateRock(); //All the logic for spawing rocks

        }


        private void UpdateValues()
        {
            if (!leftHandActive || !leftHandActive)
            {
                
                canSpawnShield = true;

                canSpawnSpikes = true;

                canSpawnShatter = true;

                canPush = true;

                if (!leftHandActive)
                {
                    canSpawnRockLeft = true;
                }
                if (!rightHandActive)
                {
                    canSpawnRockRight = true;
                } 
            }
        }

        private void UpdateShield()
        {
            if (wallCalled)
            {
                wallCalled = false;
            }

            if (canSpawnShield)
            {
                if (handsPointingDown) //If hands is pointing down
                {
                    if (Mathf.Abs(Vector3.Dot(-Player.currentCreature.transform.right, mana.casterLeft.magic.up)) < 0.4 && Mathf.Abs(Vector3.Dot(Player.currentCreature.transform.right, mana.casterLeft.magic.up)) < 0.4) //If hands are not to the side
                    {
                        if (Vector3.Dot(Vector3.up, leftVel) > shieldMinSpeed && Vector3.Dot(Vector3.up, rightVel) > shieldMinSpeed) //If hands are moving up at a set speed
                        {
                            StartCoroutine(SpawnShieldCoroutine());
                            wallCalled = true;
                            canSpawnShield = false;
                            abilityUsed = true;
                        }
                    }
                }
            }
        }

        private void UpdatePush()
        {
            if (pushCalled)
            {
                pushCalled = false;
            }

            //Push logic
            if (Mathf.Abs(Vector3.Dot(Vector3.up, mana.casterLeft.magic.forward)) < 0.3f && Mathf.Abs(Vector3.Dot(Vector3.up, mana.casterRight.magic.forward)) < 0.3f) //If hands are not pointing up or down
            {
                if (Vector3.Dot(Vector3.up, mana.casterLeft.magic.up) > 0.7f && Vector3.Dot(Vector3.up, mana.casterRight.magic.up) > 0.7f) // Fingers are pointing up
                {
                    if (Vector3.Dot(mana.casterLeft.magic.forward, leftVel) > pushMinSpeed && Vector3.Dot(mana.casterLeft.magic.forward, rightVel) > pushMinSpeed) //Hands are moving forwards
                    {
                        if (canPush)
                        {
                            canPush = false;
                            pushCalled = true;
                            abilityUsed = true;
                        }
                    }
                }
            }
        }

        private void UpdateSpike()
        {
            //Spike logic
            Vector3 vecBetweenForAndDown = Vector3.Slerp(Vector3.up, Player.currentCreature.transform.forward, 0.5f).normalized; //Get 45 degree angle between up and forwards

            if (canSpawnSpikes)
            {
                if (Vector3.Dot(mana.casterLeft.magic.forward, vecBetweenForAndDown) > 0.7f && Vector3.Dot(mana.casterRight.magic.forward, vecBetweenForAndDown) > 0.7f)
                {
                    if (Vector3.Dot(mana.casterLeft.magic.right, -Player.currentCreature.transform.right) > 0.7 && Vector3.Dot(mana.casterRight.magic.right, -Player.currentCreature.transform.right) > 0.7) //Hand are pointing to the sides
                    {
                        if (Vector3.Dot(mana.casterLeft.magic.forward, leftVel) > spikeMinSpeed && Vector3.Dot(mana.casterLeft.magic.forward, rightVel) > spikeMinSpeed) //Hands are moving forwards
                        {
                            StartCoroutine(SpikeCoroutine());
                            canSpawnSpikes = false;
                            abilityUsed = true;
                        }
                    }
                }
            }
        }

        private void UpdateRock()
        {
            if (leftHandActive)
            {
                if (!PlayerControl.GetHand(Side.Left).gripPressed) //If grip is not pressed
                {
                    if (canSpawnRockLeft)
                    {
                        if (Vector3.Dot(Vector3.down, mana.casterLeft.magic.forward) > 0.7f) //If hand is pointing down
                        {
                            if (Mathf.Abs(Vector3.Dot(-Player.currentCreature.transform.right, mana.casterLeft.magic.up)) < 0.7) //If hand is not to the side
                            {
                                if (Vector3.Dot(Vector3.up, leftVel) > shieldMinSpeed) //If speed is greater than min
                                {
                                    StartCoroutine(SpawnRockCoroutine(mana.casterLeft));
                                    canSpawnRockLeft = false;
                                }
                            }
                        }
                    }
                }
            }

            //if only right
            if (rightHandActive)
            {
                if (!PlayerControl.GetHand(Side.Right).gripPressed) //If grip is not pressed
                {
                    if (canSpawnRockRight)
                    {
                        if (Vector3.Dot(Vector3.down, mana.casterRight.magic.forward) > 0.7f) //If hand is pointing down
                        {
                            if (Mathf.Abs(Vector3.Dot(-Player.currentCreature.transform.right, mana.casterRight.magic.up)) < 0.7) //If hand is not to the side
                            {
                                if (Vector3.Dot(Vector3.up, rightVel) > shieldMinSpeed)
                                {
                                    StartCoroutine(SpawnRockCoroutine(mana.casterRight));
                                    canSpawnRockRight = false;
                                }
                            }
                        }
                    }
                }
            }
        }

        private void UpdateShatter()
        {
            if (shatterCalled)
            {
                shatterCalled = false;
            }

            if (canSpawnShatter)
            {
                if (handsPointingDown)
                {
                    if (Vector3.Dot(Player.currentCreature.transform.right, mana.casterLeft.magic.right) > 0.8f && Vector3.Dot(Player.currentCreature.transform.right, mana.casterRight.magic.right) > 0.8f)
                    {
                        if (Vector3.Dot(mana.casterLeft.magic.up, leftVel) > shatterMinSpeed && Vector3.Dot(mana.casterLeft.magic.up, rightVel) > shatterMinSpeed) // Moving hands forwards
                        {
                            StartCoroutine(ShatterCoroutine());
                            shatterCalled = true;
                            canSpawnShatter = false;
                            abilityUsed = true;
                        }
                    }
                }
            }
        }

        private void UpdateRockPillar()
        {
            if (canSpawnPillars)
            {
                if (handsPointingDown)
                {
                    if (Vector3.Dot(mana.casterLeft.magic.forward, leftVel) > rockPillarMinSpeed && Vector3.Dot(mana.casterLeft.magic.forward, rightVel) > rockPillarMinSpeed) // Moving hands forwards down
                    {
                        StartCoroutine(PillarDownCoroutine());
                        canSpawnPillars = false;
                        abilityUsed = true;
                    }
                }
            }
        }

        IEnumerator PillarDownCoroutine()
        {
            Vector3 middlePoint = Vector3.Lerp(mana.casterLeft.magic.position, mana.casterRight.magic.position, 0.5f) + Player.currentCreature.transform.forward * 0.25f;
            RaycastHit hit;
            if (Physics.Raycast(middlePoint, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
            {
                EffectInstance spawnPoints = rockPillarPointsData.Spawn(hit.point, Player.currentCreature.transform.rotation);

                foreach (Transform child in spawnPoints.effects[0].transform)
                {
                    rockPillarItemData.SpawnAsync(delegate (Item rockPillar)
                    {
                        rockPillar.transform.position = child.position;
                        rockPillar.transform.rotation = child.rotation;

                        rockPillar.rb.velocity = -Vector3.up * 2;

                        RockPillarCollision scr = rockPillar.gameObject.AddComponent<RockPillarCollision>();
                        scr.effectData = rockPillarCollisionEffectData;

                        StartCoroutine(DespawnItemAfterTime(rockPillarLifeTime, rockPillar));
                    });
                    
                    yield return new WaitForSeconds(rockPillarSpawnDelay);
                }

                yield return new WaitForSeconds(rockPillarLifeTime - (rockPillarSpawnDelay * spawnPoints.effects[0].transform.childCount));
            }

            canSpawnPillars = true;
        }

        IEnumerator DespawnItemAfterTime(float delay, Item item)
        {
            yield return new WaitForSeconds(delay);
            item.gameObject.SetActive(false);
            yield return new WaitForEndOfFrame();
            item.Despawn();
        }

        IEnumerator ShatterCoroutine()
        {
            Vector3 middlePoint = Vector3.Lerp(mana.casterLeft.magic.position, mana.casterRight.magic.position, 0.5f) + ((leftVel.normalized + rightVel.normalized) * 0.25f);
            RaycastHit hit;
            RaycastHit hitEnd;

            Vector3 forwards = Player.currentCreature.transform.forward;

            if (Physics.Raycast(middlePoint, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Default")) && Physics.Raycast(middlePoint+forwards*shatterRange, Vector3.down, out hitEnd, 2.5f, LayerMask.GetMask("Default")))
            {
                EffectInstance shatter = shatterEffectData.Spawn(hit.point, Player.currentCreature.transform.rotation);
                shatter.Play();

                Collider[] colliders = Physics.OverlapCapsule(hit.point, hit.point + (forwards * shatterRange), shatterRadius);

                foreach (Collider collider in colliders)
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

                                StartCoroutine(AddForceInOneFrame(collider.attachedRigidbody, forwards));
                            }
                        }
                        else if (collider.GetComponentInParent<Item>())
                        {
                            Item item = collider.GetComponentInParent<Item>();

                            if (item.mainHandler)
                            {
                                if (item.mainHandler.creature != Player.currentCreature)
                                {
                                    StartCoroutine(AddForceInOneFrame(collider.attachedRigidbody, forwards));
                                }
                            }

                        }
                    }
                }
            }
            yield return null;

        }

        IEnumerator AddForceInOneFrame(Rigidbody rb, Vector3 forwards)
        {
            yield return new WaitForEndOfFrame();
            rb.AddForce(shatterForce * (forwards.normalized + Vector3.up), ForceMode.Impulse);
        }

        IEnumerator SpikeCoroutine()
        {
            Vector3 middlePoint = Vector3.Lerp(mana.casterLeft.magic.position, mana.casterRight.magic.position, 0.5f) + ((leftVel.normalized + rightVel.normalized) * 0.25f);
            RaycastHit hit;
            RaycastHit hitEnd;

            Vector3 forwards = Player.currentCreature.transform.forward;

            if (Physics.Raycast(middlePoint, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Default")) && Physics.Raycast(middlePoint + forwards * spikeRange, Vector3.down, out hitEnd, 2.5f, LayerMask.GetMask("Default")))
            {
                
                /*
                for (int i = 0; i < 12; i++)
                {
                    Quaternion shootRot = Quaternion.Euler(forwards + new Vector3(0, UnityEngine.Random.Range(-15, 15)));

                    GameObject point = new GameObject("SpawnPoint");
                    point.transform.rotation = shootRot;
                    point.transform.position = hit.point;

                    StartCoroutine(SpikeSpawnCoroutine(point));
                }*/
                
                EffectInstance spikes = spikeEffectData.Spawn(hit.point, Player.currentCreature.transform.rotation);
                spikes.Play();

                //Get creatures in front
                foreach (Creature creature in Creature.list)
                {
                    if (creature != Player.currentCreature)
                    {

                        float dist = Vector3.Distance(creature.transform.position, hit.point);

                        if (dist < spikeRange)
                        {
                            Vector3 dir = (creature.transform.position - hit.point).normalized;
                            if (Vector3.Dot(dir, forwards) > spikeMinAngle)
                            {
                                if (creature.state != Creature.State.Dead)
                                {
                                    creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                                    CollisionInstance collisionStruct = new CollisionInstance(new DamageStruct(DamageType.Pierce, spikeDamage));
           
                                    creature.Damage(collisionStruct);


                                    //Spawn itemSpike
                                    Catalog.GetData<ItemData>("EarthBendingRockSpikes").SpawnAsync(delegate(Item spike)
                                    {
                                        spike.transform.position = creature.transform.position;
                                        spike.transform.rotation = Player.currentCreature.transform.rotation;

                                        Transform spikeMeshTransform = spike.GetCustomReference("Mesh");

                                        spikeMeshTransform.localScale = new Vector3(.15f, .15f, .15f);

                                        spike.transform.localEulerAngles += new Vector3(35, 0, 0);

                                        spike.Despawn(5f);
                                    });
                                }
                            }
                        }
                    }
                } 
            }
            yield return null;
        }

        IEnumerator SpawnShieldCoroutine()
        {
            Vector3 middlePoint = Vector3.Lerp(mana.casterLeft.magic.position, mana.casterRight.magic.position, 0.5f) + Player.currentCreature.transform.forward * 0.7f;

            RaycastHit hit;
            if (Physics.Raycast(middlePoint, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
            {

                shieldItemData.SpawnAsync(delegate (Item shield)
                {
                    shield.StartCoroutine(ShieldSpawnedCoroutine(shield, hit));
                }, null, null, null,true, new List<Item.SavedValue>() { new Item.SavedValue("HP", shieldHealth.ToString()) });
            }

            yield return null;
        }

        private IEnumerator ShieldSpawnedCoroutine(Item shield, RaycastHit hit)
        {
            shield.SetSavedValue("HP", shieldHealth.ToString());

            Vector3 spawnPoint = hit.point;

            shield.transform.position = spawnPoint;
            shield.transform.rotation = Player.currentCreature.transform.rotation;

            Animation shieldAnimation = shield.GetCustomReference("RockAnim").GetComponent<Animation>();

            shieldAnimation.Play();

            yield return new WaitForSeconds(shieldAnimation.clip.length);

            shield.colliderGroups[0].collisionHandler.OnCollisionStartEvent += Shield_OnDamageReceivedEvent;

            float startTime = Time.time;

            while (Time.time - startTime < shieldFreezeTime)
            {

                if (pushCalled)
                {
                    //Get forwards
                    Vector3 forwards = Player.currentCreature.transform.forward;
                    //Get wall to player direction
                    Vector3 dir = (shield.transform.position - Player.currentCreature.transform.position).normalized;

                    //Dot
                    if (Vector3.Dot(dir, forwards) > 0.6f)
                    {
                        StartCoroutine(ShieldPush(shield, (leftVel.normalized + rightVel.normalized)));
                        break;
                    }
                }


                yield return new WaitForEndOfFrame();
            }

            yield return new WaitUntil(() => Time.time - startTime > shieldFreezeTime);

            shield.colliderGroups[0].collisionHandler.OnCollisionStartEvent -= Shield_OnDamageReceivedEvent;

            StartCoroutine(DespawnShield(shield));
            
        }

        private IEnumerator ShieldPush(Item shield, Vector3 direction)
        {
            shield.rb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            shield.rb.isKinematic = false;
            shield.rb.useGravity = true;

            EffectInstance effectInstance = pushEffectData.Spawn(shield.transform.position, Quaternion.identity);
            effectInstance.Play();

            shield.rb.AddForce(direction * pushForce * shieldPushMul, ForceMode.Impulse);

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            yield return new WaitUntil(() => shield.rb.velocity.magnitude < 8f);

            shield.rb.constraints = RigidbodyConstraints.None;
            shield.rb.isKinematic = true;
            shield.rb.useGravity = false;
        }

        private void Shield_OnDamageReceivedEvent(CollisionInstance collisionStruct)
        {
            if (collisionStruct.impactVelocity.magnitude > 2)
            {
                bool l_leftS = (collisionStruct.sourceColliderGroup != null ? collisionStruct.sourceColliderGroup == Player.currentCreature.handLeft.colliderGroup : false);
                bool l_leftT = (collisionStruct.targetColliderGroup != null ? collisionStruct.targetColliderGroup == Player.currentCreature.handLeft.colliderGroup : false);

                bool l_rightS = (collisionStruct.sourceColliderGroup != null ? collisionStruct.sourceColliderGroup == Player.currentCreature.handRight.colliderGroup : false);
                bool l_rightT = (collisionStruct.targetColliderGroup != null ? collisionStruct.targetColliderGroup == Player.currentCreature.handRight.colliderGroup : false);

                Item shield = null;
                Collider hitCol = null;

                if (collisionStruct.targetCollider.GetComponentInParent<Item>())
                {
                    if (collisionStruct.targetCollider.GetComponentInParent<Item>().itemId == shieldItemId)
                    {
                        shield = collisionStruct.targetCollider.GetComponentInParent<Item>();
                    }
                } else
                {
                    shield = collisionStruct.sourceCollider.GetComponentInParent<Item>();
                }

                if (l_leftS || l_rightS)
                {
                    shield = collisionStruct.targetCollider.GetComponentInParent<Item>();
                    hitCol = collisionStruct.targetCollider;

                }
                if (l_leftT || l_rightT)
                {
                    shield = collisionStruct.sourceCollider.GetComponentInParent<Item>();
                    hitCol = collisionStruct.sourceCollider;
                }

                string HP;
                shield.TryGetSavedValue("HP", out HP);

                float OldHP = float.Parse(HP);

                if (shield != null)
                {
                    if (OldHP - 1 > 0)
                    {
                        if (l_leftS || l_leftT)
                        {
                            StartCoroutine(ShieldPush(shield, leftVel.normalized.normalized));
                        }
                        if (l_rightS || l_rightT)
                        {
                            StartCoroutine(ShieldPush(shield, rightVel.normalized.normalized));
                        }

                        
                    }
                }

                string lastHit = "0";
                shield.TryGetSavedValue("LastHit", out lastHit);

                if (Time.time - float.Parse(lastHit) > 0.1f)
                {
                    if (OldHP - 1 < 1)
                    {
                        StartCoroutine(DespawnShield(shield));
                    }
                    else
                    {
                        shield.SetSavedValue("HP", (OldHP - 1).ToString());
                    }

                    shield.SetSavedValue("LastHit", Time.time.ToString());

                }


            }
        }


        IEnumerator DespawnShield(Item shield)
        {
            ParticleSystem ps = shield.GetCustomReference("ShatterParticles").GetComponent<ParticleSystem>();
            ParticleSystem activePS = shield.GetCustomReference("ActiveParticles").GetComponent<ParticleSystem>();

            if (!ps.isPlaying) // Only run if it aint playing, fixes multiple calls
            {
                //disable colliders
                foreach (ColliderGroup colliderGroup in shield.colliderGroups)
                {
                    foreach (Collider collider in colliderGroup.colliders)
                    {
                        collider.enabled = false;
                    }
                }

                //Disable mesh renderer
                foreach (Renderer meshRenderer in shield.renderers)
                {
                    meshRenderer.enabled = false;
                }

                //Play break particles
                ps.Play();

                //Stop active particles
                activePS.Stop();

                //Despawn after particles
                yield return new WaitForSeconds(ps.main.duration + ps.main.startLifetime.constantMax);

                if (shield)
                {
                    shield.Despawn();
                }
            }


        }

        IEnumerator SpawnRockCoroutine(SpellCaster spellCasterSide)
        {
            Vector3 middlePoint = spellCasterSide.transform.position + Player.currentCreature.transform.forward * 0.25f;

            RaycastHit hit;
            if (Physics.Raycast(middlePoint, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
            {
                Vector3 spawnPoint = hit.point + Vector3.down;

                string randRock = rockItemIds[UnityEngine.Random.Range(0, rockItemIds.Count)];


                Catalog.GetData<ItemData>(randRock).SpawnAsync(delegate (Item rock)
                {
                    rock.StartCoroutine(RockSpawnedCoroutine(rock, spawnPoint, hit));
                }, spawnPoint, Player.currentCreature.transform.rotation, null, false);
            }
            yield return null;
        }

        IEnumerator RockSpawnedCoroutine(Item rock, Vector3 spawnPoint, RaycastHit hit)
        {
            rock.rb.mass = UnityEngine.Random.Range(rockMassMinMax.x, rockMassMinMax.y);

            rock.transform.position = spawnPoint;
            rock.transform.rotation = Player.currentCreature.transform.rotation;

            EffectInstance rockSummonEffect = rockSummonEffectData.Spawn(hit.point, Quaternion.identity);
            rockSummonEffect.Play();

            foreach (ColliderGroup colliderGroup in rock.colliderGroups)
            {
                foreach (Collider collider in colliderGroup.colliders)
                {
                    collider.enabled = false;
                }
            }

            rock.rb.useGravity = false;
            //shield.rb.AddForce(Vector3.up * 60, ForceMode.Impulse);



            while (rock.transform.position.y < hit.point.y + ((Player.currentCreature.ragdoll.headPart.transform.position.y - hit.point.y) / rockHeightFromGround))
            {
                rock.transform.position = Vector3.MoveTowards(rock.transform.position, hit.point + new Vector3(0, ((Player.currentCreature.ragdoll.headPart.transform.position.y - hit.point.y) / rockHeightFromGround) + 0.05f, 0), Time.deltaTime * rockMoveSpeed);
                yield return new WaitForEndOfFrame();
            }

            foreach (ColliderGroup colliderGroup in rock.colliderGroups)
            {
                foreach (Collider collider in colliderGroup.colliders)
                {
                    collider.enabled = true;
                }
            }
 

            rock.rb.velocity = Vector3.zero;
            float startTime = Time.time;

            rock.rb.angularVelocity = UnityEngine.Random.insideUnitSphere * 2;

            rock.colliderGroups[0].collisionHandler.OnCollisionStartEvent += Rock_OnDamageReceivedEvent;

            while (Time.time - startTime < rockFreezeTime)
            {
                if (pushCalled)
                {

                    EffectInstance effectInstance = pushEffectData.Spawn(rock.transform.position, Quaternion.identity);
                    effectInstance.Play();

                    rock.rb.AddForce((leftVel.normalized + rightVel.normalized) * pushForce * rockForceMul, ForceMode.Impulse);
                    break;
                }
                yield return new WaitForEndOfFrame();
            }

            rock.rb.useGravity = true;

            rock.colliderGroups[0].collisionHandler.OnCollisionStartEvent -= Rock_OnDamageReceivedEvent;

            yield return null;
        }

        private void Rock_OnDamageReceivedEvent(CollisionInstance collisionStruct)
        {
            Debug.Log("Rock on damage");

            if (collisionStruct.impactVelocity.magnitude > 1)
            {
                bool l_leftS = (collisionStruct.sourceColliderGroup == Player.currentCreature.handLeft.colliderGroup);
                bool l_leftT = (collisionStruct.targetColliderGroup == Player.currentCreature.handLeft.colliderGroup);

                bool l_rightS = (collisionStruct.sourceColliderGroup == Player.currentCreature.handRight.colliderGroup);
                bool l_rightT = (collisionStruct.targetColliderGroup == Player.currentCreature.handRight.colliderGroup);


                Item ownItem = null;
                Collider hitCol = null;
                Vector3 forceVec = Vector3.zero;



                if (l_leftS || l_rightS)
                {
                    ownItem = collisionStruct.targetCollider.GetComponentInParent<Item>();
                    hitCol = collisionStruct.targetCollider;

                }
                if (l_leftT || l_rightT)
                {
                    ownItem = collisionStruct.sourceCollider.GetComponentInParent<Item>();
                    hitCol = collisionStruct.sourceCollider;
                }



                if (ownItem == null)
                    return;

                ownItem.rb.useGravity = true;

                EffectInstance effectInstance = Catalog.GetData<EffectData>(punchEffectId, true).Spawn(ownItem.transform.position, Quaternion.identity);
                effectInstance.Play();

                if (l_leftS || l_leftT)
                {
                    forceVec = AimAssist(ownItem.transform.position, leftVel.normalized, punchAimPrecision, punchAimRandomness);
                }
                if (l_rightS || l_rightT)
                {
                    forceVec = AimAssist(ownItem.transform.position, rightVel.normalized, punchAimPrecision, punchAimRandomness);
                }


                hitCol.attachedRigidbody.AddForce(forceVec * punchForce, ForceMode.Impulse);
            }
        }

        private Vector3 AimAssist(Vector3 ownPosition, Vector3 ownDirection, float aimPrecision, float randomness)
        {
            Creature toHit = null;
            float closest = -1;
            Vector3 dirS = Vector3.zero;

            foreach (Creature creature in Creature.list)
            {
                if (creature != Player.currentCreature && !creature.isKilled)
                {
                    Vector3 dir = (creature.ragdoll.GetPart(RagdollPart.Type.Head).transform.position - ownPosition).normalized;
                    if (Vector3.Dot(ownDirection, dir) > aimPrecision)
                    {
                        if (Vector3.Dot(ownDirection, dir) > closest)
                        {
                            closest = Vector3.Dot(ownDirection, dir);
                            toHit = creature;
                            dirS = dir;
                        }
                    }
                }
            }

            if (toHit != null)
            {
                Vector3 rand = UnityEngine.Random.insideUnitSphere * randomness;

                return (dirS + rand).normalized;
            } else
            {
                return ownDirection;
            }

            
        }
    }    
    
    public class RockPillarCollision : MonoBehaviour
    {
        public EffectData effectData;

        private void OnCollisionEnter(Collision col)
        {
            EffectInstance hitEffect = effectData.Spawn(col.contacts[0].point, Quaternion.identity);
            hitEffect.Play();

            if (col.collider.GetComponentInParent<Creature>())
            {
                Creature creature = col.collider.GetComponentInParent<Creature>();
                if (creature != Player.currentCreature)
                {
                    if (!creature.isKilled)
                    {
                        CollisionInstance collisionStruct = new CollisionInstance(new DamageStruct(DamageType.Energy, 20.0f));

                        creature.Damage(collisionStruct);
                    }
                }
            }

            Destroy(this);
        }
    }
}
