/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CodeEditorComponent } from '@app/framework';

@Component({
    selector: 'sqx-asset-text-editor',
    styleUrls: ['./asset-text-editor.component.scss'],
    templateUrl: './asset-text-editor.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    imports: [
        CodeEditorComponent,
        FormsModule,
    ],
})
export class AssetTextEditorComponent implements OnInit {
    @Input()
    public fileSource = '';

    @Input()
    public fileName = '';

    @Input()
    public mimeType = '';

    public text = '';

    constructor(
        private readonly changeDetector: ChangeDetectorRef,
        private readonly httpClient: HttpClient,
    ) {
    }

    public ngOnInit() {
        this.httpClient.get(this.fileSource, { responseType: 'text' })
            .subscribe(text => {
                this.text = text;

                this.changeDetector.detectChanges();
            });
    }

    public toFile(): Promise<File | null> {
        return new Promise<File | null>(resolve => {
            const blob = new Blob([this.text || ''], {
                type: this.mimeType,
            });

            const file = new File([blob], 'content.txt', {
                type: this.mimeType,
            });

            resolve(file);
        });
    }
}
