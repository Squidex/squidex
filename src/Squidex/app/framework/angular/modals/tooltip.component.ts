/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnDestroy, OnInit, Renderer2 } from '@angular/core';

import { ModalModel } from './../../utils/modal-view';

import { fadeAnimation } from './../animations';

@Component({
    selector: 'sqx-tooltip',
    styleUrls: ['./tooltip.component.scss'],
    templateUrl: './tooltip.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class TooltipComponent implements OnDestroy, OnInit {
    private targetMouseEnterListener: any;
    private targetMouseLeaveListener: any;

    @Input()
    public target: any;

    @Input()
    public position = 'topLeft';

    public modal = new ModalModel();

    constructor(
        private readonly renderer: Renderer2
    ) {
    }

    public ngOnDestroy() {
        if (this.targetMouseEnterListener) {
            this.targetMouseEnterListener();
        }

        if (this.targetMouseLeaveListener) {
            this.targetMouseLeaveListener();
        }
    }

    public ngOnInit() {
        if (this.target) {
            this.targetMouseEnterListener =
                this.renderer.listen(this.target, 'mouseenter', () => {
                    this.modal.show();
                });

            this.targetMouseLeaveListener =
                this.renderer.listen(this.target, 'mouseleave', () => {
                    this.modal.hide();
                });
        }
    }
}