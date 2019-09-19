// =================================================
// MinigameSO
// =================================================
package BuildingContestMinigame
{	
	function MinigameSO::addMember(%this, %client)
	{
		parent::addMember(%this, %client);
		
		if(!%this.isBuildingContest)
			return;
			
		%contest = %this.contest;
		%contesters = %contest.getObject(Contestants);
		
		%contesters.add(%client);
		%client.isContester = true;
		%client.hasBeenJudged = false;
		
		%cCount = %contesters.getCount();
		
		%basePos = %contest.pos;
		%pos = getWord(%basePos, 0) + $CONTEST::ZONE::SEPERATION * %cCount SPC getWord(%basePos, 1) SPC getWord(%basePos, 2);
		
		%client.contestSpawn = getWord(%pos, 0) SPC getWord(%pos, 1) SPC getWord(%pos, 2) + $CONTEST::ZONE::DROPHEIGHT;
		
		%client.buildZone = createBuildZone(%contest, %pos);
		
		%client.spawnSchedule = schedule($CONTEST::TIME::GHOST, %client, contestSpawnPlayer, %client);
	}
	
	function MinigameSO::removeMember(%this, %client)
    {
        parent::removeMember(%this, %client);
		
        if(!%this.isBuildingContest)
            return;

		%contest = %this.contest;
		// if(%contest.isStarted && %this.numMembers > 1)
			// contestStop(%contest);	
		
		// if(%this.owner == %client)
		// {
			// contestEnd(%contest);
			// return;
		// }
		//talk("WE REMOVED A MEMBER!");
		//talk("WE REMOVED \c3" @ %client.name);
		%client.isContester = false;
		
		%contesters = %contest.getObject(Contestants);
		%contesters.remove(%client);
		
        %buildZone = %client.buildZone;
		%contestBuild = %buildZone.getObject(ContestBuild);
		%contestBuild.deleteAll();
		%buildZone.deleteAll();
		%buildZone.delete();
		
		%client.instantRespawn();
    }
	
	function MinigameSO::checkLastManStanding(%this)
	{
		if(%this.isBuildingContest)
			return;
		parent::checkLastManStanding(%this, %client);
	}
};
activatePackage(BuildingContestMinigame);