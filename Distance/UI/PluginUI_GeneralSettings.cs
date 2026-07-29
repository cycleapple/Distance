using System;
using System.Numerics;
using System.Threading.Tasks;

using CheapLoc;

using Dalamud.Interface.Utility;

using Dalamud.Bindings.ImGui;

namespace Distance;

internal sealed class PluginUI_GeneralSettings : IDisposable
{
	internal PluginUI_GeneralSettings( Plugin plugin, PluginUI ui, Configuration configuration )
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
		float maxCategoryWidth = 0;
		foreach( GeneralSettingsCategory category in Enum.GetValues( typeof( GeneralSettingsCategory ) ) )
		{
			maxCategoryWidth = Math.Max( maxCategoryWidth, ImGui.CalcTextSize( category.GetTranslatedName() ).X );
		}
		maxCategoryWidth += ImGui.GetStyle().WindowPadding.X;
		float leftPaneWidth = maxCategoryWidth + ImGui.GetStyle().WindowPadding.X * 2 - ImGui.GetStyle().ItemSpacing.X / 2;
		float rightPaneWidth = contentRegionSize.X - leftPaneWidth - ImGui.GetStyle().ItemSpacing.X;
		if( ImGui.BeginChild( "###DistanceSettingsGeneralChildWindow_Select", new( leftPaneWidth, ImGui.GetContentRegionAvail().Y - buttonHeight ), true ) )
		{
			foreach( GeneralSettingsCategory category in Enum.GetValues( typeof( GeneralSettingsCategory ) ) )
			{
				if( ImGui.Selectable( category.GetTranslatedName() + $"###GeneralSettings{(int)category}Selectable", category == mSelectedGeneralSettingsCategory ) )
				{
					mSelectedGeneralSettingsCategory = category;
				}
			}
		}
		ImGui.EndChild();

