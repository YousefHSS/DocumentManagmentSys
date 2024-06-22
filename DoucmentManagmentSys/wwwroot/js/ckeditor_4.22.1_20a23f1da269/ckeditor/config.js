/**
 * @license Copyright (c) 2003-2023, CKSource Holding sp. z o.o. All rights reserved.
 * For licensing, see https://ckeditor.com/legal/ckeditor-oss-license
*/


function triggerClickOnParagraph(editor, paragraphElement) {
	// Dispatch a click event to the paragraph element
	if (paragraphElement) {
		var clickEvent = document.createEvent('MouseEvents');
		clickEvent.initEvent('click', true, true);
		paragraphElement.$.dispatchEvent(clickEvent);
	}
}
function checkSelectedElement(editor) {
	let selection = editor.getSelection();
	let ranges = selection.getRanges(); // Get all ranges from the selection
	console.log(selection.getStartElement());
	for (let i = 0; i < ranges.length; i++) {
		let range = ranges[i];
		let startNode = range.startContainer;
		let endNode = range.endContainer;

		// Check start container if it's a paragraph
		if (startNode.type == CKEDITOR.NODE_ELEMENT && (startNode.is('p')|| startNode.is('div'))) {
			return true;
		}

		// Iterate through nodes between start and end containers
		let node = startNode;
		while (node && !node.equals(endNode)) {
			node = node.getNext();
			if (node && node.type == CKEDITOR.NODE_ELEMENT && (node.is('p') || node.is('div'))) {
				return true;
			}
		}

		// Check end container if it's a paragraph
		if (endNode.type == CKEDITOR.NODE_ELEMENT && (endNode.is('p') || endNode.is('div'))) {
			return true;
		}
	}

	return false;
}
CKEDITOR.editorConfig = function( config ) {
	// Define changes to default configuration here.
	// For complete reference see:
	// https://ckeditor.com/docs/ckeditor4/latest/api/CKEDITOR_config.html
	
	// The toolbar groups arrangement, optimized for a single toolbar row.
	config.toolbarGroups = [
		{ name: 'document',	   groups: [ 'mode', 'document', 'doctools' ] },
		{ name: 'clipboard',   groups: [ 'clipboard', 'undo' ] },
		{ name: 'editing',     groups: [ 'find', 'selection', 'spellchecker' ] },
		{ name: 'forms' },
		{ name: 'basicstyles', groups: [ 'basicstyles', 'cleanup' ] },
		{ name: 'paragraph',   groups: [ 'list', 'indent', 'blocks', 'align', 'bidi' ] },
		{ name: 'links' },
		{ name: 'insert' },
		{ name: 'styles' },
		{ name: 'colors' },
		{ name: 'tools' },
		{ name: 'others' },
		{ name: 'about' }
	];

	// The default plugins included in the basic setup define some buttons that
	// are not needed in a basic editor. They are removed here.
	config.removeButtons = 'Anchor,Strike,Font';
	// Dialog windows are also simplified.
	config.removeDialogTabs = 'link:advanced';
	

	config.allowedContent = true;

};


