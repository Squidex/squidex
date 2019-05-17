function getCharacterCount(editor)
{
    // Get the contents of the wysiwyg editor
    var contents = editor.wwEditor.getValue();
    
    // Remove tags, space-tags and spaces from character-count
    contents = contents.replace(/(<([^>]+)>)/ig,'');
    contents = contents.replace(/&nbsp;/gi,'');
    contents = contents.replace(' ', '');

    var charCountDiv = document.getElementById("charCount");
    charCountDiv.innerHTML = contents.length;
}

var field = new SquidexFormField();
var editor = new tui.Editor({
    el: document.getElementById('mdEditor'),
    height: '300px',
    hideModeSwitch: true,
    initialEditType: 'wysiwyg',
    toolbarItems: ["bold","italic","divider","ul","ol","divider"],
    events: {
        change: function () {
            var data = editor.getMarkdown();
            field.valueChanged(data);
            getCharacterCount(editor);
        }


    }
});

field.onValueChanged(function (value) {
    if (value) {
        editor.setValue(value);
        getCharacterCount(editor);
    }
});