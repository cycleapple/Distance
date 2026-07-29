using CheapLoc;

namespace Distance;

public enum TargetCategory : int
{
	Targets = 0,
	Self = 1,
	AllEnemies = 2,
	AllPlayers = 3,
}

public static class TargetCategoryExtensions
{
	public static string GetTranslatedName( this TargetCategory targetCategory )
	{
		return targetCategory switch
		{
			TargetCategory.Targets => Loc.Localize( "Terminology: Target Category - Targets", "目標" ),
			TargetCategory.Self => Loc.Localize( "Terminology: Target Category - Self", "自己" ),
			TargetCategory.AllEnemies => Loc.Localize( "Terminology: Target Category - All Enemies", "所有敵人" ),
			TargetCategory.AllPlayers => Loc.Localize( "Terminology: Target Category - All Players", "所有玩家" ),
			_ => "You should never see this!",
		};
	}
}
