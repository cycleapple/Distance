using System;
using System.Numerics;

using CheapLoc;

using Dalamud.Interface;
using Dalamud.Interface.Utility;

using Dalamud.Bindings.ImGui;

namespace Distance;

internal sealed class PluginUI_CustomWidgets : IDisposable
{
	internal PluginUI_CustomWidgets( Plugin plugin, PluginUI ui, Configuration configuration )
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
		float maxCategoryWidth = ImGui.CalcTextSize( Loc.Localize( "Config Text: Custom Widget Placeholder", "沒有自訂小工具（按一下＋新增）" ) ).X;
		foreach( var config in mConfiguration.DistanceWidgetConfigs )
		{
			maxCategoryWidth = Math.Max( maxCategoryWidth, ImGui.CalcTextSize( config.DisplayedWidgetName ).X );
		}
		maxCategoryWidth += ImGui.GetStyle().WindowPadding.X;
		float leftPaneWidth = maxCategoryWidth + ImGui.GetStyle().WindowPadding.X * 2 - ImGui.GetStyle().ItemSpacing.X / 2;
		float rightPaneWidth = contentRegionSize.X - leftPaneWidth - ImGui.GetStyle().ItemSpacing.X;
		if( ImGui.BeginChild( "###DistanceSettingsCustomWidgetsChildWindow_Select_Outer", new( leftPaneWidth, ImGui.GetContentRegionAvail().Y - buttonHeight ), false ) )
		{
			if( ImGui.BeginChild( "###DistanceSettingsCustomWidgetsChildWindow_Select", new( leftPaneWidth, ImGui.GetContentRegionAvail().Y - buttonHeight ), true ) )
			{
				if( mConfiguration.DistanceWidgetConfigs.Count > 0 )
				{
					for( int i = 0; i < mConfiguration.DistanceWidgetConfigs.Count; ++i )
					{
						if( ImGui.Selectable( mConfiguration.DistanceWidgetConfigs[i].DisplayedWidgetName + $"###DistanceWidget{i}Selectable", i == mSelectedCustomWidget ) )
						{
							mSelectedCustomWidget = i;
							mWidgetIndexWantToDelete = -1;
						}
					}
				}
				else
				{
					ImGui.Text( Loc.Localize( "Config Text: Custom Widget Placeholder", "沒有自訂小工具（按一下＋新增）" ) );
					mSelectedCustomWidget = -1;
				}
			}
			ImGui.EndChild();
			ImGui.PushFont( UiBuilder.IconFont );
			try
			{
				if( ImGui.SmallButton( "\uF067" + "###AddWidgetButton" ) )
				{
					mConfiguration.DistanceWidgetConfigs.Add( new() );
					mSelectedCustomWidget = mConfiguration.DistanceWidgetConfigs.Count - 1;
				}
				ImGui.SameLine();
				if( ImGui.SmallButton( "\uF068" + "###RemoveWidgetButton" ) && mSelectedCustomWidget >= 0 )
				{
					mWidgetIndexWantToDelete = mSelectedCustomWidget;
				}
			}
			finally
			{
				ImGui.PopFont();
			}

			if( mWidgetIndexWantToDelete >= 0 && mWidgetIndexWantToDelete == mSelectedCustomWidget )
			{
				ImGui.SameLine();
				ImGui.Text( "/" );
				ImGui.SameLine();
				ImGui.PushStyleColor( ImGuiCol.Text, 0xee4444ff );
				if( ImGui.SmallButton( Loc.Localize( "Button: Yes", "是" ) + "###DeleteWidgetYesButton" ) )
				{
					DeleteWidget( mWidgetIndexWantToDelete );
					mSelectedCustomWidget = -1;
					mWidgetIndexWantToDelete = -1;
				}
				ImGui.PopStyleColor();
				ImGui.SameLine();
				if( ImGui.SmallButton( Loc.Localize( "Button: No", "否" ) + "###DeleteWidgetNoButton" ) )
				{
					mWidgetIndexWantToDelete = -1;
				}
			}
		}
		ImGui.EndChild();
		ImGui.SameLine();
		if( ImGui.BeginChild( "###DistanceSettingsCustomWidgetsChildWindow_Settings", new( rightPaneWidth, ImGui.GetContentRegionAvail().Y - buttonHeight ), true ) )
		{
			ImGui.PushID( "CustomWidgetOptions" );
			try
			{
				DrawConfigOptions( mSelectedCustomWidget );
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
		if( i < 0 || i >= mConfiguration.DistanceWidgetConfigs.Count )
		{
			ImGui.Text( Loc.Localize( "Config Text: No Widget Selected", "未選擇小工具" ) );
			return;
		}

		ImGui.PushID( i );
		try
		{
			var config = mConfiguration.DistanceWidgetConfigs[i];
		ImGui.Text( Loc.Localize( "Config Option: Widget Name", "小工具名稱：" ) );
			ImGui.SameLine();
			ImGui.InputTextWithHint( "###WidgetNameInputBox", config.ApplicableTargetType.GetTranslatedName(), ref config.WidgetName, 50 );
			ImGuiUtils.HelpMarker( Loc.Localize( "Help: Widget Name", "為小工具設定自訂名稱，以供部分文字指令使用。留空時標題會顯示目標類型，但無法透過名稱供文字指令使用。" ) );
		ImGui.Checkbox( Loc.Localize( "Config Option: Widget Enabled", "啟用" ) + "###WidgetEnabledCheckbox", ref config.Enabled );
		if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Widget Rules", "距離規則" ) + "###DistanceWidgetRulesHeader" ) )
			{
			ImGui.Text( Loc.Localize( "Config Option: Target Type", "目標類型：" ) );
				ImGuiUtils.HelpMarker( Loc.Localize("Help: Widget Applicable Target Type", "小工具要顯示距離的目標類型。「軟目標」主要供控制器或部分雙手鍵盤操作使用；「場景滑鼠指向目標」是場景中的物件，「介面滑鼠指向目標」則是小隊列表等介面目標。" ) );
				if( ImGui.BeginCombo( "###DistanceTypeDropdown", config.ApplicableTargetType.GetTranslatedName() ) )
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
				ImGui.Checkbox( Loc.Localize("Config Option: Widget Distance is to Ring", "顯示到目標環的距離，而非目標中心。" ) + "###DistanceIsToRing", ref config.DistanceIsToRing );
				ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Measurement Offset", "距離讀數偏移量（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Readout Offset", "顯示距離會從實際距離減去此數值，例如可用來顯示距離首領進入技能有效範圍還差多少。" ) );
				ImGui.DragFloat( "###DistanceOffsetSlider", ref config.DistanceOffset_Yalms, 0.1f, -30f, 30f );
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Widget Filters", "物件類型篩選" ) + "###DistanceWidgetFiltersHeader" ) )
			{
				config.Filters.DrawObjectKindOptions();
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize("Config Section Header: Distance Widget Condition", "狀態篩選" ) + "###DistanceWidgetConditionsHeader" ) )
			{
				config.Filters.DrawConditionOptions();
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Widget Classjobs", "職業篩選" ) + "###DistanceWidgetClassjobsHeader" ) )
			{
				config.Filters.DrawClassJobOptions();
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Widget Appearance", "外觀" ) + "###DistanceWidgetAppearanceHeader" ) )
			{
				ImGui.Text( Loc.Localize( "Config Option: UI Attach Point", "介面附著位置：" ) );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: UI Attach Point", "選擇小工具要附著的介面元素。「自動」會依目標類型選擇合適位置；「螢幕空間」不附著特定介面，會浮在所有內容上方。若附著滑鼠游標時顯示異常，可在遊戲設定改用軟體游標。" ) );
				ImGui.Combo( "###UIAttachTypeDropdown", ref config.mUIAttachType, PluginUI.UIAttachDropdownOptions, PluginUI.UIAttachDropdownOptions.Length );
				bool useScreenText = config.UIAttachType.GetGameAddonToUse( config.ApplicableTargetType ) == GameAddonEnum.ScreenText;
				Vector2 sliderLimits = new( useScreenText ? 0 : -1000, useScreenText ? Math.Max( ImGuiHelpers.MainViewport.Size.X, ImGuiHelpers.MainViewport.Size.Y ) : 1000 );
				ImGui.Text( Loc.Localize( "Config Option: Distance Text Position", "距離讀數位置（X、Y）：" ) );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Position", "附著介面元素時此數值為相對偏移；未附著時則為螢幕絕對位置。" ) );
				ImGui.DragFloat2( "###DistanceTextPositionSlider", ref config.TextPosition, 1f, sliderLimits.X, sliderLimits.Y, "%g", ImGuiSliderFlags.AlwaysClamp );
				ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Use Heavy Font", "距離文字使用粗體字型。" ) + "###DistanceFontHeavy", ref config.FontHeavy );
				ImGui.Text( Loc.Localize( "Config Option: Distance Text Font Size", "距離文字字型大小：" ) );
				ImGui.SliderInt( "###DistanceTextFontSizeSlider", ref config.FontSize, 6, 36 );
				ImGui.Text( Loc.Localize( "Config Option: Distance Text Font Alignment", "文字對齊：" ) );
				ImGui.SliderInt( "###DistanceTextFontAlignmentSlider", ref config.mFontAlignment, 6, 8, "", ImGuiSliderFlags.NoInput );
				ImGui.Checkbox( Loc.Localize( "Config Option: Show Distance Units", "距離數值顯示單位。" ) + "###ShowDistanceUnits", ref config.ShowUnits );
				ImGui.Checkbox( Loc.Localize( "Config Option: Show Distance Mode Indicator", "顯示距離計算模式標記。" ) + "###ShowDistanceTypeMarker", ref config.ShowDistanceModeMarker );
				ImGui.Checkbox( Loc.Localize( "Config Option: Allow Negative Distances", "允許顯示負距離。" ) + "###AllowNegativeDistances", ref config.AllowNegativeDistances );
				ImGui.Text( Loc.Localize( "Config Option: Decimal Precision", "距離顯示的小數位數：" ) );
				ImGui.SliderInt( "###DistancePrecisionSlider", ref config.DecimalPrecision, 0, 3 );
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Widget Colors", "顏色" ) + "###DistanceWidgetColorsHeader" ) )
			{
				ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Track Target Bar Color", "嘗試使用目標資訊的文字顏色。" ) + "###DistanceTextUseTargetBarColor", ref config.TrackTargetBarTextColor );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Track Target Bar Color", "若能判斷目標資訊或焦點目標的文字顏色，會優先使用；否則使用下方設定的顏色。" ) );
				ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Use Distance-based Colors", "依距離使用不同文字顏色。" ) + "###DistanceTextUseDistanceBasedColors", ref config.UseDistanceBasedColor );
				ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Use Distance-based Colors", "可依距離臨界值設定不同顏色：超過「遠」距離使用遠色，否則超過「近」距離時使用近色，其餘使用基本顏色。若啟用追蹤目標資訊顏色，將忽略此設定。" ) );
				ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Color", "距離文字顏色" ) + "###DistanceTextColorPicker", ref config.TextColor, ImGuiColorEditFlags.NoInputs );
				ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Glow Color", "距離文字發光顏色" ) + "###DistanceTextEdgeColorPicker", ref config.TextEdgeColor, ImGuiColorEditFlags.NoInputs );
				if( config.UseDistanceBasedColor )
				{
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Color Near", "距離文字顏色（近）" ) + "###DistanceTextColorPickerNear", ref config.NearThresholdTextColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize("Config Option: Distance Text Glow Color Near", "距離文字發光顏色（近）" ) + "###DistanceTextEdgeColorPickerNear", ref config.NearThresholdTextEdgeColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Color Far", "距離文字顏色（遠）" ) + "###DistanceTextColorPickerFar", ref config.FarThresholdTextColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Distance Text Glow Color Far", "距離文字發光顏色（遠）" ) + "###DistanceTextEdgeColorPickerFar", ref config.FarThresholdTextEdgeColor, ImGuiColorEditFlags.NoInputs );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Near Range", "「近」距離範圍（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceNearRangeSlider", ref config.NearThresholdDistance_Yalms, 0.5f, -30f, 30f );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Far Range", "「遠」距離範圍（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceFarRangeSlider", ref config.FarThresholdDistance_Yalms, 0.5f, -30f, 30f );
				}
				ImGui.TreePop();
			}

			if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Distance Widget Fading", "淡出" ) + "###DistanceWidgetFadingHeader" ) )
			{
				ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Enable Fading", "啟用依距離淡出。" ), ref config.EnableFading );
				if( config.EnableFading )
				{
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Inside Fade Distance", "目標內側開始淡出的距離（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Inside Fade Distance", "若使用到目標中心的距離而非目標環，只有上方設定正數距離偏移時，內側淡出才會生效。" ) );
					ImGui.DragFloat( "###DistanceTextInnerFadeThresholdSlider", ref config.FadeoutThresholdInner_Yalms, 0.5f, 1f, 50f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Inside Fade Interval", "淡出區間（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextInnerFadeIntervalSlider", ref config.FadeoutIntervalInner_Yalms, 0.5f, 0f, 50f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Outside Fade Distance", "離開目標後開始淡出的距離（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextOuterFadeThresholdSlider", ref config.FadeoutThresholdOuter_Yalms, 0.5f, 1f, 100f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Distance Text Outside Fade Interval", "淡出區間（{0}）：" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.DragFloat( "###DistanceTextOuterFadeIntervalSlider", ref config.FadeoutIntervalOuter_Yalms, 0.5f, 0f, 100f, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Checkbox( Loc.Localize( "Config Option: Distance Text Invert Fading", "反轉淡出範圍。" ) + "###InvertedFadingCheckbox", ref config.InvertFading );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Invert Fading", "改為只在上述範圍外顯示距離文字；啟用時淡出區間會加到所設定的臨界距離。" ) );
				}

				ImGui.TreePop();
			}
		}
		finally
		{
			ImGui.PopID();
		}
	}

	internal void DeleteWidget( int index )
	{
		if( index >= 0 && index < mConfiguration.DistanceWidgetConfigs.Count )
		{
			// Hide the last node since we've reduced count by one, making the highest node effectively
			// dormant.  The visibilities for all lower nodes will be updated on the next frame.
			PluginUI.HideTextNode( PluginUI.mDistanceNodeIDBase + (uint)mConfiguration.DistanceWidgetConfigs.Count - 1 );
			mConfiguration.DistanceWidgetConfigs.RemoveAt( index );
		}
	}

	private int mSelectedCustomWidget = -1;
	private int mWidgetIndexWantToDelete = -1;

	private readonly Plugin mPlugin;
	private readonly PluginUI mUI;
	private readonly Configuration mConfiguration;
}
