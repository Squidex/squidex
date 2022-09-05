/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Types } from './types';

export type AnchorX =
    'center' |
    'left-to-right' |
    'left-to-left' |
    'right-to-left' |
    'right-to-right';

export type AnchorY =
    'bottom-to-bottom' |
    'bottom-to-top' |
    'center' |
    'top-to-bottom' |
    'top-to-top';

export type RelativePosition = SimplePosition | [AnchorX, AnchorY];

export type SimplePosition =
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

export function computeAnchors(value: RelativePosition): [AnchorX, AnchorY] {
    if (Types.isArray(value)) {
        return value;
    }

    switch (value) {
        case 'bottom-center':
            return ['center', 'top-to-bottom'];
        case 'bottom-left':
            return ['left-to-left', 'top-to-bottom'];
        case 'bottom-right':
            return ['right-to-right', 'top-to-bottom'];
        case 'left-bottom':
            return ['right-to-left', 'bottom-to-bottom'];
        case 'left-center':
            return ['right-to-left', 'center'];
        case 'left-top':
            return ['right-to-left', 'top-to-top'];
        case 'right-bottom':
            return ['left-to-right', 'bottom-to-bottom'];
        case 'right-center':
            return ['left-to-right', 'center'];
        case 'right-top':
            return ['left-to-right', 'top-to-top'];
        case 'top-center':
            return ['center', 'bottom-to-top'];
        case 'top-left':
            return ['left-to-left', 'bottom-to-top'];
        case 'top-right':
            return ['right-to-right', 'bottom-to-top'];
        default:
            return ['center', 'center'];
    }
}

export type PositionResult = {
    height?: number;
    maxHeight: number;
    maxWidth: number;
    width?: number;
    x: number;
    y: number;
};

export type PositionRequest = {
    adjust?: boolean;
    anchorX: AnchorX;
    anchorY: AnchorY;
    clientHeight: number;
    clientWidth: number;
    computeHeight?: boolean;
    computeWidth?: boolean;
    modalRect: DOMRect;
    offsetX?: number;
    offsetY?: number;
    spaceX?: number;
    spaceY?: number;
    targetRect: DOMRect;
};

export function positionModal(request: PositionRequest): PositionResult {
    const {
        adjust,
        anchorX,
        anchorY,
        clientHeight,
        clientWidth,
        computeHeight,
        computeWidth,
        modalRect,
        offsetX,
        offsetY,
        spaceX,
        spaceY,
        targetRect,
    } = request;

    const actualOffsetX = offsetX || 0;
    const actualOffsetY = offsetY || 0;

    let height = 0;
    let maxHeight = 0;
    let maxWidth = 0;
    let width = 0;
    let x = 0;
    let y = 0;

    switch (anchorY) {
        case 'center':
            y = targetRect.top + targetRect.height * 0.5 - modalRect.height * 0.5;
            break;
        case 'top-to-top': {
            y = targetRect.top + actualOffsetY;
            break;
        }
        case 'top-to-bottom': {
            y = targetRect.bottom + actualOffsetY;

            maxHeight = clientHeight - y;

            if (modalRect.height <= maxHeight) {
                // Unset maxHeight if we have enough space.
                maxHeight = 0;
            } else if (adjust) {
                // Find a position at the other side of the rect (top).
                const candidate = targetRect.top - modalRect.height - actualOffsetY;

                if (candidate > 0) {
                    y = candidate;
                    // Reset space to zero (full space), because we fix only if we have the space.
                    maxHeight = 0;
                }
            }
            break;
        }
        case 'bottom-to-bottom': {
            y = targetRect.bottom - modalRect.height - actualOffsetY;
            break;
        }
        case 'bottom-to-top': {
            y = targetRect.top - modalRect.height - actualOffsetY;

            maxHeight = targetRect.top - actualOffsetY;

            if (modalRect.height <= maxHeight) {
                // Unset maxHeight if we have enough space.
                maxHeight = 0;
            } else if (adjust) {
                // Find a position at the other side of the rect (bottom).
                const candidate = targetRect.bottom + actualOffsetY;

                if (candidate + modalRect.height < clientHeight) {
                    y = candidate;
                    // Reset space to zero (full space), because we fix only if we have the space.
                    maxHeight = 0;
                }
            }
            break;
        }
    }

    switch (anchorX) {
        case 'center':
            x = targetRect.left + targetRect.width * 0.5 - modalRect.width * 0.5;
            break;
        case 'left-to-left': {
            x = targetRect.left + actualOffsetX;
            break;
        }
        case 'left-to-right': {
            x = targetRect.right + actualOffsetX;

            maxWidth = clientWidth - x;

            if (modalRect.width <= maxWidth) {
                // Unset maxWidth if we have enough space.
                maxWidth = 0;
            } else if (adjust) {
                // Find a position at the other side of the rect (left).
                const candidate = targetRect.left - modalRect.width - actualOffsetX;

                if (candidate > 0) {
                    x = candidate;
                    // Reset space to zero (full space), because we fix only if we have the space.
                    maxWidth = 0;
                }
            }
            break;
        }
        case 'right-to-right': {
            x = targetRect.right - modalRect.width - actualOffsetX;
            break;
        }
        case 'right-to-left': {
            x = targetRect.left - modalRect.width - actualOffsetX;

            maxWidth = targetRect.left - actualOffsetX;

            if (modalRect.width <= maxWidth) {
                // Unset maxWidth if we have enough space.
                maxWidth = 0;
            } else if (adjust) {
                // Find a position at the other side of the rect (right).
                const candidate = targetRect.right + actualOffsetX;

                if (candidate + modalRect.width < clientWidth) {
                    x = candidate;
                    // Reset space to zero (full space), because we fix only if we have the space.
                    maxWidth = 0;
                }
            }
            break;
        }
    }

    if (computeWidth) {
        width = targetRect.width + 2 * (spaceX || 0);
    }

    if (computeHeight) {
        height = targetRect.height + 2 * (spaceY || 0);
    }

    return { x, y, maxWidth, maxHeight, width, height };
}
