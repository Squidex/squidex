
function SquidexFormField() {
    var disabledHandler;
    var disabled;
    var valueHandler;
    var value;
    var timer;
    var height = document.body.offsetHeight;

    function eventListener(event) {
        if (event.source !== window) {
            if (event.data.type === 'disabled') {
                disabled = event.data.isDisabled;

                if (disabledHandler) {
                    disabledHandler(disabled);
                }
            } else if (event.data.type === 'valueChanged') {
                var value = event.data.value;

                if (valueHandler) {
                    valueHandler(value);
                }
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

            window.parent.postMessage({ type: 'resize', height: height }, '*')
        }
    }, 500);

    var editor = {
        /*
         * Notifies the control container that the editor has been touched.
         */
        touched: function () {
            if (window.parent) {
                window.parent.postMessage({ type: 'touched' }, '*');
            }
        },

        /*
         * Notifies the control container that the value has been changed.
         */
        valueChanged: function (value) {
            if (window.parent) {
                window.parent.postMessage({ type: 'valueChanged', value: value }, '*');
            }
        },

        /*
         * Register the disabled handler.
         */
        onDisabled: function (callback) {
            disabledHandler = callback;

            if (callback) {
                callback(disabled);
            }
        },

        /*
         * Register the disabled handler.
         */
        onValueChanged: function (callback) {
            valueHandler = callback;

            if (callback) {
                callback(value);
            }
        },

        /*
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