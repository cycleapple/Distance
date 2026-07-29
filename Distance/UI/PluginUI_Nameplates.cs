using System;
using System.Numerics;

using CheapLoc;

using Dalamud.Bindings.ImGui;

namespace Distance;

internal sealed class PluginUI_Nameplates : IDisposable
{
	internal PluginUI_Nameplates( Plugin plugin, PluginUI ui, Configuration configuration )
	{
		mPlugin = plugin;
		mUI = ui;
		mConfiguration = configuration;
	}

	public void Dispose()
	{
	}

	internal void DrawConfigOptions()
	{
		ImGui.Checkbox( Loc.Localize( "Config Option: Show Nameplate Distances", "在名牌上顯示距離。" ) + "###Show nameplate distances.", ref mConfiguration.NameplateDistancesConfig.ShowNameplateDistances );
		if( mConfiguration.NameplateDistancesConfig.ShowNameplateDistances )
		{
			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Nameplate Distance Rules", "距離規則" ) + $"###Nameplate Distance Rules Header." ) )
			{
				ImGui.Checkbox( Loc.Localize("Config Option: Distance is to Ring for nameplates", "顯示到目標環的距離，而非目標中心。" ) + $"###Distance is to ring (nameplates).", ref mConfiguration.NameplateDistancesConfig.DistanceIsToRing );
				ImGui.Checkbox( Loc.Localize( "Config Option: Nameplates - Show All", "在所有名牌上顯示距離。" ) + $"###Show distance to all nameplates.", ref mConfiguration.NameplateDistancesConfig.ShowAll );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Nameplates - Show All", "在符合下一節物件類型篩選的所有名牌上顯示距離。取消勾選後，下方會出現其他條件。" ) );
				if( !mConfiguration.NameplateDistancesConfig.ShowAll )
				{
					ImGui.Indent();
					ImGui.Checkbox( Loc.Localize( "Config Option: Show Distance on Target", "顯示目前目標的距離。" ) + $"###Show distance to Target (nameplates).", ref mConfiguration.NameplateDistancesConfig.ShowTarget );
					ImGui.Checkbox( Loc.Localize( "Config Option: Show Distance on Soft Target", "顯示軟目標的距離。" ) + $"###Show distance to Soft Target (nameplates).", ref mConfiguration.NameplateDistancesConfig.ShowSoftTarget );
					ImGui.Checkbox( Loc.Localize( "Config Option: Show Distance on Focus Target", "顯示焦點目標的距離。" ) + $"###Show distance to Focus Target (nameplates).", ref mConfiguration.NameplateDistancesConfig.ShowFocusTarget );
					ImGui.Checkbox( Loc.Localize( "Config Option: Show Distance on Mouseover Target", "顯示滑鼠指向目標的距離。" ) + $"###Show distance to mouseover target (nameplates).", ref mConfiguration.NameplateDistancesConfig.ShowMouseoverTarget );
					ImGui.Checkbox( Loc.Localize( "Config Option: Nameplates - Show Distance on Aggro", "顯示已對你產生敵意之敵人的距離。" ) + $"###Show distance to aggro (nameplates).", ref mConfiguration.NameplateDistancesConfig.ShowAggressive );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Nameplates - Show Distance on Aggro", "僅套用於敵對列表中的目標；不會顯示其他已對你產生敵意之敵人的距離。" ) );
					ImGui.Checkbox( Loc.Localize( "Config Option: Nameplates - Show Distance on Party", "顯示小隊成員的距離。" ) + $"###Show distance to party (nameplates).", ref mConfiguration.NameplateDistancesConfig.ShowPartyMembers );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Nameplates - Show Distance on Party", "進入副本任務前，不會套用於跨界小隊成員。" ) );
					ImGui.Checkbox( Loc.Localize( "Config Option: Nameplates - Show Distance on Alliance", "顯示團隊成員的距離。" ) + $"###Show distance to alliance (nameplates).", ref mConfiguration.NameplateDistancesConfig.ShowAllianceMembers );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Nameplates - Show Distance on Alliance", "進入副本任務前，不會套用於跨界團隊成員。" ) );
					ImGui.Checkbox( Loc.Localize( "Config Option: Filters are Exclusive", "所有篩選條件必須同時符合。" ) + $"###Filters are exclusive (nameplates).", ref mConfiguration.NameplateDistancesConfig.FiltersAreExclusive );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Nameplates - Filters are Exclusive", "勾選時，物件必須同時符合上方條件與下方篩選才顯示距離；未勾選時，符合任一組條件即可。" ) );
					ImGui.Unindent();
				}
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Nameplate Offsets", "距離偏移" ) + $"###NameplateOffsetsHeader." ) )
			{
				ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Measurement Offset (Player)", "Amount to offset the distance readout for players ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Readout Offset", "This value is subtracted from the real distance to determine the displayed distance.  This can be used to get the widget to show the distance from being able to hit the boss with a skill, for example." ) );
				ImGui.DragFloat( "###DistanceOffsetSlider_Player", ref mConfiguration.NameplateDistancesConfig.DistanceOffset_Player_Yalms, 0.1f, -30f, 30f );
				ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Measurement Offset (BNpc)", "Amount to offset the distance readout for combatant NPCs ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Readout Offset", "This value is subtracted from the real distance to determine the displayed distance.  This can be used to get the widget to show the distance from being able to hit the boss with a skill, for example." ) );
				ImGui.DragFloat( "###DistanceOffsetSlider_BNpc", ref mConfiguration.NameplateDistancesConfig.DistanceOffset_BNpc_Yalms, 0.1f, -30f, 30f );
				ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Measurement Offset (Other)", "Amount to offset the distance readout for all other entities ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Readout Offset", "This value is subtracted from the real distance to determine the displayed distance.  This can be used to get the widget to show the distance from being able to hit the boss with a skill, for example." ) );
				ImGui.DragFloat( "###DistanceOffsetSlider_Other", ref mConfiguration.NameplateDistancesConfig.DistanceOffset_Other_Yalms, 0.1f, -30f, 30f );
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Nameplate Filters", "物件類型篩選" ) + $"###NameplateFiltersHeader." ) )
			{
				mConfiguration.NameplateDistancesConfig.Filters.DrawObjectKindOptions();
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize("Config Section Header: Distance Nameplate Condition", "狀態篩選" ) + "###NameplateConditionsHeader" ) )
			{
				mConfiguration.NameplateDistancesConfig.Filters.DrawConditionOptions();
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Nameplate Classjobs", "職業篩選" ) + "###NameplateClassjobsHeader" ) )
			{
				mConfiguration.NameplateDistancesConfig.Filters.DrawClassJobOptions();
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Nameplate Appearance", "外觀" ) + $"###Nameplate Appearance Header." ) )
			{
				Vector2 sliderLimits = new( -300, 300 );
				ImGui.Text( Loc.Localize( "Config Option: Nameplates - Text Offset", "距離文字位置偏移（X、Y）：" ) );
				ImGui.DragFloat2( "###NameplateDistanceTextOffsetSlider", ref mConfiguration.NameplateDistancesConfig.DistanceTextOffset, 1f, sliderLimits.X, sliderLimits.Y, "%g", ImGuiSliderFlags.AlwaysClamp );
				ImGui.Checkbox( Loc.Localize( "Config Option: Nameplates - Automatic Alignment", "自動排列距離文字位置。" ) + $"###automatic alignment (nameplates).", ref mConfiguration.NameplateDistancesConfig.AutomaticallyAlignText );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Nameplates - Automatic Alignment", "勾選時，距離文字會依名牌文字自動靠右、置中或靠左，並套用上方偏移；未勾選時則固定位置，不受名稱長度影響。" ) );
				if( mConfiguration.NameplateDistancesConfig.AutomaticallyAlignText )
				{
					ImGui.Checkbox( Loc.Localize( "Config Option: Nameplates - Place Below Name", "將距離放在名牌下方。" ) + $"###align below name (nameplates).", ref mConfiguration.NameplateDistancesConfig.PlaceTextBelowName );
				}
				ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Use Heavy Font", "距離文字使用粗體字型。" ) + $"###Distance font heavy  (nameplates).", ref mConfiguration.NameplateDistancesConfig.DistanceFontHeavy );
				//ImGui.RadioButton( Loc.Localize( "Config Option: Nameplate Style - Match Game", "Match Game" ) + "###NameplateStyleMatchGameButton", ref mConfiguration.NameplateDistancesConfig.mNameplateStyle, (int)NameplateStyle.MatchGame );
				//ImGui.SameLine();
				//ImGui.RadioButton( Loc.Localize( "Config Option: Nameplate Style - Old", "Old" ) + "###NameplateStyleOldButton", ref mConfiguration.NameplateDistancesConfig.mNameplateStyle, (int)NameplateStyle.Old );
				//ImGui.SameLine();
				//ImGui.RadioButton( Loc.Localize( "Config Option: Nameplate Style - New", "New" ) + "###NameplateStyleNewButton", ref mConfiguration.NameplateDistancesConfig.mNameplateStyle, (int)NameplateStyle.New );
				ImGui.Text( Loc.Localize( "Config Option: Distance Text Font Size", "距離文字字型大小：" ) );
				ImGui.SliderInt( $"###DistanceTextFontSizeSlider (nameplates)", ref mConfiguration.NameplateDistancesConfig.DistanceFontSize, 6, 36 );
				ImGui.Text( Loc.Localize( "Config Option: Distance Text Font Alignment", "文字對齊：" ) );
				ImGui.SliderInt( "###DistanceTextFontAlignmentSlider (nameplates)", ref mConfiguration.NameplateDistancesConfig.mDistanceFontAlignment, 6, 8, "", ImGuiSliderFlags.NoInput );
				ImGui.Checkbox( Loc.Localize( "Config Option: Show Distance Units", "距離數值顯示單位。" ) + $"###Show distance units (nameplates).", ref mConfiguration.NameplateDistancesConfig.ShowUnitsOnDistance );
				ImGui.Checkbox( Loc.Localize( "Config Option: Allow Negative Distances", "允許顯示負距離。" ) + $"###Allow negative distances (nameplates).", ref mConfiguration.NameplateDistancesConfig.AllowNegativeDistances );
				ImGui.Text( Loc.Localize( "Config Option: Decimal Precision", "距離顯示的小數位數：" ) );
				ImGui.SliderInt( $"###DistancePrecisionSlider (nameplates)", ref mConfiguration.NameplateDistancesConfig.DistanceDecimalPrecision, 0, 3 );

				ImGui.TreePop();
			}
			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Nameplate Colors", "顏色" ) + $"###Nameplate Colors Header." ) )
			{
				ImGui.Checkbox( Loc.Localize( "Config Option: Nameplate Distance Text Use Distance-based Colors (Party)", "小隊成員依距離使用不同文字顏色。" ) + $"###Distance Text Use distance-based colors (Nameplates - Party).", ref mConfiguration.NameplateDistancesConfig.UseDistanceBasedColor_Party );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Nameplate Distance Text Use Distance-based Colors", "可依距離臨界值設定不同顏色：超過遠距離使用「遠」色，介於遠與近之間使用「中」色，近距離內使用「近」色。" ) );
				if( mConfiguration.NameplateDistancesConfig.UseDistanceBasedColor_Party )
				{
					ImGui.Indent();
					ImGui.Checkbox( Loc.Localize( "Config Option: Nameplate Distance Text Use Nameplate Color Near", "近距離使用預設名牌顏色。" ) + $"###Distance Text Use nameplate color (near) (Nameplates - Party).", ref mConfiguration.NameplateDistancesConfig.NearRangeTextUseNameplateColor_Party );
					if( !mConfiguration.NameplateDistancesConfig.NearRangeTextUseNameplateColor_Party )
					{
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Color Near", "Distance text color (near)" ) + $"###DistanceTextColorPicker Near (Nameplates - Party)", ref mConfiguration.NameplateDistancesConfig.NearRangeTextColor_Party, ImGuiColorEditFlags.NoInputs );
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Glow Color Near", "Distance text glow color (near)" ) + $"###DistanceTextEdgeColorPicker Near (Nameplates - Party)", ref mConfiguration.NameplateDistancesConfig.NearRangeTextEdgeColor_Party, ImGuiColorEditFlags.NoInputs );
					}
					ImGui.Checkbox( Loc.Localize( "Config Option: Nameplate Distance Text Use Nameplate Color Mid", "中距離使用預設名牌顏色。" ) + $"###Distance Text Use nameplate color (mid) (Nameplates - Party).", ref mConfiguration.NameplateDistancesConfig.MidRangeTextUseNameplateColor_Party );
					if( !mConfiguration.NameplateDistancesConfig.MidRangeTextUseNameplateColor_Party )
					{
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Color Mid", "Distance text color (mid)" ) + $"###DistanceTextColorPicker Mid (Nameplates - Party)", ref mConfiguration.NameplateDistancesConfig.MidRangeTextColor_Party, ImGuiColorEditFlags.NoInputs );
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Glow Color Mid", "Distance text glow color (mid)" ) + $"###DistanceTextEdgeColorPicker Mid (Nameplates - Party)", ref mConfiguration.NameplateDistancesConfig.MidRangeTextEdgeColor_Party, ImGuiColorEditFlags.NoInputs );
					}
					ImGui.Checkbox( Loc.Localize( "Config Option: Nameplate Distance Text Use Nameplate Color Far", "遠距離使用預設名牌顏色。" ) + $"###Distance Text Use nameplate color (far) (Nameplates - Party).", ref mConfiguration.NameplateDistancesConfig.FarRangeTextUseNameplateColor_Party );
					if( !mConfiguration.NameplateDistancesConfig.FarRangeTextUseNameplateColor_Party )
					{
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Color Far", "Distancet text color (far)" ) + $"###DistanceTextColorPicker Far (Nameplates - Party)", ref mConfiguration.NameplateDistancesConfig.FarRangeTextColor_Party, ImGuiColorEditFlags.NoInputs );
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Glow Color Far", "Distance text glow color (far)" ) + $"###DistanceTextEdgeColorPicker Far (Nameplates - Party)", ref mConfiguration.NameplateDistancesConfig.FarRangeTextEdgeColor_Party, ImGuiColorEditFlags.NoInputs );
					}
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Near Range", "Distance \"near\" range ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( $"###DistanceNearRangeSlider (Nameplates - Party)", ref mConfiguration.NameplateDistancesConfig.NearThresholdDistance_Party_Yalms, 0.5f, -30f, 30f );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Far Range", "Distance \"far\" range ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( $"###DistanceFarRangeSlider (Nameplates - Party)", ref mConfiguration.NameplateDistancesConfig.FarThresholdDistance_Party_Yalms, 0.5f, -30f, 30f );
					ImGui.Unindent();
					ImGui.Spacing();
				}

				ImGui.Checkbox( Loc.Localize( "Config Option: Nameplate Distance Text Use Distance-based Colors (BNpc)", "戰鬥 NPC 依距離使用不同文字顏色。" ) + $"###Distance Text Use distance-based colors (Nameplates - BNpc).", ref mConfiguration.NameplateDistancesConfig.UseDistanceBasedColor_BNpc );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Nameplate Distance Text Use Distance-based Colors", "可依距離臨界值設定不同顏色：超過遠距離使用「遠」色，介於遠與近之間使用「中」色，近距離內使用「近」色。" ) );
				if( mConfiguration.NameplateDistancesConfig.UseDistanceBasedColor_BNpc )
				{
					ImGui.Indent();
					ImGui.Checkbox( Loc.Localize( "Config Option: Nameplate Distance Text Use Nameplate Color Near", "近距離使用預設名牌顏色。" ) + $"###Distance Text Use nameplate color (near) (Nameplates - BNpc).", ref mConfiguration.NameplateDistancesConfig.NearRangeTextUseNameplateColor_BNpc );
					if( !mConfiguration.NameplateDistancesConfig.NearRangeTextUseNameplateColor_BNpc )
					{
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Color Near", "Distance text color (near)" ) + $"###DistanceTextColorPicker Near (Nameplates - BNpc)", ref mConfiguration.NameplateDistancesConfig.NearRangeTextColor_BNpc, ImGuiColorEditFlags.NoInputs );
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Glow Color Near", "Distance text glow color (near)" ) + $"###DistanceTextEdgeColorPicker Near (Nameplates - BNpc)", ref mConfiguration.NameplateDistancesConfig.NearRangeTextEdgeColor_BNpc, ImGuiColorEditFlags.NoInputs );
					}
					ImGui.Checkbox( Loc.Localize( "Config Option: Nameplate Distance Text Use Nameplate Color Mid", "中距離使用預設名牌顏色。" ) + $"###Distance Text Use nameplate color (mid) (Nameplates - BNpc).", ref mConfiguration.NameplateDistancesConfig.MidRangeTextUseNameplateColor_BNpc );
					if( !mConfiguration.NameplateDistancesConfig.MidRangeTextUseNameplateColor_BNpc )
					{
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Color Mid", "Distance text color (mid)" ) + $"###DistanceTextColorPicker Mid (Nameplates - BNpc)", ref mConfiguration.NameplateDistancesConfig.MidRangeTextColor_BNpc, ImGuiColorEditFlags.NoInputs );
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Glow Color Mid", "Distance text glow color (mid)" ) + $"###DistanceTextEdgeColorPicker Mid (Nameplates - BNpc)", ref mConfiguration.NameplateDistancesConfig.MidRangeTextEdgeColor_BNpc, ImGuiColorEditFlags.NoInputs );
					}
					ImGui.Checkbox( Loc.Localize( "Config Option: Nameplate Distance Text Use Nameplate Color Far", "遠距離使用預設名牌顏色。" ) + $"###Distance Text Use nameplate color (far) (Nameplates - BNpc).", ref mConfiguration.NameplateDistancesConfig.FarRangeTextUseNameplateColor_BNpc );
					if( !mConfiguration.NameplateDistancesConfig.FarRangeTextUseNameplateColor_BNpc )
					{
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Color Far", "Distancet text color (far)" ) + $"###DistanceTextColorPicker Far (Nameplates - BNpc)", ref mConfiguration.NameplateDistancesConfig.FarRangeTextColor_BNpc, ImGuiColorEditFlags.NoInputs );
						ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Glow Color Far", "Distance text glow color (far)" ) + $"###DistanceTextEdgeColorPicker Far (Nameplates - BNpc)", ref mConfiguration.NameplateDistancesConfig.FarRangeTextEdgeColor_BNpc, ImGuiColorEditFlags.NoInputs );
					}
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Near Range", "Distance \"near\" range ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( $"###DistanceNearRangeSlider (Nameplates - BNpc)", ref mConfiguration.NameplateDistancesConfig.NearThresholdDistance_BNpc_Yalms, 0.5f, -30f, 30f );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Far Range", "Distance \"far\" range ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( $"###DistanceFarRangeSlider (Nameplates - BNpc)", ref mConfiguration.NameplateDistancesConfig.FarThresholdDistance_BNpc_Yalms, 0.5f, -30f, 30f );
					ImGui.Unindent();
				}
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Nameplate Fading", "淡出" ) + "###NameplateFadingHeader" ) )
			{
				ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Enable Fading (BNpc)", "戰鬥 NPC 啟用依距離淡出。" ), ref mConfiguration.NameplateDistancesConfig.EnableFading_BNpc );
				if( mConfiguration.NameplateDistancesConfig.EnableFading_BNpc )
				{
					ImGui.Indent();
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Inside Fade Distance", "Distance inside of the target to start fading ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Inside Fade Distance", "If you use the distance from center instead of distance from target ring, the inner fade settings will only ever have an effect if you configured a positive distance offset above." ) );
					ImGui.DragFloat( "###DistanceTextInnerFadeThresholdSlider_BNpc", ref mConfiguration.NameplateDistancesConfig.FadeoutThresholdInner_BNpc_Yalms, 0.5f, 1f, 50f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Inside Fade Interval", "Fade over ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextInnerFadeIntervalSlider_BNpc", ref mConfiguration.NameplateDistancesConfig.FadeoutIntervalInner_BNpc_Yalms, 0.5f, 0f, 50f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Outside Fade Distance", "Distance away from the target to start fading ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextOuterFadeThresholdSlider_BNpc", ref mConfiguration.NameplateDistancesConfig.FadeoutThresholdOuter_BNpc_Yalms, 0.5f, 1f, 100f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Outside Fade Interval", "Fade over ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextOuterFadeIntervalSlider_BNpc", ref mConfiguration.NameplateDistancesConfig.FadeoutIntervalOuter_BNpc_Yalms, 0.5f, 0f, 100f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Invert Fading", "Inverted fading." ) + "###InvertedFadingCheckbox_BNpc", ref mConfiguration.NameplateDistancesConfig.InvertFading_BNpc );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Invert Fading", "Instead of showing the distance text within the range defined above, show it only outside of the defined range instead.  The fadeout interval is added to the configured threshold distance when this option is used." ) );
					ImGui.Unindent();
				}

				ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Enable Fading (Party)", "小隊成員啟用依距離淡出。" ), ref mConfiguration.NameplateDistancesConfig.EnableFading_Party );
				if( mConfiguration.NameplateDistancesConfig.EnableFading_Party )
				{
					ImGui.Indent();
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Inside Fade Distance", "Distance inside of the target to start fading ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Inside Fade Distance", "If you use the distance from center instead of distance from target ring, the inner fade settings will only ever have an effect if you configured a positive distance offset above." ) );
					ImGui.DragFloat( "###DistanceTextInnerFadeThresholdSlider_Party", ref mConfiguration.NameplateDistancesConfig.FadeoutThresholdInner_Party_Yalms, 0.5f, 1f, 50f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Inside Fade Interval", "Fade over ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextInnerFadeIntervalSlider_Party", ref mConfiguration.NameplateDistancesConfig.FadeoutIntervalInner_Party_Yalms, 0.5f, 0f, 50f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Outside Fade Distance", "Distance away from the target to start fading ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextOuterFadeThresholdSlider_Party", ref mConfiguration.NameplateDistancesConfig.FadeoutThresholdOuter_Party_Yalms, 0.5f, 1f, 100f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Outside Fade Interval", "Fade over ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextOuterFadeIntervalSlider_Party", ref mConfiguration.NameplateDistancesConfig.FadeoutIntervalOuter_Party_Yalms, 0.5f, 0f, 100f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Invert Fading", "Inverted fading." ) + "###InvertedFadingCheckbox_Party", ref mConfiguration.NameplateDistancesConfig.InvertFading_Party );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Invert Fading", "Instead of showing the distance text within the range defined above, show it only outside of the defined range instead.  The fadeout interval is added to the configured threshold distance when this option is used." ) );
					ImGui.Unindent();
				}

				ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Enable Fading (Other)", "其他目標啟用依距離淡出。" ), ref mConfiguration.NameplateDistancesConfig.EnableFading_Other );
				if( mConfiguration.NameplateDistancesConfig.EnableFading_Other )
				{
					ImGui.Indent();
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Inside Fade Distance", "Distance inside of the target to start fading ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Inside Fade Distance", "If you use the distance from center instead of distance from target ring, the inner fade settings will only ever have an effect if you configured a positive distance offset above." ) );
					ImGui.DragFloat( "###DistanceTextInnerFadeThresholdSlider_Other", ref mConfiguration.NameplateDistancesConfig.FadeoutThresholdInner_Other_Yalms, 0.5f, 1f, 50f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Inside Fade Interval", "Fade over ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextInnerFadeIntervalSlider_Other", ref mConfiguration.NameplateDistancesConfig.FadeoutIntervalInner_Other_Yalms, 0.5f, 0f, 50f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Outside Fade Distance", "Distance away from the target to start fading ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextOuterFadeThresholdSlider_Other", ref mConfiguration.NameplateDistancesConfig.FadeoutThresholdOuter_Other_Yalms, 0.5f, 1f, 100f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Outside Fade Interval", "Fade over ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextOuterFadeIntervalSlider_Other", ref mConfiguration.NameplateDistancesConfig.FadeoutIntervalOuter_Other_Yalms, 0.5f, 0f, 100f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Invert Fading", "Inverted fading." ) + "###InvertedFadingCheckbox_Other", ref mConfiguration.NameplateDistancesConfig.InvertFading_Other );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Invert Fading", "Instead of showing the distance text within the range defined above, show it only outside of the defined range instead.  The fadeout interval is added to the configured threshold distance when this option is used." ) );
					ImGui.Unindent();
				}

				ImGui.TreePop();
			}
		}
	}

	private readonly Plugin mPlugin;
	private readonly PluginUI mUI;
	private readonly Configuration mConfiguration;
}
