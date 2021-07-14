﻿using Microsoft.Xna.Framework;
using SummonHeart.Effects.Animations.Aura;
using SummonHeart.Extensions;
using SummonHeart.Models;
using SummonHeart.ui;
using SummonHeart.Utilities;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using static SummonHeart.SummonHeartMod;

namespace SummonHeart
{
    public class SummonHeartPlayer : ModPlayer
	{	
		public bool SummonHeart = false;
		public int PlayerClass = 0;
		public bool Berserked = false;
		public float AttackSpeed;
		public float tungstenPrevSizeSave;
		public bool FishSoul = false;
		public bool BattleCry = false;		
		public bool llPet = false;
		public bool chargeAttack = false;
		public bool showRadius = false;
		public int BBP = 0;
		public int SummonCrit = 0;
		public int exp = 0;
		public float bodyDef = 0;
		public int killResourceCurrent = 0;
		public int killResourceMax;
		public int killResourceMax2;
		public int killResourceCost;
		public int killResourceCostCount;
		public int killResourceMulti;

		public int eyeBloodGas = 0;
		public int handBloodGas = 0;
		public int bodyBloodGas = 0;
		public int footBloodGas = 0;

		public int eyeMax = 0;
		public int handMax = 0;
		public int bodyMax = 0;
		public int footMax = 0;

		public int bloodGasMax = 10000;

		public int swordBlood = 1;
		public int shortSwordBlood = 1;
		public int swordBloodMax = 100;

		public bool practiceEye = false;
		public bool practiceHand = false;
		public bool practiceBody = false;
		public bool practiceFoot = false;
		public bool soulSplit = false;

		public int HealCount = 0;
		private int healCD = 0;
		private int bodyHealCD = 0;
		private int killHealCD = 0;

		public List<bool> boughtbuffList;

		// animation helper fields
		public AuraAnimationInfo currentAura;
		public int lightningFrameTimer = 500000;
		public int auraFrameTimer = 0;
		public int auraCurrentFrame = 0;

		public SummonHeartPlayer()
		{
			var size = SummonHeartMod.getBuffLength();
			boughtbuffList = new List<bool>();
			for(int i = 1; i <= size; i++)
            {
				boughtbuffList.Add(false);
            }
		}

		public override void ResetEffects()
		{
			SummonHeart = false;
			AttackSpeed = 1f;
			FishSoul = false;
			llPet = false;
			healCD++;
			if (healCD == 60)
			{
				healCD = 0;
				HealCount = SummonCrit;
			}
			bodyHealCD++;
			if (bodyHealCD == 15)
			{
				bodyHealCD = 0;
			}
			killHealCD++;
			if (killHealCD == 12)
			{
				killHealCD = 0;
			}
			killResourceMax2 = killResourceMax;

			eyeMax = SummonHeartConfig.Instance.eyeMax;
			handMax = SummonHeartConfig.Instance.handMax;
			bodyMax = SummonHeartConfig.Instance.bodyMax;
			footMax = SummonHeartConfig.Instance.footMax;
		}

		public override void PreUpdate()
		{
			if (player.HasItemInAcc(mod.ItemType("MysteriousCrystal")) != -1 && base.player.respawnTimer > 300)
			{
				player.respawnTimer = 300;
			}
		}

		public override void PostUpdate()
		{
			currentAura = this.GetAuraEffectOnPlayer();
			IncrementAuraFrameTimers(currentAura);
		}

		public override void PostUpdateMiscEffects()
        {
			player.statDefense += (int)bodyDef;
			if(PlayerClass == 1)
            {
				//战士
				EffectMelee();
            }else if(PlayerClass == 2)
			{
				//刺客
				EffectKill();
			}
		}
		

