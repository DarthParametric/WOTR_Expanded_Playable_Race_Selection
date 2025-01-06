using DP_WOTR_PlayableRaceExp.Utilities;
using Kingmaker.Blueprints;
using Kingmaker.Blueprints.CharGen;
using Kingmaker.Blueprints.Classes;
using Kingmaker.Blueprints.Root;
using Kingmaker.Designers.Mechanics.Buffs;
using Kingmaker.EntitySystem.Stats;
using Kingmaker.Enums;
using Kingmaker.ResourceLinks;
using Kingmaker.UnitLogic.FactLogic;
using static DP_WOTR_PlayableRaceExp.Main;

namespace DP_WOTR_PlayableRaceExp.Races
{
	class DPDrow
	{
		private static BlueprintRace DPDrowRace;
		private static string DPDrowPortraitGuidFem;
		private static string DPDrowPortraitGuidMale;
		public static ref BlueprintRaceReference[] Races => ref BlueprintRoot.Instance.Progression.m_CharacterRaces;

		public static void Install()
		{
			RaceExpContext.Logger.Log("Creating blueprints for added Drow race.");

			var OrigElf = RaceRefs.ElfRace;

			RaceExpContext.Logger.LogDebug($"Cloning original race blueprint {OrigElf.name}.");

			// Copy vanilla XYZRace blueprint and replace its values.
			DPDrowRace = Helpers.CreateCopyAlt<BlueprintRace>(OrigElf, race => {
				race.name = "DPDrow";
				race.AssetGuid = RaceExpContext.Blueprints.GetGUID(race.name);
				race.m_DisplayName = Helpers.CreateString(RaceExpContext, "DPDrow_DisplayName", "Drow");
				race.m_Description = Helpers.CreateString(RaceExpContext, "DPDrow_Description", "Cruel and cunning, drow are a dark reflection of the elven race. Also called dark elves, they dwell deep underground in elaborate cities shaped from the rock of cyclopean caverns. Drow seldom make themselves known to surface folk, preferring to remain legends while advancing their sinister agendas through proxies and agents. Drow have no love for anyone but themselves, and are adept at manipulating other creatures. While they are not born evil, malignancy is deep-rooted in their culture and society, and nonconformists rarely survive for long. Some stories tell that given the right circumstances, a particularly hateful elf might turn into a drow, though such a transformation would require a truly heinous individual.");

				// The "m_Features" array contains the set of base features inherent to a class like weapon proficiencies, immunities, etc.
				// Most important are likely the heritage selections, which may need to be changed depending on how divergent from the vanilla
				// race the custom race is.

				// Vanilla Elf features:
				// 9c747d24f6321f744aa1bb4bd343880d - Keen Senses
				// 55edf82380a1c8540af6c6037d34f322 - Elven Magic
				// 2483a523984f44944a7cf157b21bf79c - Elven Immunities
				// 03fd1e043fc678a4baf73fe67c3780ce - Elven Weapon Familiarity
				// 5482f879dcfd40f9a3168fdb48bc938c - Elven Heritage Selection

				// Example from original BubbleRaces code, added Grippli race.
				//race.m_Features = Helpers.Arr(camouflageFeature.BaseRef(), defTrainingFeature.BaseRef(), grippliHeritage.BaseRef());

				// Empty the "Components" array. This defines racial stat bonuses/penalties.
				race.Components = Array.Empty<BlueprintComponent>();

				// The "m_Presets" field in the race blueprint has GUID references to the XYZ_VisualPreset blueprints, typically default, fat, and thin.
				BlueprintRaceVisualPreset[] presets = new BlueprintRaceVisualPreset[race.m_Presets.Length];

				EquipmentEntityLink originalBodyM = null;
				EquipmentEntityLink originalBodyF = null;

				// The "m_Skin" field in the presets point to the KEE_Body_XYZ blueprint, which defines the body meshes.
				var skin = Helpers.CreateCopyAlt(race.m_Presets[0].Get().Skin, skin => {
					skin.name = skin.name.Replace("Elf", "DPDrow");

					RaceExpContext.Logger.LogDebug($"Creating body KEE:");

					skin.AssetGuid = RaceExpContext.Blueprints.GetGUID(skin.name);
					BlueprintTools.AddBlueprint(RaceExpContext, skin);

					// Body model arrays. Points to the AssetID of the body models EE in the KEE_Body_XYZ blueprint.
					originalBodyM = skin.m_MaleArray[0];
					originalBodyF = skin.m_FemaleArray[0];

					skin.m_MaleArray = Helpers.Arr(new EquipmentEntityLink {AssetId = EE_Names_IDs.Get_EE_ID("ee_body01_m_de")});
					skin.m_FemaleArray = Helpers.Arr(new EquipmentEntityLink {AssetId = EE_Names_IDs.Get_EE_ID("ee_body01_f_de")});

					RaceExpContext.Logger.LogDebug("Adding body EEs to VisualPresets");
				});

				// Create a set of new unique VisualPreset blueprints based on the donor race originals.
				RaceExpContext.Logger.LogDebug("Creating VisualPreset blueprints:");

				for (int i = 0; i < presets.Length; i++)
				{
					// TTT's version of CreateCopy (actually ObjectDeepCopier which it calls) causes
					// a stack overflow if used here. Use the older version instead.
					presets[i] = Helpers.CreateCopyAlt(race.m_Presets[i].Get(), p => {

						RaceExpContext.Logger.LogDebug($"New preset {i} (original name {p.name}):");

						p.name = p.name.Replace("Elf", "DPDrow");
						p.AssetGuid = RaceExpContext.Blueprints.GetGUID(p.name);
						BlueprintTools.AddBlueprint(RaceExpContext, p);

						p.m_Skin = skin.ToReference<KingmakerEquipmentEntityReference>();
					});
				}

				// Adds the newly created VisualPresets to the "m_Presets" array in the new base race blueprint.
				race.m_Presets = presets.Select(p => p.ToReference<BlueprintRaceVisualPresetReference>()).ToArray();

				RaceExpContext.Logger.LogDebug("Adding racial stat bonuses:");

				// Populate the base race blueprint "Components" array. This allows for defining custom racial stat bonuses/penalties.
				// Not strictly necessary in this instance as the new values are identical to the original Elf values.
				// +2 Dex.
				race.AddComponent<AddStatBonus>(stat => {
					stat.Descriptor = ModifierDescriptor.Racial;
					stat.Value = 2;
					stat.Stat = StatType.Dexterity;

					RaceExpContext.Logger.LogDebug($"{stat.Stat} = {string.Format("{0:+#;-#;+0}", stat.Value)}");

				});
				// +2 Cha.
				race.AddComponent<AddStatBonus>(stat => {
					stat.Descriptor = ModifierDescriptor.Racial;
					stat.Value = 2;
					stat.Stat = StatType.Charisma;

					RaceExpContext.Logger.LogDebug($"{stat.Stat} = {string.Format("{0:+#;-#;+0}", stat.Value)}");

				});
				// -2 Con.
				race.AddComponent<AddStatBonusIfHasFact>(stat => {
					stat.Descriptor = ModifierDescriptor.Racial;
					stat.Value = -2;
					stat.Stat = StatType.Constitution;
					// This checked fact for stat penalties allows them to be removed with the "Destiny Beyond Birth" mythic feat.
					stat.m_CheckedFacts = Helpers.Arr(BlueprintTools.Ref<BlueprintUnitFactReference>("325f078c584318849bfe3da9ea245b9d"));
					stat.InvertCondition = true;

					RaceExpContext.Logger.LogDebug($"{stat.Stat} = {string.Format("{0:+#;-#;+0}", stat.Value)}");

				});

				race.SelectableRaceStat = false;

				RaceExpContext.Logger.LogDebug("Adding body part EEs.");

				// Populate "MaleOptions" / "FemaleOptions" arrays, which define available body part options in the character creator.
				race.MaleOptions.Beards = Array.Empty<EquipmentEntityLink>();
				race.MaleOptions.Horns = Array.Empty<EquipmentEntityLink>();
				race.FemaleOptions.Horns = Array.Empty<EquipmentEntityLink>();

				// Define new head arrays.
				EquipmentEntityLink[] MaleHeadArray = [
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Head01_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Head02_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Head05_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Head04_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Head03_M_DPDrow")}
				];

				EquipmentEntityLink[] FemHeadArray = [
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Head03_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Head01_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Head05_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Head02Ember_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Head04_F_DPDrow")}
				];

				race.MaleOptions.m_Heads = Helpers.Arr(MaleHeadArray);
				race.MaleOptions.m_HeadsCache = null;
				race.FemaleOptions.m_Heads = Helpers.Arr(FemHeadArray);
				race.FemaleOptions.m_HeadsCache = null;

				// Define new eyebrow arrays. Make sure these match the same order as the head they go with! (Ask me how I know....)
				EquipmentEntityLink[] MaleBrowArray = [
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Brows01_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Brows02_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Brows05_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Brows04_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Brows03_M_DPDrow")}
				];

				EquipmentEntityLink[] FemBrowArray = [
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Brows03_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Brows01_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Brows05_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Brows02Ember_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Brows04_F_DPDrow")}
				];

				race.MaleOptions.m_Eyebrows = Helpers.Arr(MaleBrowArray);
				race.MaleOptions.m_EyebrowsCache = null;
				race.FemaleOptions.m_Eyebrows = Helpers.Arr(FemBrowArray);
				race.FemaleOptions.m_EyebrowsCache = null;

				// Define new hair arrays.
				EquipmentEntityLink[] MaleHairArray = [
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair00Slick_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair07PonytailClassic_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair02MediumTinyBraid_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair01MediumSide_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair03LongBraids_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair06MediumBun_M_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair04LongStraight_M_DPDrow")},
					// Bald. EE_EMPTY_HairStyleColors.
					new EquipmentEntityLink {AssetId = "b85db19d7adf6aa48b5dd2bb7bfe1502"}
				];

				EquipmentEntityLink[] FemHairArray = [
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair07Long_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair01LongEmber_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair02FrenchBraid_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair04PonyTailLush_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair08SideKare_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair06MediumAnevia_F_DPDrow")},
					new EquipmentEntityLink {AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("EE_Hair03Pompadour_F_DPDrow")},
					// Bald. EE_EMPTY_HairStyleColors.
					new EquipmentEntityLink {AssetId = "b85db19d7adf6aa48b5dd2bb7bfe1502"}
				];

				race.MaleOptions.m_Hair = Helpers.Arr(MaleHairArray);
				race.MaleOptions.m_HairCache = null;
				race.FemaleOptions.m_Hair = Helpers.Arr(FemHairArray);
				race.FemaleOptions.m_HairCache = null;
			});

			// Create new racial default portraits in the character creator.

			RaceExpContext.Logger.LogDebug("Creating portrait presets:");

			var portraitmale = Helpers.CreateBlueprint<BlueprintPortrait>(RaceExpContext, "DPDrow_PortraitMale", p => {
				p.Data = new()
				{
					PortraitCategory = PortraitCategory.Wrath,
					m_FullLengthImage = new SpriteLink { AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("Drow_Male_Portrait_Fulllength") },
					m_HalfLengthImage = new SpriteLink { AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("Drow_Male_Portrait_Medium") },
					m_PortraitImage = new SpriteLink { AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("Drow_Male_Portrait_Small") },
				};
				p.AddComponent<PortraitDollSettings>(set => {
					set.m_Race = DPDrowRace.ToReference<BlueprintRaceReference>();
					set.Gender = Gender.Male;
				});
				DPDrowPortraitGuidMale = p.AssetGuid.ToString();
			});

			Helpers.AppendInPlace(ref BlueprintRoot.Instance.CharGen.m_Portraits, portraitmale.ToReference<BlueprintPortraitReference>());

			var portraitfem = Helpers.CreateBlueprint<BlueprintPortrait>(RaceExpContext, "DPDrow_PortraitFem", p => {
				p.Data = new()
				{
					PortraitCategory = PortraitCategory.Wrath,
					m_FullLengthImage = new SpriteLink { AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("Drow_Female_Portrait_Fulllength") },
					m_HalfLengthImage = new SpriteLink { AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("Drow_Female_Portrait_Medium") },
					m_PortraitImage = new SpriteLink { AssetId = EE_Bundle_Asset_IDs.Get_Bundle_Asset_ID("Drow_Female_Portrait_Small") },
				};
				p.AddComponent<PortraitDollSettings>(set => {
					set.m_Race = DPDrowRace.ToReference<BlueprintRaceReference>();
					set.Gender = Gender.Female;
				});
				DPDrowPortraitGuidFem = p.AssetGuid.ToString();
			});

			Helpers.AppendInPlace(ref BlueprintRoot.Instance.CharGen.m_Portraits, portraitfem.ToReference<BlueprintPortraitReference>());

			RaceExpContext.Logger.LogDebug("Creating race blueprint:");

			BlueprintTools.AddBlueprint(RaceExpContext, DPDrowRace);

			Helpers.AppendInPlace(ref Races, DPDrowRace.ToReference<BlueprintRaceReference>());

			RaceExpContext.Logger.LogDebug("Installation of added Drow race complete.");
		}

		public static void Uninstall()
		{
			if (DPDrowRace != null)
			{
				BlueprintRoot.Instance.Progression.m_CharacterRaces = BlueprintRoot.Instance.Progression.m_CharacterRaces.Where(r => r.deserializedGuid != DPDrowRace.AssetGuid.ToString()).ToArray();
				DPDrowRace = null;
			}

			if (DPDrowPortraitGuidFem != null)
			{
				BlueprintRoot.Instance.CharGen.m_Portraits = BlueprintRoot.Instance.CharGen.m_Portraits.Where(p => p.deserializedGuid != DPDrowPortraitGuidFem).ToArray();
				DPDrowPortraitGuidFem = null;
			}

			if (DPDrowPortraitGuidMale != null)
			{
				BlueprintRoot.Instance.CharGen.m_Portraits = BlueprintRoot.Instance.CharGen.m_Portraits.Where(p => p.deserializedGuid != DPDrowPortraitGuidMale).ToArray();
				DPDrowPortraitGuidMale = null;
			}
		}
	}
}
