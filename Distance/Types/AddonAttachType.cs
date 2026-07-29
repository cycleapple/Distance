using CheapLoc;

namespace Distance;

internal enum AddonAttachType : int
{
	Auto,
	ScreenText,
	Target,
	FocusTarget,
	Cursor,
}

internal static class AddonAttachTypeExtensions
{
	internal static string GetTranslatedName( this AddonAttachType attachType )
	{
		return attachType switch
		{
			AddonAttachType.Auto => Loc.Localize( "Terminology: UI Attach Point - Auto", "自動" ),
			AddonAttachType.ScreenText => Loc.Localize( "Terminology: UI Attach Point - Screen Text", "螢幕空間" ),
			AddonAttachType.Target => Loc.Localize( "Terminology: UI Attach Point - Target", "目標資訊" ),
			AddonAttachType.FocusTarget => Loc.Localize( "Terminology: UI Attach Point - Focus Target", "焦點目標資訊" ),
			AddonAttachType.Cursor => Loc.Localize( "Terminology: UI Attach Point - Mouse Cursor", "滑鼠游標" ),
			_ => "You should never see this!",
		};
	}


	internal static GameAddonEnum GetGameAddonToUse( this AddonAttachType attachType, TargetType widgetTargetType )
	{
		if( attachType == AddonAttachType.ScreenText )
		{
			return GameAddonEnum.ScreenText;
		}
		else if( attachType == AddonAttachType.Cursor )
		{
			return GameAddonEnum.ScreenText;
		}
		else if( attachType == AddonAttachType.Target )
		{
			return GameAddonEnum.TargetBar;
		}
		else if( attachType == AddonAttachType.FocusTarget )
		{
			return GameAddonEnum.FocusTargetBar;
		}
		else /*Auto*/
		{
			if( widgetTargetType is TargetType.Target or TargetType.SoftTarget or TargetType.Target_And_Soft_Target )
			{
				return GameAddonEnum.TargetBar;
			}
			else if( widgetTargetType is TargetType.FocusTarget )
			{
				return GameAddonEnum.FocusTargetBar;
			}
			else if( widgetTargetType is TargetType.TargetOfTarget )
			{
				return GameAddonEnum.TargetBar;
			}
			else
			{
				return GameAddonEnum.ScreenText;
			}
		}
	}
}
