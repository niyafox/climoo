# CliMOO
## A MOO (Multi-User Dungeon, Object Oriented) for the web
### Copyright (C) 2010 - 2014 Kayateia

CliMOO is licensed under the GPL, version 3 or higher. Please see the LICENSE file for more details.

For more information about what CliMOO is, see [its web page](http://kayateia.net/climoo/) or the [Hacker's Guide](https://github.com/kayateia/climoo/wiki/Hacker's-Guide).

#### Setting up your own CliMOO instance

I don't recommend publishing CliMOO on the public internet just yet, because it really needs more security work. But to get it going on your own internal machine...

* Build the project.
   You'll need ASP.NET MVC 3 installed if you're working under Windows, and the project is compatible with VS2010 and above.
   For Mono, I'm not sure what the minimum version is, but I'm using MonoDevelop 3.0.3.5.
* Set up a MySQL database and load notes/mysql.sql into it.
* Edit the sample config file in impexporter/ and set up your own values. Use impexporter to pull from notes/data-export/.
* Edit connectionStrings-sample.config and save it as connectionStrings.config.
* Run. You should get a web browser tab.
* A text console should appear in the web browser, and you'll get a welcome screen and a prompt. Type 'login' and use these default credentials:
   user: admin
   password: foo
* You should be logged in and will appear in an area of the MOO.
* Read the [hacker's guide](https://github.com/kayateia/climoo/wiki/Hacker's-Guide) for more info on how everything works.
