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

/*

Javascript / JQuery based modal popup interface

Requires:
	jquery-x.x.x.js
	jquery.evenifhidden.js
*/

/*

Create a modal popup object, attached to a block of HTML. It should be
in this format:

<div class="modalpopup">
	<div class="title">
		<div class="left">stuff</div>
		<div class="right">stuff</div>
	</div>
	<div class="body">
		stuff
	</div>
</div>

'title' is optional but encouraged ('body' is sized to assume it).

Note that width and height can be not passed in, in which case we will
ask the DOM to determine the default size for us. (Make sure you size
the contents as large as you want them to be.)

*/
function ModalPopup(selector, width, height) {
	this.id = selector;
	this.edup = false;
	this.popW = width;
	this.popH = height;
	this.preset = true;

	if (!this.popW || !this.popH)
		this.preset = false;

	this.popup = $.proxy(function() {
		if (this.edup) return;

		if (!this.preset && (!this.popW || !this.popH)) {
			outerThis = this;
			$(selector).evenIfHidden(function(e) {
				outerThis.popW = e.width();
				outerThis.popH = e.height();
			});
		}

		centerX = $(window).width() / 2;
		centerY = $(window).height() / 2;

		pop = $(this.id);

		// Can't seem to get this to lay out right. This is simplest.
		if (this.preset) {
			$(this.id + ' .body').css({
				width: (this.popW - 6) + 'px',
				height: (this.popH - 30) + 'px'
			});
		}

		pop.css({
			width: '6px',
			height: '6px',
			left: (centerX-3) + 'px',
			top: (centerY-3) + 'px',
			display: 'block'
		}).animate({
			width: this.popW,
			height: this.popH,
			left: (centerX - this.popW/2),
			top: (centerY - this.popH/2),
			opacity: 1.0
		}, 200, 'swing', function() {
			pop.css({ overflow: 'visible' });
		});
		this.edup = true;
	}, this);

	this.popdown = $.proxy(function() {
		if (!this.edup) return;
		pop = $(this.id);
		centerX = pop.position().left + pop.width() / 2;
		centerY = pop.position().top + pop.height() / 2;
		pop.css({ overflow: 'hidden' });
		pop.animate({
			width: '6px',
			height: '6px',
			left: (centerX-3) + 'px',
			top: (centerY-3) + 'px',
			opacity: 0.0
		}, 200, 'swing', $.proxy(function() {
			pop.hide();
			this.edup = false;
		}, this));
	}, this);
}
