import './style/index.scss';
import { renderOverlay } from './render';

function init(script: HTMLOrSVGScriptElement | null) {
    renderOverlay();
    renderStyles(script);
}

function renderStyles(script: HTMLOrSVGScriptElement | null) {
    const src = (script as any)?.['src'] as string;

    if (!src) {
        return;
    }

    const url = src.substring(0, src.lastIndexOf('/')) + '/' + 'embed-sdk.css';

    const styleElement = document.createElement('link');

    styleElement.rel = 'stylesheet';
    styleElement.href = url;
    styleElement.type = 'text/css';

    document.head?.appendChild(styleElement);
}

let script = document.currentScript;

if (document.readyState === 'complete') {
    init(script);
} else {
    window.addEventListener('load', () => init(script));
}