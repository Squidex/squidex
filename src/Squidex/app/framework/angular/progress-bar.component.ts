/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Sebastian Stehle. All rights reserved
 */

import { Component, ElementRef, Input, OnChanges, OnInit, Renderer } from '@angular/core';

const ProgressBar = require('progressbar.js');

@Component({
    selector: 'sqx-progress-bar',
    template: ''
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
    public value = 0;

    constructor(
        private readonly elementRef: ElementRef,
        private readonly renderer: Renderer
    ) {
    }

    public ngOnInit() {
        const options = {
            color: this.color,
            trailColor: this.trailColor,
            trailWidth: this.trailWidth,
            strokeWidth: this.strokeWidth
        };

        this.renderer.setElementStyle(this.elementRef.nativeElement, 'display', 'block');

        if (this.mode === 'Circle') {
            this.progressBar = new ProgressBar.Circle(this.elementRef.nativeElement, options);
        } else {
            this.progressBar = new ProgressBar.Line(this.elementRef.nativeElement, options);
        }

        this.updateValue();
    }

    public ngOnChanges() {
        if (this.progressBar) {
            this.updateValue();
        }
    }

    private updateValue() {
        const value = this.value;

        this.progressBar.animate(value / 100);

        if (value > 0) {
            this.progressBar.setText(Math.round(value) + '%');
        }
    }
}