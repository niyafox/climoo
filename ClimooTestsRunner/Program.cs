/*
	CliMOO - Multi-User Dungeon, Object Oriented for the web
	Copyright (C) 2010-2014 Kayateia

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ClimooTestsRunner
{
	// This is a really simple console test app that loads up the test assembly and
	// runs stuff. It's meant to help with debugging unit tests since apparently
	// that is a problem with the way NUnit loads them.
	class Program
	{
		static void Main( string[] args )
		{
			var wt = new Kayateia.Climoo.Tests.WorldTest();
			wt.Basic();
		}
	}
}
