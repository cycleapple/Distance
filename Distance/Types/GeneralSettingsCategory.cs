using CheapLoc;

namespace Distance;

internal enum GeneralSettingsCategory : int
{
	AggroWidget,
	AggroArcs,
	AggroData,
	Nameplates,
	Miscellaneous,
}

internal static class GeneralSettingsCategoryExtensions
{
	internal static string GetTranslatedName( this GeneralSettingsCategory category )
	{
		return category switch
		{
			GeneralSettingsCategory.AggroWidget => Loc.Localize( "Config Category: Aggro Widget", "敵意距離小工具" ),
			GeneralSettingsCategory.AggroArcs => Loc.Localize( "Config Category: Aggro Arcs", "敵意距離弧線" ),
			GeneralSettingsCategory.AggroData => Loc.Localize( "Config Category: Aggro Data", "敵意距離資料" ),
			GeneralSettingsCategory.Nameplates => Loc.Localize( "Config Category: Nameplates", "名牌設定" ),
			GeneralSettingsCategory.Miscellaneous => Loc.Localize( "Config Category: Miscellaneous", "其他設定" ),
			_ => "You should never see this!",
		};
	}
}
