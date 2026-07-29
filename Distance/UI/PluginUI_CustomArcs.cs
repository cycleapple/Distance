using System;
using System.Linq;
using System.Numerics;

using CheapLoc;

using Dalamud.Game.ClientState.Conditions;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Interface;

using FFXIVClientStructs.FFXIV.Client.Graphics.Scene;

using Dalamud.Bindings.ImGui;

namespace Distance;

internal sealed class PluginUI_CustomArcs : IDisposable
{
	internal PluginUI_CustomArcs( Plugin plugin, PluginUI ui, Configuration configuration )
	{
		mPlugin = plugin;
		mUI = ui;
		mConfiguration = configuration;
	}

	public void Dispose()
	{
	}

	internal void DrawSettingsTab()
	{
		float buttonHeight = ImGui.GetFrameHeightWithSpacing();
		Vector2 contentRegionSize = ImGui.GetWindowContentRegionMax() - ImGui.GetWindowContentRegionMin();
		float maxCategoryWidth = ImGui.CalcTextSize( Loc.Localize( "Config Text: Custom Arc Placeholder", "沒有自訂弧線（按一下＋新增）" ) ).X;
		foreach( var config in mConfiguration.DistanceArcConfigs )
		{
			maxCategoryWidth = Math.Max( maxCategoryWidth, ImGui.CalcTextSize( config.DisplayedArcName ).X );
		}
		maxCategoryWidth += ImGui.GetStyle().WindowPadding.X;
		float leftPaneWidth = maxCategoryWidth + ImGui.GetStyle().WindowPadding.X * 2 - ImGui.GetStyle().ItemSpacing.X / 2;
		float rightPaneWidth = contentRegionSize.X - leftPaneWidth - ImGui.GetStyle().ItemSpacing.X;
		if( ImGui.BeginChild( "###DistanceSettingsCustomArcsChildWindow_Select_Outer", new( leftPaneWidth, ImGui.GetContentRegionAvail().Y - buttonHeight ), false ) )
		{
			if( ImGui.BeginChild( "###DistanceSettingsCustomArcsChildWindow_Select", new( leftPaneWidth, ImGui.GetContentRegionAvail().Y - buttonHeight ), true ) )
			{
				if( mConfiguration.DistanceArcConfigs.Count > 0 )
				{
					for( int i = 0; i < mConfiguration.DistanceArcConfigs.Count; ++i )
					{
						if( ImGui.Selectable( mConfiguration.DistanceArcConfigs[i].DisplayedArcName + $"###DistanceArc{i}Selectable", i == mSelectedCustomArc ) )
						{
							mSelectedCustomArc = i;
							mArcIndexWantToDelete = -1;
						}
					}
				}
				else
				{
					ImGui.Text( Loc.Localize( "Config Text: Custom Arc Placeholder", "沒有自訂弧線（按一下＋新增）" ) );
					mSelectedCustomArc = -1;
				}
			}
			ImGui.EndChild();
			ImGui.PushFont( UiBuilder.IconFont );
			try
			{
				if( ImGui.SmallButton( "\uF067" + "###AddArcButton" ) )
				{
					mConfiguration.DistanceArcConfigs.Add( new() );
					mSelectedCustomArc = mConfiguration.DistanceArcConfigs.Count - 1;
				}
				ImGui.SameLine();
				if( ImGui.SmallButton( "\uF068" + "###RemoveArcButton" ) && mSelectedCustomArc >= 0 )
				{
					mArcIndexWantToDelete = mSelectedCustomArc;
				}
			}
			finally
			{
				ImGui.PopFont();
			}

			if( mArcIndexWantToDelete >= 0 && mArcIndexWantToDelete == mSelectedCustomArc )
			{
				ImGui.SameLine();
				ImGui.Text( "/" );
				ImGui.SameLine();
				ImGui.PushStyleColor( ImGuiCol.Text, 0xee4444ff );
				if( ImGui.SmallButton( Loc.Localize( "Button: Yes", "是" ) + "###DeleteArcYesButton" ) )
				{
					DeleteArc( mArcIndexWantToDelete );
					mSelectedCustomArc = -1;
					mArcIndexWantToDelete = -1;
				}
				ImGui.PopStyleColor();
				ImGui.SameLine();
				if( ImGui.SmallButton( Loc.Localize( "Button: No", "否" ) + "###DeleteArcNoButton" ) )
				{
					mArcIndexWantToDelete = -1;
				}
			}
		}
		ImGui.EndChild();
		ImGui.SameLine();
		if( ImGui.BeginChild( "###DistanceSettingsCustomArcsChildWindow_Settings", new( rightPaneWidth, ImGui.GetContentRegionAvail().Y - buttonHeight ), true ) )
		{
			ImGui.PushID( "CustomArcOptions" );
			try
			{
				DrawConfigOptions( mSelectedCustomArc );
			}
			finally
			{
				ImGui.PopID();
			}
		}
		ImGui.EndChild();
	}

