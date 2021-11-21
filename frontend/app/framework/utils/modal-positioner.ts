/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export type RelativePosition =
    'bottom-center' |
    'bottom-left' |
    'bottom-right' |
    'left-bottom' |
    'left-center' |
    'left-top' |
    'right-bottom' |
    'right-center' |
    'right-top' |
    'top-center' |
    'top-left' |
    'top-right';

const POSITION_BOTTOM_CENTER = 'bottom-center';
const POSITION_BOTTOM_LEFT = 'bottom-left';
const POSITION_BOTTOM_RIGHT = 'bottom-right';
const POSITION_LEFT_BOTTOM = 'left-bottom';
const POSITION_LEFT_CENTER = 'left-center';
const POSITION_LEFT_TOP = 'left-top';
const POSITION_RIGHT_BOTTOM = 'right-bottom';
const POSITION_RIGHT_CENTER = 'right-center';
const POSITION_RIGHT_TOP = 'right-top';
const POSITION_TOP_CENTER = 'top-center';
const POSITION_TOP_LEFT = 'top-left';
const POSITION_TOP_RIGHT = 'top-right';

export type PositionResult = {
    x: number;
    y: number;
    xMax: number;
    yMax: number;
};

export function positionModal(targetRect: DOMRect, modalRect: DOMRect, relativePosition: RelativePosition, offset: number, fix: boolean, clientWidth: number, clientHeight: number): PositionResult {
    let y = 0;
    let x = 0;

    // Available space in x/y direction.
    let xMax = 0;
    let yMax = 0;

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

            yMax = clientHeight - y;
            // Unset yMax if we have enough space.
            if (modalRect.height <= yMax) {
                yMax = 0;
            } else if (fix) {
                // Find a position at the other side of the rect (top).
                const candidate = targetRect.top - modalRect.height - offset;

                if (candidate > 0) {
                    y = candidate;
                    // Reset space to zero (full space), becuase we fix only if we have the space.
                    yMax = 0;
                }
            }
            break;
        }
        case POSITION_TOP_CENTER:
        case POSITION_TOP_LEFT:
        case POSITION_TOP_RIGHT: {
            y = targetRect.top - modalRect.height - offset;

            yMax = targetRect.top - offset;
            // Unset yMax if we have enough space.
            if (modalRect.height <= yMax) {
                yMax = 0;
            } else if (fix) {
                // Find a position at the other side of the rect (bottom).
                const candidate = targetRect.bottom + offset;

                if (candidate + modalRect.height < clientHeight) {
                    y = candidate;
                    // Reset space to zero (full space), becuase we fix only if we have the space.
                    yMax = 0;
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

            xMax = clientWidth - x;
            // Unset xMax if we have enough space.
            if (modalRect.width <= xMax) {
                xMax = 0;
            } else if (fix) {
                // Find a position at the other side of the rect (left).
                const candidate = targetRect.left - modalRect.width - offset;

                if (candidate > 0) {
                    x = candidate;
                    // Reset space to zero (full space), becuase we fix only if we have the space.
                    xMax = 0;
                }
            }
            break;
        }
        case POSITION_LEFT_CENTER:
        case POSITION_LEFT_TOP:
        case POSITION_LEFT_BOTTOM: {
            x = targetRect.left - modalRect.width - offset;

            xMax = targetRect.left - offset;
            // Unset xMax if we have enough space.
            if (modalRect.width <= xMax) {
                xMax = 0;
            } else if (fix) {
                // Find a position at the other side of the rect (right).
                const candidate = targetRect.right + offset;

                if (candidate + modalRect.width < clientWidth) {
                    x = candidate;
                    // Reset space to zero (full space), becuase we fix only if we have the space.
                    xMax = 0;
                }
            }
            break;
        }
        case POSITION_TOP_CENTER:
        case POSITION_BOTTOM_CENTER:
            x = targetRect.left + targetRect.width * 0.5 - modalRect.width * 0.5;
            break;
    }

    return { x, y, xMax, yMax };
}
