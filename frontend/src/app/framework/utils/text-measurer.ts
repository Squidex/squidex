/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ElementRef } from '@angular/core';
import { Types } from '../internal';

let CANVAS: HTMLCanvasElement | null = null;

export class TextMeasurer {
    private font?: string;

    constructor(
        private readonly element: () => any | ElementRef<any>,
    ) {
    }

    public getTextSize(text: string) {
        if (!CANVAS) {
            CANVAS = document.createElement('canvas');
        }

        if (!this.font) {
            let currentElement = this.element();

            if (Types.is(currentElement, ElementRef)) {
                currentElement = currentElement.nativeElement;
            }

            if (!currentElement) {
                return -1000;
            }

            const style = window.getComputedStyle(currentElement);

            const fontSize = style.getPropertyValue('font-size');
            const fontFamily = style.getPropertyValue('font-family');

            if (!fontSize || !fontFamily) {
                return -1000;
            }

            this.font = `${fontSize} ${fontFamily}`;
        }

        if (!this.font) {
            return -1000;
        }

        const ctx = CANVAS.getContext('2d');

        if (!ctx) {
            return -1000;
        }

        ctx.font = this.font;

        return ctx.measureText(text).width;
    }
}