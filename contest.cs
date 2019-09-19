function contestDisplayLoop(%contest, %stop)
{
	if(%stop)
		return;
	
	if(!isObject(%contest))
		return;
	
	cancel(%contest.displaySchedule);
	
	%timeLeft = %contest.timeLeft;
	%formattedTime = getStringFromTime(%timeLeft);
	if(%timeLeft <= 30000 && %timeLeft > 10000)
		%formattedTime = "<color:FFA500>" @ %formattedTime;
	else if(%timeLeft <= 10000)
		%formattedTime = "\c0" @ %formattedTime;
	%contestants = %contest.getObject(Contestants);
	for(%i = 0; %i < %contestants.getCount(); %i++)
	{
		%c = %contestants.getObject(%i);
		%c.bottomPrint("<font:Palatino Linotype:43>\c3" @ strupr(%contest.phase) @ "<just:right> <font:Palatino Linotype:36>\c3" @ %formattedTime NL
		(%c == %contest.currJudging ? "" : "<just:left><font:Palatino Linotype:20>\c6" @ %contest.hint), 3, 1);
	}
	
	if(%contest.phase $= "Preparing" && %timeLeft <= 0)
	{
		contestStart(%contest);
		$SelectedContestMinigame = "";
	}
	else if(%contest.phase $= "Building" && %timeLeft == 30000)
		contestMessageAll(%contest, "<font:Palatino Linotype:35>\c530 SECONDS LEFT!");
	else if(%contest.phase $= "Building" && %timeLeft <= 0)
		contestPreJudge(%contest);
	else if(%contest.phase $= "Time is up!" && %timeLeft <= 0)
		contestJudge(%contest);
	else if(%contest.phase $= "Done Judging")
		contestWin(%contest);
	else if(%contest.phase $= "THE END" && %timeLeft <= 0)
		contestEnd(%contest);
	
	%contest.timeLeft -= 1000;
	%contest.displaySchedule = schedule(1000, %contest, contestDisplayLoop, %contest);
}

// in MS
function getStringFromTime(%time)
{
	%minutes = mFloor(%time / 60000);
	%seconds = mFloor((%time - %minutes * 60000) / 1000);
	if(%seconds < 10)
		%seconds = "0" @ %seconds;
		
	return %minutes @ ":" @ %seconds;
}

function contestPreRound(%contest)
{
	%contest.phase = "Preparing";
	%contest.hint = "\c6Waiting for players...";
	%contest.timeLeft = $CONTEST::TIME::PREP;
	contestDisplayLoop(%contest);
}

function contestStart(%contest)
{
	if(%contest.getObject(Contestants).getCount() <= 1)
	{
		contestMessageAll(%contest, "Not enough players to have a contest!");
		contestEnd(%contest);
	}
	
	%contest.phase = "Building";
	%contest.hint = "\c6THEME: \c3" @ %contest.theme;
	%theme = %contest.theme;
	%time = %contest.time;
	
	%contest.timeLeft = %time;
	
	%msg = "<font:Palatino Linotype:35>\c5BEGIN BUILDING!";
	contestMessageAll(%contest, %msg);
}

function contestPreJudge(%contest)
{
	%contest.phase = "Time is up!";
	%contest.hint = "";

	%contest.timeLeft = $CONTEST::TIME::PREJUDGE;
	
	%msg = "<font:Palatino Linotype:35>\c5BUILDING PHASE ENDED!";
	contestMessageAll(%contest, %msg);
}

function contestJudge(%contest)
{
	cancel(%contest.judgeSchedule);
	%contest.phase = "Judging";
	%contest.hint = "\c6Type \c3/rate x/10 \c6 to judge. \c4 EX: /rate 7/10";
	
	%contestants = %contest.getObject(Contestants);
	%cCount = %contestants.getCount();
	
	if(%contest.time > 2 * 60000)
		%contest.timeLeft = $CONTEST::TIME::JUDGE;
	else
		%contest.timeLeft = $CONTEST::TIME::JUDGE / 2;
	
	// Get contestant
	%search = 0;
	%foundUnJudged = false;
	
	for(%i = 0; %i < %cCount; %i++)
	{
		%c = %contestants.getObject(%i);
		if(!%c.hasBeenJudged)
		{
			%foundUnJudged = true;
			break;
		}
	}
	
	if(%foundUnJudged == false)
	{
		%contest.phase = "Done Judging";
		return;
	}
	
	%contest.currJudging = %c;
	%buildZone = %c.buildZone;
	%basePos = %c.contestSpawn;
	
	// TP all players
	// Make it support all members of the minigame in case there are spectators that join later.
	for(%i = 0; %i < %cCount; %i++)
	{
		%rand = getRandom( -1 * $CONTEST::ZONE::HALFLENGTH, $CONTEST::ZONE::HALFLENGTH);
		%pos =  getWord(%basePos, 0) - 10 SPC getWord(%basePos, 1) + %rand SPC getWord(%basePos, 2);
		%contestant = %contestants.getObject(%i);
		%contestant.player.setTransform(%pos);
		%id = %contestant.bl_id;
		
		%buildZone.rating[%id] = 0;
		
		messageClient(%contestant, '', "<font:Palatino Linotype:45>\c6Now Judging: \c3" @ %c.name);
		if(%contestant != %c)
			messageClient(%contestant, '', "<font:Palatino Linotype:20>\c6Type \c3/rate x/10 \c6 to judge. \c4 EX: /rate 7/10");
	}
	
	%c.hasBeenJudged = true;
	%contest.judgeSchedule = schedule(%contest.timeLeft, %contest, contestJudge, %contest);	
}

