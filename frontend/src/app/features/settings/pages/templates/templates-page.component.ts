/*
 * Squidex Headless CMS
 *
 * @license
 * Copyright (c) Squidex UG (haftungsbeschränkt). All rights reserved.
 */

import { Component, OnInit } from '@angular/core';
import { ClientsState, ResourceOwner, TemplateDto, TemplatesState } from '@app/shared';

@Component({
    selector: 'sqx-templates-page',
    styleUrls: ['./templates-page.component.scss'],
    templateUrl: './templates-page.component.html',
})
export class TemplatesPageComponent extends ResourceOwner implements OnInit {
    constructor(
        public readonly clientsState: ClientsState,
        public readonly templatesState: TemplatesState,
    ) {
        super();
    }

    public ngOnInit() {
        this.clientsState.load();

        this.templatesState.load();
    }

    public reload() {
        this.templatesState.load(true);
    }

    public trackByTemplate(_index: number, item: TemplateDto) {
        return item.name;
    }
}