		private void EffectKill()
        {
			killResourceMax = 100 + shortSwordBlood ;
			killResourceMulti = 3;
			killResourceCost = 25;
            if (boughtbuffList[1] && handBloodGas >= bodyBloodGas)
            {
				//一刀流
				killResourceCost += handBloodGas / 5333;
				killResourceMulti += (handBloodGas / 4000 + 2);
			}
            if (boughtbuffList[2] && bodyBloodGas > handBloodGas)
            {
				//神通流
				killResourceMax += bodyBloodGas / 40;
				killResourceCost -= (bodyBloodGas / 40000 + 5);
			}
			killResourceCostCount = killResourceMax * killResourceCost / 100;
			if (killResourceCurrent < killResourceMax2 && killHealCD == 0)
			{
				int heal = 1;
				if (boughtbuffList[2] && bodyBloodGas > handBloodGas)
				{
					//神通流
					heal += (bodyBloodGas / 800 + 15) / 4;
				}
				killResourceCurrent += heal;
				if (killResourceCurrent > killResourceMax2)
					killResourceCurrent = killResourceMax2;
			}
			
			//魔神之腿
			if (boughtbuffList[3])
			{
				player.noFallDmg = true;
				player.moveSpeed += (footBloodGas / 10000 + 10) * 0.01f;
				player.wingTimeMax += (footBloodGas / 2222 + 10) * 60;
				player.jumpSpeedBoost += (footBloodGas / 1333 + 50) * 0.01f;
			}
		}

        private void EffectMelee()
        {
			//魔神之眼
			if (boughtbuffList[0])
            {
				player.meleeCrit += eyeBloodGas / 2222 + 10;
			}
			/*player.magicCrit += eyeBloodGas / 1000;
			player.rangedCrit += eyeBloodGas / 1000;
			player.thrownCrit += eyeBloodGas / 1000;*/

			//魔神之手
			if (boughtbuffList[1])
			{
				player.meleeDamage += handBloodGas / 200 * 0.01f;
				AttackSpeed += (handBloodGas / 1111 + 20) * 0.01f;
			}

			//魔神之躯
			if (boughtbuffList[2])
			{
				player.noKnockback = true;
				player.statLifeMax2 += bodyBloodGas / 200;
				int heal = (int)(player.statLifeMax2 * (0.01 + bodyBloodGas / 20000 * 0.01f)) / 4;
				if (player.statLife < player.statLifeMax2 && bodyHealCD == 1)
				{
					if (heal < 1)
						heal = 1;
					player.statLife += heal;
					player.HealEffect(heal);
				}
			}

			//魔神之腿
			if (boughtbuffList[3])
			{
				player.noFallDmg = true;
				player.moveSpeed += (footBloodGas / 10000 + 10) * 0.01f;
				player.wingTimeMax += (footBloodGas / 2222 + 10) * 60;
				player.jumpSpeedBoost += (footBloodGas / 1333 + 50) * 0.01f;
				/*if (footBloodGas >= 150000)
				{
					player.wingTime = footBloodGas / 1000 * 60;
				}*/
			}
		}

        public struct SoundData
		{
			public int Type;
			public int x;
			public int y;
			public int Style;
			public float volumeScale;
			public float pitchOffset;
			public SoundData(int Type)
			{ this.Type = Type; x = -1; y = -1; Style = 1; volumeScale = 1f; pitchOffset = 0f; }
		}
		public static void ItemFlashFX(Player player, int dustType = 45, SoundData sDat = default(SoundData))
		{
			if (sDat.Type == 0) { sDat = new SoundData(25); }
			if (player.whoAmI == Main.myPlayer)
			{ 
				Main.PlaySound(sDat.Type, sDat.x, sDat.y, sDat.Style, sDat.volumeScale, sDat.pitchOffset); 
			}
			for (int i = 0; i < 5; i++)
			{
				int d = Dust.NewDust(
					player.position, player.width, player.height, dustType, 0f, 0f, 255,
					default(Color), (float)Main.rand.Next(20, 26) * 0.1f);
				Main.dust[d].noLight = true;
				Main.dust[d].noGravity = true;
				Main.dust[d].velocity *= 0.5f;
			}
		}
		

		public bool GetPratice(int currentBuffIndex)
		{
			bool praticeBool = false;
			if (currentBuffIndex == 0)
			{
				praticeBool = practiceEye;
			}
			else if (currentBuffIndex == 1)
			{
				praticeBool = practiceHand;
			}
			else if (currentBuffIndex == 2)
			{
				praticeBool = practiceBody;
			}
			else if (currentBuffIndex == 3)
			{
				praticeBool = practiceFoot;
			}
			else if (currentBuffIndex == 4)
			{
				praticeBool = soulSplit;
			}
			return praticeBool;
		}

