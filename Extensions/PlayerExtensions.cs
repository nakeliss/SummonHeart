﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SummonHeart.Models;
using SummonHeart.NPCs;
using SummonHeart.Utilities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace SummonHeart.Extensions
{
    /// <summary>
    ///     A class housing all the player/ModPlayer/MyPlayer based extensions
    /// </summary>
    public static class PlayerExtensions
    {
		public static SummonHeartPlayer getModPlayer(this Player player) => player.GetModPlayer<SummonHeartPlayer>();
		public static void DrawAura(this SummonHeartPlayer modPlayer, AuraAnimationInfo aura)
        {
            Player player = modPlayer.player;
            Texture2D texture = aura.GetTexture();
            Rectangle textureRectangle = new Rectangle(0, aura.GetHeight() * modPlayer.auraCurrentFrame, texture.Width, aura.GetHeight());
            float scale = aura.GetAuraScale(modPlayer);
            Tuple<float, Vector2> rotationAndPosition = aura.GetAuraRotationAndPosition(modPlayer);
            float rotation = rotationAndPosition.Item1;
            Vector2 position = rotationAndPosition.Item2;

            AnimationHelper.SetSpriteBatchForPlayerLayerCustomDraw(aura.blendState, player.GetPlayerSamplerState());

            // custom draw routine
            Main.spriteBatch.Draw(texture, position - Main.screenPosition, textureRectangle, Color.White, rotation, new Vector2(aura.GetWidth(), aura.GetHeight()) * 0.5f, scale, SpriteEffects.None, 0f);

            AnimationHelper.ResetSpriteBatchForPlayerDrawLayers(player.GetPlayerSamplerState());
        }

        public static SamplerState GetPlayerSamplerState(this Player player)
        {
            return player.mount.Active ? Main.MountedSamplerState : Main.DefaultSamplerState;
        }

        public static int getPower(this SummonHeartPlayer modPlayer)
        {
            int power = 0;
            int x = 1;
            if (Main.hardMode)
            {
                x = 2;
            }
            if (NPC.downedMoonlord)
            {
                x = 5;
            }
            power = modPlayer.eyeBloodGas + modPlayer.handBloodGas + modPlayer.bodyBloodGas + modPlayer.footBloodGas;
            power += modPlayer.player.statLifeMax2 * x;
            power += modPlayer.player.statDefense * 30;
            power += modPlayer.SummonCrit * 20 * x;
            power += modPlayer.killResourceMax2 * 10;
            Item item = modPlayer.player.HeldItem;
            if (item.damage > 0)
            {
                power += (int)(item.damage * 5 * (60f / (float)item.useTime));
            }
           
            return power;
        }

        public static int getAllBloodGas(this SummonHeartPlayer modPlayer)
        {
            int all = 0;
            all = modPlayer.eyeBloodGas + modPlayer.handBloodGas + modPlayer.bodyBloodGas + modPlayer.footBloodGas;
            return all;
        }

        public static int HasItemInAcc(this Player player, int type)
        {
            for (int i = 3; i < 8 + player.extraAccessorySlots; i++)
            {
                if (player.armor[i].type == type)
                {
                    return i;
                }
            }
            SummonHeartPlayer mp = player.GetModPlayer<SummonHeartPlayer>();
			for (int i = 0; i < SummonHeartPlayer.MaxExtraAccessories; i++)
			{
				if (mp.ExtraAccessories[i].type == type)
				{
					return i;
				}
			}
			return -1;
        }

		public static int HasItemInInventory(this Player player, int type)
		{
			SummonHeartPlayer mp = player.GetModPlayer<SummonHeartPlayer>();
			for (int i = 0; i < player.inventory.Length; i++)
			{
				if (player.inventory[i].type == type)
				{
					return i;
				}
			}
			return -1;
		}

		public static void doKillNpcExp(this Player player, NPC npc)
        {
			SummonHeartPlayer modPlayer = player.GetModPlayer<SummonHeartPlayer>();

			if (player.HeldItem.modItem != null && player.HeldItem.modItem.Name == "DemonSword")
			{
				if (npc.boss && NPC.downedMoonlord)
				{
					int swordMax = npc.getPower() / 400;
					if (modPlayer.swordBloodMax < swordMax)
					{
						modPlayer.swordBloodMax = swordMax;
						string curMax = (modPlayer.swordBloodMax * 1.0f / 100f).ToString("0.00");
						string text = $"{player.name}手持魔剑·弑神吞噬了{npc.FullName}的血肉，突破觉醒上限，当前觉醒上限：{curMax}%";
						Main.NewText(text, Color.Green);
					}
				}
				if (modPlayer.swordBlood < modPlayer.swordBloodMax)
				{
					modPlayer.swordBlood += (modPlayer.swordBloodMax / 10000 + 1);
					if (modPlayer.swordBlood > modPlayer.swordBloodMax)
						modPlayer.swordBlood = modPlayer.swordBloodMax;
				}
			}
			if (player.HeldItem.modItem != null && player.HeldItem.modItem.Name == "Raiden")
			{
				if (npc.boss && NPC.downedMoonlord)
				{
					int swordMax = npc.getPower() / 800;
					if (swordMax > 999999)
						swordMax = 999999;
					if (modPlayer.swordBloodMax < swordMax)
					{
						modPlayer.swordBloodMax = swordMax;
						string curMax = (modPlayer.swordBloodMax * 1.0f / 100f).ToString("0.00");
						string text = $"{player.name}手持魔剑·神陨吞噬了{npc.FullName}的血肉，突破觉醒上限，当前觉醒上限：{curMax}%";
						Main.NewText(text, Color.Green);
					}
				}
				if (modPlayer.shortSwordBlood < modPlayer.swordBloodMax)
				{
					modPlayer.shortSwordBlood += (modPlayer.swordBloodMax / 10000 + 1);
					if (modPlayer.shortSwordBlood > modPlayer.swordBloodMax)
						modPlayer.shortSwordBlood = modPlayer.swordBloodMax;
				}
				if (modPlayer.PlayerClass == 2)
				{
					int heal = 5 * SummonHeartWorld.WorldLevel;
					if (modPlayer.boughtbuffList[1])
					{
						heal += (modPlayer.eyeBloodGas / 400);
					}
					modPlayer.killResourceCurrent += heal;
					CombatText.NewText(player.getRect(), new Color(0, 255, 0), "+" + heal + "杀意值");
					if (modPlayer.killResourceCurrent > modPlayer.killResourceMax2)
						modPlayer.killResourceCurrent = modPlayer.killResourceMax2;
					if (Main.netMode == NetmodeID.Server)
					{
						modPlayer.KillResourceCountMsg();
					}
				}
			}
			if (player.HeldItem.modItem != null && player.HeldItem.modItem.Name == "DemonFlySword")
			{
				if (npc.boss)
				{
					int swordMax = npc.getPower() / 800;
					if (modPlayer.swordBloodMax < swordMax && NPC.downedMoonlord)
					{
						modPlayer.swordBloodMax = swordMax;
						string curMax = (modPlayer.swordBloodMax * 1.0f / 100f).ToString("0.00");
						string text = $"{player.name}手持魔剑·神灭吞噬了{npc.FullName}的血肉，突破觉醒上限，当前觉醒上限：{curMax}%";
						Main.NewText(text, Color.Green);
					}
					if (modPlayer.flySwordBlood < modPlayer.swordBloodMax)
					{
						modPlayer.flySwordBlood += (modPlayer.swordBloodMax / 2000 + 5);
						if (modPlayer.flySwordBlood > modPlayer.swordBloodMax)
							modPlayer.flySwordBlood = modPlayer.swordBloodMax;
					}
				}
			}
			if (player.HeldItem.modItem != null && player.HeldItem.modItem.Name == "DemonStaff")
			{
				if (npc.boss && NPC.downedMoonlord)
				{
					int swordMax = npc.getPower() / 400;
					if (modPlayer.swordBloodMax < swordMax)
					{
						modPlayer.swordBloodMax = swordMax;
						string curMax = (modPlayer.swordBloodMax * 1.0f / 100f).ToString("0.00");
						string text = $"{player.name}手持魔力之源吞噬了{npc.FullName}的血肉，突破觉醒上限，当前觉醒上限：{curMax}%";
						Main.NewText(text, Color.Green);
					}
				}

				if (modPlayer.magicSwordBlood < modPlayer.swordBloodMax)
				{
					modPlayer.magicSwordBlood += (modPlayer.swordBloodMax / 10000 + 1);
					if (modPlayer.magicSwordBlood > modPlayer.swordBloodMax)
						modPlayer.magicSwordBlood = modPlayer.swordBloodMax;
				}
			}

			if(modPlayer.PlayerClass == 5 && modPlayer.boughtbuffList[0])
            {
				int healMana = modPlayer.eyeBloodGas / 1000 + 10;
				player.HealMana(healMana);
            }

			if (modPlayer.SummonHeart)
			{
				int addExp = 0;
				int addBloodGas = 0;
				int powerLevel = npc.getPowerLevel();

				if (npc.boss)
				{
					if (powerLevel == -1)
					{
						addExp = 1;

					}
					else
					{
						addExp = npc.getPower() / 100;
						if (Main.hardMode)
						{
							addExp = npc.getPower() / 200;
						}
						if (NPC.downedMoonlord)
						{
							addExp = npc.getPower() / 500;
						}
					}
				}
				else
				{
					addExp = 1;
				}
				//越阶战斗奖励
				if (powerLevel > 0)
				{
					if (powerLevel >= 10)
						powerLevel = 10;
					if(modPlayer.PlayerClass == 3)
                    {
						//召唤职业最大5倍
						if (powerLevel >= 5)
							powerLevel = 5;
					}
					addExp *= (powerLevel + 1);
				}

				//处理灵魂
				//处理难度额外灵魂
				int hardMulti = SummonHeartWorld.WorldLevel;
				if (hardMulti > 0 && !npc.boss)
				{
					addExp *= hardMulti;
				}
				modPlayer.BBP += addExp;
				if (modPlayer.BBP > 5000000)
					modPlayer.BBP = 5000000;

				if (npc.boss)
				{
					string text = "";
					if (powerLevel == -1)
					{
						text = $"{player.name}的战力碾压{npc.FullName}，可惜，其血肉灵魂已于{player.name}无用！灵魂之力+{addExp}";
					}
					if (powerLevel == 0)
					{
						text = $"{player.name}吞噬了{npc.FullName}的灵魂，灵魂之力+{addExp}";
					}
					if (powerLevel > 0)
					{
						text = $"{player.name}越级吞噬了{npc.getPowerLevelText()}强者{npc.FullName}的灵魂，获得额外{powerLevel}倍灵魂之力，+{addExp}灵魂之力";
					}

					Main.NewText(text, Color.Green);
				}

				/*//处理突破
				//最大血气上限【规则】
				int WORLDMAXBLOODGAS = SummonHeartWorld.WorldBloodGasMax;
				//int MAXBLOODGAS = 200000;
				if (powerLevel > 0 && npc.getPower() > modPlayer.bloodGasMax)
				{
					//突破的数值=（敌人战力-玩家肉身极限）/  5 * (阶位)
					int addMax = (npc.getPower() - modPlayer.bloodGasMax) / 5;

					if (powerLevel >= 5)
						addMax = npc.getPower() - modPlayer.bloodGasMax;
					else
					{
						addMax *= (powerLevel);
					}
					modPlayer.bloodGasMax += addMax;
					//判断是否超过世界上限
					if (modPlayer.bloodGasMax > WORLDMAXBLOODGAS)
						modPlayer.bloodGasMax = WORLDMAXBLOODGAS;
					string text = $"{player.name}越级斩杀了{npc.getPowerLevelText()}强者{npc.FullName}，于生死之间突破肉身极限，+{addMax}肉身极限";
					Main.NewText(text, Color.Green);
				}*/

				//修炼开始
				int bloodGasMax = modPlayer.bloodGasMax;

				if (modPlayer.getAllBloodGas() < SummonHeartWorld.WorldBloodGasMax)
				{
					addBloodGas = addExp;

					//修炼魔神之眼
					if (modPlayer.practiceEye && modPlayer.eyeBloodGas < bloodGasMax)
					{
						if (modPlayer.eyeBloodGas < modPlayer.eyeMax)
						{
							//判断是否超上限
							if (modPlayer.CheckSoul(addExp))
							{
								modPlayer.BuySoul(addExp);
								modPlayer.eyeBloodGas += addBloodGas;
								if (modPlayer.eyeBloodGas > bloodGasMax)
									modPlayer.eyeBloodGas = bloodGasMax;
								if (modPlayer.eyeBloodGas > modPlayer.eyeMax)
									modPlayer.eyeBloodGas = modPlayer.eyeMax;
								if (npc.boss)
								{
									string text = "";
									if (powerLevel == 0)
									{
										text = $"{player.name}修炼魔神之眼消耗{addExp}灵魂之力吞噬了{npc.FullName}的气血，魔神之眼气血+{addBloodGas}";
									}
									if (powerLevel > 0)
									{
										text = $"{player.name}修炼魔神之眼消耗{addExp}灵魂之力越级吞噬了{npc.getPowerLevelText()}强者{npc.FullName}的气血，额外吸收{powerLevel}倍气血，魔神之眼气血+{addBloodGas}";
									}
									Main.NewText(text, Color.Green);
								}
							}
						}
					}
					//修炼魔神之手
					if (modPlayer.practiceHand && modPlayer.handBloodGas < bloodGasMax)
					{
						int handMaxBloodGas = modPlayer.handMax;
						
						if (modPlayer.handBloodGas < modPlayer.handMax)
						{
							
							if (modPlayer.CheckSoul(addExp))
							{
								modPlayer.BuySoul(addExp);
								modPlayer.handBloodGas += addBloodGas;
								if (modPlayer.handBloodGas > bloodGasMax)
									modPlayer.handBloodGas = bloodGasMax;
								if (modPlayer.handBloodGas > handMaxBloodGas)
									modPlayer.handBloodGas = handMaxBloodGas;
								if (npc.boss)
								{
									string text = "";
									if (powerLevel == 0)
									{
										text = $"{player.name}修炼魔神之手消耗{addExp}灵魂之力吞噬了{npc.FullName}的气血，魔神之手气血+{addBloodGas}";
									}
									if (powerLevel > 0)
									{
										text = $"{player.name}修炼魔神之手消耗{addExp}灵魂之力越级吞噬了{npc.getPowerLevelText()}强者{npc.FullName}的气血，额外吸收{powerLevel}倍气血，魔神之手气血+{addBloodGas}";
									}
									Main.NewText(text, Color.Green);
								}
							}
						}
					}
					//修炼魔神之躯
					if (modPlayer.practiceBody)
					{
						int bodyMaxBloodGas = modPlayer.bodyMax;
						if (modPlayer.PlayerClass == 1)
							bodyMaxBloodGas = modPlayer.bodyMax * 2;
						int bodyMax = bloodGasMax;
						if (modPlayer.PlayerClass == 1)
							bodyMax = bloodGasMax * 2;
						if (modPlayer.bodyBloodGas < bodyMaxBloodGas && modPlayer.bodyBloodGas < bodyMax)
						{
							if (modPlayer.CheckSoul(addExp))
							{
								modPlayer.BuySoul(addExp);
								modPlayer.bodyBloodGas += addBloodGas;
								if (modPlayer.bodyBloodGas > bodyMaxBloodGas)
									modPlayer.bodyBloodGas = bodyMaxBloodGas;
								if (modPlayer.bodyBloodGas > bodyMax)
									modPlayer.bodyBloodGas = bodyMax;
								if (npc.boss)
								{
									string text = "";
									if (powerLevel == 0)
									{
										text = $"{player.name}修炼魔神之躯消耗{addExp}灵魂之力吞噬了{npc.FullName}的气血，魔神之躯气血+{addBloodGas}";
									}
									if (powerLevel > 0)
									{
										text = $"{player.name}修炼魔神之躯消耗{addExp}灵魂之力越级吞噬了{npc.getPowerLevelText()}强者{npc.FullName}的气血，额外吸收{powerLevel}倍气血，魔神之躯气血+{addBloodGas}";
									}
									Main.NewText(text, Color.Green);
								}
							}
						}
					}
					//修炼魔神之腿
					if (modPlayer.practiceFoot && modPlayer.footBloodGas < bloodGasMax)
					{
						if (modPlayer.footBloodGas < modPlayer.footMax)
						{
							if (modPlayer.CheckSoul(addExp))
							{
								modPlayer.BuySoul(addExp);
								modPlayer.footBloodGas += addBloodGas;
								if (modPlayer.footBloodGas > modPlayer.footMax)
									modPlayer.footBloodGas = modPlayer.footMax;
								if (modPlayer.footBloodGas > bloodGasMax)
									modPlayer.footBloodGas = bloodGasMax;
								if (npc.boss)
								{
									string text = "";
									if (powerLevel == 0)
									{
										text = $"{player.name}修炼魔神之腿消耗{addExp}灵魂之力吞噬了{npc.FullName}的气血，魔神之腿气血+{addBloodGas}";
									}
									if (powerLevel > 0)
									{
										text = $"{player.name}修炼魔神之腿消耗{addExp}灵魂之力越级吞噬了{npc.getPowerLevelText()}强者{npc.FullName}的气血，额外吸收{powerLevel}倍气血，魔神之腿气血+{addBloodGas}";
									}
									Main.NewText(text, Color.Green);
								}
							}
						}
					}
				}
				modPlayer.dealLevel();
			}
		}

		public static SummonHeartPlayer SummonHeart(this Player player) => player.GetModPlayer<SummonHeartPlayer>();
		public static SummonHeartGlobalNPC SummonHeart(this NPC npc) => npc.GetGlobalNPC<SummonHeartGlobalNPC>();


		public static Color ColorOrOther(this SummonHeartPlayer modPlayer, Color other)
		{
			if (!modPlayer.useOscColor)
			{
				return other;
			}
			return modPlayer.oscColor;
		}

		public static bool AnyBossAlive(this Player player)
		{
			for (int i = 0; i < Main.npc.Length; i++)
			{
				NPC npc = Main.npc[i];
				if (npc.active && npc.boss) return true;
				if (npc.active && npc.type == NPCID.EaterofWorldsHead) return true;
			}
			return false;
		}

		public static bool CheckItemsFromOtherMods(this Player player)
		{
			foreach (Item item in player.inventory)
			{
				if (item.modItem != null && SummonHeartMod.rejPassItemList.Contains(item.modItem.Name))
				{
					return true;
				}
			}
			return false;
		}

		///<摘要>
		///通过免疫和摄像机lerp管理破折号的逻辑。
		///也接管了sparket.frame，所以在一个普通的斜杠ai之后调用它。
		///</summary>
		///<param name=“dashFrameDuration”>要冲刺的帧数
		///<param name=“dashSpeed”>短跑速度，例如player.maxRunSpeed*5f</param>
		///<param name=“freezeFrame”>要在例如2处冻结的帧
		///<param name=“dashEndVelocity”>结束速度，或为空以使用短划线速度，例如preDashVelocity</param>
		///<returns>如果当前在破折号中，则为True</returns>
		public static bool AIDashSlash(this Player player, Projectile projectile, float dashFrameDuration, float dashSpeed, int freezeFrame, ref Vector2? dashEndVelocity)
		{
			if (player.dead || !player.active)
			{
				projectile.timeLeft = 0;
				return false;
			}
			if (freezeFrame < 1) freezeFrame = 1;

			bool dashing = false;
			if ((int)projectile.ai[0] < dashFrameDuration)
			{
				// Fine-tuned tilecollision
				player.armorEffectDrawShadow = true;
				Vector2 projVel = projectile.velocity;
				if (player.gravDir < 0) projVel.Y = -projVel.Y;
				for (int i = 0; i < 4; i++)
				{
					player.position += Collision.TileCollision(player.position, projVel * dashSpeed / 4,
						player.width, player.height, false, false, (int)player.gravDir);
				}

				if (player.velocity.Y == 0)
				{ player.velocity = new Vector2(0, (projectile.velocity * dashSpeed).Y); }
				else
				{ player.velocity = new Vector2(0, player.gravDir * player.gravity); }

				// Prolong mid-slash player animation
				if (player.direction < 0) projectile.position.X += projectile.width;

				float dist = Math.Max(0, projectile.width - projectile.height); // total distance covered by the moving hitbox

				Vector2 direction = new Vector2(
					(float)Math.Cos(projectile.rotation),
					(float)Math.Sin(projectile.rotation));
				direction.Y *= player.gravDir;
				Vector2 centre = player.MountedCenter;
				Vector2 playerOffset = player.Size.X * projectile.scale * direction;

				projectile.Center = centre
					+ direction * (dist + projectile.height) / 2
					- playerOffset;
				if (player.itemAnimation <= player.itemAnimationMax - freezeFrame)
				{ player.itemAnimation = player.itemAnimationMax - freezeFrame; }

				// Set immunities
				player.immune = true;
				player.immuneTime = Math.Max(player.immuneTime, 6);
				player.immuneNoBlink = true;

				dashing = true;
			}
			else if ((int)projectile.ai[0] >= dashFrameDuration && dashEndVelocity != new Vector2(float.MinValue, float.MinValue))
			{
				if (dashEndVelocity == null)
				{
					Vector2 projVel = projectile.velocity.SafeNormalize(Vector2.Zero);
					if (player.gravDir < 0) projVel.Y = -projVel.Y;
					float speed = dashSpeed / 4f;
					if (speed < player.maxFallSpeed)
					{ player.velocity = projVel * speed; }
					else
					{ player.velocity = projVel * player.maxFallSpeed; }

					// Reset fall damage
					player.fallStart = (int)(player.position.Y / 16f);
					player.fallStart2 = player.fallStart;
				}
				else
				{
					player.velocity = (Vector2)dashEndVelocity;
				}

				// Set the vector to a "reset" state
				dashEndVelocity = new Vector2(float.MinValue, float.MinValue);
			}

			// Trigger lerp by offsetting camera
			if (projectile.timeLeft == 60)
			{
				Main.SetCameraLerp(0.1f, 10);
				Main.screenPosition -= projectile.velocity * 2;
			}

			// Set new projectile frame
			projectile.frame = (int)Math.Max(0, projectile.ai[0] - dashFrameDuration);

			return dashing;
		}
	}
}