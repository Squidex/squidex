/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { ChangeDetectionStrategy, ChangeDetectorRef, Component, Input, OnInit } from '@angular/core';

import {
    ContentDto,
    fadeAnimation,
    interpolate,
    LocalStoreService,
    ModalModel,
    SchemaDetailsDto,
    StatefulComponent
} from '@app/shared';

interface State {
    selectedName?: string;

    alternativeNames: string[];
}

@Component({
    selector: 'sqx-preview-button',
    styleUrls: ['./preview-button.component.scss'],
    templateUrl: './preview-button.component.html',
    animations: [
        fadeAnimation
    ],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class PreviewButtonComponent extends StatefulComponent<State> implements OnInit {
    @Input()
    public content: ContentDto;

    @Input()
    public schema: SchemaDetailsDto;

    public dropdown = new ModalModel();

    constructor(changeDetector: ChangeDetectorRef,
        private readonly localStore: LocalStoreService
    ) {
        super(changeDetector, {
            alternativeNames: []
        });
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
        this.next(s => {
            if (selectedName === s.selectedName) {
                return s;
            }
            const state = { ...s };

            const keys = Object.keys(this.schema.previewUrls);

            state.selectedName = selectedName;

            state.alternativeNames = keys.filter(x => x !== s.selectedName);
            state.alternativeNames.sort();

            this.localStore.set(this.configKey(), selectedName);

            return state;
        });
    }

    private configKey() {
        return `squidex.schemas.${this.schema.id}.preview-button`;
    }
}