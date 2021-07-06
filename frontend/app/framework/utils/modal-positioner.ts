/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

const POSITION_TOP_CENTER = 'top';
const POSITION_TOP_LEFT = 'top-left';
const POSITION_TOP_RIGHT = 'top-right';
const POSITION_BOTTOM_CENTER = 'bottom';
const POSITION_BOTTOM_LEFT = 'bottom-left';
const POSITION_BOTTOM_RIGHT = 'bottom-right';
const POSITION_LEFT_CENTER = 'left';
const POSITION_LEFT_TOP = 'left-top';
const POSITION_LEFT_BOTTOM = 'left-bottom';
const POSITION_RIGHT_CENTER = 'right';
const POSITION_RIGHT_TOP = 'right-top';
const POSITION_RIGHT_BOTTOM = 'right-bottom';

export function positionModal(targetRect: ClientRect, modalRect: ClientRect, relativePosition: string, offset: number, fix: boolean, viewportWidth: number, viewportHeight: number): { x: number; y: number } {
    let y = 0;
    let x = 0;

    switch (relativePosition) {
        case POSITION_LEFT_TOP:
        case POSITION_RIGHT_TOP: {
            y = targetRect.top;
            break;
        }
        case POSITION_LEFT_BOTTOM:
        case POSITION_RIGHT_BOTTOM: {
            y = targetRect.bottom - modalRect.height;
            break;
        }
        case POSITION_BOTTOM_CENTER:
        case POSITION_BOTTOM_LEFT:
        case POSITION_BOTTOM_RIGHT: {
            y = targetRect.bottom + offset;

            if (fix && y + modalRect.height > viewportHeight) {
                const candidate = targetRect.top - modalRect.height - offset;

                if (candidate > 0) {
                    y = candidate;
                }
            }
            break;
        }
        case POSITION_TOP_CENTER:
        case POSITION_TOP_LEFT:
        case POSITION_TOP_RIGHT: {
            y = targetRect.top - modalRect.height - offset;

            if (fix && y < 0) {
                const candidate = targetRect.bottom + offset;

                if (candidate + modalRect.height < viewportHeight) {
                    y = candidate;
                }
            }
            break;
        }
        case POSITION_LEFT_CENTER:
        case POSITION_RIGHT_CENTER:
            y = targetRect.top + targetRect.height * 0.5 - modalRect.height * 0.5;
            break;
    }

    switch (relativePosition) {
        case POSITION_TOP_LEFT:
        case POSITION_BOTTOM_LEFT: {
            x = targetRect.left;
            break;
        }
        case POSITION_TOP_RIGHT:
        case POSITION_BOTTOM_RIGHT: {
            x = targetRect.right - modalRect.width;
            break;
        }
        case POSITION_RIGHT_CENTER:
        case POSITION_RIGHT_TOP:
        case POSITION_RIGHT_BOTTOM: {
            x = targetRect.right + offset;

            if (fix && x + modalRect.width > viewportWidth) {
                const candidate = targetRect.left - modalRect.width - offset;

                if (candidate > 0) {
                    x = candidate;
                }
            }
            break;
        }
        case POSITION_LEFT_CENTER:
        case POSITION_LEFT_TOP:
        case POSITION_LEFT_BOTTOM: {
            x = targetRect.left - modalRect.width - offset;

            if (fix && x < 0) {
                const candidate = targetRect.right + offset;

                if (candidate + modalRect.width < viewportWidth) {
                    x = candidate;
                }
            }
            break;
        }
        case POSITION_TOP_CENTER:
        case POSITION_BOTTOM_CENTER:
            x = targetRect.left + targetRect.width * 0.5 - modalRect.width * 0.5;
            break;
    }

    return { x, y };
}
