namespace Kayateia.Climoo.Models
{
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Kayateia.Climoo.MooCore;

/// <summary>
/// Contains info related to sidebar updates that may go out with the console.
/// </summary>
public class SidebarInfo
{
	/// <summary>
	/// The player. This will be null if the player isn't logged in yet.
	/// </summary>
	public Player player;

	/// <summary>
	/// The player's current location. This will be null if the player isn't logged in yet.
	/// </summary>
	public Mob location;

	/// <summary>
	/// The World object for this game instance.
	/// </summary>
	public World world;
}

}