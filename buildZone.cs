// ====================================================================
// ZONE CREATION
// ====================================================================
function serverCmdBuildContest(%client, %time, %w0, %w1, %w2, %w3, %w4, %w5, %w6, %w7, %w8)
{
	if(isObject(%client.minigame))
	{
		messageClient(%client, '', "ERROR:\c3 Exit your current minigame before starting a contest.");
		return;
	}
	
	if($SelectedContestMinigame !$= "")
	{
		messageClient(%client, '', "ERROR:\c3 Wait for the current Build Zone preparation to complete.");
		return;
	}
	
	%theme = trim(%w0 SPC %w1 SPC %w2 SPC %w3 SPC %w4 SPC %w5 SPC %w6 SPC %w7 SPC %w8);
	%theme = stripMLControlChars(%theme);
	
	if(%theme $= "")
	{
		messageClient(%client, '', "ERROR:\c3 No theme name given!");
		messageClient(%client, '', "\c3 Ex. \c3/buildContest \c41 Chair \c2-- Gives everyone a minute to build a chair!");
		return;
	}
	
	%time = %time / 1;
	if(%time <= 0)
	{
		messageClient(%client, '', "ERROR:\c3 Time has to be greater than 0!");
		messageClient(%client, '', "\c3 Ex. \c3/buildContest \c41 Chair \c2-- Gives everyone a minute to build a chair!");
		return;
	}
	//
	if(%time > 60)
	{
		messageClient(%client, '', "ERROR:\c3 Time must be less than 60!");
		return;
	}
	
	%contest = new SimSet(BuildContest);
	%contest.time = %time * 1000 * 60;
	%contest.theme = %theme;
	%contest.pos = getSubStr(%client.bl_id, 0, 2) * 100 SPC getRandom(20, 40) * 100 SPC "2000";
	
	%contestants = new SimSet(Contestants);
	%contest.add(%contestants);
	
	contestPreRound(%contest);
	
	%title = "[BUILD CONTEST] -- " @ %theme;
	%minigame = new ScriptObject(buildContestMinigame)
	{
		class = "MiniGameSO";
		owner = %client;
		numMembers = 0;
		
		isBuildingContest = true;
		contest = %contest;
		
		title = %title;
		colorIdx = "6";
		inviteOnly = false;
		UseAllPlayersBricks = false;
		PlayersUseOwnBricks = true;
		
		Points_BreakBrick = 0;
		Points_PlantBrick = 0;
		Points_KillPlayer = 0;
		Points_KillSelf = 0;
		Points_Die = 0;
		
		respawnTime = 2;
		vehiclerespawntime = "10000";
		brickRespawnTime = "0";
		playerDatablock = "PlayerStandardArmor";
		
		useSpawnBricks = false;
		fallingdamage = false;
		weapondamage = false;
		SelfDamage = false;
		VehicleDamage = false;
		brickDamage = false;
		
		enableWand = true;
		EnableBuilding = true;
		enablePainting = true;
		
		StartEquip0 = nametoid(hammerItem);
		StartEquip1 = nametoid(wrenchItem);
		StartEquip2 = nametoid(printGun);
		StartEquip3 = 0;
		StartEquip4 = 0;
	};
	
	$SelectedContestMinigame = %minigame;
	
	%formattedTime = getStringFromTime(%contest.time);
	messageAll('',"<font:Palatino Linotype:36>\c3" @ %client.name @ "\c5 started a contest for building \c3" @ %theme @ "\c5 in \c3" @ %formattedTime @ "\c5!");
	messageAllExcept(%client, %client, '',"<font:Palatino Linotype:18>\c6Type \c3/compete \c6to join!");
	
	%contest.minigame = %minigame;
	%minigame.addMember(%client);
}

function serverCmdCompete(%client)
{
	if($LoadingBricks_Client == 1.0)
	{
		schedule(800, %client, serverCmdCompete, %client);
		return;
	}
	%minigame = $SelectedContestMinigame;
	if(isObject(%minigame) && !isObject(%client.minigame))
		%minigame.addMember(%client);
}

function createBuildZone(%contest, %pos)
{
	%zone = new SimSet(BuildZone);
	%contest.add(%zone);
	
	%contestBuild = new SimSet(ContestBuild);
	%zone.add(%contestBuild);
	
	%buildGroup = %zone;
	$loadOffset = %pos;
	$SelectedSimSet = %buildGroup;
	serverDirectSaveFileLoad($CONTEST_TEMPLATE, 3, "", 2, 1);
	$loadOffset = "0 0 0";
	
	return %zone;
}

function isInBuildZone(%buildPos, %brickPos)
{
	%x1 = getWord(%buildPos, 0);
	%y1 = getWord(%buildPos, 1);
	%z1 = getWord(%buildPos, 2);
	
	%x2 = getWord(%brickPos, 0);
	%y2 = getWord(%brickPos, 1);
	%z2 = getWord(%brickPos, 2);
	
	if(%x1 + 8 < %x2 || %x1 - 8 > %x2)
		return false;
	
	if(%y1 + 8 < %y2 || %y1 - 8 > %y2)
		return false;
	
	if(%z1 - $CONTEST::ZONE::DROPHEIGHT - 0.2 > %z2 || %z1 + 48 < %z2)
		return false;
	
	return true;
}
// ====================================================================
// BUILDING PACKAGE
// ====================================================================
package BuildingContestPackage
{	
	function ServerLoadSaveFile_End()
	{
		Parent::ServerLoadSaveFile_End();
		$SelectedSimSet = -1;
	}
	
	function fxDTSBrick::onLoadPlant(%obj)
	{
		Parent::onLoadPlant(%obj);
		if(!($SelectedSimSet == -1 || $SelectedSimSet $= ""))
		{
			%obj.isBaseplate = true;
			$SelectedSimSet.add(%obj);
		}
	}
	
	function serverCmdPlantBrick(%client)
	{
		if(%client.isContester && isObject(getContestFromContestant(%client)))
		{
			if(isObject(%obj = %client.player.tempBrick))
			{
				%brickPos = %obj.position;
				%buildPos = %client.contestSpawn;
				if(%client.minigame.contest.phase !$= "Building" || !isInBuildZone(%buildPos, %brickPos))
				{
					return;
				}
			}
		}		
		
		parent::serverCmdPlantBrick(%client);
		
	}
	
	function fxDTSBrick::onAdd(%obj)
	{
		Parent::onAdd(%obj);
		if(%obj.client.isContester && isObject(getContestFromContestant(%obj.client)))
		{			
			//talk(isObject(findclientbyname(lego).buildZone.getObject(ContestBuild)));
			%contestBuild = %obj.client.buildZone.getObject(ContestBuild);
			if(isObject(%contestBuild))
				%contestBuild.add(%obj);
			//talk("WE TRYING TO ADD THE OBJECT TO THE CONTEST BUILD!!!");
		}
	}
	// function ND_Selection::plantBrick(%this, %i, %position, %angleID, %brickGroup, %client, %bl_id)
	// {
		// parent::plantBrick(%this, %i, %position, %angleID, %brickGroup, %client, %bl_id);
		// %obj = %this;
		// if(%client.isContester)
		// {			
			// %contestBuild = %client.buildZone.getObject(ContestBuild);
			// %contestBuild.add(%obj);
		// }
	// }
	function GameConnection::spawnPlayer(%client)
	{
		parent::spawnPlayer(%client);
		
		%obj = %client.player;
		if(isObject(%obj) && %client.isContester && isObject(getContestFromContestant(%client)))
			%obj.setTransform(%client.contestSpawn);
		
	}
};
activatePackage(BuildingContestPackage);