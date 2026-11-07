/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, effect, inject, input, signal } from '@angular/core';
import { CodeEditorComponent } from '@app/framework';

@Component({
    selector: 'sqx-asset-text-editor',
    styleUrls: ['./asset-text-editor.component.scss'],
    templateUrl: './asset-text-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
    ],
})
export class AssetTextEditorComponent {
    /** The URL to load the text from. */
    public readonly fileSource = input('');

    /** The file name, to choose the syntax highlighting. */
    public readonly fileName = input('');

    /** The mime type of the file that is created from the text. */
    public readonly mimeType = input('');

    protected readonly text = signal('');

    constructor() {
        const httpClient = inject(HttpClient);

        effect(onCleanup => {
            const subscription =
                httpClient.get(this.fileSource(), { responseType: 'text' })
                    .subscribe(text => {
                        this.text.set(text);
                    });

            onCleanup(() => subscription.unsubscribe());
        });
    }

    public toFile(): Promise<File | null> {
        return new Promise<File | null>(resolve => {
            const mimeType = this.mimeType();

            const blob = new Blob([this.text() || ''], {
                type: mimeType,
            });

            const file = new File([blob], 'content.txt', {
                type: mimeType,
            });

            resolve(file);
        });
    }
}