		public void SetPratice(int index, bool flag)
		{
			if (index == 0)
			{
				practiceEye = flag;
			}
			else if (index == 1)
			{
				practiceHand = flag;
			}
			else if (index == 2)
			{
				practiceBody = flag;
			}
			else if (index == 3)
			{
				practiceFoot = flag;
			}
			else if (index == 4)
			{
				soulSplit = flag;
			}
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
				SendClientChanges(this);
			}
		}

		public bool CheckSoul(int count)
		{
			return BBP >= count;
		}

		public void BuySoul(int count)
		{
			BBP -= count;
			dealLevel();
		}


		private void dealLevel()
		{
			int lvExp = 1;
			int exp = lvExp;
			int level = 0;
			while (exp <= BBP)
			{
				exp += lvExp;
				level++;
				lvExp += 1;
			}
			int needExp = exp - BBP;
			exp = needExp;
			SummonCrit = level;
			if (!Main.hardMode && SummonCrit > 299)
				SummonCrit = 299;
			if (Main.hardMode && SummonCrit > 499)
				SummonCrit = 500;
		}

		

		public void CauseDirectDamage(NPC npc, int originalDamage, bool crit)
		{
			/*Player player = Main.player[Main.myPlayer];
            SummonHeartPlayer modPlayer = player.GetModPlayer<SummonHeartPlayer>();

			int num = 0;
			if (crit)
				originalDamage *= 2;

            if (modPlayer.SummonHeart)
                num = originalDamage * SummonCrit / 5000 + SummonCrit / 5 + SummonHeartWorld.WorldLevel * 5;

			if (num >= 1)
			{
				npc.LoseLife(num, new Color?(new Color(240, 20, 20, 255)));
			}*/
		}
		public override void SetupStartInventory(IList<Item> items, bool mediumcoreDeath)
        {
			Item item = new Item();
			item.SetDefaults(ModLoader.GetMod("SummonHeart").ItemType("GuideNote"));
			item.stack = 1;
			items.Add(item);
			item = new Item();
			item.SetDefaults(ModLoader.GetMod("SummonHeart").ItemType("Level0"));
			item.stack = 1;
			items.Add(item);
			item = new Item();
			item.SetDefaults(ModLoader.GetMod("SummonHeart").ItemType("DemonScroll"));
			item.stack = 1;
			items.Add(item);
			item = new Item();
			item.SetDefaults(ItemID.LifeCrystal);
			item.stack = 1;
			items.Add(item);
			item = new Item();
			item.SetDefaults(ModLoader.GetMod("SummonHeart").ItemType("LlPet"));
			item.stack = 1;
			items.Add(item);
			
			item = new Item();
			item.SetDefaults(ModLoader.GetMod("SummonHeart").ItemType("MysteriousCrystal"));
			item.stack = 1;
			items.Add(item);
			item = new Item();
			item.SetDefaults(ItemID.WaterBucket);
			item.stack = 2;
			items.Add(item);
			if (ModLoader.GetMod("Luiafk") != null)
			{
				item = new Item();
				item.SetDefaults(ModLoader.GetMod("Luiafk").ItemType("ToolTime"));
				item.stack = 1;
				items.Add(item);
			}
			if (ModLoader.GetMod("MagicStorage") != null)
			{
				item = new Item();
				item.SetDefaults(ModLoader.GetMod("MagicStorage").ItemType("StorageHeart"));
				item.stack = 1;
				items.Add(item);
				item = new Item();
				item.SetDefaults(ModLoader.GetMod("MagicStorage").ItemType("CraftingAccess"));
				item.stack = 1;
				items.Add(item);
				item = new Item();
				item.SetDefaults(ModLoader.GetMod("MagicStorage").ItemType("StorageUnit"));
				item.stack = 16;
				items.Add(item);
			}
		}

        public override void clientClone(ModPlayer clientClone)
		{
			SummonHeartPlayer clone = clientClone as SummonHeartPlayer;
			clone.BBP = BBP;
			clone.SummonCrit = SummonCrit;
			clone.exp = exp;
			clone.PlayerClass = PlayerClass;
			clone.bodyDef = bodyDef;
			clone.eyeBloodGas = eyeBloodGas;
			clone.handBloodGas = handBloodGas;
			clone.bodyBloodGas = bodyBloodGas;
			clone.footBloodGas = footBloodGas;
			clone.bloodGasMax = bloodGasMax;
			clone.swordBlood = swordBlood;
			clone.shortSwordBlood = shortSwordBlood;
			clone.swordBloodMax = swordBloodMax;
			clone.practiceEye = practiceEye;
			clone.practiceHand = practiceHand;
			clone.practiceBody = practiceBody;
			clone.practiceFoot = practiceFoot;
			clone.soulSplit = soulSplit;
			//clone.boughtbuffList = boughtbuffList;
		}

