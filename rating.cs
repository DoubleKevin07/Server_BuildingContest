function getContestFromContestant(%contestant)
{
	if(!isObject(%contestant))
		return -1;
	if(!isObject(%contestant.minigame))
		return -1;
	if(!%contestant.isContester)
		return -1;
	if(!isObject(%contestant.minigame.contest))
		return -1;
	
	return %contestant.minigame.contest;
}


function serverCmdRate(%client, %rating)
{
	%contest = getContestFromContestant(%client);
	
	if(!isObject(%contest))
		return;
	
	if(!isObject(%contest.currJudging))
		return;
	
	if(%contest.phase !$= "Judging")
		return;
	
	%score = getSubStr(%rating, 0, 2);
	if(%score !$= "10")
		%score = getSubStr(%rating, 0, 1);
	
	if(%score < 1 || %score > 10)
	{
		messageClient(%client, '', "ERROR: SCORE NEEDS TO BE BETWEEN 1 & 10. EX. /rate 7/10");
		return;
	}
	
	%toJudge = %contest.currJudging;
	%buildZone = %toJudge.buildZone;
	
	if(%client == %toJudge)
		return;
	
	%id = %client.bl_id;
	
	%buildZone.rating[%id] = %score;
	
	messageClient(%client, '', "\c6You rated the build a \c3" @ %score @ "/10\c6.");
	updateAverageScore(%contest, %buildZone);
}

function updateAverageScore(%contest, %buildZone)
{
	%contestants = %contest.getObject(Contestants);
	%cCount = %contestants.getCount();
	
	%numRatings = 0;
	%ratingsAdded = 0;
	
	for(%i = 0; %i < %cCount; %i++)
	{
		%c = %contestants.getObject(%i);
		%id = %c.bl_id;
		
		%rating = %buildZone.rating[%id];
		
		if(%rating > 0)
		{
			//talk("Adding Rating \c3" @ %rating);
			%numRatings++;
			%ratingsAdded += %rating;
		}
	}
	
	
	%avgRating = %ratingsAdded / %numRatings;
	//talk("Avg Rating: \c2" @ %avgRating);
	%buildZone.avgRating = %avgRating;
}