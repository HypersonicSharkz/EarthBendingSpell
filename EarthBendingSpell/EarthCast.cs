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
        public string shieldItemId;

        public string pushEffectId;

        public List<string> rockItemIds = new List<string>();
        public Vector2 rockMassMinMax;

        public string rockSummonEffectId;
        public string punchEffectId;
        public float punchAimRandomness;
        public string spikeEffectId;
        public float spikeRange;
        public float spikeMinAngle;
        public string shatterEffectId;
        public float shatterRange;
        public float shatterRadius;
        public string rockPillarPointsId;
        public string rockPillarItemId;
        public string rockPillarCollisionEffectId;
        public float rockPillarSpawnDelay;
        public string imbueHitGroundEffectId;
        public float imbueHitGroundConsumption;
        public float imbueHitGroundRechargeDelay;
        public float imbueHitGroundMinVelocity;

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
                scr2.shieldItemId = shieldItemId;


                scr2.pushEffectId = pushEffectId;


                scr2.rockItemIds = rockItemIds;
                scr2.rockMassMinMax = rockMassMinMax;
                scr2.rockSummonEffectId = rockSummonEffectId;
                scr2.punchEffectId = punchEffectId;
                scr2.punchAimRandomness = punchAimRandomness;

                scr2.spikeEffectId = spikeEffectId;
                scr2.spikeRange = spikeRange;
                scr2.spikeMinAngle = spikeMinAngle;

                scr2.shatterEffectId = shatterEffectId;
                scr2.shatterRadius = shatterRadius;
                scr2.shatterRange = shatterRange;

                scr2.rockPillarPointsId = rockPillarPointsId;
                scr2.rockPillarItemId = rockPillarItemId;
                scr2.rockPillarCollisionEffectId = rockPillarCollisionEffectId;
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

        public override bool OnImbueCollisionStart(CollisionInstance collisionInstance)
        {
            base.OnImbueCollisionStart(collisionInstance);

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
                            creature.ragdoll.TrySlice(ragdollPart);
                            creature.Kill();
                        }
                        
                    }
                }

            }

            return true;
            
        }

        public override bool OnCrystalSlam(CollisionInstance collisionInstance)
        {
            if (!EarthBendingController.skillStaffSlam)
                return false;

            imbue.colliderGroup.collisionHandler.item.StartCoroutine(RockShockWaveCoroutine(collisionInstance.contactPoint, collisionInstance.contactNormal, collisionInstance.sourceColliderGroup.transform.up, collisionInstance.impactVelocity));
            imbueHitGroundLastTime = Time.time;
            return true;
        }


        IEnumerator RockShockWaveCoroutine(Vector3 contactPoint, Vector3 contactNormal, Vector3 contactNormalUpward, Vector3 impactVelocity)
        {

            EffectInstance effectInstance = Catalog.GetData<EffectData>(imbueHitGroundEffectId).Spawn(contactPoint, Quaternion.identity);
            effectInstance.Play();

            yield return new WaitForSeconds(0.4f);

            Collider[] sphereCast = Physics.OverlapSphere(contactPoint, EarthBendingController.imbueHitGroundRadius);

            foreach (Collider collider in sphereCast)
            {
                if (collider.attachedRigidbody)
                {
                    if (collider.attachedRigidbody.gameObject.layer != GameManager.GetLayer(LayerName.NPC) && imbue.colliderGroup.collisionHandler.physicBody.rigidBody != collider.attachedRigidbody)
                    {
                        //Is item
                        collider.attachedRigidbody.AddExplosionForce(EarthBendingController.imbueHitGroundExplosionForce, contactPoint, EarthBendingController.imbueHitGroundRadius * 2, EarthBendingController.imbueHitGroundExplosionUpwardModifier, ForceMode.Impulse);
                    }
                    if (collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.NPC) || collider.attachedRigidbody.gameObject.layer == GameManager.GetLayer(LayerName.Ragdoll))
                    {
                        //Is creature
                        Creature creature = collider.GetComponentInParent<Creature>();
                        if (creature != Player.currentCreature && !creature.isKilled)
                        {
                            creature.ragdoll.SetState(Ragdoll.State.Destabilized);
                            collider.attachedRigidbody.AddExplosionForce(EarthBendingController.imbueHitGroundExplosionForce, contactPoint, EarthBendingController.imbueHitGroundRadius * 2, EarthBendingController.imbueHitGroundExplosionUpwardModifier, ForceMode.Impulse);
                        }
                    }
                }
            }

            yield return new WaitForSeconds(0.1f);
        }

        public override bool OnCrystalUse(RagdollHand hand, bool active)
        {
            if (!EarthBendingController.skillStaffShoot)
                return false;

            Debug.Log("crystal use");
            if (!active)
            {
                Debug.Log("not active");
                Rigidbody rb;
                if (!imbue.colliderGroup.collisionHandler.item)
                {
                    RagdollPart ragdollPart = imbue.colliderGroup.collisionHandler.ragdollPart;
                    rb = ((ragdollPart != null) ? ragdollPart.physicBody.rigidBody : null);
                }
                else
                {
                    rb = imbue.colliderGroup.collisionHandler.item.physicBody.rigidBody;
                }
                rb.GetPointVelocity(imbue.colliderGroup.imbueShoot.position);
                if (rb.GetPointVelocity(imbue.colliderGroup.imbueShoot.position).magnitude > SpellCaster.throwMinHandVelocity && imbue.CanConsume(EarthBendingController.imbueCrystalUseCost))
                {
                    Debug.Log("magnitude good");
                    imbue.ConsumeInstant(EarthBendingController.imbueCrystalUseCost);
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

                        rock.physicBody.AddForce(imbue.colliderGroup.imbueShoot.forward * EarthBendingController.imbueCrystalShootForce * rb.GetPointVelocity(imbue.colliderGroup.imbueShoot.position).magnitude, ForceMode.Impulse);
                        rock.Throw(1f, Item.FlyDetection.Forced);
                    });

                    return true;
                }
            }

            return false;
        }
    }


    public class EarthBendingRagdollPart : MonoBehaviour
    {
        [ModOption(category = "Petrification", name = "Petrify Duration", tooltip = "How long the petrification of body parts will last")]
        [ModOptionIntValues(1, 30, 1)]
        public static int petrifyDuration = 10;

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
                creature.locomotion.speedModifiers.Add(new Locomotion.SpeedModifier(this, 0,0,0,0,0,0));
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
                creature.locomotion.speedModifiers.Add(new Locomotion.SpeedModifier(this, 0, 0, 0, 0, 0, 0));
                creature.animator.speed = 0f;
            }

            Player.currentCreature.StartCoroutine(ImbueCoroutine());
        }

        public void ResetCreature()
        {
            ragdollPart.physicBody.rigidBody.constraints = RigidbodyConstraints.None;
            creature.locomotion.ClearSpeedModifiers();

            if (!creature.brain.instance.isActive)
            {
                creature.brain.instance.Start();
            }
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

            ragdollPart.physicBody.rigidBody.constraints = RigidbodyConstraints.FreezeRotation;

            float startTime = Time.time;

            yield return new WaitUntil(() => Time.time - startTime > petrifyDuration);

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
        public static EarthBendingController Instance { get; private set; }

        [ModOption(category = "Aim Assist", name = "Apply Aim Assist to Push", tooltip = "Apply Aim Assist to Push")]
        public static bool pushAimAssist = false;

        public static bool skillSummonShield;
        public static bool skillPush;
        public static bool skillSpikes;
        public static bool skillShatter;
        public static bool skillRockPillars;
        public static bool skillAimAssist;
        public static bool skillStaffSlam;
        public static bool skillStaffShoot;

        public bool leftHandActive;
        public bool rightHandActive;

        public string shieldItemId;

        [ModOption(category = "Shield", name = "Shield Life Time", tooltip = "How long the wall will stay summoned")]
        [ModOptionIntValues(1, 30, 1)]
        public static int shieldFreezeTime = 10;

        [ModOption(category = "Shield", name = "Shield Health", tooltip = "How many collisions it will take to break the wall")]
        [ModOptionIntValues(1, 10, 1)]
        public static int shieldHealth = 3;

        [ModOption(category = "Shield", name = "Shield Min Speed", tooltip = "The speed of the hand gesture needed to summon wall")]
        public static int shieldMinSpeed = 2;

        [ModOption(category = "Push", name = "Push Min Speed", tooltip = "The speed of the hand gesture needed to push all summoned rocks or walls")]
        public static int pushMinSpeed = 2;

        [ModOption(category = "Push", name = "Rock Push Force", tooltip = "The force applied to rocks when pushed")]
        [ModOptionIntValues(0, 100, 1)]
        public static int pushForceRock = 20;

        [ModOption(category = "Push", name = "Shield Push Force", tooltip = "The force applied to shields when pushed")]
        [ModOptionIntValues(0, 100, 1)]
        public static int pushForceShield = 20;

        [ModOption(category = "Rock", name = "Rock Freeze Time", tooltip = "How long the rock will stay floating when summoned")]
        [ModOptionIntValues(1, 30, 1)]
        public static int rockFreezeTime = 12;

        [ModOption(category = "Rock", name = "Rock Summon Height", tooltip = "How high the summoned rock will float")]
        [ModOptionFloatValues(0, 3, 0.1f)]
        public static float rockHeightFromGround = 1.2f;

        [ModOption(category = "Rock", name = "Rock Summon Speed", tooltip = "How fast the rock will up from the ground")]
        [ModOptionIntValues(1, 10, 1)]
        public static int rockMoveSpeed = 4;

        [ModOption(category = "Punch", name = "Rock Punch Force", tooltip = "Force added to rocks when punched")]
        [ModOptionIntValues(0, 100, 1)]
        public static int punchForceRock = 6;

        [ModOption(category = "Punch", name = "Shield Punch Force", tooltip = "Force added to shields when punched")]
        [ModOptionIntValues(0, 100, 1)]
        public static int punchForceShield = 6;

        [ModOption(category = "Aim Assist", name = "Aim Precision", tooltip = "How precise your initial hit must be to activate Aim Assist, higher will make it easier to hit further away targets")]
        [ModOptionFloatValues(0, 5, 0.1f)]
        public static float punchAimPrecision = 0.5f;

        [ModOption(category = "Earth Spikes", name = "Spikes Min Speed", tooltip = "The speed of the hand gesture needed to summon spikes")]
        public static int spikeMinSpeed = 2;

        [ModOption(category = "Earth Spikes", name = "Spikes Damage", tooltip = "Damage applied to enemies hit by the spikes")]
        [ModOptionIntValues(5, 100, 5)]
        public static int spikeDamage = 20;

        [ModOption(category = "Earth Shatter", name = "Shatter Min Speed", tooltip = "The speed of the hand gesture needed to summon Earth Shatter")]
        public static int shatterMinSpeed = 3;

        [ModOption(category = "Earth Shatter", name = "Shatter Force", tooltip = "Force applied to enemies hit by Earth Shatter")]
        [ModOptionIntValues(10, 500, 5)]
        public static int shatterForce = 80;

        [ModOption(category = "Rock Pillar", name = "Pillar Min Speed", tooltip = "The speed of the hand gesture needed to summon Rock Pillar")]
        public static int rockPillarMinSpeed = 2;

        [ModOption(category = "Rock Pillar", name = "Rock Pillar Life Time", tooltip = "Lifetime of the pillars after they have been summoned")]
        [ModOptionIntValues(1, 30, 1)]
        public static int rockPillarLifeTime = 8;

        [ModOption(category = "Staff Slam", name = "Staff Slam Upwards Modifier", tooltip = "Upwards modifier for explosion force applied to enemies")]
        [ModOptionFloatValues(0, 10, 0.2f)]
        public static float imbueHitGroundExplosionUpwardModifier = 1.0f;

        [ModOption(category = "Staff Slam", name = "Staff Slam Radius", tooltip = "Radius of the staff slam explosion")]
        [ModOptionFloatValues(1, 5, 0.5f)]
        public static float imbueHitGroundRadius = 2.5f;

        [ModOption(category = "Staff Slam", name = "Staff Slam Explosion Force", tooltip = "Force of Staff Slam explosion")]
        [ModOptionIntValues(5, 100, 5)]
        public static int imbueHitGroundExplosionForce = 25;

        [ModOption(category = "Staff Shoot", name = "Staff Shoot Imbue Cost", tooltip = "Imbue cost for each rock shot")]
        [ModOptionIntValues(1, 20, 1)]
        public static int imbueCrystalUseCost = 2;

        [ModOption(category = "Staff Shoot", name = "Staff Shoot Force", tooltip = "Force added to rocks shot")]
        [ModOptionIntValues(1, 30, 1)]
        public static int imbueCrystalShootForce = 8;


        public string pushEffectId;

        public List<string> rockItemIds = new List<string>();
        public Vector2 rockMassMinMax;
        public string rockSummonEffectId;
        public string punchEffectId;
        public float punchAimRandomness;

        public string spikeEffectId;
        public float spikeRange;
        public float spikeMinAngle;

        public string shatterEffectId;
        public float shatterRange;
        public float shatterRadius;

        private bool canSpawnShield = true;
        private bool canSpawnRockLeft = true;
        private bool canSpawnRockRight = true;
        private bool canSpawnSpikes = true;
        private bool canSpawnShatter = true;
        private bool canSpawnPillars = true;
        private bool canPush = true;

        public string rockPillarPointsId;
        public string rockPillarItemId;
        public string rockPillarCollisionEffectId;
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
            Instance = this;

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


            if (Vector3.Dot(Vector3.down, mana.casterLeft.magicSource.forward) > 0.8f && Vector3.Dot(Vector3.down, mana.casterRight.magicSource.forward) > 0.8f) //Hands are pointing down
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
            if (!skillSummonShield)
                return;

            if (wallCalled)
            {
                wallCalled = false;
            }

            if (canSpawnShield)
            {
                if (handsPointingDown) //If hands is pointing down
                {
                    if (Mathf.Abs(Vector3.Dot(-Player.currentCreature.transform.right, mana.casterLeft.magicSource.up)) < 0.4 && Mathf.Abs(Vector3.Dot(Player.currentCreature.transform.right, mana.casterLeft.magicSource.up)) < 0.4) //If hands are not to the side
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
            if (!skillPush)
                return;

            if (pushCalled)
            {
                pushCalled = false;
            }

            //Push logic
            if (Mathf.Abs(Vector3.Dot(Vector3.up, mana.casterLeft.magicSource.forward)) < 0.3f && Mathf.Abs(Vector3.Dot(Vector3.up, mana.casterRight.magicSource.forward)) < 0.3f) //If hands are not pointing up or down
            {
                if (Vector3.Dot(Vector3.up, mana.casterLeft.magicSource.up) > 0.7f && Vector3.Dot(Vector3.up, mana.casterRight.magicSource.up) > 0.7f) // Fingers are pointing up
                {
                    if (Vector3.Dot(mana.casterLeft.magicSource.forward, leftVel) > pushMinSpeed && Vector3.Dot(mana.casterLeft.magicSource.forward, rightVel) > pushMinSpeed) //Hands are moving forwards
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
            if (!skillSpikes)
                return;

            //Spike logic
            Vector3 vecBetweenForAndDown = Vector3.Slerp(Vector3.up, Player.currentCreature.transform.forward, 0.5f).normalized; //Get 45 degree angle between up and forwards

            if (canSpawnSpikes)
            {
                if (Vector3.Dot(mana.casterLeft.magicSource.forward, vecBetweenForAndDown) > 0.7f && Vector3.Dot(mana.casterRight.magicSource.forward, vecBetweenForAndDown) > 0.7f)
                {
                    if (Vector3.Dot(mana.casterLeft.magicSource.right, -Player.currentCreature.transform.right) > 0.7 && Vector3.Dot(mana.casterRight.magicSource.right, -Player.currentCreature.transform.right) > 0.7) //Hand are pointing to the sides
                    {
                        if (Vector3.Dot(mana.casterLeft.magicSource.forward, leftVel) > spikeMinSpeed && Vector3.Dot(mana.casterLeft.magicSource.forward, rightVel) > spikeMinSpeed) //Hands are moving forwards
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
                        if (Vector3.Dot(Vector3.down, mana.casterLeft.magicSource.forward) > 0.7f) //If hand is pointing down
                        {
                            if (Mathf.Abs(Vector3.Dot(-Player.currentCreature.transform.right, mana.casterLeft.magicSource.up)) < 0.7) //If hand is not to the side
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
                        if (Vector3.Dot(Vector3.down, mana.casterRight.magicSource.forward) > 0.7f) //If hand is pointing down
                        {
                            if (Mathf.Abs(Vector3.Dot(-Player.currentCreature.transform.right, mana.casterRight.magicSource.up)) < 0.7) //If hand is not to the side
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
            if (!skillShatter)
                return;

            if (shatterCalled)
            {
                shatterCalled = false;
            }

            if (canSpawnShatter)
            {
                if (handsPointingDown)
                {
                    if (Vector3.Dot(Player.currentCreature.transform.right, mana.casterLeft.magicSource.right) > 0.8f && Vector3.Dot(Player.currentCreature.transform.right, mana.casterRight.magicSource.right) > 0.8f)
                    {
                        if (Vector3.Dot(mana.casterLeft.magicSource.up, leftVel) > shatterMinSpeed && Vector3.Dot(mana.casterLeft.magicSource.up, rightVel) > shatterMinSpeed) // Moving hands forwards
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
            if (!skillRockPillars)
                return;

            if (canSpawnPillars)
            {
                if (handsPointingDown)
                {
                    if (Vector3.Dot(mana.casterLeft.magicSource.forward, leftVel) > rockPillarMinSpeed && Vector3.Dot(mana.casterLeft.magicSource.forward, rightVel) > rockPillarMinSpeed) // Moving hands forwards down
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
            Vector3 middlePoint = Vector3.Lerp(mana.casterLeft.magicSource.position, mana.casterRight.magicSource.position, 0.5f) + Player.currentCreature.transform.forward * 0.25f;
            RaycastHit hit;
            if (Physics.Raycast(middlePoint, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
            {
                EffectInstance spawnPoints = rockPillarPointsData.Spawn(hit.point, Player.currentCreature.transform.rotation);

                foreach (Transform child in spawnPoints.effects[0].transform)
                {
                    rockPillarItemData.SpawnAsync(delegate (Item rockPillar)
                    {
                        rockPillar.Throw();

                        rockPillar.transform.position = child.position;
                        rockPillar.transform.rotation = child.rotation;

                        rockPillar.physicBody.rigidBody.velocity = -Vector3.up * 2;

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
            Vector3 middlePoint = Vector3.Lerp(mana.casterLeft.magicSource.position, mana.casterRight.magicSource.position, 0.5f) + ((leftVel.normalized + rightVel.normalized) * 0.25f);
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
            yield return new WaitForEndOfFrame();
            rb.AddForce(shatterForce * (forwards.normalized + Vector3.up), ForceMode.Impulse);
        }

        IEnumerator SpikeCoroutine()
        {
            Vector3 middlePoint = Vector3.Lerp(mana.casterLeft.magicSource.position, mana.casterRight.magicSource.position, 0.5f) + ((leftVel.normalized + rightVel.normalized) * 0.25f);
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
                foreach (Creature creature in Creature.allActive)
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
            Vector3 middlePoint = Vector3.Lerp(mana.casterLeft.magicSource.position, mana.casterRight.magicSource.position, 0.5f) + Player.currentCreature.transform.forward * 0.7f;

            RaycastHit hit;
            if (Physics.Raycast(middlePoint, Vector3.down, out hit, Mathf.Infinity, LayerMask.GetMask("Default")))
            {

                shieldItemData.SpawnAsync(delegate (Item shield)
                {
                    shield.Throw();
                    shield.StartCoroutine(ShieldSpawnedCoroutine(shield, hit));
                }, null, null, null,true);
            }

            yield return null;
        }

        private IEnumerator ShieldSpawnedCoroutine(Item shield, RaycastHit hit)
        {
            canSpawnRockLeft = false;
            canSpawnRockRight = false;

            shield.AddCustomData<ShieldCustomData>(new ShieldCustomData(shieldHealth));

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
                        StartCoroutine(ShieldPush(shield, (leftVel.normalized + rightVel.normalized) * pushForceShield));
                        break;
                    }
                }


                yield return new WaitForEndOfFrame();
            }

            yield return new WaitUntil(() => Time.time - startTime > shieldFreezeTime);

            shield.colliderGroups[0].collisionHandler.OnCollisionStartEvent -= Shield_OnDamageReceivedEvent;

            StartCoroutine(DespawnShield(shield));
            
        }

        private IEnumerator ShieldPush(Item shield, Vector3 force)
        {
            shield.physicBody.rigidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            shield.physicBody.rigidBody.isKinematic = false;
            shield.physicBody.rigidBody.useGravity = true;

            EffectInstance effectInstance = pushEffectData.Spawn(shield.transform.position, Quaternion.identity);
            effectInstance.Play();

            shield.physicBody.rigidBody.velocity = force;

            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            while (shield.physicBody.rigidBody.velocity.magnitude > 8f)
            {
                RaycastHit hit;
                if (Physics.Raycast(shield.transform.position + Vector3.up * 0.25f, Vector3.down, out hit, 0.8f, LayerMask.GetMask("Default")))
                {
                    shield.transform.position = new Vector3(shield.transform.position.x, hit.point.y + 0.1f, shield.transform.position.z);
                } else
                {
                    shield.physicBody.rigidBody.constraints = RigidbodyConstraints.None;
                    shield.physicBody.rigidBody.AddForceAtPosition(-shield.physicBody.rigidBody.velocity, shield.transform.position, ForceMode.Impulse);
                    yield break;
                }

                yield return new WaitForEndOfFrame();
            }

            shield.physicBody.rigidBody.constraints = RigidbodyConstraints.None;
            shield.physicBody.rigidBody.isKinematic = true;
            shield.physicBody.rigidBody.useGravity = false;
        }

        private void Shield_OnDamageReceivedEvent(CollisionInstance collisionStruct)
        {
            if (collisionStruct.impactVelocity.magnitude > 2)
            {
                Item shield = null;
                Vector3 forceDir = collisionStruct.impactVelocity.normalized;

                if (!collisionStruct.sourceColliderGroup.gameObject.GetComponentInParent<Player>() && !collisionStruct.targetColliderGroup.gameObject.GetComponentInParent<Player>())
                    return;

                if (collisionStruct.targetCollider.GetComponentInParent<Item>())
                {
                    if (collisionStruct.targetCollider.GetComponentInParent<Item>().itemId == shieldItemId)
                    {
                        shield = collisionStruct.targetCollider.GetComponentInParent<Item>();
                     
                    }
                } 
                else if (collisionStruct.sourceCollider.GetComponentInParent<Item>())
                {
                    if (collisionStruct.sourceCollider.GetComponentInParent<Item>().itemId == shieldItemId)
                    {
                        shield = collisionStruct.sourceCollider.GetComponentInParent<Item>();
                        forceDir *= -1;
                    }
                }

                if (shield == null)
                    return;
                
                ShieldCustomData HP;
                shield.TryGetCustomData<ShieldCustomData>(out HP);

                int OldHP = HP.hp;

                if (OldHP - 1 > 0)
                {
                    StartCoroutine(ShieldPush(shield, forceDir * punchForceShield * collisionStruct.impactVelocity.magnitude));
                }

                if (Time.time - HP.lastHit > 0.1f)
                {
                    if (OldHP - 1 < 1)
                    {
                        StartCoroutine(DespawnShield(shield));
                    }
                    else
                    {
                        HP.hp = OldHP - 1;
                    }
                    HP.lastHit = Time.time;
                }
            }


        }

        class ShieldCustomData : ContentCustomData
        {
            public int hp;
            public float lastHit;

            public ShieldCustomData(int hp)
            {
                this.hp = hp;
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
            rock.Throw();

            rock.physicBody.rigidBody.mass = UnityEngine.Random.Range(rockMassMinMax.x, rockMassMinMax.y);

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

            rock.physicBody.rigidBody.useGravity = false;
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
 

            rock.physicBody.rigidBody.velocity = Vector3.zero;
            float startTime = Time.time;

            rock.physicBody.rigidBody.angularVelocity = UnityEngine.Random.insideUnitSphere * 2;

            rock.colliderGroups[0].collisionHandler.OnCollisionStartEvent += Rock_OnDamageReceivedEvent;

            while (Time.time - startTime < rockFreezeTime && !rock.physicBody.rigidBody.useGravity)
            {
                if (pushCalled)
                {
                    rock.physicBody.rigidBody.useGravity = true;
                    rock.physicBody.rigidBody.drag = 0;

                    EffectInstance effectInstance = pushEffectData.Spawn(rock.transform.position, Quaternion.identity);
                    effectInstance.Play();

                    Vector3 forceVec = (leftVel + rightVel).normalized * pushForceRock;
                    if (pushAimAssist && skillAimAssist)
                    {
                        Vector3 aimVec = AimAssist(rock.transform.position, forceVec.normalized, punchAimPrecision, punchAimRandomness, pushForceRock);
                        if (aimVec != Vector3.zero)
                            forceVec = aimVec;
                    }

                    rock.physicBody.rigidBody.velocity = forceVec;
                    break;
                }
                yield return new WaitForEndOfFrame();
            }

            rock.physicBody.rigidBody.useGravity = true;

            rock.colliderGroups[0].collisionHandler.OnCollisionStartEvent -= Rock_OnDamageReceivedEvent;

            yield return new WaitForSeconds(10f);

            rock.colliderGroups.ForEach(cg => cg.colliders.ForEach(c => c.enabled = false));
            rock.Despawn(0.5f);

            yield return null;
        }

        private void Rock_OnDamageReceivedEvent(CollisionInstance collisionStruct)
        {

            if (collisionStruct.impactVelocity.magnitude > 1)
            {
                Item ownItem = null;
                Vector3 forceDir = collisionStruct.impactVelocity.normalized;

                if (collisionStruct.sourceColliderGroup?.gameObject.GetComponentInParent<Player>() || collisionStruct.targetColliderGroup?.gameObject.GetComponentInParent<Player>())
                {

                }
                else
                {
                    return;
                }

                if (collisionStruct.targetCollider?.GetComponentInParent<Item>())
                {
                    if (rockItemIds.Any(r => collisionStruct.targetCollider.GetComponentInParent<Item>().data.id == r))
                    {
                        ownItem = collisionStruct.targetCollider.GetComponentInParent<Item>();
                    }
                }
                else if (collisionStruct.sourceCollider?.GetComponentInParent<Item>())
                {
                    if (rockItemIds.Any(r => collisionStruct.sourceCollider.GetComponentInParent<Item>().data.id == r))
                    {
                        ownItem = collisionStruct.sourceCollider.GetComponentInParent<Item>();
                        forceDir *= -1;
                    }
                }
                if (ownItem == null)
                    return;

                ownItem.physicBody.rigidBody.useGravity = true;
                ownItem.physicBody.rigidBody.drag = 0;



                EffectInstance effectInstance = Catalog.GetData<EffectData>(punchEffectId, true).Spawn(ownItem.transform.position, Quaternion.identity);
                effectInstance.Play();

                Vector3 forceVec = forceDir * collisionStruct.impactVelocity.magnitude * punchForceRock;
                if (skillAimAssist) 
                {
                    Vector3 aimVec = AimAssist(ownItem.transform.position, forceDir, punchAimPrecision, punchAimRandomness, collisionStruct.impactVelocity.magnitude * punchForceRock);
                    if (aimVec != Vector3.zero)
                        forceVec = aimVec;
                }

                ownItem.physicBody.rigidBody.velocity = forceVec;
            }
        }

        public static Vector3 CalculateLaunchDirection(Vector3 targetPosition, Vector3 ballPosition, float initialSpeed)
        {
            // Constants
            float gravity = Physics.gravity.y;

            // Calculate distance components
            Vector3 distance = targetPosition - ballPosition;
            float horizontalDistance = new Vector3(distance.x, 0, distance.z).magnitude;
            float verticalDistance = distance.y;

            // Calculate initial velocity components
            float speedSquared = initialSpeed * initialSpeed;
            float speedQuad = speedSquared * speedSquared;
            float g = Mathf.Abs(gravity);

            // Calculate the angle required to hit the target using the quadratic formula
            // θ = 0.5 * arcsin((g * x^2) / (v^2 * (v^2 - 2 * g * y)))
            float term1 = speedSquared * speedSquared - g * (g * horizontalDistance * horizontalDistance + 2 * verticalDistance * speedSquared);

            if (term1 < 0)
            {
                // No solution exists if term1 is negative
                return Vector3.zero;
            }

            float angle1 = Mathf.Atan((speedSquared + Mathf.Sqrt(term1)) / (g * horizontalDistance));
            float angle2 = Mathf.Atan((speedSquared - Mathf.Sqrt(term1)) / (g * horizontalDistance));

            // Select the smaller angle that hits the target
            float angle = angle1 < angle2 ? angle1 : angle2;

            // Calculate the initial velocity components
            float vx = initialSpeed * Mathf.Cos(angle);
            float vy = initialSpeed * Mathf.Sin(angle);

            // Create the velocity vector
            Vector3 launchVelocity = new Vector3(distance.x, 0, distance.z).normalized * vx;
            launchVelocity.y = vy;

            return launchVelocity;
        }

        private Vector3 AimAssist(Vector3 ownPosition, Vector3 ownDirection, float aimPrecision, float randomness, float startVelocity)
        {
            Transform toHit = null;
            float closest = float.MaxValue;

            List<Transform> transformsToCheck = new List<Transform>();

            
            //Add creatures to the list of targets
            foreach (Creature creature in Creature.allActive)
            {
                if (creature != Player.currentCreature && !creature.isKilled)
                {
                    transformsToCheck.Add(creature.ragdoll.GetPart(RagdollPart.Type.Head).transform);
                }
            }

            //Add golem crystals to list of targets
            if (Golem.local != null)
            {
                foreach (GolemCrystal crystal in Golem.local.crystals)
                {
                    transformsToCheck.Add(crystal.transform);
                }
            }

            foreach (Transform transform in transformsToCheck)
            {
                Vector3 creaturePos = transform.position;
                Vector3 toCreature = (creaturePos - ownPosition).normalized;

                //Must be in front
                if (Vector3.Dot(ownDirection, toCreature) > 0)
                {
                    float perpendicularDistance = Vector3.Cross(ownDirection, toCreature).magnitude;

                    if (perpendicularDistance > aimPrecision)
                        continue;

                    if (perpendicularDistance > closest)
                        continue;

                    closest = perpendicularDistance;
                    toHit = transform;
                }
            }

            if (toHit != null)
            {
                return CalculateLaunchDirection(toHit.position, ownPosition, startVelocity);
            } else
            {
                return Vector3.zero;
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
