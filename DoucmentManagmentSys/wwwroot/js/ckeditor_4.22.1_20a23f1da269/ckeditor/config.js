/**
 * @license Copyright (c) 2003-2023, CKSource Holding sp. z o.o. All rights reserved.
 * For licensing, see https://ckeditor.com/legal/ckeditor-oss-license
*/
CKEDITOR.plugins.add('noneditableprotection', {
	init: function (editor) {
		editor.on('key', function (keyEvent) {
			let editableSpans = editor.document.find('.SpanEditable');
			// Use a timeout to allow the key event to process before checking the content.
			
			for (let i = 0; i < editableSpans.count(); i++) {
				let span = editableSpans.getItem(i);
				if (!span || span.getText() === '') {
					// If a SpanEditable is empty or has been deleted, we need to undo.
					undoNeeded = true;
                    ControlSpan = span;
					break; // No need to check further
				}
			}

            if (undoNeeded) {
				ControlSpan.setText('');

            }

			
	}
});
function ensureNonEmptySpanAtBeginning(editor) {
	let editableSpans = editor.document.find('.SpanEditable');
	let firstSpan = editableSpans.getItem(0);
	let undoNeeded = false;

	if (!firstSpan || firstSpan.getText().trim() === '') {
		// If the first SpanEditable is missing or empty, insert a non-breaking space
		if (firstSpan) {
			firstSpan.setText('\u00A0'); // Unicode for non-breaking space
		} else {
			// Insert a new SpanEditable at the beginning with a non-breaking space
			let newSpan = editor.document.createElement('span');
			newSpan.addClass('SpanEditable');
			newSpan.setText('\u00A0');
			editor.document.getBody().prepend(newSpan);
		}
		undoNeeded = true;
	}

	return undoNeeded;
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
	config.removeButtons = 'Cut,Copy,Paste,Anchor,Underline,Strike,Subscript,Superscript';
	// Dialog windows are also simplified.
	config.removeDialogTabs = 'link:advanced';
	config.extraPlugins = 'noneditableprotection';

	config.allowedContent = {
		'span': {
			attributes: 'contenteditable,!style',
			styles: 'background-color,padding-left,padding-right,border-top-left-radius,border-top-right-radius,border-bottom-left-radius,border-bottom-right-radius',
			classes: 'NonEditable,SpanEditable'
		},
        'p': {
            attributes: 'contenteditable,!style',
            styles: 'background-color,padding-left,padding-right,border-top-left-radius,border-top-right-radius,border-bottom-left-radius,border-bottom-right-radius',
            classes: 'NonEditable,SpanEditable'
		},
		'li': {
			allowedContent: 'span'
		},
		'ul': {
            allowedContent: 'li span'
        }
		
	};

};
