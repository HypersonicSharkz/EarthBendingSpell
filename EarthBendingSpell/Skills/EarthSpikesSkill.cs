using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThunderRoad;
using ThunderRoad.Skill.SpellPower;

namespace EarthBendingSpell.Skills
{
    internal class EarthSpikesSkill : SkillData
    {
        public override void OnSkillLoaded(SkillData skillData, Creature creature)
        {
            base.OnSkillLoaded(skillData, creature);
            EarthBendingController.skillSpikes = true;
        }

        public override void OnSkillUnloaded(SkillData skillData, Creature creature)
        {
            base.OnSkillUnloaded(skillData, creature);
            EarthBendingController.skillSpikes = false;
        }
    }
}
