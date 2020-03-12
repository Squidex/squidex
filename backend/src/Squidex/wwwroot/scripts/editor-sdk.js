
function SquidexFormField() {
    var initHandler;
    var initCalled = false;
    var disabledHandler;
    var disabled = false;
    var valueHandler;
    var value;
    var formValueHandler;
    var formValue;
    var context;
    var timer;
    var height = document.body.offsetHeight;

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
            } else if (type === 'init') {
                context = event.data.context;

                raiseInit();
            }
        }
    }

    document.body.style.margin = '0';
    document.body.style.padding = '0';

    window.addEventListener('message', eventListener, false);

    window.parent.postMessage({ type: 'started' }, '*');
    window.parent.postMessage({ type: 'resize', height: height }, '*');

    timer = setInterval(function () {
        var newHeight = document.body.offsetHeight;

        if (height !== newHeight) {
            height = newHeight;

            window.parent.postMessage({ type: 'resize', height: height }, '*');
        }
    }, 500);

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

        /**
         * Notifies the control container that the editor has been touched.
         */
        touched: function () {
            if (window.parent) {
                window.parent.postMessage({ type: 'touched' }, '*');
            }
        },

        /**
         * Notifies the control container that the value has been changed.
         */
        valueChanged: function (newValue) {
            value = newValue;

            if (window.parent) {
                window.parent.postMessage({ type: 'valueChanged', value: newValue }, '*');
            }
        },

        /**
         * Register the init handler.
         */
        onInit: function (callback) {
            initHandler = callback;

            raiseInit();
        },

        /**
         * Register the disabled handler.
         */
        onDisabled: function (callback) {
            disabledHandler = callback;

            raiseDisabled();
        },

        /**
         * Register the value changed handler.
         */
        onValueChanged: function (callback) {
            valueHandler = callback;

            raiseValueChanged();
        },
        
        /**
         * Register the form value changed handler.
         */
        onFormValueChanged: function (callback) {
            formValueHandler = callback;

            raiseFormValueChanged();
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