		public override void SyncPlayer(int toWho, int fromWho, bool newPlayer)
		{
			ModPacket packet = mod.GetPacket();
			packet.Write((byte)0);
			packet.Write((byte)player.whoAmI);
			packet.Write(BBP);
			packet.Write(SummonCrit);
			packet.Write(exp);
			packet.Write(PlayerClass);
			packet.Write(bodyDef);
			packet.Write(eyeBloodGas);
			packet.Write(handBloodGas);
			packet.Write(bodyBloodGas);
			packet.Write(footBloodGas);
			packet.Write(bloodGasMax);
			packet.Write(swordBlood);
			packet.Write(shortSwordBlood);
			packet.Write(swordBloodMax);
			packet.Write(practiceEye);
			packet.Write(practiceHand);
			packet.Write(practiceBody);
			packet.Write(practiceFoot);
			packet.Write(soulSplit);
			/*for (int i = 0; i < boughtbuffList.Count; i++)
			{
				packet.Write(boughtbuffList[i]);
			}*/
			packet.Send(toWho, fromWho);
		}

		public override void SendClientChanges(ModPlayer clientPlayer)
		{
			SummonHeartPlayer clone = clientPlayer as SummonHeartPlayer;
			bool send = false;

			if (clone.BBP != BBP || clone.SummonCrit != SummonCrit || clone.exp != exp
					|| clone.bodyDef != bodyDef || clone.PlayerClass != PlayerClass
					|| clone.eyeBloodGas != eyeBloodGas || clone.handBloodGas != handBloodGas
					|| clone.bodyBloodGas != bodyBloodGas || clone.footBloodGas != footBloodGas
					|| clone.bloodGasMax != bloodGasMax || clone.swordBlood != swordBlood
					|| clone.shortSwordBlood != shortSwordBlood || clone.swordBloodMax != swordBloodMax
					|| clone.practiceEye != practiceEye || clone.practiceHand != practiceHand
					|| clone.practiceBody != practiceBody || clone.practiceFoot != practiceFoot
					|| clone.soulSplit != soulSplit)
			{
				send = true;
			}
			/*for (int i = 0; i < boughtbuffList.Count; i++)
			{ 
				if (clone.boughtbuffList[i] != boughtbuffList[i])
				{
					send = true;
				}
			}*/

			if (send)
			{
				var packet = mod.GetPacket();
				packet.Write((byte)0);
				packet.Write((byte)player.whoAmI);
				packet.Write(BBP);
				packet.Write(SummonCrit);
				packet.Write(exp);
				packet.Write(PlayerClass);
				packet.Write(bodyDef);
				packet.Write(eyeBloodGas);
				packet.Write(handBloodGas);
				packet.Write(bodyBloodGas);
				packet.Write(footBloodGas);
				packet.Write(bloodGasMax);
				packet.Write(swordBlood);
				packet.Write(shortSwordBlood);
				packet.Write(swordBloodMax);
				packet.Write(practiceEye);
				packet.Write(practiceHand);
				packet.Write(practiceBody);
				packet.Write(practiceFoot);
				packet.Write(soulSplit);
				/*for (int i = 0; i < boughtbuffList.Count; i++)
				{
					packet.Write(boughtbuffList[i]);
				}*/
				packet.Send();
			}
		}

		public override TagCompound Save()
		{
			var tagComp = new TagCompound();
			tagComp.Add("BBP", BBP);
			tagComp.Add("SummonCrit", SummonCrit);
			tagComp.Add("exp", exp);
			tagComp.Add("PlayerClass", PlayerClass);
			tagComp.Add("bodyDef", bodyDef);
			tagComp.Add("eyeBloodGas", eyeBloodGas);
			tagComp.Add("handBloodGas", handBloodGas);
			tagComp.Add("bodyBloodGas", bodyBloodGas);
			tagComp.Add("footBloodGas", footBloodGas);
			tagComp.Add("bloodGasMax", bloodGasMax);
			tagComp.Add("swordBlood", swordBlood);
			tagComp.Add("shortSwordBlood", shortSwordBlood);
			tagComp.Add("swordBloodMax", swordBloodMax);
			tagComp.Add("killResourceCurrent", killResourceCurrent);
			tagComp.Add("practiceEye", practiceEye);
			tagComp.Add("practiceHand", practiceHand);
			tagComp.Add("practiceBody", practiceBody);
			tagComp.Add("practiceFoot", practiceFoot);
			tagComp.Add("soulSplit", soulSplit);
			tagComp.Add("boughtbuffList", boughtbuffList);
			return tagComp;
		}
		
