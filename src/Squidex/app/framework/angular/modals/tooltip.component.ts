/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnDestroy, OnInit, Renderer2 } from '@angular/core';

import {
    fadeAnimation,
    ModalModel,
    StatefulComponent
} from '@app/framework/internal';

@Component({
    selector: 'sqx-tooltip',
    styleUrls: ['./tooltip.component.scss'],
    templateUrl: './tooltip.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TooltipComponent extends StatefulComponent implements OnDestroy, OnInit {
    @Input()
    public target: any;

    @Input()
    public position = 'topLeft';

    public modal = new ModalModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly renderer: Renderer2
    ) {
        super(changeDetector, {});
    }

    public ngOnInit() {
        if (this.target) {
            this.observe(
                this.renderer.listen(this.target, 'mouseenter', () => {
                    this.modal.show();
                }));

            this.observe(
                this.renderer.listen(this.target, 'mouseleave', () => {
                    this.modal.hide();
                }));
        }
    }
}