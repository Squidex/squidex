/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { HttpClient } from '@angular/common/http';
import { ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';
import { StatefulComponent } from '@app/framework';

interface Snapshot {
    // The text to edit.
    text?: string;
}

@Component({
    selector: 'sqx-asset-text-editor',
    styleUrls: ['./asset-text-editor.component.scss'],
    templateUrl: './asset-text-editor.component.html'
})
export class AssetTextEditorComponent extends StatefulComponent<Snapshot> implements OnInit {
    @Input()
    public fileSource: string;

    @Input()
    public fileName: string;

    @Input()
    public mimeType: string;

    constructor(changeDetector: ChangeDetectorRef,
        private readonly httpClient: HttpClient
    ) {
        super(changeDetector, {});
    }

    public ngOnInit() {
        this.httpClient.get(this.fileSource, { responseType: 'text' })
            .subscribe(text => {
                this.next({ text });
            });
    }

    public toFile(): Promise<Blob | null> {
        return new Promise<Blob | null>(resolve => {
            const blob = new Blob([this.snapshot.text || ''], {
                type: this.mimeType
            });

            resolve(blob);
        });
    }
}