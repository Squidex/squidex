/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { AfterViewInit, booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, EventEmitter, Input, Output, Renderer2, ViewChild } from '@angular/core';
import { fadeAnimation } from '@app/framework/internal';
import { TranslatePipe } from '../pipes/translate.pipe';
import { ShortcutComponent } from '../shortcut.component';
import { TourStepDirective } from './tour-step.directive';

@Component({
    standalone: true,
    selector: 'sqx-modal-dialog',
    styleUrls: ['./modal-dialog.component.scss'],
    templateUrl: './modal-dialog.component.html',
    animations: [
        fadeAnimation,
    ],
    changeDetection: ChangeDetectionStrategy.Default,
    imports: [
        ShortcutComponent,
        TranslatePipe,
        TourStepDirective,
    ],
})
export class ModalDialogComponent implements AfterViewInit {
    @Output()
    public dialogClose = new EventEmitter();

    @Input({ transform: booleanAttribute })
    public showClose?: boolean | null = true;

    @Input({ transform: booleanAttribute })
    public showHeader?: boolean | null = true;

    @Input({ transform: booleanAttribute })
    public showFooter?: boolean | null = true;

    @Input({ transform: booleanAttribute })
    public hasTabs?: boolean | null = false;

    @Input()
    public size: 'sm' | 'md' | 'lg' | 'xl' = 'md';

    @Input({ transform: booleanAttribute })
    public flexBody?: boolean | null;

    @Input({ transform: booleanAttribute })
    public fullHeight?: boolean | null;

    @Input()
    public tourId?: string;

    @ViewChild('tabsElement', { static: false })
    public tabsElement!: ElementRef<ParentNode>;

    @ViewChild('footerElement', { static: false })
    public footerElement!: ElementRef<ParentNode>;

    constructor(
        private readonly renderer: Renderer2,
    ) {
    }

    public ngAfterViewInit() {
        this.hideWhenEmpty(this.tabsElement);
        this.hideWhenEmpty(this.footerElement);
    }

    private hideWhenEmpty(element: ElementRef) {
        if (element && element.nativeElement) {
            const isEmpty = element.nativeElement.children.length === 0;

            if (isEmpty) {
                this.renderer.setStyle(element.nativeElement, 'display', 'none');
            }
        }
    }
}
