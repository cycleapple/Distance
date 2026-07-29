using System;
using System.Numerics;

using CheapLoc;

using Dalamud.Bindings.ImGui;

namespace Distance;

internal sealed class PluginUI_AggroArcs : IDisposable
{
	internal PluginUI_AggroArcs( Plugin plugin, PluginUI ui, Configuration configuration )
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
		ImGui.Checkbox( Loc.Localize( "Config Option: Show Aggro Arc", "顯示表示敵意偵測範圍的弧線。" ) + "###Show aggro arc.", ref mConfiguration.DrawAggroArc );
		ImGuiUtils.HelpMarker( Loc.Localize( "Help: Show Aggro Arc", "此弧線以視覺方式呈現敵意距離小工具的讀數；顏色與目標類型會沿用敵意距離小工具的設定。" ) );
		if( mConfiguration.DrawAggroArc )
		{
			ImGui.Text( Loc.Localize( "Config Option: Aggro Arc Length", "敵意弧線長度（度）：" ) );
			ImGui.SliderInt( "###AggroArcLengthSlider", ref mConfiguration.AggroArcLength_Deg, 0, 15 );
		}
	}

	internal void DrawOnOverlay()
	{
		if( mConfiguration.DrawAggroArc && mPlugin.ShouldDrawAggroDistanceInfo() )
		{
			DrawAggroDistanceArc();
		}
	}

	private void DrawAggroDistanceArc()
	{
		var distanceInfo = mPlugin.GetDistanceInfo( mConfiguration.AggroDistanceApplicableTargetType );
		if( !distanceInfo.IsValid ) return;

		//***** TODO: Maybe grab the alpha off of the focus target addon when aggro node is attached to focus target bar to make things make sense.
		Vector4 color = mConfiguration.AggroDistanceTextColor;
		Vector4 edgeColor = mConfiguration.AggroDistanceTextEdgeColor;
		if( distanceInfo.DistanceFromTargetAggro_Yalms < mConfiguration.AggroWarningDistance_Yalms )
		{
			color = mConfiguration.AggroDistanceWarningTextColor;
			edgeColor = mConfiguration.AggroDistanceWarningTextEdgeColor;
		}
		else if( distanceInfo.DistanceFromTargetAggro_Yalms < mConfiguration.AggroCautionDistance_Yalms )
		{
			color = mConfiguration.AggroDistanceCautionTextColor;
			edgeColor = mConfiguration.AggroDistanceCautionTextEdgeColor;
		}

		ArcUtils.DrawArc_ScreenSpace(
			distanceInfo.TargetPosition,
			distanceInfo.PlayerPosition,
			distanceInfo.AggroRange_Yalms + distanceInfo.TargetRadius_Yalms,
			mConfiguration.AggroArcLength_Deg,
			false,
			true,
			color,
			edgeColor );
	}

	private readonly Plugin mPlugin;
	private readonly PluginUI mUI;
	private readonly Configuration mConfiguration;
}
