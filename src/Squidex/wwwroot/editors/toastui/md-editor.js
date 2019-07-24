var isFirstCall = true;
var field = new SquidexFormField();
var editor = new tui.Editor({
    el: document.getElementById('mdEditor'),
    height: '300px',
    hideModeSwitch: true,
    initialEditType: 'wysiwyg',
    toolbarItems: ["bold","italic","divider","ul","ol","divider"],
    events: {
        change: function () {
			if (isFirstCall) {
				isFirstCall = false;
			} else {
				var data = editor.getMarkdown();
				field.valueChanged(data);
			}
        }
    }
});

field.onValueChanged(function (value) {
    if (value) {
        editor.setValue(value);
    }
});