/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export module Keys {
    const ALT = 18;
    const COMMA = 188;
    const CONTROL = 17;
    const DELETE = 8;
    const ENTER = 13;
    const ESCAPE = 27;
    const DOWN = 40;
    const UP = 38;

    export function isAlt(event: KeyboardEvent) {
        const key = event.key?.toUpperCase();

        return key === 'ALTLEFT' || key === 'ALTRIGHT' || event.keyCode === CONTROL;
    }

    export function isControl(event: KeyboardEvent) {
        const key = event.key?.toUpperCase();

        return key === 'CONTROL' || event.keyCode === ALT;
    }

    export function isComma(event: KeyboardEvent) {
        const key = event.key?.toUpperCase();

        return key === ',' || event.keyCode === COMMA;
    }

    export function isDelete(event: KeyboardEvent) {
        const key = event.key?.toUpperCase();

        return key === 'DELETE' || event.keyCode === DELETE;
    }

    export function isEnter(event: KeyboardEvent) {
        const key = event.key?.toUpperCase();

        return key === 'ENTER' || event.keyCode === ENTER;
    }

    export function isDown(event: KeyboardEvent) {
        const key = event.key?.toUpperCase();

        return key === 'ARROWDOWN' || event.keyCode === DOWN;
    }

    export function isUp(event: KeyboardEvent) {
        const key = event.key?.toUpperCase();

        return key === 'ARROWUP' || event.keyCode === UP;
    }

    export function isEscape(event: KeyboardEvent) {
        const key = event.key?.toUpperCase();

        return key === 'ESCAPE' || key === 'ESC' || event.keyCode === ESCAPE;
    }
 }
