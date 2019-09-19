// ====================================================================
// GLOBAL CONSTANTS
// ====================================================================
$CONTEST_TEMPLATE = "Add-Ons/Server_BuildingContest/BuildZone.bls";
$CONTEST::TIME::GHOST = 1000 * 		2;
$CONTEST::TIME::PREP = 1000 * 		30;
$CONTEST::TIME::PREJUDGE = 1000 * 	5;
$CONTEST::TIME::JUDGE = 1000 * 		60;
$CONTEST::TIME::WIN = 1000 * 		10;

// Don't mess with these unless u konw what youre doing!!! <3
$CONTEST::ZONE::BASEPOS = "0 0 0.2";
$CONTEST::ZONE::VIEWPOS = "0 -10 0.2";
$CONTEST::ZONE::HALFLENGTH = 6;
$CONTEST::ZONE::SEPERATION = 30;
$CONTEST::ZONE::DROPHEIGHT = 2;

exec("./buildZone.cs");
exec("./contest.cs");
exec("./minigame.cs");
exec("./rating.cs");