﻿using BulletUnity;
using ETModel;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace ETHotfix
{

    public static class UnitStateComponentSystem
    {
        public static void FixedUpdate(this UnitStateComponent unitStateComponent)
        {

            if (!unitStateComponent.haveInited) return;
            if (unitStateComponent.unitStatesDic.Count == 0) return;
            UnitStateMgrComponent mgr = Game.Scene.GetComponent<UnitStateMgrComponent>();
            Log.Info(string.Format("frame {0} : 玩家位置信息 {1}", mgr.currFrame, unitStateComponent.unit.Position));
            //每间隔3帧发一次数据
            if (mgr.currFrame - unitStateComponent.preSendMsgFrame < UnitStateMgrComponent.sendMsgDelta)
            {
                return;
            }
            unitStateComponent.preSendMsgFrame = mgr.currFrame;

            if (!unitStateComponent.collectInput)
            {
                return;
            }

            //TODO : 这里有大量GC,需要处理
            if (unitStateComponent.currGetInputFrame - unitStateComponent.preClearInputFrame >= UnitStateComponent.maxFrameCount_SaveStateDelta)
            {
                for (int i = unitStateComponent.preClearInputFrame; i < unitStateComponent.currGetInputFrame - UnitStateComponent.maxFrameCount_SaveStateDelta; i++)
                {
                    if (unitStateComponent.unitStatesDic.ContainsKey(i))
                    {
                        unitStateComponent.unitStatesDic.Remove(i);
                    }
                }
            }

            //每次发送都发最新的的结果
            var state = unitStateComponent.unitStatesDic[unitStateComponent.currGetInputFrame];

            foreach (var v in state.commandResults)
            {
                CommandResultInfo_Move commandResultInfo_Move = new CommandResultInfo_Move();
                commandResultInfo_Move.Frame = state.frame;

                switch (v.Value)
                {
                    case CommandResult_Move result_Move:
                        commandResultInfo_Move.Pos = new Vector3Info();
                        Property_Position postionPro = unitStateComponent.unitProperty[typeof(Property_Position)] as Property_Position;
                        commandResultInfo_Move.Pos.X = postionPro.Get().x;
                        commandResultInfo_Move.Pos.Y = postionPro.Get().y;
                        commandResultInfo_Move.Pos.Z = postionPro.Get().z;
                        commandResultInfo_Move.Id = unitStateComponent.unit.Id;
                        Log.Info("广播角色单位信息");
                        MessageHelper.Broadcast(commandResultInfo_Move);
                        continue;
                }
            }
            unitStateComponent.collectInput = false;


        }

        public static void Add(UnitStateDelta a, UnitStateDelta b)
        {

            foreach (var v in b.commandResults)
            {
                if (a.commandResults.ContainsKey(v.Key))
                {
                    switch (a.commandResults[v.Key])
                    {
                        case CommandResult_Move result_Move:
                            CommandResult_Move value = v.Value as CommandResult_Move;
                            result_Move.postion = value.postion;
                            continue;
                    }
                }
                else
                {
                    a.commandResults[v.Key] = v.Value;
                }
            }
        }


        //玩家刚开始的时候初始化所有的属性
        public static void Init(this UnitStateComponent unitStateComponent, Dictionary<Type, IProperty> unitProperty)
        {
            unitStateComponent.unitProperty = unitProperty;
            unitStateComponent.haveInited = true;
            unitStateComponent.preSendMsgFrame = -3;
        }

        public static void GetInput(this UnitStateComponent unitStateComponent, int frame, ICommandInput commandInput)
        {
            try
            {
                unitStateComponent.collectInput = true;
                if (unitStateComponent.currGetInputFrame < frame)
                    unitStateComponent.currGetInputFrame = frame;
                UnitStateDelta unitStateDelta = new UnitStateDelta();
                unitStateDelta.frame = frame;
                var result = unitStateComponent.simulaterComponent.commandSimulaters[commandInput.GetType()].Simulate(commandInput, unitStateComponent.unit);
                switch (result)
                {
                    case CommandResult_Move result_Move:
                        unitStateComponent.unit.GetComponent<CharacterCtrComponent>().MoveTo(result_Move.postion);
                        break;
                }
                unitStateDelta.commandResults.Add(result.GetType(), result);
                unitStateComponent.unitStatesDic[unitStateComponent.currGetInputFrame] = unitStateDelta;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());

            }
        }
    }
}