using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Timers;
using Smod2;
using Smod2.API;
using Smod2.Attributes;
using Smod2.Config;
using UnityEngine;

namespace SCPAnnouncer
{
	[PluginDetails(
		author = "Seager",
		name = "SCP-Position-Announcer",
		description = "This plugin periodically announces positions of SCP's",
		id = "seager.scp.announcer",
		configPrefix = "scpann",
		langFile = "scpannouncer",
		version = "1.0",
		SmodMajor = 3,
		SmodMinor = 4,
		SmodRevision = 0
		)]
	public class SCPAnnouncer : Plugin
	{

		public static int miliSeconds;
		public static SCPAnnouncer plugin;

		public override void OnDisable()
		{

		}

		public override void OnEnable()
		{
			plugin = this;
		}



		public override void Register()
		{
			this.AddConfig(new ConfigSetting("scp-announce-time", 30, true, "Sets the delay of position announcing."));
			miliSeconds = this.GetConfigInt("scp-announce-time") * 1000;

			System.Timers.Timer myTimer = new System.Timers.Timer();
			myTimer.Elapsed += new ElapsedEventHandler(AnnounceThread);
			myTimer.Interval = miliSeconds;
			myTimer.Start();
		}

		public static void AnnounceThread(object source, ElapsedEventArgs e)
		{

			Player[] players = plugin.Server.GetPlayers().ToArray();
			foreach (Player p in players)
			{
				plugin.Info(miliSeconds + "");
				if (p.TeamRole.Team.ToString() == "SCP")
				{
					ZoneType playerZone = FindRoomAtPoint(new Vector3(p.GetPosition().x, p.GetPosition().y, p.GetPosition().z));
					Role playerRole = p.TeamRole.Role;
					string zoneName = "";
					bool notDetected = false;
					string scpName = "";

					switch(playerZone) {
						case ZoneType.ENTRANCE:
							zoneName = "entrance zone";
							break;
						case ZoneType.HCZ:
							zoneName = "heavy containment zone";
							break;
						case ZoneType.LCZ:
							zoneName = "light containment zone";
							break;
						case ZoneType.UNDEFINED:
							notDetected = true;
							break;
					}

					string inputString = playerRole.ToString();
					string reg = @"(?<=SCP_)(\d\d\d)";
					Match m = Regex.Match(inputString, reg);
					string scpn = m.Value;
					scpName = string.Join(" ", scpn.ToArray());
					if (notDetected) { plugin.Server.Map.AnnounceCustomMessage("SCP " + scpName + " not detected"); return; }
					plugin.Server.Map.AnnounceCustomMessage("SCP " + scpName + " detected in " + zoneName);

				}
			}

		}

		public static ZoneType FindRoomAtPoint(Vector3 point)
		{
			var currentRoom = "::NONE::";
			var currentZone = "::NONE::";

			RaycastHit raycastHit;
			if (Physics.Raycast(new Ray(point, Vector3.down), out raycastHit, 100f, Interface079.singleton.roomDetectionMask))
			{
				Transform transform = raycastHit.transform;
				while (transform != null && !transform.transform.name.ToUpper().Contains("ROOT"))
				{
					transform = transform.transform.parent;
				}

				if (transform != null)
				{
					currentRoom = transform.transform.name;
					currentZone = transform.transform.parent.name;

					ZoneType zone = ZoneType.UNDEFINED;
					switch (transform.transform.parent.name)
					{
						case "EntranceRooms":
							zone = ZoneType.ENTRANCE;
							break;
						case "HeavyRooms":
							zone = ZoneType.HCZ;
							break;
						case "LightRooms":
							zone = ZoneType.LCZ;
							break;
					}

					var roomName = transform.transform.name;
					return zone;
				}

			}
			return ZoneType.UNDEFINED;
		}

	}
}
