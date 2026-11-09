/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export namespace Keys {
    export function isComma(event: KeyboardEvent) {
        return event.key === ',';
    }

    export function isDelete(event: KeyboardEvent) {
        return event.key === 'Delete' || event.key === 'Backspace';
    }

    export function isEnter(event: KeyboardEvent) {
        return event.key === 'Enter';
    }

    export function isDown(event: KeyboardEvent) {
        return event.key === 'ArrowDown';
    }

    export function isUp(event: KeyboardEvent) {
        return event.key === 'ArrowUp';
    }

    export function isEscape(event: KeyboardEvent) {
        return event.key === 'Escape' || event.key === 'Esc';
    }
}
