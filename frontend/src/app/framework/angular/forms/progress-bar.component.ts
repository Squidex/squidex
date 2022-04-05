/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, ElementRef, Input, OnChanges, OnInit, Renderer2, SimpleChanges } from '@angular/core';
import * as ProgressBar from 'progressbar.js';

@Component({
    selector: 'sqx-progress-bar',
    styles: [`
        :host ::ng-deep svg {
            vertical-align: middle
        }`,
    ],
    template: '',
    changeDetection: ChangeDetectionStrategy.OnPush,
})
export class ProgressBarComponent implements OnChanges, OnInit {
    private progressBar: any;

    @Input()
    public mode = 'Line';

    @Input()
    public color = '#3d7dd5';

    @Input()
    public trailColor = '#f4f4f4';

    @Input()
    public trailWidth = 4;

    @Input()
    public strokeWidth = 4;

    @Input()
    public showText?: boolean | null = true;

    @Input()
    public animated?: boolean | null = true;

    @Input()
    public value = 0;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly element: ElementRef,
        private readonly renderer: Renderer2,
    ) {
        changeDetector.detach();
    }

    public ngOnInit() {
        const options = {
            color: this.color,
            trailColor: this.trailColor,
            trailWidth: this.trailWidth,
            strokeWidth: this.strokeWidth,
            svgStyle: { width: '100%', height: '100%' },
        };

        this.renderer.setStyle(this.element.nativeElement, 'display', 'block');

        if (this.mode === 'Circle') {
            this.progressBar = new ProgressBar.Circle(this.element.nativeElement, options);
        } else {
            this.progressBar = new ProgressBar.Line(this.element.nativeElement, options);
        }

        this.updateValue();
    }

    public ngOnChanges(changes: SimpleChanges) {
        if (this.progressBar && changes.value) {
            this.updateValue();
        }
    }

    private updateValue() {
        const value = this.value;

        if (this.animated) {
            this.progressBar.animate(value / 100);
        } else {
            this.progressBar.set(value / 100);
        }

        if (value > 0 && this.showText) {
            this.progressBar.setText(`${Math.round(value)}%`);
        }
    }
}
