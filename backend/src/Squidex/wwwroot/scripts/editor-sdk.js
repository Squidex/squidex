function measureAndNotifyParent() {
    var height = 0;

    document.body.style.margin = '0';
    document.body.style.padding = '0';

    window.parent.postMessage({ type: 'started' }, '*');

    function notifySize() {
        var newHeight = document.body.offsetHeight;

        if (height !== newHeight) {
            height = newHeight;

            if (window.parent) {
                window.parent.postMessage({ type: 'resize', height: height }, '*');
            }
        }

        window.parent.postMessage({ type: 'resize', height: height }, '*');
    }

    notifySize();

    return setInterval(function () {
        notifySize();
    }, 50);
}

function SquidexPlugin() {
    var initHandler;
    var initCalled = false;
    var contentHandler;
    var content;
    var context;
    var timer;

    function raiseContentChanged() {
        if (contentHandler && content) {
            contentHandler(content);
        }
    }

    function raiseInit() {
        if (initHandler && !initCalled && context) {
            initHandler(context);
            initCalled = true;
        }
    }

    function eventListener(event) {
        if (event.source !== window) {
            var type = event.data.type;
            
            if (type === 'contentChanged') {
                content = event.data.content;

                raiseContentChanged();
            } else if (type === 'init') {
                context = event.data.context;

                raiseInit();
            }
        }
    }

    window.addEventListener('message', eventListener, false);

    timer = measureAndNotifyParent();

    var editor = {
        /**
         * Get the current value.
         */
        getContext: function () {
            return context;
        },

        /*
         * Notifies the parent to navigate to the path.
         */
        navigate: function (url) {
            if (window.parent) {
                window.parent.postMessage({ type: 'navigate', url: url }, '*');
            }
        },

        /**
         * Register an function that is called when the sidebar is initialized.
         */
        onInit: function (callback) {
            initHandler = callback;

            raiseInit();
        },

        /**
         * Register an function that is called whenever the value of the content has changed.
         * 
         * The callback has one argument with the value of the content (any).
         */
        onContentChanged: function (callback) {
            contentHandler = callback;

            raiseContentChanged();
        },

        /**
         * Clean the editor SDK.
         */
        clean: function () {
            if (timer) {
                window.removeEventListener('message', eventListener);

                timer();
            }
        }
    };

    return editor;

}

function SquidexFormField() {
    var initHandler;
    var initCalled = false;
    var disabledHandler;
    var disabled = false;
    var fullscreen = false;
    var fullscreenHandler = false;
    var valueHandler;
    var value;
    var languageHandler;
    var language;
    var formValueHandler;
    var formValue;
    var context;
    var timer;

    function raiseDisabled() {
        if (disabledHandler) {
            disabledHandler(disabled);
        }
    }

    function raiseValueChanged() {
        if (valueHandler) {
            valueHandler(value);
        }
    }

    function raiseFormValueChanged() {
        if (formValueHandler && formValue) {
            formValueHandler(formValue);
        }
    }

    function raiseLanguageChanged() {
        if (languageHandler && language) {
            languageHandler(language);
        }
    }

    function raiseFullscreen() {
        if (fullscreenHandler) {
            fullscreenHandler(fullscreen);
        }
    }

    function raiseInit() {
        if (initHandler && !initCalled && context) {
            initHandler(context);
            initCalled = true;
        }
    }

    function eventListener(event) {
        if (event.source !== window) {
            var type = event.data.type;

            if (type === 'disabled') {
                if (disabled !== event.data.isDisabled) {
                    disabled = event.data.isDisabled;

                    raiseDisabled();
                }
            } else if (type === 'valueChanged') {
                value = event.data.value;

                raiseValueChanged();
            } else if (type === 'formValueChanged') {
                formValue = event.data.formValue;

                raiseFormValueChanged();
            } else if (type === 'fullscreenChanged') {
                fullscreen = event.data.fullscreen;

                raiseFullscreen();
            } else if (type === 'languageChanged') {
                language = event.data.language;

                raiseLanguageChanged();                 
            } else if (type === 'init') {
                context = event.data.context;

                raiseInit();
            }
        }
    }

    window.addEventListener('message', eventListener, false);

    timer = measureAndNotifyParent();

    var editor = {
        /**
         * Get the current value.
         */
        getValue: function () {
            return value;
        },

        /**
         * Get the current value.
         */
        getContext: function () {
            return context;
        },

        /**
         * Get the current form value.
         */
        getFormValue: function () {
            return formValue;
        },

        /*
         * Get the current field language.
         */
        getLanguage: function () {
            return language;
        },

        /*
         * Get the disabled state.
         */
        isDisabled: function () {
            return disabled;
        },

        /*
         * Get the fullscreen state.
         */
        isFullscreen: function () {
            return fullscreen;
        },

        /**
         * Notifies the control container that the editor has been touched.
         */
        touched: function () {
            if (window.parent) {
                window.parent.postMessage({ type: 'touched' }, '*');
            }
        },

        /*
         * Notifies the parent to navigate to the path.
         *
         * @params url: string: The url to navigate to.
         */
        navigate: function (url) {
            if (window.parent) {
                window.parent.postMessage({ type: 'navigate', url: url }, '*');
            }
        },

        /*
         * Notifies the parent to go to fullscreen mode.
         *
         * @params mode: boolean: The fullscreen mode.
         */
        toggleFullscreen: function () {
            if (window.parent) {
                window.parent.postMessage({ type: 'fullscreen', mode: !fullscreen }, '*');
            }
        },

        /**
         * Notifies the control container that the value has been changed.
         *
         * @params newValue: any: The new field value.
         */
        valueChanged: function (newValue) {
            value = newValue;

            if (window.parent) {
                window.parent.postMessage({ type: 'valueChanged', value: newValue }, '*');
            }
        },

        /**
         * Register an function that is called when the field is initialized.
         */
        onInit: function (callback) {
            initHandler = callback;

            raiseInit();
        },

        /**
         * Register an function that is called whenever the field is disabled or enabled.
         * 
         * The callback has one argument with disabled state (disabled = true, enabled = false).
         */
        onDisabled: function (callback) {
            disabledHandler = callback;

            raiseDisabled();
        },

        /**
         * Register an function that is called whenever the field language is changed.
         * 
         * The callback has one argument with the language of the field (string).
         */
        onLanguageChanged: function (callback) {
            languageHandler = callback;

            raiseLanguageChanged();
        },

        /**
         * Register an function that is called whenever the value of the field has changed.
         * 
         * The callback has one argument with the value of the field (any).
         */
        onValueChanged: function (callback) {
            valueHandler = callback;

            raiseValueChanged();
        },

        /**
         * Register an function that is called whenever the value of the content has changed.
         * 
         * The callback has one argument with the value of the content (any).
         */
        onFormValueChanged: function (callback) {
            formValueHandler = callback;

            raiseFormValueChanged();
        },

        /**
         * Register an function that is called whenever the fullscreen mode has changed.
         * 
         * The callback has one argument with fullscreen state (fullscreen on = true, fullscreen off = false).
         */
        onFullscreen: function (callback) {
            fullscreenHandler = callback;

            raiseFullscreen();
        },

        /**
         * Clean the editor SDK.
         */
        clean: function () {
            if (timer) {
                window.removeEventListener('message', eventListener);

                timer();
            }
        }
    };

    return editor;
};