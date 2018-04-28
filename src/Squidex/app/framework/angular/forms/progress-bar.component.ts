/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, ElementRef, Input, OnChanges, OnInit, Renderer, SimpleChanges } from '@angular/core';

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
        private readonly element: ElementRef,
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

        this.renderer.setElementStyle(this.element.nativeElement, 'display', 'block');

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

        this.progressBar.animate(value / 100);

        if (value > 0) {
            this.progressBar.setText(Math.round(value) + '%');
        }
    }
}