		ImGui.SameLine();
		if( ImGui.BeginChild( "###DistanceSettingsGeneralChildWindow_Settings", new( rightPaneWidth, ImGui.GetContentRegionAvail().Y - buttonHeight ), true ) )
		{
			ImGui.PushID( "GeneralOptions" );
			try
			{
				DrawConfigOptions( mSelectedGeneralSettingsCategory );
			}
			finally
			{
				ImGui.PopID();
			}
		}
		ImGui.EndChild();
	}

	private void DrawConfigOptions( GeneralSettingsCategory category )
	{
		if( category == GeneralSettingsCategory.AggroWidget )
		{
			ImGui.Checkbox( Loc.Localize( "Config Option: Show Aggro Distance", "顯示敵人偵測到你之前的剩餘距離。" ) + "###Show aggro distance.", ref mConfiguration.ShowAggroDistance );
			ImGuiUtils.HelpMarker( Loc.Localize( "Help: Show Aggro Distance", "只有已知距離的主要首領會顯示，且進入戰鬥後便不再顯示。" ) );
			if( mConfiguration.ShowAggroDistance )
			{
				if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Aggro Widget Distance Rules", "距離規則" ) + $"###Aggro Widget Distance Rules Header." ) )
				{
					ImGui.Text( Loc.Localize( "Config Option: Target Type", "目標類型：" ) );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Applicable Target Type", "The type of target for which this widget will show distance.  \"Soft Target\" generally only matters for controller players and some two-handed keyboard players.  \"Field Mouseover\" is for when you mouseover an object in the world.  \"UI Mouseover\" is for when you mouseover the party list." ) );
					if( ImGui.BeginCombo( $"###AggroDistanceTypeDropdown", mConfiguration.AggroDistanceApplicableTargetType.GetTranslatedName() ) )
					{
						foreach( var item in PluginUI.TargetDropdownMenuItems )
						{
							if( ImGui.Selectable( item.GetTranslatedName(), mConfiguration.AggroDistanceApplicableTargetType == item ) )
							{
								mConfiguration.AggroDistanceApplicableTargetType = item;
							}
						}
						ImGui.EndCombo();
					}

					ImGui.TreePop();
				}

				if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Aggro Widget Appearance", "外觀" ) + $"###Aggro Widget Appearance Header." ) )
				{
					ImGui.Text( Loc.Localize( "Config Option: UI Attach Point", "介面附著位置：" ) );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: UI Attach Point", "This is the UI element to which you wish to attach this widget.  \"Automatic\" tries to infer the best choice based on the target type you've selected for this widget.  \"Screen Space\" does not attach to a specific UI element, but floats above everything.  The others should be self-explanatory.  Note: Attaching to the mouse cursor can look odd if you use the hardware cursor; switch to the software cursor in the game options if necessary." ) );
					ImGui.Combo( $"###AggroDistanceUIAttachTypeDropdown", ref mConfiguration.mAggroDistanceUIAttachType, PluginUI.UIAttachDropdownOptions, PluginUI.UIAttachDropdownOptions.Length );
					bool useScreenText = mConfiguration.AggroDistanceUIAttachType.GetGameAddonToUse( mConfiguration.AggroDistanceApplicableTargetType ) == GameAddonEnum.ScreenText;
					Vector2 sliderLimits = new( useScreenText ? 0 : -1000, useScreenText ? Math.Max( ImGuiHelpers.MainViewport.Size.X, ImGuiHelpers.MainViewport.Size.Y ) : 1000 );
					ImGui.Text( Loc.Localize( "Config Option: Aggro Distance Text Position", "敵意距離小工具位置（X、Y）：" ) );
					ImGuiUtils.HelpMarker( Loc.Localize( "Help: Distance Text Position", "This is an offset relative to the UI element if it is attached to one, or is an absolute position on the screen if not." ) );
					ImGui.DragFloat2( "###AggroDistanceTextPositionSlider", ref mConfiguration.AggroDistanceTextPosition, 1f, sliderLimits.X, sliderLimits.Y, "%g", ImGuiSliderFlags.AlwaysClamp );
					ImGui.Checkbox( Loc.Localize( "Config Option: Aggro Distance Text Use Heavy Font", "敵意距離小工具使用粗體字型。" ) + "###Aggro Distance font heavy.", ref mConfiguration.AggroDistanceFontHeavy );
					ImGui.Text( Loc.Localize( "Config Option: Aggro Distance Text Font Size", "敵意距離小工具字型大小：" ) );
					ImGui.SliderInt( "###AggroDistanceTextFontSizeSlider", ref mConfiguration.AggroDistanceFontSize, 6, 36 );
					ImGui.Text( Loc.Localize( "Config Option: Aggro Distance Text Alignment", "文字對齊：" ) );
					ImGui.SliderInt( "###AggroDistanceTextFontAlignmentSlider", ref mConfiguration.mAggroDistanceFontAlignment, 6, 8, "", ImGuiSliderFlags.NoInput );
					ImGui.Checkbox( Loc.Localize( "Config Option: Show Distance Units", "距離數值顯示單位。" ) + "###Show aggro distance units.", ref mConfiguration.ShowUnitsOnAggroDistance );
					ImGui.Text( Loc.Localize( "Config Option: Decimal Precision", "距離顯示的小數位數：" ) );
					ImGuiUtils.HelpMarker( String.Format( Loc.Localize( "Help: Aggro Distance Precision", "Aggro ranges are only accurate to within ~0.05{0}, so please be wary when using more than one decimal point of precision." ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.SliderInt( "###AggroDistancePrecisionSlider", ref mConfiguration.AggroDistanceDecimalPrecision, 0, 3 );
					ImGui.TreePop();
				}

				if( ImGui.TreeNode( Loc.Localize( "Config Section Header: Aggro Widget Colors", "顏色" ) + $"###Aggro Widget Colors Header." ) )
				{
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Aggro Distance Text Color", "Aggro widget text color" ) + "###AggroDistanceTextColorPicker", ref mConfiguration.AggroDistanceTextColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Aggro Distance Text Glow Color", "Aggro widget text glow color" ) + "###AggroDistanceTextEdgeColorPicker", ref mConfiguration.AggroDistanceTextEdgeColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Aggro Distance Text Color Caution", "Aggro widget text color (caution range)" ) + "###AggroDistanceCautionTextColorPicker", ref mConfiguration.AggroDistanceCautionTextColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Aggro Distance Text Glow Color Caution", "Aggro widget text glow color (caution range)" ) + "###AggroDistanceCautionTextEdgeColorPicker", ref mConfiguration.AggroDistanceCautionTextEdgeColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Aggro Distance Text Color Warning", "Aggro widget text color (warning range)" ) + "###AggroDistanceWarningTextColorPicker", ref mConfiguration.AggroDistanceWarningTextColor, ImGuiColorEditFlags.NoInputs );
					ImGui.ColorEdit4( Loc.Localize( "Config Option: Aggro Distance Text Glow Color Warning", "Aggro widget text glow color (warning range)" ) + "###AggroDistanceWarningTextEdgeColorPicker", ref mConfiguration.AggroDistanceWarningTextEdgeColor, ImGuiColorEditFlags.NoInputs );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Aggro Distance Caution Range", "Aggro distance \"caution\" range ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.SliderFloat( "###AggroDistanceCautionRangeSlider", ref mConfiguration.AggroCautionDistance_Yalms, 0, 30 );
					ImGui.Text( String.Format( Loc.Localize( "Config Option: Aggro Distance Warning Range", "Aggro distance \"warning\" range ({0}):" ), LocalizationHelpers.DistanceUnitShort ) );
					ImGui.SliderFloat( "###AggroDistanceWarningRangeSlider", ref mConfiguration.AggroWarningDistance_Yalms, 0, 30 );
					ImGui.TreePop();
				}
			}
		}
		else if( category == GeneralSettingsCategory.AggroArcs )
		{
			ImGui.PushID( "AggroArcOptions" );
			try
			{
				mUI.AggroArcsUI.DrawConfigOptions();
			}
			finally
			{
				ImGui.PopID();
			}
		}
		else if( category == GeneralSettingsCategory.AggroData )
		{
			ImGui.Checkbox( Loc.Localize( "Config Option: Auto Update Aggro Data", "啟動時嘗試自動取得最新敵意距離資料。" ) + "###Auto Update Aggro Data.", ref mConfiguration.AutoUpdateAggroData );
			if( ImGui.Button( Loc.Localize( "Button: Download Aggro Distances", "檢查敵意距離資料更新" ) + "###Download updated aggro distances." ) )
			{
				Task.Run( async () =>
				{
					var downloadedFile = await BNpcAggroInfoDownloader.DownloadUpdatedAggroDataAsync( mPlugin.AggroDataPath );
					if( downloadedFile != null ) BNpcAggroInfo.Init( Service.DataManager, downloadedFile );
				} );
			}
			if( BNpcAggroInfoDownloader.CurrentDownloadStatus != BNpcAggroInfoDownloader.DownloadStatus.None )
			{
				ImGui.Text( Loc.Localize( "Config Text: Download Status Indicator", "最近一次更新狀態：" ) + $"\r\n{BNpcAggroInfoDownloader.GetCurrentDownloadStatusMessage()}" );
			}
		}
		else if( category == GeneralSettingsCategory.Nameplates )
		{
			ImGui.PushID( "NameplateOptions" );
			try
			{
				mUI.NameplatesUI.DrawConfigOptions();
			}
			finally
			{
				ImGui.PopID();
			}
		}
		else if( category == GeneralSettingsCategory.Miscellaneous )
		{
			ImGui.Checkbox( Loc.Localize( "Config Option: Suppress Text Command Responses", "隱藏文字指令回應。" ) + "###Suppress text command responses.", ref mConfiguration.SuppressCommandLineResponses );
			ImGuiUtils.HelpMarker( Loc.Localize( "Help: Suppress Text Command Responses", "Selecting this prevents any text commands you use from printing responses to chat.  Responses to the help command will always be printed." ) );
		}
		else
		{
			ImGui.Text( Loc.Localize( "Config Text: No Category Selected", "未選擇分類" ) );
		}
	}

	private GeneralSettingsCategory mSelectedGeneralSettingsCategory = GeneralSettingsCategory.Nameplates;

	private readonly Plugin mPlugin;
	private readonly PluginUI mUI;
	private readonly Configuration mConfiguration;
}
