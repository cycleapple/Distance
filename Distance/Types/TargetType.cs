using CheapLoc;

namespace Distance;

public enum TargetType : int
{
	Target = 0,
	SoftTarget = 1,
	FocusTarget = 2,
	MouseOver_And_UIMouseOver_Target = 3,
	MouseOverTarget = 4,
	UIMouseOverTarget = 5,
	Target_And_Soft_Target = 6,
	TargetOfTarget = 7,
}

public static class TargetTypeExtensions
{
	public static string GetTranslatedName( this TargetType targetType )
	{
		return targetType switch
		{
			TargetType.Target => Loc.Localize( "Terminology: Target Type - Target", "目前目標" ),
			TargetType.SoftTarget => Loc.Localize( "Terminology: Target Type - Soft Target", "軟目標" ),
			TargetType.FocusTarget => Loc.Localize( "Terminology: Target Type - Focus Target", "焦點目標" ),
			TargetType.MouseOver_And_UIMouseOver_Target => Loc.Localize( "Terminology: Target Type - Mouseover and UIMouseover Target", "滑鼠指向目標" ),
			TargetType.MouseOverTarget => Loc.Localize( "Terminology: Target Type - Mouseover Target", "場景滑鼠指向目標" ),
			TargetType.UIMouseOverTarget => Loc.Localize( "Terminology: Target Type - UIMouseover Target", "介面滑鼠指向目標" ),
			TargetType.Target_And_Soft_Target => Loc.Localize( "Terminology: Target Type - Target and Soft Target", "目前目標＋軟目標" ),
			TargetType.TargetOfTarget => Loc.Localize( "Terminology: Target Type - Target of Target", "目標的目標" ),
			_ => "You should never see this!",
		};
	}
}
