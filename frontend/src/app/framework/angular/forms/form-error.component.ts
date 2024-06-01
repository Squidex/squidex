/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */


import { booleanAttribute, ChangeDetectionStrategy, Component, Input } from '@angular/core';
import { ErrorDto, TypedSimpleChanges } from '@app/framework/internal';
import { MarkdownPipe } from '../pipes/markdown.pipe';
import { TranslatePipe } from '../pipes/translate.pipe';
import { SafeHtmlPipe } from '../safe-html.pipe';

@Component({
    standalone: true,
    selector: 'sqx-form-error',
    styleUrls: ['./form-error.component.scss'],
    templateUrl: './form-error.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        MarkdownPipe,
        SafeHtmlPipe,
        TranslatePipe,
    ],
})
export class FormErrorComponent {
    @Input({ required: true })
    public error?: ErrorDto | null;

    @Input({ transform: booleanAttribute })
    public bubble?: boolean | null;

    @Input({ transform: booleanAttribute })
    public closeable?: boolean | null;

    public show = false;

    public ngOnChanges(changes: TypedSimpleChanges<this>) {
        if (changes.error) {
            this.show = !!this.error;
        }
    }

    public close() {
        this.show = false;
    }
}
