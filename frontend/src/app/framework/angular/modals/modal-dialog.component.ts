/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { CdkTrapFocus  } from '@angular/cdk/a11y';
import { afterNextRender, booleanAttribute, ChangeDetectionStrategy, Component, ElementRef, input, output, Renderer2, viewChild, inject } from '@angular/core';
import { TranslatePipe } from '../pipes/translate.pipe';
import { ShortcutComponent } from '../shortcut.component';
import { TourStepDirective } from './tour-step.directive';

@Component({
    selector: 'sqx-modal-dialog',
    styleUrls: ['./modal-dialog.component.scss'],
    templateUrl: './modal-dialog.component.html',
    changeDetection: ChangeDetectionStrategy.Default,
    imports: [
        CdkTrapFocus,
        ShortcutComponent,
        TranslatePipe,
        TourStepDirective,
    ],
})
export class ModalDialogComponent {
    private readonly tabsElement = viewChild<ElementRef<ParentNode>>('tabsElement');
    private readonly footerElement = viewChild<ElementRef<ParentNode>>('footerElement');

    /** Emits when the dialog is closed with the close button or the escape key. */
    public readonly dialogClose = output();

    /** Shows the close button in the header. */
    public readonly showClose = input(true, { transform: booleanAttribute });

    /** Shows the header with the title. */
    public readonly showHeader = input(true, { transform: booleanAttribute });

    /** Shows the footer. It is hidden anyway when no footer content is provided. */
    public readonly showFooter = input(true, { transform: booleanAttribute });

    /** Removes the bottom border of the header, when the dialog has tabs. */
    public readonly hasTabs = input(false, { transform: booleanAttribute });

    /** The width of the dialog. */
    public readonly size = input<'sm' | 'md' | 'lg' | 'xl'>('md');

    /** Renders the body as flex container. */
    public readonly flexBody = input(false, { transform: booleanAttribute });

    /** Uses the full height of the screen. */
    public readonly fullHeight = input(false, { transform: booleanAttribute });

    /** The ID of the tour step that points to this dialog. */
    public readonly tourId = input<string>();

    constructor() {
        const renderer = inject(Renderer2);

        afterNextRender(() => {
            hideWhenEmpty(renderer, this.tabsElement());
            hideWhenEmpty(renderer, this.footerElement());
        });
    }
}

function hideWhenEmpty(renderer: Renderer2, element: ElementRef<ParentNode> | undefined) {
    if (!element?.nativeElement) {
        return;
    }

    if (element.nativeElement.children.length === 0) {
        renderer.setStyle(element.nativeElement, 'display', 'none');
    }
}
