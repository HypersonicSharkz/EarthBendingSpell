using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;

namespace EarthBendingSpell.Skills
{
    internal class EarthPushSkill : SkillData
    {
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);
            EarthBendingController.skillPush = true;
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);
            EarthBendingController.skillPush = false;
        }
    }
}