function contestWin(%contest)
{
	%contest.phase = "THE END";
	%contest.hint = "\c6Thanks for playing!";
	%contestants = %contest.getObject(Contestants);	
	
	%contest.timeLeft = $CONTEST::TIME::WIN;
	
	%contest.winnerText = "";
	
	%maxRating = 0;
	%winner = 0;
	
	for(%i = 0; %i < %contestants.getCount(); %i++)
	{
		%c = %contestants.getObject(%i);
		
		%rating = %c.buildZone.avgrating;
		if(%rating > %maxRating)
		{
			%maxRating = %rating;
			%winner = %c;
		}
	}
	
	%contest.winnerText = "<font:Palatino Linotype:29>\c3" @ %winner.name @ " \c2WON WITH A RATING OF \c3" @ %maxRating @ "/10\c2!!!!!";
	//talk(%contest.winnerText);
	for(%j = 0; %j < %contestants.getCount(); %j++)
	{
		%j = %contestants.getObject(%j);
		
		if(isObject(%Winner))
			CenterPrint(%c, %contest.winnerText, $CONTEST::TIME::WIN);
		else
			CenterPrint(%c, "No winners!!!", $CONTEST::TIME::WIN);
	}
	if(isObject(%winner))
		addBCWin(%winner, %contest.theme, %maxRating);
}

function addBCWin(%winner, %theme, %maxRating)
{
	%filePath = "config/server/BuildingContest/" @ %winner.bl_id @ ".txt";
	
	%file = new FileObject();
	%file.openForRead(%filePath);
	
	%wins = 0;
	
	if(!%file)
		%wins = 0;
	else
	{
		%line = %file.readLine();
		%wins = getField(%line, 0);
	}
	%file.close();
	%file.delete();
	
	%wins++;
	
	%file = new FileObject();
	%file.openForWrite(%filePath);
	
	%file.writeLine(%wins);
	
	%file.close();
	%file.delete();
	
	messageAll('',"\c3" @ %winner.name @ "\c5 won the building contest for \c3" @ %theme @ " \c5with an overall rating of \c3" @ %maxRating @ "/10\c5!");
	messageAll('',"\c4 -- \c5TOTAL WINS: \c3" @ %wins);
}

function contestEnd(%contest)
{		
	%contesters = %contest.getObject(Contestants);
	
	// for(%i = 0; %i < %contesters.getCount(); %i++)
	// {
		// %c = %contesters.getObject(%i)
		// %c.buildZone.deleteAll();
	// }
	
	// for(%i = 0; %i < %contesters.getCount(); %i++)
	// {
		// %c = %contesters.getObject(%i);
		// %contesters.remove(%i);
	// }
	contestClearAllBuilds(%contest);
	%minigame = %contest.minigame;
	%owner = %minigame.owner;
	for(%i = 0; %i < %contesters.getCount(); %i++)
	{
		%client = %contesters.getObject(%i);
		if(%client != %owner)
			%minigame.removeMember(%client);
	}
	%minigame.removeMember(%owner);
	
	%minigame.delete();
	%contest.deleteAll();
	%contest.delete();
}

function contestClearAllBuilds(%contest)
{
	%contestants = %contest.getObject(Contestants);
	for(%i = 0; %i < %contestants.getCount(); %i++)
	{
		%client = %contestants.getObject(%i);
		%buildZone = %client.buildZone;
		%contestBuild = %buildZone.getObject(ContestBuild);
		while(%contestBuild.getCount())
		{
			%brick = %contestBuild.getObject(0);
			%brick.delete();
		}
	}
}

function contestSpawnPlayer(%client)
{
	%client.instantRespawn();
	%client.player.setTransform(%client.contestSpawn);
}

function contestMessageAll(%contest, %msg)
{
	%minigame = %contest.minigame;
	%minigame.chatMessageAll('', %msg);
}