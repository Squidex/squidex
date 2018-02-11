/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnDestroy, OnInit, Renderer } from '@angular/core';

import { ModalView } from './../utils/modal-view';

import { fadeAnimation } from './animations';

@Component({
    selector: 'sqx-tooltip',
    styleUrls: ['./tooltip.component.scss'],
    template: `
        <div class="tooltip-container" *sqxModalView="modal;onRoot:true;closeAuto:false" [sqxModalTarget]="target" position="topLeft">
            <ng-content></ng-content>
        </div>`,
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

    public modal = new ModalView(false, false);

    constructor(
        private readonly renderer: Renderer
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
                    // this.changeDetector.detectChanges();
                });

            this.targetMouseLeaveListener =
                this.renderer.listen(this.target, 'mouseleave', () => {
                    this.modal.hide();
                });
        }
    }
}