	private void DrawConfigOptions( int i )
	{
		if( i < 0 || i >= mConfiguration.DistanceArcConfigs.Count )
		{
			ImGui.Text( Loc.Localize( "Config Text: No Arc Selected", "未選擇弧線" ) );
			return;
		}

		ImGui.PushID( i );
		try
		{
			var config = mConfiguration.DistanceArcConfigs[i];
			string defaultName = config.ApplicableTargetCategory == TargetCategory.Targets ? config.ApplicableTargetType.GetTranslatedName() : config.ApplicableTargetCategory.GetTranslatedName();
			ImGui.Text( Loc.Localize( "Config Option: Arc Name", "弧線名稱：" ) );
			ImGui.SameLine();
			string nameHint = config.ApplicableTargetCategory == TargetCategory.Targets ? config.ApplicableTargetType.GetTranslatedName() : config.ApplicableTargetCategory.GetTranslatedName();
			ImGui.InputTextWithHint( "###ArcNameInputBox", nameHint, ref config.ArcName, 50 );
			ImGuiUtils.HelpMarker( Loc.Localize( "Help: Arc Name", "為弧線設定自訂名稱，以供部分文字指令使用。留空時標題會顯示目標類型，但無法透過名稱供文字指令使用。" ) );
			ImGui.Checkbox( Loc.Localize( "Config Option: Arc Enabled", "啟用" ) + "###Arc Enabled Checkbox", ref config.Enabled );
			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Arc Rules", "距離規則" ) + "###DistanceArcRulesHeader" ) )
			{
				ImGui.Text( Loc.Localize( "Config Option: Object Type", "物件類型：" ) );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Applicable Object Type", "選擇此弧線要顯示距離的物件基本分類。" ) );
				if( ImGui.BeginCombo( "###ArcObjectCategoryDropdown", config.ApplicableTargetCategory.GetTranslatedName() ) )
				{
					foreach( var item in Enum.GetValues<TargetCategory>() )
					{
						if( ImGui.Selectable( item.GetTranslatedName(), config.ApplicableTargetCategory == item ) )
						{
							config.ApplicableTargetCategory = item;
						}
					}
					ImGui.EndCombo();
				}

				if( config.ApplicableTargetCategory == TargetCategory.Targets )
				{
					ImGui.Text( Loc.Localize( "Config Option: Target Type", "目標類型：" ) );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Arc Applicable Target Type", "弧線要顯示距離的目標類型。「軟目標」主要供控制器或部分雙手鍵盤操作使用；「場景滑鼠指向目標」是場景中的物件，「介面滑鼠指向目標」則是小隊列表等介面目標。" ) );
					if( ImGui.BeginCombo( "###ArcTargetTypeDropdown", config.ApplicableTargetType.GetTranslatedName() ) )
					{
						foreach( var item in PluginUI.TargetDropdownMenuItems )
						{
							if( ImGui.Selectable( item.GetTranslatedName(), config.ApplicableTargetType == item ) )
							{
								config.ApplicableTargetType = item;
							}
						}
						ImGui.EndCombo();
					}
				}
				else if( config.ApplicableTargetCategory == TargetCategory.Self )
				{
					ImGui.Text( Loc.Localize( "Config Option: Arc Self Azimuth", "弧線中心方位角：" ) );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Arc Self Azimuth", "弧線相對角色的顯示方向：0 為前方、90 為右方、180 為後方、270 為左方。" ) );
					ImGui.DragInt( "###ArcSelfAzimuthSlider", ref config.SelfTargetedArcAzimuth_Deg, 1f, 0, 359, "%d", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Checkbox( Loc.Localize( "Config Option: Camera-relative Self Arc", "Camera Relative" ) + "###ArcSelfCameraRelativeCheckbox", ref config.SelfTargetedArcCameraRelative );
				}
				else if( config.ApplicableTargetCategory == TargetCategory.AllEnemies )
				{
					ImGui.Checkbox( Loc.Localize( "Config Option: All Enemies Aggressive", "顯示已產生敵意的敵人" ) + "###ArcAllEnemiesAggressiveCheckbox", ref config.AllEnemiesShowAggressive );
					ImGui.Checkbox( Loc.Localize( "Config Option: All Enemies Unaggressive", "顯示未產生敵意的敵人" ) + "###ArcAllEnemiesUnaggressiveCheckbox", ref config.AllEnemiesShowUnaggressive );
					ImGui.Checkbox( Loc.Localize( "Config Option: Show Dead Objects", "Show Dead" ) + "###ShowDeadObjectsCheckbox", ref config.ShowDeadObjects );
				}
				else if( config.ApplicableTargetCategory == TargetCategory.AllPlayers )
				{
					ImGui.Checkbox( Loc.Localize( "Config Option: All Players Party", "顯示小隊成員" ) + "###ArcAllPlayersPartyCheckbox", ref config.AllPlayersShowParty );
					ImGui.Checkbox( Loc.Localize( "Config Option: All Players Alliance", "顯示團隊成員" ) + "###ArcAllPlayersAllianceCheckbox", ref config.AllPlayersShowAlliance );
					ImGui.Checkbox( Loc.Localize( "Config Option: All Players Other", "顯示其他玩家" ) + "###ArcAllPlayersOtherCheckbox", ref config.AllPlayersShowOthers );
					ImGui.Checkbox( Loc.Localize( "Config Option: Show Dead Objects", "Show Dead" ) + "###ShowDeadObjectsCheckbox", ref config.ShowDeadObjects );
				}

				ImGui.Checkbox( Loc.Localize( "Config Option: Distance is to Ring", "使用到目標環的距離，而非目標中心。" ) + "###ArcDistanceIsToRingCheckbox", ref config.DistanceIsToRing );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance is to Ring", "除非弧線以自己為中心，通常建議勾選；實際仍依用途調整。" ) );
				ImGui.Text( String.Format( Loc.Localize( "Config Option: Arc Radius", "弧線半徑（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
				ImGuiUtils.HelpMarker( String.Format( Loc.Localize( "Help: Arc Radius", "此距離依上方設定，以物件中心或目標環為基準。近戰最大有效距離約為 3.5{0}。" ), LocalizationHelpers.DistanceUnitShort ) );
				ImGui.DragFloat( "###ArcDistanceSlider", ref config.ArcRadius_Yalms, 0.1f, -30f, 30f );
				ImGui.TreePop();
			}

			if( config.ApplicableTargetCategory is not TargetCategory.Self and not TargetCategory.AllEnemies and not TargetCategory.AllPlayers &&
				ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Arc Filters", "物件類型篩選" ) + $"###DistanceArcFiltersHeader" ) )
			{
				config.Filters.DrawObjectKindOptions();
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize("Config Section Header: Distance Arc Condition", "狀態篩選" ) + "###DistanceArcConditionsHeader" ) )
			{
				config.Filters.DrawConditionOptions();
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Arc Classjobs", "職業篩選" ) + "###DistanceArcClassjobsHeader" ) )
			{
				config.Filters.DrawClassJobOptions();
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Arc Appearance", "外觀" ) + "###DistanceArcAppearanceHeader" ) )
			{
				ImGui.Checkbox( Loc.Localize( "Config Option: Show Arc Pip", "顯示方向標記。" ) + "###ArcShowPip", ref config.ShowPip );
				ImGui.Text( Loc.Localize( "Config Option: Arc Length", "弧線長度：" ) );
				if( ImGui.RadioButton( Loc.Localize( "Unit String: Degrees Short Lower", "deg" ) + "###ButtonUnitsDegrees", !config.ArcLengthIsYalms ) ) config.ArcLengthIsYalms = false;
				ImGui.SameLine();
				if( ImGui.RadioButton( LocalizationHelpers.DistanceUnitLowerPlural + "###ButtonUnitsYalms", config.ArcLengthIsYalms ) ) config.ArcLengthIsYalms = true;
				ImGui.SliderFloat( "###ArcLengthSlider", ref config.ArcLength, 0, 30 );

				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Arc Colors", "顏色" ) + "###Distance Arc Colors Header" ) )
			{
				if( config.ApplicableTargetCategory != TargetCategory.Self )
				{
					ImGui.Checkbox( Loc.Localize( "Config Option: Distance Arc Use Distance-based Colors", "依距離使用不同弧線顏色。" ) + "###DistanceArcUseDistanceBasedColors", ref config.UseDistanceBasedColor );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Arc Use Distance-based Colors", "可依距離臨界值設定不同顏色：超過「遠」距離使用遠色，否則超過「近」距離時使用近色，其餘使用下方基本顏色。" ) );
				}
				ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Arc Color", "弧線顏色" ) + "###ArcColorPicker", ref config.Color, ImGuiColorEditFlags.NoInputs );
				ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Arc Glow Color", "弧線發光顏色" ) + "###ArcEdgeColorPicker", ref config.EdgeColor, ImGuiColorEditFlags.NoInputs );
				if( config.UseDistanceBasedColor && config.ApplicableTargetCategory != TargetCategory.Self )
				{
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Arc Color Inside Near", "弧線顏色（內側近）" ) + "###ArcColorPickerInnerNear", ref config.InnerNearThresholdColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Arc Glow Color Inside Near", "弧線發光顏色（內側近）" ) + "###ArcEdgeColorPickerInnerNear", ref config.InnerNearThresholdEdgeColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Arc Color Inside Far", "弧線顏色（內側遠）" ) + "###ArcColorPickerInnerFar", ref config.InnerFarThresholdColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Arc Glow Color Inside Far", "弧線發光顏色（內側遠）" ) + "###ArcEdgeColorPickerInnerFar", ref config.InnerFarThresholdEdgeColor, ImGuiColorEditFlags.NoInputs );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Arc Inside Near Range", "「內側近」距離範圍（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###ArcDistanceNearInsideRangeSlider", ref config.InnerNearThresholdDistance_Yalms, 0.5f, 0f, 30f );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Arc Inside Far Range", "「內側遠」距離範圍（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###ArcDistanceFarInsideRangeSlider", ref config.InnerFarThresholdDistance_Yalms, 0.5f, 0f, 30f );

					ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Arc Color Outside Near", "弧線顏色（外側近）" ) + "###ArcColorPickerOuterNear", ref config.OuterNearThresholdColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Arc Glow Color Outside Near", "弧線發光顏色（外側近）" ) + "###ArcEdgeColorPickerOuterNear", ref config.OuterNearThresholdEdgeColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Arc Color Outside Far", "弧線顏色（外側遠）" ) + "###ArcColorPickerOuterFar", ref config.OuterFarThresholdColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Arc Glow Color Outside Far", "弧線發光顏色（外側遠）" ) + "###ArcEdgeColorPickerOuterFar", ref config.OuterFarThresholdEdgeColor, ImGuiColorEditFlags.NoInputs );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Arc Outside Near Range", "「外側近」距離範圍（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###ArcDistanceNearOutsideRangeSlider", ref config.OuterNearThresholdDistance_Yalms, 0.5f, 0f, 30f );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Arc Outside Far Range", "「外側遠」距離範圍（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###ArcDistanceFarOutsideRangeSlider", ref config.OuterFarThresholdDistance_Yalms, 0.5f, 0f, 30f );
				}
				ImGui.TreePop();
			}

			if( config.ApplicableTargetCategory != TargetCategory.Self &&
				ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Arc Fading", "淡出" ) + "###DistanceArcFadingHeader" ) )
			{
				ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Arc Inside Fade Distance", "弧線內側開始淡出的距離（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
				ImGui.DragFloat( "###ArcDistanceInnerFadeThresholdSlider", ref config.FadeoutThresholdInner_Yalms, 0.5f, 1f, 30f, "%g", ImGuiSliderFlags.AlwaysClamp );
				ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Arc Inside Fade Interval", "淡出區間（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
				ImGui.DragFloat( "###ArcDistanceInnerFadeIntervalSlider", ref config.FadeoutIntervalInner_Yalms, 0.5f, 0f, 30f, "%g", ImGuiSliderFlags.AlwaysClamp );
				ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Arc Outside Fade Distance", "弧線外側開始淡出的距離（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
				ImGui.DragFloat( "###ArcDistanceOuterFadeThresholdSlider", ref config.FadeoutThresholdOuter_Yalms, 0.5f, 1f, 30f, "%g", ImGuiSliderFlags.AlwaysClamp );
				ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Arc Outside Fade Interval", "淡出區間（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
				ImGui.DragFloat( "###ArcDistanceOuterFadeIntervalSlider", ref config.FadeoutIntervalOuter_Yalms, 0.5f, 0f, 30f, "%g", ImGuiSliderFlags.AlwaysClamp );
				ImGui.Checkbox( Loc.Localize( "Config Option: Distance Arc Invert Fading", "反轉淡出範圍。" ) + "###InvertedFadingCheckbox", ref config.InvertFading );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Arc Invert Fading", "改為只在上述範圍外顯示弧線；啟用時淡出區間會加到所設定的臨界距離。" ) );

				ImGui.TreePop();
			}
		}
		finally
		{
			ImGui.PopID();
		}
	}

	internal void DeleteArc( int index )
	{
		if( index >= 0 && index < mConfiguration.DistanceArcConfigs.Count )
		{
			mConfiguration.DistanceArcConfigs.RemoveAt( index );
		}
	}

	internal void DrawOnOverlay()
	{
		if( Service.ClientState.IsPvP ) return;
		if( Service.ClientState.LocalPlayer == null ) return;

		foreach( var config in mConfiguration.DistanceArcConfigs )
		{
			if( !config.Enabled ) continue;
			if( !config.Filters.ShowDistanceForConditions( Service.Condition[ConditionFlag.InCombat], Service.Condition[ConditionFlag.BoundByDuty] ) ) continue;
			if( !config.Filters.ShowDistanceForClassJob( Service.ClientState.LocalPlayer?.ClassJob.RowId ?? 0 ) ) continue;
			//	Note that we cannot evaluate the object type filters here, because they may behave differently by target category.

			if( config.ApplicableTargetCategory == TargetCategory.Targets ) DrawCustomArc_Targets( config );
			else if( config.ApplicableTargetCategory == TargetCategory.Self ) DrawCustomArc_Self( config );
			else if( config.ApplicableTargetCategory == TargetCategory.AllEnemies ) DrawCustomArc_AllEnemies( config );
			else if( config.ApplicableTargetCategory == TargetCategory.AllPlayers ) DrawCustomArc_AllPlayers( config );
		}
	}
	private void DrawCustomArc_Targets( DistanceArcConfig config )
	{
		var distanceInfo = mPlugin.GetDistanceInfo( config.ApplicableTargetType );
		if( !distanceInfo.IsValid ) return;
		if( !config.Filters.ShowDistanceForObjectKind( distanceInfo.TargetKind ) ) return;

		float trueArcRadius_Yalms = config.ArcRadius_Yalms + ( config.DistanceIsToRing ? distanceInfo.TargetRadius_Yalms : 0 );
		float distanceFromArc_Yalms = distanceInfo.DistanceFromTarget_Yalms - trueArcRadius_Yalms;

		if( !config.WithinDisplayRangeOfArc( distanceFromArc_Yalms ) ) return;

		var colors = config.GetColors( distanceFromArc_Yalms );

		ArcUtils.DrawArc_ScreenSpace(
			distanceInfo.TargetPosition,
			distanceInfo.PlayerPosition,
			trueArcRadius_Yalms,
			config.ArcLength,
			config.ArcLengthIsYalms,
			config.ShowPip,
			colors.Item1,
			colors.Item2 );
	}

	private unsafe void DrawCustomArc_Self( DistanceArcConfig config )
	{
		double playerRelativeArcAngle_Rad = config.SelfTargetedArcAzimuth_Deg * Math.PI / 180.0;
		double absoluteArcAngle_Rad = -Service.ClientState.LocalPlayer.Rotation - Math.PI / 2.0 + playerRelativeArcAngle_Rad;

		bool cameraValid = CameraManager.Instance() != null && CameraManager.Instance()->CurrentCamera != null;
		if( config.SelfTargetedArcCameraRelative && cameraValid )
		{
			Vector3 cameraDirection = CameraManager.Instance()->CurrentCamera->LookAtVector - CameraManager.Instance()->CurrentCamera->Object.Position;
			double cameraAngle_Rad = -Math.Atan2( cameraDirection.Z, cameraDirection.X ) + Math.PI / 2.0;
			double cameraOffset_Rad = Service.ClientState.LocalPlayer.Rotation - cameraAngle_Rad;
			absoluteArcAngle_Rad += cameraOffset_Rad;
		}

		MathUtils.Wrap( ref absoluteArcAngle_Rad, -Math.PI, Math.PI );

		float trueArcRadius_Yalms = config.ArcRadius_Yalms + ( config.DistanceIsToRing ? 0.5f : 0f );
		Vector3 arcMidpoint = new()
		{
			X = (float)Math.Cos( absoluteArcAngle_Rad ) * trueArcRadius_Yalms,
			Y = 0f,
			Z = (float)Math.Sin( absoluteArcAngle_Rad ) * trueArcRadius_Yalms,
		};
		arcMidpoint += Service.ClientState.LocalPlayer.Position;

		ArcUtils.DrawArc_ScreenSpace(
			Service.ClientState.LocalPlayer.Position,
			arcMidpoint,
			trueArcRadius_Yalms,
			config.ArcLength,
			config.ArcLengthIsYalms,
			config.ShowPip,
			config.Color,
			config.EdgeColor );
	}

	private void DrawCustomArc_AllEnemies( DistanceArcConfig config )
	{
		var relevantBNpcs = Service.ObjectTable.Where( x =>
										x != null &&
										x.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.BattleNpc &&
										x.SubKind == (byte)BattleNpcSubKind.Enemy &&
										x.IsTargetable &&
										( !x.IsDead || config.ShowDeadObjects ) &&
										x.Position.DistanceTo_XZ( Service.ClientState.LocalPlayer.Position ) < 50f &&
										( x.IsAggressive() && config.AllEnemiesShowAggressive || !x.IsAggressive() && config.AllEnemiesShowUnaggressive )
										);

		foreach( var bnpc in relevantBNpcs )
		{
			float bnpcDistance_Yalms = bnpc.Position.DistanceTo_XZ( Service.ClientState.LocalPlayer.Position );
			float trueArcRadius_Yalms = config.ArcRadius_Yalms + ( config.DistanceIsToRing ? bnpc.HitboxRadius : 0 );
			float distanceFromArc_Yalms = bnpcDistance_Yalms - trueArcRadius_Yalms;

			if( !config.WithinDisplayRangeOfArc( distanceFromArc_Yalms ) ) continue;

			var colors = config.GetColors( distanceFromArc_Yalms );

			ArcUtils.DrawArc_ScreenSpace(
				bnpc.Position,
				Service.ClientState.LocalPlayer.Position,
				trueArcRadius_Yalms,
				config.ArcLength,
				config.ArcLengthIsYalms,
				config.ShowPip,
				colors.Item1,
				colors.Item2 );
		}
	}

	private void DrawCustomArc_AllPlayers( DistanceArcConfig config )
	{
		var relevantBNpcs = Service.ObjectTable.Where( x =>
										x != null &&
										x.ObjectKind == Dalamud.Game.ClientState.Objects.Enums.ObjectKind.Player && (
											x.IsPartyMember() && config.AllPlayersShowParty ||
											x.IsAllianceMember() && config.AllPlayersShowAlliance ||
											!x.IsPartyMember() && !x.IsAllianceMember() && config.AllPlayersShowOthers ) &&
										x.IsTargetable &&
										( !x.IsDead || config.ShowDeadObjects ) &&
										x.Position.DistanceTo_XZ( Service.ClientState.LocalPlayer.Position ) < 50f
										);

		foreach( var bnpc in relevantBNpcs )
		{
			float bnpcDistance_Yalms = bnpc.Position.DistanceTo_XZ( Service.ClientState.LocalPlayer.Position );
			float trueArcRadius_Yalms = config.ArcRadius_Yalms + ( config.DistanceIsToRing ? bnpc.HitboxRadius : 0 );
			float distanceFromArc_Yalms = bnpcDistance_Yalms - trueArcRadius_Yalms;

			if( !config.WithinDisplayRangeOfArc( distanceFromArc_Yalms ) ) continue;

			var colors = config.GetColors( distanceFromArc_Yalms );

			ArcUtils.DrawArc_ScreenSpace(
				bnpc.Position,
				Service.ClientState.LocalPlayer.Position,
				trueArcRadius_Yalms,
				config.ArcLength,
				config.ArcLengthIsYalms,
				config.ShowPip,
				colors.Item1,
				colors.Item2 );
		}
	}

	private int mSelectedCustomArc = -1;
	private int mArcIndexWantToDelete = -1;

	private readonly Plugin mPlugin;
	private readonly PluginUI mUI;
	private readonly Configuration mConfiguration;
}
