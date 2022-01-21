import { h, render } from 'preact';
import { OverlayContainer } from './components/overlay-container';

let element: HTMLDivElement | null = null;

export function renderOverlay() {
    if (!element) {
        element = document.body.appendChild(document.createElement('div'));
    }

    render(<OverlayContainer />, element)
}