		public override void Load(TagCompound tag)
		{
			BBP = tag.GetInt("BBP");
			SummonCrit = tag.GetInt("SummonCrit");
			exp = tag.GetInt("exp");
			PlayerClass = tag.GetInt("PlayerClass");
			bodyDef = tag.GetFloat("bodyDef");
			eyeBloodGas = tag.GetInt("eyeBloodGas");
			handBloodGas = tag.GetInt("handBloodGas");
			bodyBloodGas = tag.GetInt("bodyBloodGas");
			footBloodGas = tag.GetInt("footBloodGas");
			bloodGasMax = tag.GetInt("bloodGasMax");
			swordBlood = tag.GetInt("swordBlood");
			shortSwordBlood = tag.GetInt("shortSwordBlood");
			swordBloodMax = tag.GetInt("swordBloodMax");
			killResourceCurrent = tag.GetInt("killResourceCurrent");
			practiceEye = tag.GetBool("practiceEye");
			practiceHand = tag.GetBool("practiceHand");
			practiceBody = tag.GetBool("practiceBody");
			practiceFoot = tag.GetBool("practiceFoot");
			soulSplit = tag.GetBool("soulSplit");
			boughtbuffList = tag.Get<List<bool>>("boughtbuffList");

			while (boughtbuffList.Count < modBuffValues.Count)
			{
				boughtbuffList.Add(false);
			}
		}


		public override void ProcessTriggers(TriggersSet triggersSet)
		{
			if (SummonHeartMod.AutoAttackKey.JustPressed)
			{
				Berserked = !Berserked;
				if(Berserked)
					Main.NewText($"自动使用武器: 开", Color.SkyBlue);
				else
					Main.NewText($"自动使用武器: 关", Color.SkyBlue);
			}

			if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
			{
				PanelMelee.visible = false;
			}

			if (SummonHeartMod.ShowUI.JustPressed)
			{
				if(PlayerClass == 0)
                {
					Main.NewText($"你暂未获得任何传承，请先使用职业传承书选择你的道", Color.Red);
				}
				else if(PlayerClass == 1)
                {
					PanelMelee.visible = !PanelMelee.visible;
                }else if(PlayerClass == 2)
                {
					PanelKill.visible = !PanelKill.visible;
				}
			}
			if (SummonHeartMod.KillSkillKey.JustPressed)
			{
				if(PlayerClass == 2)
                {
                    if (killResourceCurrent < killResourceCostCount)
                    {
						showRadius = false;
						Main.NewText($"杀意值不足{killResourceCostCount}，无法开启刺杀技能", Color.Red);
						return;
					}
					showRadius = !showRadius;
                    if (showRadius)
                    {
						Main.NewText($"已开启刺杀技能", Color.White);
					}
                    else
                    {
						Main.NewText($"已关闭刺杀技能", Color.White);
					}
                }
                else
                {
					Main.NewText($"只有刺客才能使用刺杀技能", Color.Red);
				}
			}
		}

        public override void OnHitNPC(Item item, NPC target, int damage, float knockback, bool crit)
        {
			if (target.type == NPCID.TargetDummy || target.friendly)
				return;
			
			SummonHeartPlayer modPlayer = player.GetModPlayer<SummonHeartPlayer>();
			/*if (modPlayer.SummonHeart)
			{
				int heal = damage * modPlayer.SummonCrit / 5000;

				if (heal > modPlayer.SummonCrit / 25)
				{
					heal = modPlayer.SummonCrit / 25;
				}
				if (heal > HealCount)
                {
					heal = HealCount;
                }
				if (heal > 0 && HealCount > 0)
                {
					HealCount -= heal;
					player.statLife += heal;
					player.HealEffect(heal);
                }
			}*/
		}

