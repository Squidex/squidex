/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export module Keys {
    const COMMA = 188;
    const DELETE = 8;
    const ENTER = 13;
    const ESCAPE = 27;
    const DOWN = 40;
    const UP = 38;

    export function isComma(event: KeyboardEvent) {
        const key = event.key || event.keyCode;

        return key === ',' || key === COMMA;
    }

    export function isDelete(event: KeyboardEvent) {
        const key = event.key?.toUpperCase() || event.keyCode;

        return key === 'DELETE' || key === DELETE;
    }

    export function isEnter(event: KeyboardEvent) {
        const key = event.key?.toUpperCase() || event.keyCode;

        return key === 'ENTER' || key === ENTER;
    }

    export function isDown(event: KeyboardEvent) {
        const key = event.key?.toUpperCase() || event.keyCode;

        return key === 'ARROWDOWN' || key === DOWN;
    }

    export function isUp(event: KeyboardEvent) {
        const key = event.key?.toUpperCase() || event.keyCode;

        return key === 'ARROWUP' || key === UP;
    }

    export function isEscape(event: KeyboardEvent) {
        const key = event.key?.toUpperCase() || event.keyCode;

        return key === 'ESCAPE' || key === 'ESC' || key === ESCAPE;
    }
 }
