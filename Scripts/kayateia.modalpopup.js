/*

Javascript / JQuery based modal popup interface
Copyright (C) 2010 Kayateia

Requires:
	jquery-x.x.x.js
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

*/
function ModalPopup(selector, width, height) {
	this.id = selector;
	this.edup = false;
	this.popW = width;
	this.popH = height;

	this.popup = $.proxy(function() {
		if (this.edup) return;
		centerX = $(window).width() / 2;
		centerY = $(window).height() / 2;

		// Can't seem to get this to lay out right. This is simplest.
		$(this.id + ' .body').css({
			width: (this.popW - 6) + 'px',
			height: (this.popH - 30) + 'px'
		});

		$(this.id).css({
			left: centerX + 'px',
			top: centerY + 'px',
			display: 'block'
		}).animate({
			width: this.popW + 'px',
			height: this.popH + 'px',
			left: (centerX - this.popW/2) + 'px',
			top: (centerY - this.popH/2) + 'px',
			opacity: 1.0
		}, 200);
		this.edup = true;
	}, this);

	this.popdown = $.proxy(function() {
		if (!this.edup) return;
		ed = $(this.id);
		centerX = ed.position().left + ed.width() / 2;
		centerY = ed.position().top + ed.height() / 2;
		$(this.id).animate({
			width: '5px',
			height: '5px',
			left: centerX + 'px',
			top: centerY + 'px',
			opacity: 0.0
		}, 200, 'swing', $.proxy(function() {
			$(this.id).hide();
			this.edup = false;
		}, this));
	}, this);
}
