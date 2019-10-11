var isFirstCall = true;
var currentCommentaryTypeId = '';
var currentCharacterCount = 0;
var currentTextLength = 0;
var field = new SquidexFormField();
var editor = new tui.Editor({
    el: document.getElementById('mdEditor'),
    height: '300px',
    hideModeSwitch: true,
    initialEditType: 'wysiwyg',
    toolbarItems: ["bold", "italic", "divider", "ul", "ol", "divider"],
    events: {
        change: function () {
            if (isFirstCall) {
                isFirstCall = false;
            } else {
                var data = editor.getMarkdown();
                field.valueChanged(data);
                updaterFooter(data);
            }
        },
        blur: function () {
            if (isFirstCall) {
                isFirstCall = false;
            } else {
                var data = editor.getMarkdown();
                field.valueChanged(data);
                updaterFooter(data);
            }
        }
    }
});

field.onValueChanged(function (value) {
    if (value) {
        editor.setValue(value);
        updaterFooter(value);
    }
});

updateFooterText();

field.onFormValueChanged(function (value) {
    let commentaryTypeId = '';

    if (value && value.commentarytype && value.commentarytype.iv) {
        commentaryTypeId = value.commentarytype.iv[0] || '';
    }

    if (currentCommentaryTypeId !== commentaryTypeId) {
        currentCommentaryTypeId = commentaryTypeId;
        
        if (commentaryTypeId) {
            fetchCommentaryCount();
        } else {
            currentCharacterCount = 0;
            updateFooterText();
        }
    }
});

function fetchCommentaryCount() {
    const apiKey = field.getContext().user.user.access_token;
    const apiUrl = field.getContext().apiUrl;

    fetch(`${apiUrl}/content/commentary/commentary-type/${currentCommentaryTypeId}`, {
            method: 'GET',
            headers: {
                'Content-Type': 'application/json',
                'Authorization': `Bearer ${apiKey}`,
                'X-Unpublished': '1'
            },
        })
        .then(x => x.json())
        .then(x => {
            if (x.dataDraft && x.dataDraft['character-limit']) {
                this.currentCharacterCount = x.dataDraft['character-limit'].iv;
                updateFooterText();
            }
        });
}

var renderer = new PlainTextRenderer();

function updaterFooter(markdown) {
    if (markdown) {
        var text = marked(markdown, { renderer: renderer });

        currentTextLength = text.length;
    } else {
        currentTextLength = 0;
    }

    updateFooterText();
}

function updateFooterText() {
    var footerText = `character count: ${currentTextLength}`;

    if (currentCharacterCount > 0) {
        footerText += `/${currentCharacterCount}`;
    }

    var footer = document.getElementById('footer');

    if (currentCharacterCount > 0 && currentTextLength > currentCharacterCount) {
        footer.className = 'footer error';
    } else {
        footer.className = 'footer';
    }
    
    footer.innerText = footerText;
}

function PlainTextRenderer() {
}

PlainTextRenderer.prototype.code = function (code, lang, escaped) {
    return code
}
PlainTextRenderer.prototype.blockquote = function (quote) {
    return quote;
}
PlainTextRenderer.prototype.html = function (html) {
    return html;
}
PlainTextRenderer.prototype.heading = function (text, level, raw) {
    return text;
}
PlainTextRenderer.prototype.hr = function () {
    return '';
}
PlainTextRenderer.prototype.list = function (body, ordered) {
    return body;
}
PlainTextRenderer.prototype.listitem = function (text) {
    return text;
}
PlainTextRenderer.prototype.paragraph = function (text) {
    return text;
}
PlainTextRenderer.prototype.table = function (header, body) {
    return header + body;
}
PlainTextRenderer.prototype.tablerow = function (content) {
    return content;
}
PlainTextRenderer.prototype.tablecell = function (content, flags) {
    return content;
}
PlainTextRenderer.prototype.strong = function (text) {
    return text;
}
PlainTextRenderer.prototype.em = function (text) {
    return text;
}
PlainTextRenderer.prototype.codespan = function (text) {
    return text;
}
PlainTextRenderer.prototype.br = function () {
    return '';
}
PlainTextRenderer.prototype.del = function (text) {
    return text;
}
PlainTextRenderer.prototype.link = function (href, title, text) {
    return text;
}
PlainTextRenderer.prototype.image = function (href, title, text) {
    return text || '';
}
PlainTextRenderer.prototype.text = function (text) {
    return text;
}
