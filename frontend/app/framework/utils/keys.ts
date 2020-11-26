/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

export module Keys {
    export const COMMA = 188;
    export const DELETE = 8;
    export const ENTER = 13;
    export const ESCAPE = 27;
    export const DOWN = 40;
    export const UP = 38;

    export function isComma(event: KeyboardEvent) {
        const key = event.key || event.keyCode;

        return key === ',' || key === COMMA;
    }

    export function isDelete(event: KeyboardEvent) {
        const key = event.key || event.keyCode;

        return key === 'Delete' || key === DELETE;
    }

    export function isEnter(event: KeyboardEvent) {
        const key = event.key || event.keyCode;

        return key === 'ENTER' || key === ENTER;
    }

    export function isDown(event: KeyboardEvent) {
        const key = event.key || event.keyCode;

        return key === 'ArrowDown' || key === DOWN;
    }

    export function isUp(event: KeyboardEvent) {
        const key = event.key || event.keyCode;

        return key === 'ArrowUp' || key === UP;
    }

    export function isEscape(event: KeyboardEvent) {
        const key = event.key || event.keyCode;

        return key === 'Escape' || key === 'Esc' || key === UP;
    }
 }