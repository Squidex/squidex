/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, Component, input, linkedSignal } from '@angular/core';
import { ErrorDto } from '@app/framework/internal';
import { MarkdownDirective } from '../markdown.directive';
import { TranslatePipe } from '../pipes/translate.pipe';

@Component({
    selector: 'sqx-form-error',
    styleUrls: ['./form-error.component.scss'],
    templateUrl: './form-error.component.html',
    imports: [
        MarkdownDirective,
        TranslatePipe,
    ],
})
export class FormErrorComponent {
    /** The error to render. Nothing is rendered when not defined. */
    public readonly error = input.required<ErrorDto | undefined | null>();

    /** Renders the error as a bubble on top of the content. */
    public readonly bubble = input(false, { transform: booleanAttribute });

    /** Shows a button to close the error. */
    public readonly closeable = input(false, { transform: booleanAttribute });

    protected readonly show = linkedSignal(() => !!this.error());

    protected close() {
        this.show.set(false);
    }
}
