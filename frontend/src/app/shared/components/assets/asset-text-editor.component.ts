/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, Component, inject, input, linkedSignal } from '@angular/core';
import { CodeEditorComponent, safeResource } from '@app/framework';

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
    private readonly httpClient = inject(HttpClient);

    /** The URL to load the text from. */
    public readonly fileSource = input('');

    /** The file name, to choose the syntax highlighting. */
    public readonly fileName = input('');

    /** The mime type of the file that is created from the text. */
    public readonly mimeType = input('');

    protected readonly text = linkedSignal(() => this.textLoaded.value());

    protected readonly textLoaded = safeResource({
        params: () => ({ fileSource: this.fileSource() }),
        stream: ({ params }) => this.httpClient.get(params.fileSource, { responseType: 'text' }),
        defaultValue: '',
    });

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