        public override void OnHitNPCWithProj(Projectile proj, NPC target, int damage, float knockback, bool crit)
        {
			if (target.type == NPCID.TargetDummy || target.friendly)
				return;
			SummonHeartPlayer modPlayer = player.GetModPlayer<SummonHeartPlayer>();
			/*if (modPlayer.SummonHeart)
			{
				int heal = damage * modPlayer.SummonCrit / 5000;

				if (heal > modPlayer.SummonCrit / 25)
				{
					heal = modPlayer.SummonCrit / 25;
				}
				if (heal > HealCount)
				{
					heal = HealCount;
				}
				if (heal > 0 && HealCount > 0)
				{
					HealCount -= heal;
					player.statLife += heal;
					player.HealEffect(heal);
				}
			}*/
        }

        public override bool PreItemCheck()
        {
			if (Berserked)
			{
				player.controlUseItem = true;
				player.releaseUseItem = true;
				player.HeldItem.autoReuse = true;
			}
			return true;
		}
		//允许您修改 NPC 对该玩家造成的伤害等
		public override void ModifyHitByNPC(NPC npc, ref int damage, ref bool crit)
        {
			if (PlayerClass == 1 && boughtbuffList[2])
			{
				damage = (int)(damage * (1 - bodyBloodGas / 5000 * 0.01f));
				if (damage < 1)
					damage = 1;
			}
			if (PlayerClass == 2 && boughtbuffList[2])
            {
				if (Main.rand.Next(101) <= (bodyBloodGas / 5000))
				{
					player.ShadowDodge();
				}
            }
		}

        public override void ModifyDrawLayers(List<PlayerLayer> layers)
        {
			if(eyeBloodGas + handBloodGas + bodyBloodGas + footBloodGas > 0 && SummonHeartConfig.Instance.EffectVisualConfig)
            {
				//handle lightning effects
				AnimationHelper.lightningEffects.visible = true;
				layers.Add(AnimationHelper.lightningEffects);

				AnimationHelper.auraEffect.visible = true;
				// capture the back layer index, which should always exist before the hook fires.
				var index = layers.FindIndex(x => x.Name == "MiscEffectsBack");
				layers.Insert(index, AnimationHelper.auraEffect);
            }
		}

        public override void PostItemCheck()
        {
			if (Berserked)
			{
				player.controlUseItem = true;
				player.releaseUseItem = true;
				player.HeldItem.autoReuse = true;
			}
		}

		public void IncrementAuraFrameTimers(AuraAnimationInfo aura)
		{
			if (aura == null)
				return;
			// doubled frame timer while charging.
			// auraFrameTimer++;

			auraFrameTimer++;
			if (auraFrameTimer >= aura.frameTimerLimit)
			{
				auraFrameTimer = 0;
				auraCurrentFrame++;
			}
			if (auraCurrentFrame >= aura.frames)
			{
				auraCurrentFrame = 0;
			}
		}

        public override float UseTimeMultiplier(Item item)
        {
			int useTime = item.useTime;
			int useAnimate = item.useAnimation;

			if (useTime == 0 || useAnimate == 0 || item.damage <= 0)
			{
				return 1f;
			}

			if(item.modItem != null && item.modItem.Name == "DemonSword")
            {
				return AttackSpeed / 2 + 0.5f;
			}
			if(item.modItem != null && item.modItem.Name == "Raiden")
            {
				return AttackSpeed / 2 + 0.5f;
			}

			return AttackSpeed;
		}

       /* public override void GetWeaponDamage(Item item, ref int damage)
        {
            base.GetWeaponDamage(item, ref damage);
        }*/

        public override void ModifyHitNPC(Item item, NPC target, ref int damage, ref float knockback, ref bool crit)
        {
			if (crit && PlayerClass == 1 && boughtbuffList[0])
			{
				damage *= (int)(eyeBloodGas / 2000 * 0.01 + 1);
			}
			this.CauseDirectDamage(target, damage, crit);
		}

        public override void ModifyHitNPCWithProj(Projectile proj, NPC target, ref int damage, ref float knockback, ref bool crit, ref int hitDirection)
        {
			if (crit && PlayerClass == 1 && boughtbuffList[0])
			{
				damage *= (int)(eyeBloodGas / 2000 * 0.01 + 1);
			}
			this.CauseDirectDamage(target, damage, crit);
		}
    }
}