﻿using ETModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
[SkillActiveCondition(SkillActiveConditionType.CheckHPMP)]
public class SkillActiveConditionHandler_CheckHPMP : BaseSkillData.IActiveConditionHandler
{


    public bool MeetCondition(BaseSkillData.IActiveConditionData data, Unit source)
    {
        SkillActiveCondition_CheckHPMP cost = data as SkillActiveCondition_CheckHPMP;

        NumericComponent numericComponent = source.GetComponent<NumericComponent>();
        if(numericComponent.GetAsFloat( NumericType.HP)> cost.costHp 
            && numericComponent.GetAsFloat(NumericType.MP) > cost.costMp
            && numericComponent.GetAsFloat(NumericType.HP_RemainPct) > cost.costHpInPct
            && numericComponent.GetAsFloat(NumericType.MP_RemainPct) > cost.costMpInPct)
        {
            return true;
        }
        return false;
    }


    public void OnRemove(BaseSkillData.IActiveConditionData data, Unit source)
    {
        
    }
}
