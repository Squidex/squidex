/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, Component, Input, OnInit } from '@angular/core';

import {
    ContentDto,
    fadeAnimation,
    interpolate,
    LocalStoreService,
    ModalModel,
    SchemaDetailsDto
} from '@app/shared';

@Component({
    selector: 'sqx-preview-button',
    styleUrls: ['./preview-button.component.scss'],
    templateUrl: './preview-button.component.html',
    changeDetection: ChangeDetectionStrategy.OnPush,
    animations: [
        fadeAnimation
    ]
})
export class PreviewButtonComponent implements OnInit {
    @Input()
    public content: ContentDto;

    @Input()
    public schema: SchemaDetailsDto;

    public dropdown = new ModalModel();

    public selectedName: string | undefined;

    public alternativeNames: string[];

    constructor(
        private readonly localStore: LocalStoreService
    ) {
    }

    public ngOnInit() {
        let selectedName = this.localStore.get(this.configKey());

        if (!selectedName || !this.schema.previewUrls[selectedName]) {
            selectedName = Object.keys(this.schema.previewUrls)[0];
        }

        this.selectUrl(selectedName);
    }

    public follow(name: string) {
        this.selectUrl(name);

        const url = interpolate(this.schema.previewUrls[name], this.content, 'iv');

        window.open(url, '_blank');
    }

    private selectUrl(selectedName: string) {
        if (this.selectedName !== selectedName) {
            const keys = Object.keys(this.schema.previewUrls);

            this.selectedName = selectedName;

            this.alternativeNames = keys.filter(x => x !== this.selectedName);
            this.alternativeNames.sort();

            this.localStore.set(this.configKey(), selectedName);
        }
    }

    private configKey() {
        return `squidex.schemas.${this.schema.id}.preview-button`;
    }
}