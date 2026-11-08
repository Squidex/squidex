/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { booleanAttribute, Component, effect, ElementRef, inject, input, numberAttribute, signal } from '@angular/core';
import * as ProgressBar from 'progressbar.js';
import { ThemeService } from '@app/framework/internal';

@Component({
    selector: 'sqx-progress-bar',
    styles: [`
        :host ::ng-deep svg {
            vertical-align: middle
        }`,
    ],
    template: '',
    host: {
        style: 'display: block',
    },
})
export class ProgressBarComponent {
    private readonly element = inject(ElementRef);
    private readonly themeService = inject(ThemeService);

    /** Renders a horizontal line or a circle. */
    public readonly mode = input<'Line' | 'Circle' | string>('Line');

    /** The color of the progress. Falls back to the theme color. */
    public readonly color = input<string>();

    /** The color of the remaining part. Falls back to the theme color. */
    public readonly trailColor = input<string>();

    /** The width of the remaining part. */
    public readonly trailWidth = input(4, { transform: numberAttribute });

    /** The width of the progress. */
    public readonly strokeWidth = input(4, { transform: numberAttribute });

    /** Shows the value as percent text. */
    public readonly showText = input(true, { transform: booleanAttribute });

    /** Animates value changes. */
    public readonly animated = input(true, { transform: booleanAttribute });

    /** The progress in percent, between 0 and 100. */
    public readonly value = input(0, { transform: numberAttribute });

    protected readonly progressBar = signal<any>(null);

    constructor() {
        effect(onCleanup => {
            const options = {
                color: this.getColor(),
                trailColor: this.getTrailColor(),
                trailWidth: this.trailWidth(),
                strokeWidth: this.strokeWidth(),
                svgStyle: { width: '100%', height: '100%' },
            };

            const progressBar = this.mode() === 'Circle' ?
                new ProgressBar.Circle(this.element.nativeElement, options) :
                new ProgressBar.Line(this.element.nativeElement, options);

            this.progressBar.set(progressBar);

            onCleanup(() => progressBar.destroy());
        });

        effect(() => {
            const progressBar = this.progressBar();
            if (!progressBar) {
                return;
            }

            const value = this.value();
            if (this.animated()) {
                progressBar.animate(value / 100);
            } else {
                progressBar.set(value / 100);
            }

            if (value > 0 && this.showText()) {
                progressBar.setText(`${Math.round(value)}%`);
            }
        });

        effect(() => {
            this.themeService.theme();

            const progressBar = this.progressBar();

            progressBar?.path?.setAttribute('stroke', this.getColor());
            progressBar?.trail?.setAttribute('stroke', this.getTrailColor());
        });
    }

    private getColor() {
        return this.color() || this.themeService.getColor('progress');
    }

    private getTrailColor() {
        return this.trailColor() || this.themeService.getColor('progress-trail');
    }
}
