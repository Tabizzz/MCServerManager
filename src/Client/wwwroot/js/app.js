function addMonacoTheme() {
	monaco.editor.defineTheme('wsm', {
		base: 'vs-dark', // can also be vs-dark or hc-black
		inherit: true, // can also be false to completely replace the builtin rules
		rules: [
			{token: '', foreground: 'D4D4D4', background: '000000'},
			{token: 'comment', foreground: '545454'},   // comments
			{ token: 'type', foreground: 'F78C6C' },    // keys
			{ token: 'keyword', foreground: 'C792EA' }, // bools
			{ token: 'string', foreground: 'C3E88D' },  // strings
			{ token: 'number', foreground: 'FF5370' },  // numbers
			{ token: 'number.hex', foreground: 'FF5370' },
		],
		colors: {
			'editor.lineHighlightBorder': '#28282828',
			'editor.background': '#00000000' //inherit background color from parent
		}
	});
}

function triggerFileDownload (fileName, url) {
	const anchorElement = document.createElement('a');
	anchorElement.href = url;
	anchorElement.download = fileName ?? '';
	anchorElement.click();
	anchorElement.remove();
}